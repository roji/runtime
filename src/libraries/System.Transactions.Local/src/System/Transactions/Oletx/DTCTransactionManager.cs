// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Transactions.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace System.Transactions.Oletx
{
    internal class DtcTransactionManager
    {
        private string? _nodeName;
        private OletxTransactionManager _oletxTm;
        private IDtcProxyShimFactory _proxyShimFactory;
        private byte[] _whereabouts = null!; // Late-initialized
        private bool _initialized;

        internal DtcTransactionManager(string? nodeName, OletxTransactionManager oletxTm)
        {
            _nodeName = nodeName;
            _oletxTm = oletxTm;
            _initialized = false;
            _proxyShimFactory = OletxTransactionManager.ProxyShimFactory;
        }

        // This is here for the DangerousGetHandle call.  We need to do it.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        [MemberNotNull(nameof(_whereabouts))]
        void Initialize()
        {
            if (_initialized)
            {
                Debug.Assert(_whereabouts is not null);

                return;
            }

            OletxInternalResourceManager internalRM = _oletxTm.InternalResourceManager;
            //IntPtr handle = IntPtr.Zero;
            IResourceManagerShim? resourceManagerShim = null;
            bool nodeNameMatches;
            try
            {
                //handle = HandleTable.AllocHandle(internalRM);

                _proxyShimFactory.ConnectToProxy(
                    _nodeName,
                    internalRM.Identifier,
                    //handle,
                    internalRM,
                    out nodeNameMatches,
                    out _whereabouts,
                    out resourceManagerShim);

                // If the node name does not match, throw.
                if (!nodeNameMatches)
                {
                    throw new NotSupportedException(SR.ProxyCannotSupportMultipleNodeNames);
                }

                // Give the IResourceManagerShim to the internalRM and tell it to call ReenlistComplete.
                internalRM.ResourceManagerShim = resourceManagerShim;
                internalRM.CallReenlistComplete();

                _initialized = true;
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == NativeMethods.XACT_E_NOTSUPPORTED)
                {
                    throw new NotSupportedException( SR.CannotSupportNodeNameSpecification);
                }

                OletxTransactionManager.ProxyException(ex);

                // Unfortunately MSDTCPRX may return unknown error codes when attempting to connect to MSDTC
                // that error should be propagated back as a TransactionManagerCommunicationException.
                throw TransactionManagerCommunicationException.Create(SR.TransactionManagerCommunicationException, ex);
            }
            finally
            {
                // If we weren't successful at initializing ourself, clear things out
                // for next time around.
                if (!_initialized)
                {
                    //if (handle != IntPtr.Zero && resourceManagerShim == null)
                    //{
                    //    HandleTable.FreeHandle(handle);
                    //}

                    if (_whereabouts != null)
                    {
                        _whereabouts = null!;
                    }
                }
            }
        }

        internal IDtcProxyShimFactory ProxyShimFactory
        {
            get
            {
                if (!_initialized)
                {
                    lock (this)
                    {
                        Initialize();
                    }
                }

                return _proxyShimFactory;
            }
        }

        internal void ReleaseProxy()
        {
            lock (this)
            {
                _whereabouts = null!;
                _initialized = false;
            }
        }

        internal byte[] Whereabouts
        {
            get
            {
                if (!_initialized)
                {
                    lock (this)
                    {
                        Initialize();
                    }
                }

                return _whereabouts;
            }
        }

        internal static uint AdjustTimeout(TimeSpan timeout)
        {
            uint returnTimeout = 0;

            try
            {
                returnTimeout = Convert.ToUInt32(timeout.TotalMilliseconds, CultureInfo.CurrentCulture);
            }
                // timeout.TotalMilliseconds might be negative, so let's catch overflow exceptions, just in case.
            catch (OverflowException caughtEx)
            {
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(SR.TraceSourceOletx, caughtEx);
                }

                returnTimeout = uint.MaxValue;
            }
            return returnTimeout;
        }
    }
}
