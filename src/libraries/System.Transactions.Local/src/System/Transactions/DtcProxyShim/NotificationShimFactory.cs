// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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
    private static volatile object _proxyInitLock = new();

    // Lock to protect access to listOfNotifications.
    private object _csx = new();

    // This is the list of queued NotificationShimBase objects.
    //UTStaticList<NotificationShimBase*> listOfNotifications;

    // This is the list of cached ITransactionOptions interfaces.
    private List<CachedInterfaceBase> _listOfOptions = new();

    // This is the list of cached ITransactionTransmitter interfaces.
    // Lock to protect access to listOfTransmitters.
    private object _transmitterLock = new();
    private List<CachedInterfaceBase> _listOfTransmitters = new();

    // This is the list of cached ITransactionReceiver interfaces.
    // Lock to protect access to listOfReceivers.
    private object _receiverLock = new();
    private List<CachedInterfaceBase> _listOfReceivers = new();

    private ITransactionDispenser _transactionDispenser = null!; // Late-initialized in ConnectToProxy

    internal NotificationShimFactory(SafeWaitHandle notificationEventHandle)
    {
        Initialize(notificationEventHandle);
    }

    private void Initialize(SafeWaitHandle hEvent)
    {
        // TODO: DuplicateHandle into this->eventHandle
        // TODO: Instantiate all the locks (critical sections)
    }

    [UnconditionalSuppressMessage("Trimming", "IL2050", Justification = "Leave me alone")]
    public void ConnectToProxy(
        string? nodeName,
        Guid resourceManagerIdentifier,
        object managedIdentifier,
        out bool nodeNameMatches,
        out byte[] whereabouts,
        out IResourceManagerShim resourceManagerShim)
    {
        lock (_proxyInitLock)
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
            NativeMethods.Retry(() => pImportWhereabouts.GetWhereaboutsSize(out whereaboutsSize));

            // TODO: GetWhereaboutsSize returns ulong
            var tmpWhereabouts = new byte[(int)whereaboutsSize];

            // Adding retry logic as a work around for MSDTC's GetWhereAbouts/GetWhereAboutsSize API
            // which is single threaded and will return XACT_E_ALREADYINPROGRESS if another thread invokes the API.
            NativeMethods.Retry(() => pImportWhereabouts.GetWhereabouts(whereaboutsSize, tmpWhereabouts, out var pcbUsed));
            whereabouts = tmpWhereabouts;

            // Now we need to create the internal resource manager.
            var rmFactory = (IResourceManagerFactory2)localDispenser;

            var rmNotifyShim = new ResourceManagerNotifyShim(this, managedIdentifier);
            var rmShim = new ResourceManagerShim(this, rmNotifyShim);

            //     hr = rmShim->Initialize();

            NativeMethods.Retry(() =>
            {
                rmFactory.CreateEx(
                    resourceManagerIdentifier,
                    "System.Transactions.InternalRM",
                    rmNotifyShim,
                    Guid.Parse(Guids.IID_IResourceManager),
                    out var rm);

                rmShim.ResourceManager = (IResourceManager)rm;
            });

            resourceManagerShim = rmShim;
            _transactionDispenser = localDispenser;
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

    public void BeginTransaction(
        uint timeout,
        OletxTransactionIsolationLevel isolationLevel,
        object? managedIdentifier,
        out Guid transactionIdentifier,
        out ITransactionShim transactionShim)
    {
        var pCachedOptions = GetCachedOptions();

        var xactopt = new Xactopt(timeout, string.Empty);
        pCachedOptions.PTxOptions.SetOptions(xactopt);

        _transactionDispenser.BeginTransaction(IntPtr.Zero, isolationLevel, 0, pCachedOptions.PTxOptions, out var pTx);

        SetupTransaction(pTx, managedIdentifier, out transactionIdentifier, out var localIsoLevel, out transactionShim);
    }

    public void CreateResourceManager(
        Guid resourceManagerIdentifier,
        OletxResourceManager managedIdentifier,
        out IResourceManagerShim resourceManagerShim)
    {
        var rmFactory = (IResourceManagerFactory2)_transactionDispenser;

        var rmNotifyShim = new ResourceManagerNotifyShim(this, managedIdentifier);
        var rmShim = new ResourceManagerShim(this, rmNotifyShim);

        //     hr = rmShim->Initialize();

        NativeMethods.Retry(() =>
        {
            rmFactory.CreateEx(
                resourceManagerIdentifier,
                "System.Transactions.ResourceManager",
                rmNotifyShim,
                Guid.Parse(Guids.IID_IResourceManager),
                out var rm);

            rmShim.ResourceManager = (IResourceManager)rm;
        });

        resourceManagerShim = rmShim;
    }

    public void Import(uint cookieSize, byte[] cookie, IntPtr managedIdentifier, out Guid transactionIdentifier,
        out OletxTransactionIsolationLevel isolationLevel, out ITransactionShim transactionShim) =>
        throw new NotImplementedException();

    public void ReceiveTransaction(
        byte[] propagationToken,
        OutcomeEnlistment managedIdentifier,
        out Guid transactionIdentifier,
        out OletxTransactionIsolationLevel isolationLevel,
        out ITransactionShim transactionShim)
    {
        var cachedReceiver = GetCachedReceiver();

        cachedReceiver.TxReceiver.UnmarshalPropagationToken(
            Convert.ToUInt32(propagationToken.Length),
            propagationToken,
            out var tx);

        SetupTransaction(tx, managedIdentifier, out transactionIdentifier, out isolationLevel, out transactionShim);
    }

    public void CreateTransactionShim(
        IDtcTransaction transactionNative,
        IntPtr managedIdentifier,
        out Guid transactionIdentifier,
        out OletxTransactionIsolationLevel isolationLevel,
        out ITransactionShim transactionShim)
        => throw new NotImplementedException();

    internal ITransactionExportFactory ExportFactory
        => (ITransactionExportFactory)_transactionDispenser;

    public void GetNotification(
        out IntPtr managedIdentifier,
        out Oletx.ShimNotificationType shimNotificationType,
        out bool isSinglePhase,
        out bool abortingHint,
        out bool releaseRequired,
        out uint prepareInfoSize,
        out CoTaskMemHandle prepareInfo)
        => throw new NotImplementedException();

    private void SetupTransaction(
        ITransaction pTx,
        object? managedIdentifier,
        out Guid pTransactionIdentifier,
        out OletxTransactionIsolationLevel pIsolationLevel,
        out ITransactionShim ppTransactionShim)
    {
        var transactionNotifyShim = new TransactionNotifyShim(this, managedIdentifier);
        var transactionShim = new TransactionShim(this, transactionNotifyShim);
        //hr = transactionShim->Initialize();

        // Get the transaction id.
        pTx.GetTransactionInfo(out var xactInfo);

        // Register for outcome events.
        var pContainer = (IConnectionPointContainer)pTx;
        var guid = Guid.Parse(Guids.IID_ITransactionOutcomeEvents);
        pContainer.FindConnectionPoint(ref guid, out var pConnPoint);
        pConnPoint!.Advise(transactionNotifyShim, out var connPointCookie);

        transactionShim.Transaction = pTx;
        pTransactionIdentifier = xactInfo.uow;
        pIsolationLevel = xactInfo.isoLevel;
        ppTransactionShim = transactionShim;
    }

    private CachedOptions GetCachedOptions()
    {
        lock (_listOfOptions)
        {
            if (_listOfOptions.Count > 0)
            {
                var localCachedOptions = (CachedOptions)_listOfOptions[0];
                _listOfOptions.RemoveAt(0);
                return localCachedOptions;
            }

            // We need to allocate a new one.
            _transactionDispenser.GetOptionsObject(out var pOptions);
            return new(this, pOptions);
        }
    }

    internal CachedTransmitter GetCachedTransmitter(ITransaction transaction)
    {
        CachedTransmitter localCachedTransmitter;

        lock (_transmitterLock)
        {
            if (_listOfTransmitters.Count > 0)
            {
                localCachedTransmitter = (CachedTransmitter)_listOfTransmitters[0];
                _listOfTransmitters.RemoveAt(0);
            }
            else
            {
                var transmitterFactory = (ITransactionTransmitterFactory)_transactionDispenser;
                transmitterFactory.Create(out var transmitter);

                localCachedTransmitter = new CachedTransmitter(this, transmitter);
            }
        }

        return localCachedTransmitter;
    }

    internal CachedReceiver GetCachedReceiver()
    {
        lock (_receiverLock)
        {
            if (_listOfReceivers.Count > 0)
            {
                var localCachedReceiver = (CachedReceiver)_listOfReceivers[0];
                _listOfReceivers.RemoveAt(0);

                return localCachedReceiver;
            }
            else
            {
                var receiverFactory = (ITransactionReceiverFactory)_transactionDispenser;
                receiverFactory.Create(out var receiver);

                return new CachedReceiver(this, receiver);
            }
        }
    }
}
