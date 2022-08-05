// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The interface contains a single method that is used to create a new transaction voter object.
/// The transaction voter object implements the ITransactionVoterBallotAsync2 interface.
/// This interface is exported as a sink to the transaction voter for vetoing distributed transaction commitment.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms686084(v=vs.85).
/// </remarks>
[ComImport, Guid("5433376A-414D-11d3-B206-00C04FC2F3EF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionVoterFactory2
{
    /// <summary>
    /// This method creates the transaction voter object and exchanges the <see cref="ITransactionVoterNotifyAsync2" /> and
    /// <see cref="ITransactionVoterBallotAsync2" /> sinks between the transaction voter and the DTC proxy.
    /// </summary>
    /// <param name="pITransaction">
    /// Pointer to the ITransaction interface associated with the transaction object on which the transaction voter enlists.
    /// </param>
    /// <param name="pVoterNotify">
    /// Pointer to the ITransactionVoterNotifyAsync2 sink exported by the transaction voter.
    /// </param>
    /// <param name="ppVoterBallot">
    /// Pointer to a pointer of an ITransactionVoterBallotAsync2 interface supplied to vote on the outcome of a distributed transaction with which it is associated.
    /// </param>
    void Create(
        [MarshalAs(UnmanagedType.Interface)] ITransaction pITransaction,
        [MarshalAs(UnmanagedType.Interface)] ITransactionVoterNotifyAsync2 pVoterNotify,
        [MarshalAs(UnmanagedType.Interface)] out ITransactionVoterBallotAsync2 ppVoterBallot);
}
