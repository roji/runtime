// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.System.Transactions.DtcProxyShim
{
    internal static class SafeNativeMethods
    {
        //[DllImport(global::Interop.Libraries.Xolehlp)]
        public static extern void DtcGetTransactionManagerExW(IntPtr pObject);
    }
}
