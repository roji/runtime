// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// This interface is used to create a new export object.
/// The export object represents the connection between a process that exports transactions and a process that imports transactions.
/// The export object is used when propagating transactions between the systems.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms686771(v=vs.85).
/// </remarks>
[ComImport, Guid("E1CF9B53-8745-11ce-A9BA-00AA006C3706"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionExportFactory
{
    /// <summary>
    /// GetRemoteClassId returns the class identifier of the local transaction manager.
    /// The class identifier can be used to ensure that a transaction manager of the proper type is present on the remote system.
    /// Having compatible transaction managers on the two systems makes it possible to propagate transactions between the systems.
    /// </summary>
    /// <param name="pclsid"></param>
    void GetRemoteClassId(out Guid pclsid);

    /// <summary>
    /// This method creates an export object.
    /// </summary>
    /// <param name="cbWhereabouts">The size in bytes of rgbWhereabouts.</param>
    /// <param name="rgbWhereabouts">Pointer to the whereabouts.</param>
    /// <param name="ppExport">
    /// Pointer to the pointer to the <see cref="ITransactionExport" /> interface on the export object.
    /// The export object represents the connection between the caller of <see cref="Create" /> and the destination process which provided the whereabouts.
    /// The caller uses the export object returned by this method to marshal a transaction object for export to the destination process.
    /// </param>
    void Create(
        uint cbWhereabouts,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] rgbWhereabouts,
        [MarshalAs(UnmanagedType.Interface)] out ITransactionExport ppExport);
}
