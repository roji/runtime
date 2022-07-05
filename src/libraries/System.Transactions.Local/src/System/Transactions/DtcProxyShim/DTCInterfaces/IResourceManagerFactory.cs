// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

[ComImport, Guid("13741d20-87eb-11ce-8081-0080c758527e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IResourceManagerFactory
{
    internal void Create(
        Guid pguidRM,
        [MarshalAs(UnmanagedType.LPStr)] string pszRMName,
        [MarshalAs(UnmanagedType.Interface)] IResourceManagerSink pIResMgrSink,
        [MarshalAs(UnmanagedType.Interface)] out IResourceManager rm);
}
