// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITransactionTransmitter interface is used to marshal transaction tokens.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms682296(v=vs.85).
/// </remarks>
[ComImport, Guid("59313E01-B36C-11cf-A539-00AA006887C3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionTransmitter
{
    /// <summary>
    /// The Set method initializes the transaction transmitter.
    /// </summary>
    /// <param name="transaction">
    /// An object supporting the <see cref="ITransaction " /> interface that describes the transaction to be propagated.
    /// </param>
    void Set([MarshalAs(UnmanagedType.Interface)] ITransaction transaction);

    /// <summary>
    /// The GetPropagationTokenSize method obtains the size of the propagation token.
    /// </summary>
    /// <param name="pcbToken">The size of the propagation token.</param>
    void GetPropagationTokenSize(out uint pcbToken);

    /// <summary>
    /// The MarshalPropagationToken method marshals a propagation token.
    /// </summary>
    /// <param name="cbToken">The token to be marshaled.</param>
    /// <param name="rgbToken">The marshaled token.</param>
    /// <param name="pcbUsed">The token size used.</param>
    void MarshalPropagationToken(
        uint cbToken,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0), Out] byte[] rgbToken,
        out uint pcbUsed);

    /// <summary>
    /// The UnmarshalReturnToken method unmarshals the return token.
    /// </summary>
    /// <param name="cbReturnToken">The return token to be unmarshaled.</param>
    /// <param name="rgbToken">The marshaled return token.</param>
    void UnmarshalReturnToken(
        uint cbReturnToken,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] rgbToken);

    /// <summary>
    /// The Reset method resets the transaction transmitter.
    /// </summary>
    void Reset();
}
