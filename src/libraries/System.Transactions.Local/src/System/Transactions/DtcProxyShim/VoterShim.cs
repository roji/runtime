// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class VoterShim : IVoterBallotShim
{
    private NotificationShimFactory _shimFactory;
    private VoterNotifyShim _voterNotifyShim;

    internal ITransactionVoterBallotAsync2? VoterBallotAsync2 { get; set; }

    internal VoterShim(NotificationShimFactory shimFactory, VoterNotifyShim notifyShim)
    {
        _shimFactory = shimFactory;
        _voterNotifyShim = notifyShim;
    }

    public void Vote(bool voteYes)
    {
        var voteHr = NativeMethods.S_OK;
        var boid = IntPtr.Zero;

        if (!voteYes)
        {
            voteHr = NativeMethods.E_FAIL;
            // TODO
            // pBoid = &dummyBoid;
        }

        VoterBallotAsync2!.VoteRequestDone(voteHr, boid);
    }
}
