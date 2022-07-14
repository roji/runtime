// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITransactionReceiverFactory interface is used to create <see cref="ITransactionReceiver" /> objects.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms683577(v=vs.85).
/// </remarks>
[ComImport, Guid("59313E02-B36C-11cf-A539-00AA006887C3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionReceiverFactory
{
    /// <summary>
    /// The Create method creates an <see cref="ITransactionReceiver" /> object.
    /// </summary>
    /// <param name="pTxReceiver">A pointer to the <see cref="ITransactionReceiver" /> object that is created.</param>
    void Create([MarshalAs(UnmanagedType.Interface)] out ITransactionReceiver pTxReceiver);
}
