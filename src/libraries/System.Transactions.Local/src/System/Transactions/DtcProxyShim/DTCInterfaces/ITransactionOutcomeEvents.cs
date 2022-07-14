// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// This interface is used by application programs that require asynchronous notification about transaction outcomes.
/// The application program implements the methods in this interface and registers the interface with the connection point mechanism.
/// DTC calls the appropriate method on this interface to inform the application about the outcome of a transaction.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms686465(v=vs.85).
/// </remarks>
[ComImport, Guid("3A6AD9E2-23B9-11cf-AD60-00AA00A74CCD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionOutcomeEvents
{
    /// <summary>
    /// This event is raised when the transaction committed.
    /// </summary>
    /// <param name="fRetaining">Indicates whether retaining Commit was specified. Will be false.</param>
    /// <param name="pNewUOW">Always null.</param>
    /// <param name="hresult">Always S_OK.</param>
    void Committed([MarshalAs(UnmanagedType.Bool)] bool fRetaining, IntPtr pNewUOW /* always null? */, int hresult);

    /// <summary>
    /// This event is raised when the transaction aborted, either as a result of a call to Abort or an unsuccessful call to Commit*.*.
    /// </summary>
    /// <param name="pboidReason">A BOID indicating why the transaction aborted.</param>
    /// <param name="fRetaining">Indicates whether retaining Commit was specified. Will be false.</param>
    /// <param name="pNewUOW">Always null.</param>
    /// <param name="hresult">Alawys S_OK.</param>
    void Aborted(IntPtr pboidReason, [MarshalAs(UnmanagedType.Bool)] bool fRetaining, IntPtr pNewUOW, int hresult);

    /// <summary>
    /// This event is raised when one of the participants in the transaction chooses to heuristically decide the outcome of the transaction.
    /// </summary>
    /// <param name="dwDecision">Values from the enumeration <see cref="OletxTransactionHeuristic" />.</param>
    /// <param name="pboidReason">A BOID indicating why the transaction was heuristically decided. This value is provided by the party making the heuristic decision.</param>
    /// <param name="hresult">Always S_OK.</param>
    void HeuristicDecision([MarshalAs(UnmanagedType.U4)] OletxTransactionHeuristic dwDecision, IntPtr pboidReason, int hresult);

    /// <summary>
    /// This event is raised when the outcome of the transaction is in-doubt.
    /// The outcome of the transaction can be in-doubt if the connection between the MSDTC proxy and the MSDTC TM was broken after the proxy asked the transaction manager to
    /// commit or abort a transaction but before the transaction manager's response to the commit or abort was received by the proxy.
    /// Note: Receiving this method call is not the same as the state of the transaction being in-doubt.
    /// </summary>
    void Indoubt();
}
