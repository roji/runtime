// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Transactions.DtcProxyShim.DTCInterfaces;
using System.Transactions.Oletx;
using Microsoft.Win32.SafeHandles;

namespace System.Transactions.DtcProxyShim;

internal class NotificationShimFactory : IDtcProxyShimFactory
{
    // Used to synchronize access to the proxy.  This is necessary in
    // initialization because the proxy doesn't like multiple simultaneous callers
    // of GetWhereabouts[Size].  We could have this situation in cases where
    // there are multiple app domains being ititialized in the same process
    // at the same time.
    private static volatile object s_pcsxProxyInit = new();

    // Adding retry logic as a work around for MSDTC's GetWhereAbouts/GetWhereAboutsSize API
    // which is single threaded and will return XACT_E_ALREADYINPROGRESS if another thread invokes the API.
    private const int RetryInterval = 50;  // in milli seconds.
    private const int MaxRetryCount = 100;

    // Lock to protect access to listOfNotifications.
    private object _csx = new();

    // The handle returned by LoadLibraryEx of xolehlp.dll and the fptr to DtcGetTransactionManagerEx.
    //private HMODULE xoleHlpHandle;
    //private LPDtcGetTransactionManagerExW? pfDtcGetTransactionManagerExW;

    internal NotificationShimFactory(SafeWaitHandle notificationEventHandle)
    {
        Initialize(notificationEventHandle);
    }

    private void Initialize(SafeWaitHandle hEvent)
    {
        // TODO: DuplicateHandle into this->eventHandle
        // TODO: Instantiate all the locks (critical sections)
    }

    public void ConnectToProxy(
        string nodeName,
        Guid resourceManagerIdentifier,
        OletxInternalResourceManager managedIdentifier,
        out bool nodeNameMatches,
        out byte[] whereabouts,
        out IResourceManagerShim resourceManagerShim)
    {
        lock (s_pcsxProxyInit)
        {
            NativeMethods.DtcGetTransactionManagerExW(
                nodeName,
                null,
                Guid.Parse(Guids.IID_ITransactionDispenser),
                0,
                null,
                out var localDispenser);

            // Check to make sure the node name matches.
            if (nodeName is not null)
            {
                // TODO: Not ported yet
                throw new NotImplementedException();
            }
            else
                nodeNameMatches = true;

            var pImportWhereabouts = (ITransactionImportWhereabouts)localDispenser;

            // Adding retry logic as a work around for MSDTC's GetWhereAbouts/GetWhereAboutsSize API
            // which is single threaded and will return XACT_E_ALREADYINPROGRESS if another thread invokes the API.
            ulong whereaboutsSize = 0;
            Retry(() => pImportWhereabouts.GetWhereaboutsSize(out whereaboutsSize));

            // TODO: GetWhereaboutsSize returns ulong
            var tmpWhereabouts = new byte[(int)whereaboutsSize];

            // Adding retry logic as a work around for MSDTC's GetWhereAbouts/GetWhereAboutsSize API
            // which is single threaded and will return XACT_E_ALREADYINPROGRESS if another thread invokes the API.
            Retry(() => pImportWhereabouts.GetWhereabouts(whereaboutsSize, tmpWhereabouts, out var pcbUsed));
            whereabouts = tmpWhereabouts;

            ///////// GOOD

            // Now we need to create the internal resource manager.
            var rmFactory = (IResourceManagerFactory2)localDispenser;
            //var rmFactory = (IResourceManagerFactory)localDispenser;

            var rmNotifyShim = new ResourceManagerNotifyShim(this, managedIdentifier);
            //var myNotifyShimRef = Marshal.GetComInterfaceForObject(rmNotifyShim, typeof(IResourceManagerSink));

            var rmShim = new ResourceManagerShim(this, rmNotifyShim);

            //     hr = rmShim->Initialize();
            //     if ( FAILED( hr ) )
            //     {
            //         goto ErrorExit;
            //     }
            //

            Retry(() =>
            {
                // TODO: The C++ code uses IResourceManagerFactory2.CreateEx to create the resource manager; the only difference between that and IResourceManagerFactory.CreateEx is that the latter doesn't
                // accept an riid, and my attempts to pass IID_IResourceManager to it have failed (some sort of GUID mismatch??)

                rmFactory.CreateEx(
                    resourceManagerIdentifier,
                    "System.Transactions.InternalRM",
                    rmNotifyShim,
                    Guid.Parse(Guids.IID_IResourceManager),
                    out var rm);

                //rmFactory.Create(
                //    resourceManagerIdentifier,
                //    "System.Transactions.InternalRM",
                //    rmNotifyShim,
                //    out var rm);

                //rm.ReenlistmentComplete();

                rm.GetDistributedTransactionManager(
                    Guid.Parse(Guids.IID_ITransactionDispenser),
                    out var foo);

                rmShim.ResourceManager = rm;
            });

            resourceManagerShim = rmShim;
        }

        // Adding retry logic as a work around for MSDTC's GetWhereAbouts/GetWhereAboutsSize API
        // which is single threaded and will return XACT_E_ALREADYINPROGRESS if another thread invokes the API.
        // Resource Manager Factory CreateEx under the covers calls GetWhereAbouts API.
        static void Retry(Action action)
        {
            var nRetries = MaxRetryCount;

            while (nRetries > 0)
            {
                try
                {
                    action();
                    return;
                }
                catch (COMException e) when (e.ErrorCode == NativeMethods.XACT_E_ALREADYINPROGRESS)
                {
                    Thread.Sleep(RetryInterval);
                    nRetries--;
                }
            }
        }
    }

