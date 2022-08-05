// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// A process that wishes to export a transaction calls this interface and passes a transaction object.
/// The process is returned an opaque marshaled form of the transaction object called a transaction cookie.
/// The process then sends the transaction cookie to the destination process. This is referred to as exporting the transaction.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms678954(v=vs.85).
/// </remarks>
[ComImport, Guid("0141fda5-8fc0-11ce-bd18-204c4f4f5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionExport
{
    /// <summary>
    /// This method is used to marshal a transaction object for export.
    /// If the DTC Network Node and Transaction Internet Protocol (TIP) Transactions settings are enabled and the Network Transactions setting is disabled, TIP is the protocol that is used.
    /// </summary>
    /// <param name="punkTransaction">Pointer to the IUnknown interface on the transaction object to be exported.</param>
    /// <param name="pcbTransactionCookie">Pointer to the size in bytes of the transaction cookie.</param>
    void Export([MarshalAs(UnmanagedType.Interface)] ITransaction punkTransaction, out uint pcbTransactionCookie);

    /// <summary>
    /// This method transforms a transaction object into a transaction cookie.
    /// If the DTC Network Node and the Transaction Internet Protocol(TIP) Transactions settings are enabled, and the Network Transactions setting is disabled, TIP is the protocol that is used.
    /// </summary>
    /// <param name="pITransaction">
    /// Pointer to the IUnknown interface on the transaction object to be marshaled. The <see cref="Export" /> must have been called previously.
    /// </param>
    /// <param name="cbTransactionCookie">The size in bytes of the transaction cookie representing the transaction object.</param>
    /// <param name="rgbTransactionCookie">Pointer to the caller allocated buffer that will receive the transaction cookie.</param>
    /// <param name="pcbUsed">Pointer to the size in bytes of the address returned in <paramref name="rgbTransactionCookie" />.</param>
    void GetTransactionCookie(
        [MarshalAs(UnmanagedType.Interface)] ITransaction pITransaction,
        uint cbTransactionCookie,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] rgbTransactionCookie,
        out uint pcbUsed);
}
