// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The IResourceManagerFactory2 interface contains a single method that is used to create a new resource manager object.
/// The resource manager object represents the active connection between the resource manager and the transaction manager.
/// Resource managers use this interface to register themselves with the transaction manager.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686489(v=vs.85).
/// </remarks>
[ComImport, Guid("6B369C21-FBD2-11d1-8F47-00C04F8EE57D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IResourceManagerFactory2 : IResourceManagerFactory
{
    [PreserveSig]
    public new int Create(
        in Guid pguidRM,
        [MarshalAs(UnmanagedType.LPStr)] string pszRMName,
        [MarshalAs(UnmanagedType.Interface)] IResourceManagerSink pIResMgrSink,
        [MarshalAs(UnmanagedType.Interface)] out IResourceManager rm);

    /// <summary>
    /// The CreateEx method is used to create a resource manager object.
    /// </summary>
    /// <param name="pguidRM">A UUID that uniquely identifies this resource manager.</param>
    /// <param name="pszRMName">A string name that identifies this resource manager.</param>
    /// <param name="pIResMgrSink">
    /// Pointer to the resource manager's <see cref="IResourceManagerSink" />interface.
    /// The resource manager developer must implement the <see cref="IResourceManagerSink.TMDown" /> method on this interface.
    /// </param>
    /// <param name="riidRequested">The interface ID requested by the resource manager.</param>
    /// <param name="rm">
    /// Reference to the interface on the resource manager object whose IID is specified in the <paramref name="riidRequested" /> parameter.
    /// </param>
    public void CreateEx(
        in Guid pguidRM,
        [MarshalAs(UnmanagedType.LPStr)] string pszRMName,
        [MarshalAs(UnmanagedType.Interface)] IResourceManagerSink pIResMgrSink,
        in Guid riidRequested,
        [MarshalAs(UnmanagedType.Interface)] out object rm);
}
