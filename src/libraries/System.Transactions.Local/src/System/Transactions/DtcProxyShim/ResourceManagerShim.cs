// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Transactions.Oletx;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.DtcProxyShim;

#pragma warning disable CS0169
#pragma warning disable CS0414

internal class ResourceManagerShim : IResourceManagerShim
{
    private long _refCount;
    private NotificationShimFactory _shimFactory;
    //private IUnknown* _pMarshaler;
    private IResourceManager? _pResourceManager;
    private ResourceManagerNotifyShim _pResourceManagerNotifyShim;

    internal ResourceManagerShim(NotificationShimFactory shimFactory, ResourceManagerNotifyShim pNotifyShim)
    {
        _shimFactory = shimFactory;
        //_shimFactory->AddRef();
        _pResourceManagerNotifyShim = pNotifyShim;
        //_pResourceManagerNotifyShim->AddRef();
        _refCount = 0;
    }

    public IResourceManager? ResourceManager
    {
        get => _pResourceManager;
        set
        {
            _pResourceManager = value;
            //this->pResourceManager->AddRef();
        }
    }

    public void Enlist(
        [MarshalAs(UnmanagedType.Interface)] ITransactionShim transactionShim,
        IntPtr managedIdentifier,
        [MarshalAs(UnmanagedType.Interface)] out IEnlistmentShim enlistmentShim)
        => throw new NotImplementedException();

    public void Reenlist(
        [MarshalAs(UnmanagedType.U4)] uint prepareInfoSize,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] prepareInfo,
        out OletxTransactionOutcome outcome)
        => throw new NotImplementedException();

    public void ReenlistComplete()
        => _pResourceManager!.ReenlistmentComplete();
}
