// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim;

internal class NotificationShimBase
{
    //public UTLink<NotificationShimBase*> Link;
    public object? EnlistmentIdentifier;
    public ShimNotificationType NotificationType;
    public bool AbortingHint;
    public bool IsSinglePhase;
    public byte[]? PPrepareInfo;

    protected long RefCount;
    protected NotificationShimFactory ShimFactory;

    internal NotificationShimBase(
        NotificationShimFactory shimFactory,
        object? enlistmentIdentifier)
    {
        ShimFactory = shimFactory;
        //ShimFactory->AddRef();
        EnlistmentIdentifier = enlistmentIdentifier;
        RefCount = 0;
        NotificationType = ShimNotificationType.None;
        AbortingHint = false;
        IsSinglePhase = false;
        //PPrepareInfo = null;

        // From the original C++ code:
        // do this in the derived constructors to get offsets right.
        //#pragma warning(4 : 4355)
        //      link.Init( this );
        //#pragma warning(default : 4355)
    }
}
