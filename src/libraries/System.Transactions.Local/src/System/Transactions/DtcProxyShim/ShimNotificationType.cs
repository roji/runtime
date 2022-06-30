// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Transactions.DtcProxyShim;

internal enum ShimNotificationType
{
    None                        = 0,
    Phase0RequestNotify         = 1,
    VoteRequestNotify           = 2,
    PrepareRequestNotify        = 3,
    CommitRequestNotify         = 4,
    AbortRequestNotify          = 5,
    CommittedNotify             = 6,
    AbortedNotify               = 7,
    InDoubtNotify               = 8,
    EnlistmentTMDownNotify      = 9,
    ResourceManagerTMDownNotify = 10
}
