// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.DtcProxyShim;

internal sealed class CachedOptions : CachedInterfaceBase
{
    public ITransactionOptions PTxOptions { get; }

    internal CachedOptions(NotificationShimFactory shimFactory, ITransactionOptions pOptions)
        : base(shimFactory)
    {
        PTxOptions = pOptions;
        // link.Init(this);

    }
}
