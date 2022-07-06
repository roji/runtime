// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal class TransactionShim : ITransactionShim
{
    private NotificationShimFactory _shimFactory;
    private TransactionNotifyShim _transactionNotifyShim;

    internal ITransaction? Transaction { get; set; }

    internal TransactionShim(NotificationShimFactory shimFactory, TransactionNotifyShim notifyShim)
    {
        _shimFactory = shimFactory;
        _transactionNotifyShim = notifyShim;
    }

    public void Commit()
    {
        Transaction!.Commit(false, OletxXacttc.XACTTC_ASYNC, 0);
    }

    public void Abort() => throw new NotImplementedException();

    public void CreateVoter(IntPtr managedIdentifier, out IVoterBallotShim voterBallotShim)
        => throw new NotImplementedException();

    public void Export(byte[] whereabouts, out byte[] cookieBuffer)
    {
        //_shimFactory.ExportFactory.GetRemoteClassId(out var guid);
        _shimFactory.ExportFactory.Create((ulong)whereabouts.Length, whereabouts, out var export);

        ulong cookieSizeULong = 0;

        NativeMethods.Retry(() => export.Export(Transaction!, out cookieSizeULong));

        var cookieSize = (uint)cookieSizeULong;
        var buffer = new byte[cookieSize];
        ulong bytesUsed = 0;

        NativeMethods.Retry(() => export.GetTransactionCookie(Transaction!, cookieSize, buffer, out bytesUsed));

        cookieBuffer = buffer;
    }

    public void GetITransactionNative(out IDtcTransaction transactionNative)
        => throw new NotImplementedException();

    public void GetPropagationToken(out uint propagationTokeSize, out CoTaskMemHandle propagationToken)
        => throw new NotImplementedException();

    public void Phase0Enlist(IntPtr managedIdentifier, out IPhase0EnlistmentShim phase0EnlistmentShim)
        => throw new NotImplementedException();

    public void GetTransaction(out ITransaction transaction)
        => transaction = Transaction!;
}
