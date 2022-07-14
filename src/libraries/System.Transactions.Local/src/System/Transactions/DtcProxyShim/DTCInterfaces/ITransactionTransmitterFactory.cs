// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITransactionTransmitterFactory interface is used to create <see cref="ITransactionTransmitter" /> objects.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms679232(v=vs.85).
/// </remarks>
[ComImport, Guid("59313E00-B36C-11cf-A539-00AA006887C3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionTransmitterFactory
{
    /// <summary>
    /// The Create method creates an <see cref="ITransactionTransmitter" /> object.
    /// </summary>
    /// <param name="pTxTransmitter">A pointer to the ITransactionTransmitter object that is created.</param>
    void Create([MarshalAs(UnmanagedType.Interface)] out ITransactionTransmitter pTxTransmitter);
}
