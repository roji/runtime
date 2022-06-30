// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.DtcProxyShim;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// This interface contains two methods.
/// The BeginTransaction method creates new transaction objects.
/// The GetOptionsObject method creates new transaction options objects.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms679525(v=vs.85).
/// </remarks>
[ComImport, Guid(Guids.IID_ITransactionDispenser), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionDispenser
{
    //HRESULT GetOptionsObject(ITransactionOptions** ppOptions);

    //HRESULT BeginTransaction(IUnknown* punkOuter, ISOLEVEL isoLevel, ULONG isoFlags, ITransactionOptions* pOptions, ITransaction** ppTransaction);
}
