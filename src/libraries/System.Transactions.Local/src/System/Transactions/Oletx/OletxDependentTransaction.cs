// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Transactions.Diagnostics;

namespace System.Transactions.Oletx;

[Serializable]
internal sealed class OletxDependentTransaction : OletxTransaction
{
    private OletxVolatileEnlistmentContainer _volatileEnlistmentContainer;

    private int _completed;

    internal OletxDependentTransaction(RealOletxTransaction realTransaction, bool delayCommit)
        : base(realTransaction)
    {
        if (realTransaction == null)
        {
            throw new ArgumentNullException(nameof(realTransaction));
        }

        _volatileEnlistmentContainer = RealOletxTransaction.AddDependentClone(delayCommit);

        if (DiagnosticTrace.Information)
        {
            DependentCloneCreatedTraceRecord.Trace(
                SR.TraceSourceOletx,
                TransactionTraceId,
                delayCommit
                    ? DependentCloneOption.BlockCommitUntilComplete
                    : DependentCloneOption.RollbackIfNotComplete);
        }
    }

    public void Complete()
    {
        TransactionsEtwProvider etwLog = TransactionsEtwProvider.Log;
        if (etwLog.IsEnabled())
        {
            etwLog.MethodEnter(TraceSourceType.TraceSourceOleTx, this, $"{nameof(DependentTransaction)}.{nameof(Complete)}");
        }

        Debug.Assert(Disposed == 0, "OletxTransction object is disposed");

        int localCompleted = Interlocked.CompareExchange(ref _completed, 1, 0);
        if (localCompleted == 1)
        {
            throw TransactionException.CreateTransactionCompletedException(DistributedTxId);
        }

        if (DiagnosticTrace.Information)
        {
            DependentCloneCompleteTraceRecord.Trace(SR.TraceSourceOletx, TransactionTraceId);
        }

        _volatileEnlistmentContainer.DependentCloneCompleted();

        if (etwLog.IsEnabled())
        {
            etwLog.MethodExit(TraceSourceType.TraceSourceOleTx, this, $"{nameof(DependentTransaction)}.{nameof(Complete)}");
        }
    }
}
