// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Transactions.Oletx;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// The ITransactionVoterNotifyAsync2 interface notifies a transaction voter of a vote request during the first phase of the two-phase commit protocol.
/// The transaction voter returns a vote to either approve a commit outcome or abort the transaction through the <see cref="ITransactionVoterBallotAsync2.VoteRequestDone" /> method.
/// The ITransactionVoterNotifyAsync2 interface also notifies the voter of commit, abort, heuristic, and in-doubt transaction outcomes during phase two
/// through the inherited <see cref="ITransactionOutcomeEvents" /> interface.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms678930(v=vs.85).
/// </remarks>
[ComImport, Guid("5433376B-414D-11d3-B206-00C04FC2F3EF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionVoterNotifyAsync2
{
    /// <summary>
    /// This event is raised when the transaction committed.
    /// </summary>
    /// <param name="fRetaining">Indicates whether retaining Commit was specified. Will be false.</param>
    /// <param name="pNewUOW">Always null.</param>
    /// <param name="hresult">Always S_OK.</param>
    void Committed([MarshalAs(UnmanagedType.Bool)] bool fRetaining, Guid pNewUOW /* always null? */, uint hresult);

    /// <summary>
    /// This event is raised when the transaction aborted, either as a result of a call to Abort or an unsuccessful call to Commit*.*.
    /// </summary>
    /// <param name="pboidReason">A BOID indicating why the transaction aborted.</param>
    /// <param name="fRetaining">Indicates whether retaining Commit was specified. Will be false.</param>
    /// <param name="pNewUOW">Always null.</param>
    /// <param name="hresult">Alawys S_OK.</param>
    void Aborted(IntPtr pboidReason, [MarshalAs(UnmanagedType.Bool)] bool fRetaining, Guid pNewUOW, uint hresult);

    /// <summary>
    /// This event is raised when one of the participants in the transaction chooses to heuristically decide the outcome of the transaction.
    /// </summary>
    /// <param name="dwDecision">Values from the enumeration <see cref="OletxTransactionHeuristic" />.</param>
    /// <param name="pboidReason">A BOID indicating why the transaction was heuristically decided. This value is provided by the party making the heuristic decision.</param>
    /// <param name="hresult">Always S_OK.</param>
    void HeuristicDecision([MarshalAs(UnmanagedType.U4)] OletxTransactionHeuristic dwDecision, IntPtr pboidReason, uint hresult);

    /// <summary>
    /// This event is raised when the outcome of the transaction is in-doubt.
    /// The outcome of the transaction can be in-doubt if the connection between the MSDTC proxy and the MSDTC TM was broken after the proxy asked the transaction manager to
    /// commit or abort a transaction but before the transaction manager's response to the commit or abort was received by the proxy.
    /// Note: Receiving this method call is not the same as the state of the transaction being in-doubt.
    /// </summary>
    void Indoubt();

    /// <summary>
    /// The DTC proxy calls the VoteRequest method to notify a transaction voter that phase one of the two-phase commit protocol has started.
    /// </summary>
    void VoteRequest();
}
