// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal sealed class CachedReceiver : CachedInterfaceBase
{
    public ITransactionReceiver TxReceiver { get; private set; }

    internal CachedReceiver(NotificationShimFactory shimFactory, ITransactionReceiver receiver)
        : base(shimFactory)
    {
        //         link.Init( this );

        TxReceiver = receiver;
    }
}
