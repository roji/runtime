// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class Phase0Shim : IPhase0EnlistmentShim
{
    private NotificationShimFactory _shimFactory;
    private Phase0NotifyShim _phase0NotifyShim;

    internal ITransactionPhase0EnlistmentAsync? Phase0EnlistmentAsync { get; set; }

    internal Phase0Shim(NotificationShimFactory shimFactory, Phase0NotifyShim notifyShim)
    {
        _shimFactory = shimFactory;
        _phase0NotifyShim = notifyShim;
        Phase0EnlistmentAsync = null;
    }

    public void Unenlist()
    {
        // VSWhidbey 405624 - There is a race between the enlistment and abort of a transaction
        // that could cause out proxy interface to already be released when Unenlist is called.
        Phase0EnlistmentAsync?.Unenlist();
    }

    public void Phase0Done(bool voteYes)
    {
        if (voteYes)
        {
            try
            {
                Phase0EnlistmentAsync!.Phase0Done();
            }
            catch (COMException e) when (e.ErrorCode == NativeMethods.XACT_E_PROTOCOL)
            {
                // Deal with the proxy bug where we get a Phase0Request(false) on a
                // TMDown and the proxy object state is not changed.
                return;
            }
        }
    }
}
