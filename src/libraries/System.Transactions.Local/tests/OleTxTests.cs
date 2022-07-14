// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
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
        tx.EnlistPromotableSinglePhase(promotableEnlistment2);
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
                volatiles[i] = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Aborted);
                tx.EnlistVolatile(volatiles[i], EnlistmentOptions.None);
            }
        }

        TestEnlistment durable = new TestEnlistment(Phase1Vote.Prepared, EnlistmentOutcome.Aborted);

        // Creation of two phase durable enlistment attempts to promote to MSDTC
        tx.EnlistDurable(Guid.NewGuid(), durable, EnlistmentOptions.None);

        tx.Commit();

        Retry(() => Assert.Equal(TransactionStatus.Committed, tx.TransactionInformation.Status));
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
}
