// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;

namespace System.Transactions.Tests
{
    public class ShayTests
    {
        [Fact]
        public void Foo()
        {
            var tx = new CommittableTransaction();

            var rmGuid = Guid.Parse("ceb6c6b2-7fbf-43e3-ab81-8cb67ccd0b8e");
            tx.EnlistDurable(rmGuid, new DurableResourceManager(), EnlistmentOptions.None);
        }
    }

    class DurableResourceManager : IEnlistmentNotification
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
}
