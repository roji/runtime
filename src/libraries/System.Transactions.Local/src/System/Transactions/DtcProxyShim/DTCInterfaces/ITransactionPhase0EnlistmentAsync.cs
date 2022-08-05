// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// ITransactionPhase0EnlistmentAsync is the interface a Phase0 participant uses to indicate that it is ready to receive phase0 notification,
/// that phase0 processing has completed, or that phase0 notification is no longer desired.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms685087(v=vs.85).</remarks>
[ComImport, Guid("82DC88E1-A954-11d1-8F88-00600895E7D5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionPhase0EnlistmentAsync
{
    /// <summary>
    /// An application invokes the Enable method to indicate that it is prepared to accept Phase0 notifications;
    /// which changes the phase0 enlistment object from the disabled state (in which it was created) to the enabled state.
    /// The phase0 participant must be prepared to receive its phase0 notification prior to the return from the Enable method.
    /// </summary>
    void Enable();

    /// <summary>
    /// An application invokes the WaitForEnlistment method if it wishes to block until the phase0 enlistment completes.
    /// A return from this method indicates that enlistment has either completed successfully or has failed, as indicated by the status parameter.
    /// Note that this is a synchronous call, in contrast to most of the methods in this interface.
    /// </summary>
    void WaitForEnlistment();

    /// <summary>
    /// <para>
    /// An application invokes the Phase0Done method to indicate that it has completed Phase0 processing.
    /// The implication of making this call is that the Phase0 participant has no more work that would preclude the transaction from beginning the two-phase commit processing
    /// (though other participants may still have such work).
    /// When the last Phase0 participant has called Phase0Done, and there are no new Phase0 enlistments, the transaction can proceed with two-phase commit processing.
    /// </para>
    /// <para>
    /// This method should be called only after receiving a Phase0Request notification.
    /// </para>
    /// </summary>
    void Phase0Done();

    /// <summary>
    /// <para>
    /// An application invokes the Unenlist method to indicate that it is no longer interested in receiving Phase0 notifications.
    /// This call cancels an existing enlistment.
    /// </para>
    /// <para>
    /// The implication of making this call is that the application has no further need to receive phase0 notifications;
    /// however, it is still possible that the application will receive a phase0 notification regardless.
    /// A phase0 participant who receives a phase0 notification after calling <see cref="Unenlist" /> may choose to call
    /// <see cref="Phase0Done" />, but is not required to do so.
    /// </para>
    /// <para>
    /// The only time at which the participant is guaranteed not to receive a phase0 notification is after it
    /// <see cref="ITransactionPhase0NotifyAsync" /> pointer is released by the phase0 enlistment object.
    /// </para>
    /// </summary>
    void Unenlist();

    /// <summary>
    /// An application invokes the GetTransaction method to retrieve a pointer to the transaction that is associated with the Phase0 Enlistment object.
    /// This allows a phase0 participant to perform operations on the associated transaction without being required to maintain a pointer to it.
    /// </summary>
    /// <param name="ppITransaction">
    /// Pointer to an address at which the caller wishes to receive a pointer to the transaction interface that is associated with the Phase0 Enlistment.
    /// </param>
    void GetTransaction([MarshalAs(UnmanagedType.Interface)] out ITransaction ppITransaction);
}
