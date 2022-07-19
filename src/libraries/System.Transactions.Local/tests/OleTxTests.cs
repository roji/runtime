// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Diagnostics.Runtime.Interop;
using Microsoft.DotNet.RemoteExecutor;
using Xunit;
using Xunit.Sdk;

namespace System.Transactions.Tests;

#nullable enable

[PlatformSpecific(TestPlatforms.Windows)]
public class OleTxTests
{
    [Theory]
    [InlineData(Phase1Vote.Prepared, Phase1Vote.Prepared, EnlistmentOutcome.Committed, EnlistmentOutcome.Committed, TransactionStatus.Committed)]
    [InlineData(Phase1Vote.Prepared, Phase1Vote.ForceRollback, EnlistmentOutcome.Aborted, EnlistmentOutcome.Aborted, TransactionStatus.Aborted)]
    [InlineData(Phase1Vote.ForceRollback, Phase1Vote.Prepared, EnlistmentOutcome.Aborted, EnlistmentOutcome.Aborted, TransactionStatus.Aborted)]
    public void Two_durable_enlistments_commit(Phase1Vote vote1, Phase1Vote vote2, EnlistmentOutcome expectedOutcome1, EnlistmentOutcome expectedOutcome2, TransactionStatus expectedTxStatus)
    {
        var tx = new CommittableTransaction();

        try
        {
            var enlistment1 = new TestEnlistment(vote1, expectedOutcome1);
            var enlistment2 = new TestEnlistment(vote2, expectedOutcome2);

            tx.EnlistDurable(Guid.NewGuid(), enlistment1, EnlistmentOptions.None);
            tx.EnlistDurable(Guid.NewGuid(), enlistment2, EnlistmentOptions.None);

            Assert.Equal(TransactionStatus.Active, tx.TransactionInformation.Status);
            tx.Commit();
        }
        catch (TransactionInDoubtException)
        {
            Assert.Equal(TransactionStatus.InDoubt, expectedTxStatus);
        }
        catch (TransactionAbortedException)
        {
            Assert.Equal(TransactionStatus.Aborted, expectedTxStatus);
        }

        Retry(() => Assert.Equal(expectedTxStatus, tx.TransactionInformation.Status));
    }

