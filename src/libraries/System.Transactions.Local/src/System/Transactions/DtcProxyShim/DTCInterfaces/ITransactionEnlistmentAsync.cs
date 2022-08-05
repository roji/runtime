// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The resource manager uses this interface to notify the transaction manager that it has completed the prepare,
/// abort, or commit request on the transaction object associated with this enlistment object.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms686429(v=vs.85).
/// </remarks>
[ComImport, Guid("0fb15081-af41-11ce-bd2b-204c4f4f5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionEnlistmentAsync
{
    /// <summary>
    /// The resource manager invokes this method to inform the transaction manager that it has completed the prepare phase of the two-phase commit protocol.
    /// </summary>
    /// <param name="hr">
    /// The resource manager uses the hr parameter to inform the transaction manager of the outcome of the prepare phase.
    /// </param>
    /// <param name="pmk">Must be NULL.</param>
    /// <param name="pboidReason">
    /// Pointer to a BOID explaining why the transaction could not be prepared. This parameter must be NULL if hr is S_OK, XACT_S_READONLY, or XACT_S_SINGLEPHASE.
    /// </param>
    void PrepareRequestDone(int hr, IntPtr pmk, IntPtr pboidReason);

    /// <summary>
    /// The resource manager invokes this method to inform the transaction manager that it has successfully committed the transaction.
    /// </summary>
    /// <param name="hr">S_OK indicates that the transaction has committed.</param>
    void CommitRequestDone(int hr);

    /// <summary>
    /// The resource manager invokes this method to inform the transaction manager that it has successfully aborted the transaction.
    /// </summary>
    /// <param name="hr">S_OK indicates that the transaction has been aborted.</param>
    void AbortRequestDone(int hr);
}
