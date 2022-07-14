// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// ITransactionImportWhereabouts interface is used when propagating transactions from one process to another or one system
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms682783(v=vs.85).
/// </remarks>
[ComImport, Guid("0141fda4-8fc0-11ce-bd18-204c4f4f5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionImportWhereabouts
{
    /// <summary>
    /// This method returns the size in bytes of the whereabouts, (location) of the local transaction manager.
    /// </summary>
    /// <param name="pcbSize">Pointer to the size, in bytes, of the location.</param>
    internal void GetWhereaboutsSize(out uint pcbSize);

    /// <summary>
    /// This method returns the whereabouts, (location), of the local transaction manager.
    /// </summary>
    /// <param name="cbWhereabouts">The size in bytes of the <paramref name="rgbWhereabouts" /> buffer.</param>
    /// <param name="rgbWhereabouts">Pointer to the caller allocated buffer in which the location is returned.</param>
    /// <param name="pcbUsed">Pointer to the size in bytes of the address returned in <paramref name="rgbWhereabouts"/>.</param>
    internal void GetWhereabouts(
        uint cbWhereabouts,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0), Out] byte[] rgbWhereabouts,
        out uint pcbUsed);
}
