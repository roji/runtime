// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim;

/// <summary>
/// The XACTOPT structure contains information for a transaction options object.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms679195(v=vs.85).
/// </remarks>
internal struct Xactopt
{
    internal Xactopt(ulong ulTimeout, string szDescription)
        => (UlTimeout, SzDescription) = (ulTimeout, szDescription);

    /// <summary>
    /// This parameter limits the duration of the transaction and therefore bounds the amount of time that locks are held on database records and system resources.
    /// If the time-out period expires before the transaction commits, the DTC automatically aborts the transaction.
    /// The time-out is specified in milliseconds. A time-out value of zero indicates no time-out.
    /// </summary>
    public ulong UlTimeout;

    // TODO: Marshaling...
    // TODO: Enforce the max length 40?
    [MarshalAs(UnmanagedType.LPStr)]
    public string SzDescription;
}
