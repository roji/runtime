// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.DtcProxyShim;

internal class CachedTransmitter : CachedInterfaceBase
{
    public ITransactionTransmitter TxTransmitter { get; private set; }

    internal CachedTransmitter(NotificationShimFactory shimFactory, ITransactionTransmitter transmitter)
        : base(shimFactory)
    {
        //         link.Init( this );

        TxTransmitter = transmitter;
    }
}
