// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class TransactionShim : ITransactionShim
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

    public void CreateVoter(OletxPhase1VolatileEnlistmentContainer managedIdentifier, out IVoterBallotShim voterBallotShim)
    {
        var voterNotifyShim = new VoterNotifyShim(_shimFactory, managedIdentifier);
        var voterShim = new VoterShim(_shimFactory, voterNotifyShim);
        _shimFactory.VoterFactory.Create(Transaction!, voterNotifyShim, out ITransactionVoterBallotAsync2 voterBallot);
        voterShim.VoterBallotAsync2 = voterBallot;
        voterBallotShim = voterShim;
    }

    public void Export(byte[] whereabouts, out byte[] cookieBuffer)
    {
        _shimFactory.ExportFactory.Create((uint)whereabouts.Length, whereabouts, out ITransactionExport export);

        uint cookieSizeULong = 0;

        NativeMethods.Retry(() => export.Export(Transaction!, out cookieSizeULong));

        var cookieSize = (uint)cookieSizeULong;
        var buffer = new byte[cookieSize];
        uint bytesUsed = 0;

        NativeMethods.Retry(() => export.GetTransactionCookie(Transaction!, cookieSize, buffer, out bytesUsed));

        cookieBuffer = buffer;
    }

    public void GetITransactionNative(out IDtcTransaction transactionNative)
        => throw new NotImplementedException();

    public unsafe byte[] GetPropagationToken()
    {
        var cachedTransmitter = _shimFactory.GetCachedTransmitter(Transaction!);
        cachedTransmitter.TxTransmitter.GetPropagationTokenSize(out uint propagationTokenSizeULong);

        var propagationTokenSize = (int)propagationTokenSizeULong;
        var propagationToken = new byte[propagationTokenSize];

        cachedTransmitter.TxTransmitter.MarshalPropagationToken((uint)propagationTokenSize, propagationToken, out uint propagationTokenSizeUsed);

        return propagationToken;
    }

    public void Phase0Enlist(object managedIdentifier, out IPhase0EnlistmentShim phase0EnlistmentShim)
    {
        var phase0Factory = (ITransactionPhase0Factory)Transaction!;
        var phase0NotifyShim = new Phase0NotifyShim(_shimFactory, managedIdentifier);
        var phase0Shim = new Phase0Shim(_shimFactory, phase0NotifyShim);

        phase0Factory.Create(phase0NotifyShim, out ITransactionPhase0EnlistmentAsync phase0Async);
        phase0Shim.Phase0EnlistmentAsync = phase0Async;

        phase0Async.Enable();
        phase0Async.WaitForEnlistment();

        phase0EnlistmentShim = phase0Shim;
    }

    public void GetTransaction(out ITransaction transaction)
        => transaction = Transaction!;
}
