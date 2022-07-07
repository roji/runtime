// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;

namespace System.Transactions.Tests
{
    public class ShayTests
    {
        [Fact]
        public void EnlistNonPromotable()
        {
            var tx = new CommittableTransaction();

            tx.EnlistDurable(Guid.Parse("ceb6c6b2-7fbf-43e3-ab81-8cb67ccd0b8e"), new TestEnlistmentNotification(), EnlistmentOptions.None);
        }

        [Fact]
        public void EnlistPromotableSinglePhase()
        {
            var tx = new CommittableTransaction();

            Assert.True(tx.EnlistPromotableSinglePhase(new TestPromotableSinglePhaseNotification()));
            Assert.False(tx.EnlistPromotableSinglePhase(new TestPromotableSinglePhaseNotification()));

            var whereabouts = TransactionInterop.GetWhereabouts();

            var transactionCookie = TransactionInterop.GetExportCookie(tx, whereabouts);

        }
    }

    class TestEnlistmentNotification : IEnlistmentNotification
    {
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine("Preparing");
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("Committing");
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine("Rolling back");
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("In doubt");
            enlistment.Done();
        }
    }

    class TestSinglePhaseNotification : ISinglePhaseNotification
    {
        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("Committing");
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine("Preparing");
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine("Rolling back");
            enlistment.Done();
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Console.WriteLine("Single phase commit");
            singlePhaseEnlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("In doubt");
            enlistment.Done();
        }
    }

    class TestPromotableSinglePhaseNotification : IPromotableSinglePhaseNotification
    {
        Transaction _remoteTransaction;
        TestEnlistmentNotification _remoteEnlistmentNotification;

        public void Initialize()
        {
            Console.WriteLine("Initialize");
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Console.WriteLine("SinglePhaseCommit");
            singlePhaseEnlistment.Done();
        }

        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Console.WriteLine("Rollback");
            singlePhaseEnlistment.Done();
        }

        public byte[]? Promote()
        {
            _remoteEnlistmentNotification = new TestEnlistmentNotification();
            _remoteTransaction = new CommittableTransaction();
            _remoteTransaction.EnlistDurable(Guid.Parse("ceb6c6b2-7fbf-43e3-ab81-8cb67ccd0b8e"), _remoteEnlistmentNotification, EnlistmentOptions.None);

            return TransactionInterop.GetTransmitterPropagationToken(_remoteTransaction);
        }
    }
}
