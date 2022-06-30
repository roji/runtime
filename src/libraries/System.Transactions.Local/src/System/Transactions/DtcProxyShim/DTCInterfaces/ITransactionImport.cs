// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The resource manager uses this interface to transform an imported transaction cookie into a transaction object.
/// The resource manager calls this interface and passes an opaque marshaled form of the transaction object called a transaction cookie.
/// The resource manager is returned a transaction object. This is referred to as importing the transaction. After importing the transaction,
/// the resource manager uses the IResourceManager::Enlist method to enlist in the transaction.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms681296(v=vs.85).
/// </remarks>
[ComImport, Guid("E1CF9B5A-8745-11ce-A9BA-00AA006C3706"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionImport
{
    /// <summary>
    /// This method transforms a transaction cookie into a transaction object.
    /// </summary>
    /// <param name="cbTransactionCookie">The size of the transaction cookie.</param>
    /// <param name="rgbTransactionCookie">The transaction cookie.</param>
    /// <param name="piid">The interface ID desired on the resulting transaction object.</param>
    /// <param name="ppvTransaction">Reference to the interface on the imported transaction object, requested by the <paramref name="piid" /> parameter.</param>
    void Import(
        ulong cbTransactionCookie,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] rgbTransactionCookie,
        Guid piid,
        [MarshalAs(UnmanagedType.Interface)] out object ppvTransaction);
}
