// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

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
    ///// <summary>
    ///// The Enlist method enlists a resource manager in a transaction.
    ///// </summary>
    ///// <param name="pTransaction">Pointer to the transaction object in which the resource manager wants to enlist.</param>
    ///// <param name="pRes">
    ///// Pointer to the resource manager's ITransactionResourceAsync interface.
    ///// The resource manager developer must implement the methods on this interface.
    ///// </param>
    ///// <param name="pUOW">Pointer to the transaction identifier GUID.</param>
    ///// <param name="pisoLevel">Pointer to the value of the client specified isolation level.</param>
    ///// <param name="ppEnlist">Reference to the ITransactionEnlistmentAsync interface on the enlistment object.</param>
    //void Enlist(
    //    ITransaction pTransaction,
    //    ITransactionResourceAsync pRes,
    //    out Guid pUOW,
    //    out long pisoLevel,
    //    out ITransactionEnlistmentAsync ppEnlist);

    //HRESULT Reenlist(byte* pPrepInfo, ULONG cbPrepInfo, DWORD lTimeout, XACTSTAT* pXactStat);

    /// <summary>
    /// The resource manager calls the ReenlistmentComplete method after resolving all the in-doubt transactions it knows about.
    /// </summary>
    void ReenlistmentComplete();

    void GetDistributedTransactionManager(
        Guid riid,
        out IntPtr ppvObject);
}
