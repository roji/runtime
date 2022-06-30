// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The resource manager uses the IResourceManager interface to enlist in distributed transactions.
/// Following a failure, the resource manager uses this interface to determine the outcome of in-doubt transactions.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms681790(v=vs.85).
/// </remarks>
[ComImport, Guid(Guids.IID_IResourceManager), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IResourceManager
{
    /// <summary>
    /// The Enlist method enlists a resource manager in a transaction.
    /// </summary>
    /// <param name="pTransaction">Pointer to the transaction object in which the resource manager wants to enlist.</param>
    /// <param name="pRes">
    /// Pointer to the resource manager's ITransactionResourceAsync interface.
    /// The resource manager developer must implement the methods on this interface.
    /// </param>
    /// <param name="pUOW">Pointer to the transaction identifier GUID.</param>
    /// <param name="pisoLevel">Pointer to the value of the client specified isolation level.</param>
    /// <param name="ppEnlist">Reference to the ITransactionEnlistmentAsync interface on the enlistment object.</param>
    internal void Enlist(
        [MarshalAs(UnmanagedType.Interface)] ITransaction pTransaction,
        [MarshalAs(UnmanagedType.Interface)] ITransactionResourceAsync pRes,
        out Guid pUOW,
        out OletxTransactionIsolationLevel pisoLevel,
        [MarshalAs(UnmanagedType.Interface)] out ITransactionEnlistmentAsync ppEnlist);

    /// <summary>
    /// The Reenlist method re-enlists a resource manager in a transaction.
    /// </summary>
    /// <param name="pPrepInfo">
    /// Binary BLOB containing the prepare information previously obtained from the enlistment object and written into the resource manager's log.
    /// </param>
    /// <param name="cbPrepInfom">Length in bytes of pPrepInfo.</param>
    /// <param name="lTimeout">
    /// How long the resource manager is willing to wait for the outcome of a transaction, in milliseconds.
    /// 0x0, defined as XACTCONST_TIMEOUTINFINITE, is the infinite time-out value.
    /// </param>
    /// <param name="pXactStat">
    /// The status of the transaction, provided by the transaction manager. It is one of the following values: XACTSTAT_ABORTED XACTSTAT_COMMITTED.
    /// </param>
    internal void Reenlist(
        [MarshalAs(UnmanagedType.LPArray)] byte[] pPrepInfo,
        ulong cbPrepInfom,
        int lTimeout,
        out OletxXactStat pXactStat);

    /// <summary>
    /// The resource manager calls the ReenlistmentComplete method after resolving all the in-doubt transactions it knows about.
    /// </summary>
    void ReenlistmentComplete();

    void GetDistributedTransactionManager(
        in Guid riid,
        out IntPtr ppvObject);
}
