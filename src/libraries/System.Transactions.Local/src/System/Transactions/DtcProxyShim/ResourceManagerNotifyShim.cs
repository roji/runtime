// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.Oletx;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.DtcProxyShim;

internal class ResourceManagerNotifyShim : NotificationShimBase, IResourceManagerSink
{
    internal ResourceManagerNotifyShim(
        NotificationShimFactory shimFactory,
        OletxInternalResourceManager enlistmentIdentifier)
        : base(shimFactory, enlistmentIdentifier)
    {
    }

    public void TMDown()
    {
        NotificationType = ShimNotificationType.ResourceManagerTMDownNotify;
        //ShimFactory.NewNotification(this);
    }
}
