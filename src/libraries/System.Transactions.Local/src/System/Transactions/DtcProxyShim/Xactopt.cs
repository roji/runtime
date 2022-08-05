// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim;

/// <summary>
/// The XACTOPT structure contains information for a transaction options object.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms679195(v=vs.85).
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct Xactopt
{
    internal Xactopt(uint ulTimeout, string szDescription)
        => (UlTimeout, SzDescription) = (ulTimeout, szDescription);

    /// <summary>
    /// This parameter limits the duration of the transaction and therefore bounds the amount of time that locks are held on database records and system resources.
    /// If the time-out period expires before the transaction commits, the DTC automatically aborts the transaction.
    /// The time-out is specified in milliseconds. A time-out value of zero indicates no time-out.
    /// </summary>
    public uint UlTimeout;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
    public string SzDescription;
}
