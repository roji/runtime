// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal class EnlistmentNotifyShim : NotificationShimBase, ITransactionResourceAsync
{
    private ITransactionEnlistmentAsync? _enlistmentAsync;

    internal ITransactionEnlistmentAsync? EnlistmentAsync { get; set; }

    // MSDTCPRX behaves unpredictably in that if the TM is down when we vote
    // no it will send an AbortRequest.  However if the TM does not go down
    // the enlistment is not go down the AbortRequest is not sent.  This
    // makes reliable cleanup a problem.  To work around this the enlisment
    // shim will eat the AbortRequest if it knows that it has voted No.

    // On Win2k this same problem applies to responding Committed to a
    // single phase commit request.
    private bool _ignoreSpuriousProxyNotifications;

    internal EnlistmentNotifyShim(NotificationShimFactory shimFactory, OletxEnlistment enlistmentIdentifier)
        : base(shimFactory, enlistmentIdentifier)
    {
        // link.Init( this );
        _ignoreSpuriousProxyNotifications = false;
    }

    internal void SetIgnoreSpuriousProxyNotifications()
        => _ignoreSpuriousProxyNotifications = true;

    public void PrepareRequest(bool fRetaining, OletxXactRm grfRM, bool fWantMoniker, bool fSinglePhase)
    {
        var pEnlistmentAsync = Interlocked.Exchange(ref _enlistmentAsync, null);

        if (pEnlistmentAsync is null)
        {
            throw new InvalidOperationException("Unexpected null in pEnlistmentAsync");
        }

        var pPrepareInfo = (IPrepareInfo)pEnlistmentAsync;
        pPrepareInfo.GetPrepareInfoSize(out var prepareInfoLength);
        var prepareInfoBuffer = new byte[prepareInfoLength];
        pPrepareInfo.GetPrepareInfo(prepareInfoBuffer);

        PPrepareInfo = prepareInfoBuffer;
        IsSinglePhase = fSinglePhase;
        NotificationType = ShimNotificationType.PrepareRequestNotify;
        ShimFactory.NewNotification(this);
    }

    public void CommitRequest(OletxXactRm grfRM, Guid pNewUOW)
    {
        NotificationType = ShimNotificationType.CommitRequestNotify;
        ShimFactory.NewNotification(this);
    }

    public void AbortRequest(IntPtr pboidReason, bool fRetaining, Guid pNewUOW)
    {
        if (!_ignoreSpuriousProxyNotifications)
        {
            // Only create the notification if we have not already voted.
            NotificationType = ShimNotificationType.AbortRequestNotify;
            ShimFactory.NewNotification(this);
        }
    }

    public void TMDown()
    {
        NotificationType = ShimNotificationType.EnlistmentTMDownNotify;
        ShimFactory.NewNotification(this);
    }
}
