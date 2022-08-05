// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.DtcProxyShim;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// This interface contains two methods.
/// The BeginTransaction method creates new transaction objects.
/// The GetOptionsObject method creates new transaction options objects.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms679525(v=vs.85).
/// </remarks>
[ComImport, Guid(Guids.IID_ITransactionDispenser), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionDispenser
{
    /// <summary>
    /// This method creates a transaction options object.
    /// </summary>
    /// <param name="ppOptions">
    /// Pointer to the pointer to the ITransactionOptions interface on the transaction options object. Must not be NULL.
    /// </param>
    void GetOptionsObject([MarshalAs(UnmanagedType.Interface)] out ITransactionOptions ppOptions);

    /// <summary>
    /// This method initiates a new transaction and returns a new transaction object which represents the transaction.
    /// </summary>
    /// <param name="punkOuter">Must be NULL.</param>
    /// <param name="isoLevel">
    /// The isolation level to be used for this transaction, specified by the ISOLATIONLEVEL enumeration. This value is ignored by DTC and passed on to the resource managers.
    /// </param>
    /// <param name="isoFlags">Values from ISOFLAG enumeration.</param>
    /// <param name="pOptions">
    /// A pointer to a transaction options object. This value may be NULL.
    /// If pOptions is NULL the time-out value for the transaction is infinite and the transaction will not have a description.
    /// </param>
    /// <param name="ppTransaction">Pointer to the pointer to the ITransaction interface on the new transaction object.</param>
    void BeginTransaction(
        IntPtr punkOuter,
        [MarshalAs(UnmanagedType.I4)] OletxTransactionIsolationLevel isoLevel,
        [MarshalAs(UnmanagedType.I4)] OletxTransactionIsoFlags isoFlags,
        [MarshalAs(UnmanagedType.Interface)] ITransactionOptions pOptions,
        [MarshalAs(UnmanagedType.Interface)] out ITransaction ppTransaction);
}
