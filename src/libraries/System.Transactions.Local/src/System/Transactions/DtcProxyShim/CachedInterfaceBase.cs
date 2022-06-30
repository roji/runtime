// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Transactions.DtcProxyShim;

internal class CachedInterfaceBase
{
    private readonly NotificationShimFactory _shimFactory;

    // TODO: UTLink <CachedInterfaceBase *> link;

    internal CachedInterfaceBase(NotificationShimFactory shimFactory)
        => _shimFactory = shimFactory;
}
