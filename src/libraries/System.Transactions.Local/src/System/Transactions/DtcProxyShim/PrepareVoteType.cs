// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Transactions.DtcProxyShim;

internal enum PrepareVoteType
{
    ReadOnly                    = 0,
    SinglePhase                 = 1,
    Prepared                    = 2,
    Failed                      = 3,
    InDoubt                     = 4
}