    internal void NewNotification(NotificationShimBase notification)
    {
        // assert( ! notification->link.IsLinked() );
        lock (_csx)
        {
            // notification->BaseAddRef();
            // this->listOfNotifications.InsertLast(&notification->link);
        }

        // SetEvent(this->eventHandle);
    }

    public void GetNotification(out IntPtr managedIdentifier, out ShimNotificationType shimNotificationType,
        out bool isSinglePhase, out bool abortingHint, out bool releaseRequired, out uint prepareInfoSize,
        out CoTaskMemHandle prepareInfo) =>
        throw new NotImplementedException();

    public void ReleaseNotificationLock() => throw new NotImplementedException();

    public void BeginTransaction(uint timeout, OletxTransactionIsolationLevel isolationLevel, IntPtr managedIdentifier,
        out Guid transactionIdentifier, out ITransactionShim transactionShim) =>
        throw new NotImplementedException();

    public void CreateResourceManager(Guid resourceManagerIdentifier, IntPtr managedIdentifier,
        out IResourceManagerShim resourceManagerShim) =>
        throw new NotImplementedException();

    public void Import(uint cookieSize, byte[] cookie, IntPtr managedIdentifier, out Guid transactionIdentifier,
        out OletxTransactionIsolationLevel isolationLevel, out ITransactionShim transactionShim) =>
        throw new NotImplementedException();

    public void ReceiveTransaction(uint propagationTokenSize, byte[] propgationToken, IntPtr managedIdentifier,
        out Guid transactionIdentifier, out OletxTransactionIsolationLevel isolationLevel,
        out ITransactionShim transactionShim) =>
        throw new NotImplementedException();

    public void CreateTransactionShim(IDtcTransaction transactionNative, IntPtr managedIdentifier, out Guid transactionIdentifier,
        out OletxTransactionIsolationLevel isolationLevel, out ITransactionShim transactionShim) =>
        throw new NotImplementedException();

    public void GetNotification(out IntPtr managedIdentifier, [MarshalAs(UnmanagedType.I4)] out Oletx.ShimNotificationType shimNotificationType, [MarshalAs(UnmanagedType.Bool)] out bool isSinglePhase, [MarshalAs(UnmanagedType.Bool)] out bool abortingHint, [MarshalAs(UnmanagedType.Bool)] out bool releaseRequired, [MarshalAs(UnmanagedType.U4)] out uint prepareInfoSize, out CoTaskMemHandle prepareInfo) => throw new NotImplementedException();
}
