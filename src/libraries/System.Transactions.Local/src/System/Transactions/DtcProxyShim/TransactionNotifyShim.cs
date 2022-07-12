// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class TransactionNotifyShim : NotificationShimBase, ITransactionOutcomeEvents
{
    internal TransactionNotifyShim(NotificationShimFactory shimFactory, object? enlistmentIdentifier)
        : base(shimFactory, enlistmentIdentifier)
    {
    }

    public void Committed(bool fRetaining, Guid pNewUOW /* always null? */, uint hresult)
    {
        NotificationType = ShimNotificationType.CommittedNotify;
        ShimFactory.NewNotification(this);
    }

    public void Aborted(IntPtr pboidReason, bool fRetaining, Guid pNewUOW, uint hresult)
    {
        NotificationType = ShimNotificationType.AbortedNotify;
        ShimFactory.NewNotification(this);
    }

    public void HeuristicDecision(OletxTransactionHeuristic dwDecision, IntPtr pboidReason, uint hresult)
    {
        NotificationType = dwDecision switch
        {
            OletxTransactionHeuristic.XACTHEURISTIC_ABORT => ShimNotificationType.AbortedNotify,
            OletxTransactionHeuristic.XACTHEURISTIC_COMMIT => ShimNotificationType.CommittedNotify,
            _ => ShimNotificationType.InDoubtNotify
        };

        ShimFactory.NewNotification(this);
    }

    public void Indoubt()
    {
        NotificationType = ShimNotificationType.InDoubtNotify;
        ShimFactory.NewNotification(this);
    }
}
