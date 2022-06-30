// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// This is a callback interface implemented by the resource manager.
/// The transaction manager invokes this callback interface to deliver phase-one and phase-two notifications to the resource manager.
/// The communication protocol using this interface is asynchronous. When the resource manager receives a callback on this interface, it should queue the request and immediately return from the callback.
/// The resource manager should then asynchronously process the queued request using its own threads to prepare, commit, or abort the transaction.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms678823(v=vs.85).
/// </remarks>
[ComImport, Guid("69E971F0-23CE-11cf-AD60-00AA00A74CCD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionResourceAsync
{
    /// <summary>
    /// The DTC proxy calls this method to prepare a transaction (phase one of the two-phase commit protocol).
    /// </summary>
    /// <param name="fRetaining">Always false.</param>
    /// <param name="grfRM">Values from <see cref="OletxXactRm" />.</param>
    /// <param name="fWantMoniker">Always false.</param> // TODO
    /// <param name="fSinglePhase">If true, it indicates that the RM is the only resource manager enlisted on the transaction.</param>
    void PrepareRequest(bool fRetaining, OletxXactRm grfRM, bool fWantMoniker, bool fSinglePhase);

    /// <summary>
    /// The DTC proxy calls this method to commit a transaction (phase two of the two-phase commit protocol).
    /// </summary>
    /// <param name="grfRM">Values from <see cref="OletxXactRm" />.</param>
    /// <param name="pNewUOW">Always null.</param>
    void CommitRequest(OletxXactRm grfRM, Guid pNewUOW);

    /// <summary>
    /// The DTC proxy calls this method to abort a transaction.
    /// </summary>
    /// <param name="pboidReason">Unspecified and should be ignored.</param>
    /// <param name="fRetaining">Always will be false.</param>
    /// <param name="pNewUOW">Always will be null.</param>
    void AbortRequest(IntPtr pboidReason, bool fRetaining, Guid pNewUOW);

    /// <summary>
    /// The DTC Proxy calls on this method if the connection to the transaction manager goes down and the resource manager's transaction object is prepared
    /// (that is, after the resource manager has called the ITransactionEnlistmentAsync::PrepareRequestDone method).
    /// In general, the proxy calls the ITransactionResourceAsync::TMDown method to inform the resource manager that it must perform recovery on that particular transaction.
    /// </summary>
    void TMDown();
}
