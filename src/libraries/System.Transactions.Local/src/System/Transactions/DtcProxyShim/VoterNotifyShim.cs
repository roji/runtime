// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class VoterNotifyShim : NotificationShimBase, ITransactionVoterNotifyAsync2
{
    internal VoterNotifyShim(NotificationShimFactory shimFactory, object enlistmentIdentifier)
        : base(shimFactory, enlistmentIdentifier)
    {
    }

    public void VoteRequest()
    {
        NotificationType = ShimNotificationType.VoteRequestNotify;
        ShimFactory.NewNotification(this);
    }

    // TODO
    public void Committed([MarshalAs(UnmanagedType.Bool)] bool fRetaining, Guid pNewUOW, uint hresult)
        => throw new NotImplementedException();
    public void Aborted(IntPtr pboidReason, [MarshalAs(UnmanagedType.Bool)] bool fRetaining, Guid pNewUOW, uint hresult)
        => throw new NotImplementedException();
    public void HeuristicDecision([MarshalAs(UnmanagedType.U4)] OletxTransactionHeuristic dwDecision, IntPtr pboidReason, uint hresult)
        => throw new NotImplementedException();
    public void Indoubt()
        => throw new NotImplementedException();
}