    [Fact]
    public void Two_durable_enlistments_rollback()
    {
        var tx = new CommittableTransaction();

        var enlistment1 = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Aborted);
        var enlistment2 = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Aborted);

        tx.EnlistDurable(Guid.NewGuid(), enlistment1, EnlistmentOptions.None);
        tx.EnlistDurable(Guid.NewGuid(), enlistment2, EnlistmentOptions.None);

        tx.Rollback();

        Assert.False(enlistment1.WasPreparedCalled);
        Assert.False(enlistment2.WasPreparedCalled);

        // This matches the .NET Framework behavior
        Retry(() => Assert.Equal(TransactionStatus.Aborted, tx.TransactionInformation.Status));
    }

    [Fact]
    public void Promotable_enlistments()
    {
        var tx = new CommittableTransaction();
        Transaction remoteTx;

        Func<byte[]> promoteDelegate = () =>
        {
            // Simulate creating a distributed transaction on a remote resource (e.g. SQL Server).
            remoteTx = new CommittableTransaction();
            return TransactionInterop.GetTransmitterPropagationToken(remoteTx);
        };

        var promotableEnlistment1 = new TestPromotableSinglePhaseEnlistment(promoteDelegate, EnlistmentOutcome.Aborted);

        var promotableEnlistment2 = new TestPromotableSinglePhaseEnlistment(null, EnlistmentOutcome.Aborted);

        // 1st promotable enlistment - no distributed transaction yet.
        Assert.True(tx.EnlistPromotableSinglePhase(promotableEnlistment1));
        Assert.True(promotableEnlistment1.InitializedCalled);

        // 2nd promotable enlistment returns false.
        Assert.False(tx.EnlistPromotableSinglePhase(promotableEnlistment2));
        Assert.False(promotableEnlistment2.InitializedCalled);

        // Now enlist a durable enlistment, this will cause the escalation to a distributed transaction.
        // This throws since we're using a single MSDTC here, and Sys.Tx refuses to accepts a transaction from promotion
        // which already exists (we'd need two processes to fully test this).
        var durableEnlistment = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Aborted);
        var exception = Assert.Throws<InvalidOperationException>(() => tx.EnlistDurable(Guid.NewGuid(), durableEnlistment, EnlistmentOptions.None));
        Assert.Equal("The transaction returned from Promote already exists as a distributed transaction.", exception.Message);

        Assert.True(promotableEnlistment1.PromoteCalled);
        Assert.False(promotableEnlistment2.PromoteCalled);

        Assert.Equal(TransactionStatus.Aborted, tx.TransactionInformation.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Volatile_and_durable_enlistments(int volatileCount)
    {
        var tx = new CommittableTransaction();

        if (volatileCount > 0)
        {
            TestEnlistment[] volatiles = new TestEnlistment[volatileCount];
            for (int i = 0; i < volatileCount; i++)
            {
                // It doesn't matter what we specify for SinglePhaseVote.
                volatiles[i] = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Committed);
                tx.EnlistVolatile(volatiles[i], EnlistmentOptions.None);
            }
        }

        TestEnlistment durable = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Committed);

        // Creation of two phase durable enlistment attempts to promote to MSDTC
        tx.EnlistDurable(Guid.NewGuid(), durable, EnlistmentOptions.None);

        tx.Commit();

        Retry(() => Assert.Equal(TransactionStatus.Committed, tx.TransactionInformation.Status));
    }

    [ConditionalFact(typeof(RemoteExecutor), nameof(RemoteExecutor.IsSupported))]
    public void Recovery()
    {
        var tx = new CommittableTransaction(TimeSpan.FromHours(1));

        var outcomeEvent1 = new AutoResetEvent(false);
        var enlistment1 = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Committed, outcomeReceived: outcomeEvent1);
        var guid1 = Guid.NewGuid();
        tx.EnlistDurable(guid1, enlistment1, EnlistmentOptions.None);

        // We are going to spin up an external process to also enlist in the transaction, and then to crash when it receives the commit notification.
        // We will then initiate the recovery flow.

        // The propagation token is used to propagate the transaction to that process so it can enlist to our transaction.
        // We also provide the resource manager identifier GUID, and a path where the external process will write the recovery information it will
        // receive from the MSDTC when preparing.
        // We'll need these two elements later ni order to Reenlist and trigger recovery.
        var propagationToken = TransactionInterop.GetTransmitterPropagationToken(tx);
        var propagationTokenText = Convert.ToBase64String(propagationToken);
        var guid2 = Guid.NewGuid();
        var secondEnlistmentRecoveryFilePath = Path.GetTempFileName();

        using var waitHandle = new EventWaitHandle(initialState: false, EventResetMode.ManualReset, "System.Transactions.Tests.OleTxTests.WaitHandle");

        try
        {
            using (var remoteExecutor = RemoteExecutor.Invoke(EnlistAndCrash, propagationTokenText, guid2.ToString(), secondEnlistmentRecoveryFilePath, new RemoteInvokeOptions { ExpectedExitCode = 42 }))
            {
                // Wait for the external process to enlist in the transaction, it will signal this EventWaitHandle after it does.
                waitHandle.WaitOne();

                tx.Commit();
            }

            // The other has crashed when the MSDTC notified it to commit.

            // First, reenlist with the wrong recovery information, to test that negative flow
            var enlistment3 = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Committed);
            Assert.Throws<TransactionException>(() => TransactionManager.Reenlist(guid2, enlistment1.RecoveryInformation!, enlistment3));

            // Now load the correct recovery information from disk and reenlist with the failed RM's Guid to commit.
            var secondRecoveryInformation = File.ReadAllBytes(secondEnlistmentRecoveryFilePath);
            var enlistmentWat = TransactionManager.Reenlist(guid2, secondRecoveryInformation, enlistment3);
            TransactionManager.RecoveryComplete(guid2);

            Assert.Equal(EnlistmentOutcome.Committed, enlistment3.Outcome);

            // Note: verify manually in the MSDTC console that the distributed transaction is gone (i.e. successfully committed),
            // (Start -> Component Services -> Computers -> My Computer -> Distributed Transaction Coordinator -> Local DTC -> Transaction List)
        }
        finally
        {
            if (File.Exists(secondEnlistmentRecoveryFilePath))
            {
                File.Delete(secondEnlistmentRecoveryFilePath);
            }
        }

        static void EnlistAndCrash(string propagationTokenText, string resourceManagerIdentifierGuid, string recoveryInformationFilePath)
        {
            var propagationToken = Convert.FromBase64String(propagationTokenText);
            var tx = TransactionInterop.GetTransactionFromTransmitterPropagationToken(propagationToken);

            var crashingEnlistment = new CrashingEnlistment(recoveryInformationFilePath);
            tx.EnlistDurable(Guid.Parse(resourceManagerIdentifierGuid), crashingEnlistment, EnlistmentOptions.None);

            // Signal to the main process that we've enlisted and are ready to accept prepare/commit.
            using var waitHandle = new EventWaitHandle(initialState: false, EventResetMode.ManualReset, "System.Transactions.Tests.OleTxTests.WaitHandle");
            waitHandle.Set();

            // We've enlisted, and set it up so that when the MSDTC tells us to commit, the process will crash.
            Thread.Sleep(TimeSpan.FromDays(1));
        }
    }

    public class CrashingEnlistment : IEnlistmentNotification
    {
        private string _recoveryInformationFilePath;

        public CrashingEnlistment(string recoveryInformationFilePath)
            => _recoveryInformationFilePath = recoveryInformationFilePath;

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Received a prepare notification from MSDTC, persist the recovery information so that the main process can perform recovery for it.
            File.WriteAllBytes(_recoveryInformationFilePath, preparingEnlistment.RecoveryInformation());

            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
            => Environment.Exit(42); // 42 is error code expected by RemoteExecutor

        public void Rollback(Enlistment enlistment)
            => Environment.Exit(1);

        public void InDoubt(Enlistment enlistment)
            => Environment.Exit(1);
    }

    [Fact]
    public void TransmitterPropagationToken()
    {
        var tx = new CommittableTransaction();

        Assert.Equal(Guid.Empty, tx.TransactionInformation.DistributedIdentifier);

        var propagationToken = TransactionInterop.GetTransmitterPropagationToken(tx);

        Assert.NotEqual(Guid.Empty, tx.TransactionInformation.DistributedIdentifier);

        var tx2 = TransactionInterop.GetTransactionFromTransmitterPropagationToken(propagationToken);

        Assert.Equal(tx.TransactionInformation.DistributedIdentifier, tx2.TransactionInformation.DistributedIdentifier);
    }

    [Fact]
    public void GetExportCookie()
    {
        var tx = new CommittableTransaction();

        var whereabouts = TransactionInterop.GetWhereabouts();

        Assert.Equal(Guid.Empty, tx.TransactionInformation.DistributedIdentifier);

        var exportCookie = TransactionInterop.GetExportCookie(tx, whereabouts);

        Assert.NotEqual(Guid.Empty, tx.TransactionInformation.DistributedIdentifier);

        var tx2 = TransactionInterop.GetTransactionFromExportCookie(exportCookie);

        Assert.Equal(tx.TransactionInformation.DistributedIdentifier, tx2.TransactionInformation.DistributedIdentifier);
    }

    // MSDTC is aynchronous, i.e. Commit/Rollback may return before the transaction has actually completed;
    // so allow some time for assertions to succeed.
    private static void Retry(Action action)
    {
        const int Retries = 50;

        for (var i = 0; i < Retries; i++)
        {
            try
            {
                action();
                return;
            }
            catch (EqualException)
            {
                if (i == Retries - 1)
                {
                    throw;
                }

                Thread.Sleep(100);
            }
        }
    }

    const int MaxTransactionCommitTimeoutInSeconds = 5;
}
