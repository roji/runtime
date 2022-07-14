// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The ITransactionCloner interface is used to make clones of transaction objects such that
/// <see cref="ITransaction.Commit" /> on the cloned transaction object will always fail with XACT_E_COMMITPREVENTED.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms684377(v=vs.85).
/// </remarks>
[ComImport, Guid("02656950-2152-11d0-944C-00A0C905416E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionCloner
{
    void CloneWithCommitDisabled([MarshalAs(UnmanagedType.Interface)] out ITransaction ppITransaction);
}
