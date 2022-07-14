// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// This interface contains methods that control the attributes of new transactions such as their time-out periods and descriptions.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686489(v=vs.85).
/// </remarks>
[ComImport, Guid("3A6AD9E0-23B9-11cf-AD60-00AA00A74CCD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionOptions
{
    void SetOptions(Xactopt pOptions);

    void GetOptions();
}
