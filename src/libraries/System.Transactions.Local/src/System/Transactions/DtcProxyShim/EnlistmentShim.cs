// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal class EnlistmentShim : IEnlistmentShim
{
    private readonly NotificationShimFactory _shimFactory;
    private readonly EnlistmentNotifyShim _enlistmentNotifyShim;

    internal ITransactionEnlistmentAsync? EnlistmentAsync { get; set; }

    internal EnlistmentShim(NotificationShimFactory shimFactory, EnlistmentNotifyShim notifyShim)
        => (_shimFactory, _enlistmentNotifyShim) = (shimFactory, notifyShim);

    public void PrepareRequestDone(OletxPrepareVoteType voteType)
    {
        var voteHr = NativeMethods.S_OK;
        var releaseEnlistment = false;

        switch (voteType)
        {
            case OletxPrepareVoteType.ReadOnly:
                {
                    // On W2k Proxy may send a spurious aborted notification if the TM goes down.
                    _enlistmentNotifyShim.SetIgnoreSpuriousProxyNotifications();
                    voteHr = NativeMethods.XACT_S_READONLY;
                    break;
                }

            case OletxPrepareVoteType.SinglePhase:
                {
                    // On W2k Proxy may send a spurious aborted notification if the TM goes down.
                    _enlistmentNotifyShim.SetIgnoreSpuriousProxyNotifications();
                    voteHr = NativeMethods.XACT_S_SINGLEPHASE;
                    break;
                }

            case OletxPrepareVoteType.Prepared:
                {
                    voteHr = NativeMethods.S_OK;
                    break;
                }

            case OletxPrepareVoteType.Failed:
                {
                    // Proxy may send a spurious aborted notification if the TM goes down.
                    _enlistmentNotifyShim.SetIgnoreSpuriousProxyNotifications();
                    voteHr = NativeMethods.E_FAIL;
                    //pBoid = &dummyBoid;
                    break;
                }

            case OletxPrepareVoteType.InDoubt:
                {
                    releaseEnlistment = true;
                    break;
                }

            default:  // unexpected, vote no.
                {
                    voteHr = NativeMethods.E_FAIL;
                    //pBoid = &dummyBoid;
                    break;
                }
        }

        if (!releaseEnlistment)
        {
            EnlistmentAsync!.PrepareRequestDone(
                voteHr,
                IntPtr.Zero,
                IntPtr.Zero);
        }
    }

    public void CommitRequestDone()
        => EnlistmentAsync!.CommitRequestDone(NativeMethods.S_OK);

    public void AbortRequestDone()
        => EnlistmentAsync!.AbortRequestDone(NativeMethods.S_OK);
}
