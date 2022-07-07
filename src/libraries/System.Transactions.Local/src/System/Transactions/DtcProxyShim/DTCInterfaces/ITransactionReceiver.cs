// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The ITransactionReceiver interface is used to unmarshal transaction tokens.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms679193(v=vs.85).
/// </remarks>
[ComImport, Guid("59313E03-B36C-11cf-A539-00AA006887C3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionReceiver
{
    /// <summary>
    /// The UnmarshalPropagationToken method unmarshals the propagation token.
    /// </summary>
    /// <param name="cbToken">The propagation token to be unmarshaled.</param>
    /// <param name="rgbToken">The marshaled propagation token.</param>
    /// <param name="ppTransaction">The ITransaction interface of the transaction.</param>
    void UnmarshalPropagationToken(
        ulong cbToken,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] rgbToken,
        [MarshalAs(UnmanagedType.Interface)] out ITransaction ppTransaction);

    /// <summary>
    /// The GetReturnTokenSize method obtains the size of the return token.
    /// </summary>
    /// <param name="pcbReturnToken">The size of the return token.</param>
    void GetReturnTokenSize(out ulong pcbReturnToken);

    /// <summary>
    /// The MarshalReturnToken method marshals a return token.
    /// </summary>
    /// <param name="cbReturnToken">The return token to be marshaled.</param>
    /// <param name="rgbReturnToken">The marshaled return token.</param>
    /// <param name="pcbUsed">The token size used.</param>
    void MarshalReturnToken(
        ulong cbReturnToken,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out byte[] rgbReturnToken,
        out ulong pcbUsed);

    /// <summary>
    /// The Reset method resets the transaction receiver.
    /// </summary>
    void Reset();
}
