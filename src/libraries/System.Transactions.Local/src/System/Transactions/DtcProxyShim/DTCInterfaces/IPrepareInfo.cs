// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The IPrepareInfo interface is superseded by the IPrepareInfo2 interface.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686533(v=vs.85).
/// </remarks>
[ComImport, Guid("80c7bfd0-87ee-11ce-8081-0080c758527e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPrepareInfo
{
    /// <summary>
    /// The GetPrepareInfoSize method is called by the resource manager to determine the size in bytes of the prepare information that needs to be logged.
    /// </summary>
    /// <param name="pcbPrepInfo">
    /// A pointer to the size in bytes of the prepare information. The actual prepare information is available by a call to <see cref="GetPrepareInfo" />.
    /// </param>
    void GetPrepareInfoSize(out uint pcbPrepInfo);

    /// <summary>
    /// The GetPrepareInfo method is called by the resource manager to get the transaction prepare information.
    /// </summary>
    /// <param name="pPrepInfo">
    /// Pointer to the caller allocated buffer to receive the prepare information. The size of pPrepInfo is determined by calling <see cref="GetPrepareInfoSize" />.
    /// </param>
    void GetPrepareInfo([MarshalAs(UnmanagedType.LPArray), Out] byte[] pPrepInfo);
}
