// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    // TODO: Committed
    // TODO: Aborted
    // TODO: HeuristicDecision
    // TODO: InDoubt
}
