// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Transactions.Oletx;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.DtcProxyShim;

#pragma warning disable CS0169
#pragma warning disable CS0414

internal class ResourceManagerShim : IResourceManagerShim
{
    private NotificationShimFactory _shimFactory;
    private ResourceManagerNotifyShim _pResourceManagerNotifyShim;

    internal ResourceManagerShim(NotificationShimFactory shimFactory, ResourceManagerNotifyShim pNotifyShim)
    {
        _shimFactory = shimFactory;
        _pResourceManagerNotifyShim = pNotifyShim;
    }

    public IResourceManager? ResourceManager { get; set; }

    public void Enlist(
        ITransactionShim transactionShim,
        OletxEnlistment managedIdentifier,
        out IEnlistmentShim enlistmentShim)
    {
        var pEnlistmentNotifyShim = new EnlistmentNotifyShim(_shimFactory, managedIdentifier);
        var pEnlistmentShim = new EnlistmentShim(_shimFactory, pEnlistmentNotifyShim);

        transactionShim.GetTransaction(out var pTransaction);
        ResourceManager!.Enlist(pTransaction, pEnlistmentNotifyShim, out var txUow, out var isoLevel, out var pEnlistmentAsync);

        pEnlistmentNotifyShim.EnlistmentAsync = pEnlistmentAsync;
        pEnlistmentShim.EnlistmentAsync = pEnlistmentAsync;

        enlistmentShim = pEnlistmentShim;
    }

    public void Reenlist(byte[] prepareInfo, out OletxTransactionOutcome outcome)
    {
        // Call Reenlist on the proxy, waiting for 5 milliseconds for it to get the outcome.  If it doesn't know that outcome in that
        // amount of time, tell the caller we don't know the outcome yet.  The managed code will reschedule the check by using the
        // ReenlistThread.
        try
        {
            ResourceManager!.Reenlist(prepareInfo, (ulong)prepareInfo.Length, 5, out var xactStatus);
            outcome = xactStatus switch
            {
                OletxXactStat.XACTSTAT_ABORTED => OletxTransactionOutcome.Aborted,
                OletxXactStat.XACTSTAT_COMMITTED => OletxTransactionOutcome.Committed,
                _ => OletxTransactionOutcome.Aborted
            };
        }
        catch (COMException e) when (e.ErrorCode == NativeMethods.XACT_E_REENLISTTIMEOUT)
        {
            outcome = OletxTransactionOutcome.NotKnownYet;
            return;
        }
    }

    public void ReenlistComplete()
        => ResourceManager!.ReenlistmentComplete();
}
