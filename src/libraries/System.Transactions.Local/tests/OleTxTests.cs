// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// TODO: ifdef for Windows

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions.Tests;
using Xunit;

namespace System.Transactions.Tests;

#nullable enable

public class OleTxTests
{
    [Theory]
    [InlineData(Phase1Vote.Prepared, Phase1Vote.Prepared, EnlistmentOutcome.Aborted, EnlistmentOutcome.Aborted, TransactionStatus.Committed)]
    public void Two_durable_enlistments(Phase1Vote vote1, Phase1Vote vote2, EnlistmentOutcome expectedOutcome1, EnlistmentOutcome expectedOutcome2, TransactionStatus expectedTxStatus)
    {
        var tx = new CommittableTransaction(TimeSpan.FromHours(1));

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

        Assert.Equal(expectedTxStatus, tx.TransactionInformation.Status);
    }

    [Theory]
    [InlineData(Phase1Vote.Prepared, EnlistmentOutcome.Aborted, EnlistmentOutcome.Aborted, TransactionStatus.Committed)]
    public void Promotable_enlistments(Phase1Vote vote1, EnlistmentOutcome expectedOutcome1, EnlistmentOutcome expectedOutcome2, TransactionStatus expectedTxStatus)
    {
        var tx = new CommittableTransaction(TimeSpan.FromHours(1));
        Transaction remoteTx;

        try
        {
            Func<byte[]> promoteDelegate = () =>
            {
                // Simulate creating a distributed transaction on a remote resource (e.g. SQL Server).
                remoteTx = new CommittableTransaction();
                return TransactionInterop.GetTransmitterPropagationToken(remoteTx);
            };

            var promotableEnlistment1 = new TestPromotableSinglePhaseEnlistment(promoteDelegate, expectedOutcome1);

            var promotableEnlistment2 = new TestPromotableSinglePhaseEnlistment(null, expectedOutcome2);

            // 1st promotable enlistment - no distributed transaction yet.
            Assert.True(tx.EnlistPromotableSinglePhase(promotableEnlistment1));
            Assert.True(promotableEnlistment1.InitializedCalled);

            // 2nd promotable enlistment returns false.
            tx.EnlistPromotableSinglePhase(promotableEnlistment2);
            Assert.False(promotableEnlistment2.InitializedCalled);

            // Now enlist a durable enlistment, this will cause the escalation to a distributed transaction.
            // This throws since we're using a single MSDTC here, and Sys.Tx refuses to accepts a transaction from promotion
            // which already exists (we'd need two MSDTC instances to fully test this).
            var durableEnlistment = new TestEnlistment(vote1, expectedOutcome1);
            var exception = Assert.Throws<InvalidOperationException>(() => tx.EnlistDurable(Guid.NewGuid(), durableEnlistment, EnlistmentOptions.None));
            Assert.Equal("The transaction returned from Promote already exists as a distributed transaction.", exception.Message);

            Assert.True(promotableEnlistment1.PromoteCalled);
            Assert.False(promotableEnlistment2.PromoteCalled);

            Assert.Equal(TransactionStatus.Aborted, tx.TransactionInformation.Status);
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

        Assert.Equal(expectedTxStatus, tx.TransactionInformation.Status);
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
}
