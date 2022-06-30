// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DTCInterfaces;

/// <summary>
/// ITransactionVoterBallotAsync2 is the interface transaction voters use to either approve or veto a transaction during the prepare phase of the two-phase commit protocol.
/// The transaction voter ballot object, in the DTC proxy, implements this interface and creates it on the method call <see cref="ITransactionVoterFactory2.Create" />.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms680565(v=vs.85).
/// </remarks>
[ComImport, Guid("5433376C-414D-11d3-B206-00C04FC2F3EF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionVoterBallotAsync2
{
    /// <summary>
    /// An application server invokes the VoteRequestDone method to submit a vote during the prepare phase of a transaction.
    /// </summary>
    /// <param name="hr">
    /// HRESULT returning a transaction voter's vote. An application sets this value to S_OK or XACT_S_NONOTIFY to approve the transaction.
    /// XACT_S_NONOTIFY specifies that the voter does not want the transaction manager to return with an outcome notification.
    /// E_FAIL specifies that the DTC should abort the transaction.
    /// </param>
    /// <param name="pboidReason">
    /// Pointer to a BOID returning a transaction abort reason code. This value should be NULL if the voter approved the transaction.
    /// Otherwise, the voter can optionally set this value with an abort reason.
    /// </param>
    void VoteRequestDone(int hr, IntPtr pboidReason);
}
