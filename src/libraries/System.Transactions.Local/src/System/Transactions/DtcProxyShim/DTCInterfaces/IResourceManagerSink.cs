// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// This is a callback interface implemented by the resource manager.
/// The interface is passed as a parameter to IResourceManagerFactory::Create and registered in the resource manager object.
/// The DTC proxy calls the resource manager's IResourceManagerSink:TMDown callback method if the transaction manager fails.
/// This informs the resource manager that it must perform recovery.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686073(v=vs.85).
/// </remarks>
[ComImport, Guid("0D563181-DEFB-11CE-AED1-00AA0051E2C4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IResourceManagerSink
{
    void TMDown();
}
