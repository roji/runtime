// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

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
    /// The DTC proxy calls the VoteRequest method to notify a transaction voter that phase one of the two-phase commit protocol has started.
    /// </summary>
    void VoteRequest();
}
