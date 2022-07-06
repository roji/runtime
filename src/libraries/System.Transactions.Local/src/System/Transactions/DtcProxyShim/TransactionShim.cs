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
    public void CreateVoter(IntPtr managedIdentifier, [MarshalAs(UnmanagedType.Interface)] out IVoterBallotShim voterBallotShim) => throw new NotImplementedException();
    public void Export([MarshalAs(UnmanagedType.U4)] uint whereaboutsSize, [MarshalAs(UnmanagedType.LPArray)] byte[] whereabouts, [MarshalAs(UnmanagedType.I4)] out int cookieIndex, [MarshalAs(UnmanagedType.U4)] out uint cookieSize, out CoTaskMemHandle cookieBuffer) => throw new NotImplementedException();
    public void GetITransactionNative([MarshalAs(UnmanagedType.Interface)] out IDtcTransaction transactionNative) => throw new NotImplementedException();
    public void GetPropagationToken([MarshalAs(UnmanagedType.U4)] out uint propagationTokeSize, out CoTaskMemHandle propagationToken) => throw new NotImplementedException();
    public void Phase0Enlist(IntPtr managedIdentifier, [MarshalAs(UnmanagedType.Interface)] out IPhase0EnlistmentShim phase0EnlistmentShim) => throw new NotImplementedException();

    public void GetTransaction(out ITransaction transaction)
        => transaction = Transaction!;
}
