// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The interface contains a single method that creates a new phase0 enlistment object.
/// The phase0 enlistment object implements the ITransactionPhase0EnlistmentAsync Interface.
/// This interface is exported as a sink to the Phase0 participant; the phase0 participant uses it for controlling the Phase 0 enlistment.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms682238(v=vs.85).
/// </remarks>
[ComImport, Guid("82DC88E0-A954-11d1-8F88-00600895E7D5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionPhase0Factory
{
    /// <summary>
    /// This method creates the Phase0 Enlistment object and exchanges the ITransactionPhase0NotifyAsync and ITransactionPhase0EnlistmentAsync sinks between the Phase0 participant and the DTC proxy
    /// A Phase0 participant invokes this method on the transaction object for any transaction on which it needs notification of phase0 events.
    /// The lifetime of a Phase0 Enlistment object matches that of the transaction with which it is associated, unless it is released sooner.
    /// </summary>
    /// <param name="pITransactionPhase0Notify"></param>
    /// <param name="ppITransactionPhase0Enlistment"></param>
    void Create(
        [MarshalAs(UnmanagedType.Interface)] ITransactionPhase0NotifyAsync pITransactionPhase0Notify,
        [MarshalAs(UnmanagedType.Interface)] out ITransactionPhase0EnlistmentAsync ppITransactionPhase0Enlistment);
}
