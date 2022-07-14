// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITransaction interface is used to commit and abort transactions and to obtain status information about transactions.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686531(v=vs.85).
/// </remarks>
[ComImport, Guid(Guids.IID_ITransaction), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransaction
{
    /// <summary>
    /// This method commits the transaction. The Commit method may only be called by the initiator of the transaction.
    /// </summary>
    /// <param name="fRetainingt">Must be FALSE.</param>
    /// <param name="grfTC">Values taken from the <see cref="OletxXacttc" /> enumeration</param>
    /// <param name="grfRM">Must be zero.</param>
    void Commit([MarshalAs(UnmanagedType.Bool)] bool fRetainingt, [MarshalAs(UnmanagedType.U4)] OletxXacttc grfTC, uint grfRM);

    /// <summary>
    /// This method aborts the transaction.
    /// </summary>
    /// <param name="reason">
    /// An optional BOID that indicates why the transaction is being aborted. This argument may be NULL indicating that no abort reason is provided.
    /// </param>
    /// <param name="retaining">Must be false.</param>
    /// <param name="async">
    /// When fAsync is true, an asynchronous abort is performed and the caller must use ITransactionOutcomeEvents to learn the outcome of the transaction.
    /// </param>
    void Abort(IntPtr reason, [MarshalAs(UnmanagedType.Bool)] bool retaining, [MarshalAs(UnmanagedType.Bool)] bool async);

    /// <summary>
    /// This method returns information regarding a transaction object.
    /// </summary>
    /// <param name="xactInfo">
    /// Pointer to the caller allocated XACTTRANSINFO structure which will receive information about the transaction. Must not be null.
    /// </param>
    void GetTransactionInfo(out OletxXactTransInfo xactInfo);
}
