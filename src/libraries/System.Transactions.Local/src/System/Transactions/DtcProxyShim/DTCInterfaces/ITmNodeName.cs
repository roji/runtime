// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITmNodeName interface is used to get the node name that specifies the location of the transaction manager
/// used by the Distributed Transaction Coordinator (DTC) proxy.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms687122(v=vs.85).
/// </remarks>
[ComImport, Guid("30274F88-6EE4-474e-9B95-7807BC9EF8CF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITmNodeName
{
    /// <summary>
    /// Gets the length, in bytes, of the node name that will be returned by GetNodeName.
    /// </summary>
    /// <param name="pcbNodeNameSize">
    /// The length, in bytes, of the node name that will be returned by GetNodeName.
    /// </param>
    internal void GetNodeNameSize(out uint pcbNodeNameSize);

    /// <summary>
    /// Gets the node name that specifies the location of the transaction manager used by the DTC proxy.
    /// </summary>
    internal void GetNodeName(uint cbNodeNameBufferSize, [MarshalAs(UnmanagedType.LPWStr)] out string pcbNodeSize);
}
