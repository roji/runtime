// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Transactions.DtcProxyShim.DtcInterfaces;

/// <summary>
/// The DTC Proxy calls the methods of this interface to deliver phase zero notification to a Phase0 participant, and to notify the participant that the asynchronous enlistment has completed.
/// </summary>
/// <remarks>
/// See https://docs.microsoft.com/previous-versions/windows/desktop/ms686106(v=vs.85).
/// </remarks>
[ComImport, Guid("EF081809-0C76-11d2-87A6-00C04F990F34"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITransactionPhase0NotifyAsync
{
    /// <summary>
    /// <para>
    /// The DTC Proxy calls this method to notify a Phase0 participant that phase 0 processing has commenced.
    /// The Phase0 participant must perform any operations necessary to ensure the transaction is in a state where it can begin two-phase commit and then signal completion using the ITransactionPhase0Enlistment::Phase0Done method.
    /// </para>
    /// <para>
    /// A phase0 participant that receives a phase0 notification after calling Unenlist may choose to call Phase0Done but is not required to do so.
    /// </para>
    /// </summary>
    /// <param name="fAbortHint">
    /// A value of true provides the phase0 participant with an indication that further work on this transaction is not worth pursuing, since another participant has already aborted the transaction.
    /// </param>
    void Phase0Request([MarshalAs(UnmanagedType.Bool)] bool fAbortHint);

    /// <summary>
    /// The DTC Proxy calls this method to notify a Phase0 participant that the process of enlisting with the TM is complete.
    /// The status parameter indicates whether the phase0 enlistment succeeded or failed.
    /// A Phase0 participant who is not interested in receiving this event should simply return S_OK.
    /// </summary>
    /// <param name="status">Result code indicating the success or failure of the enlistment Create request.</param>
    void EnlistCompleted(int status);
}
