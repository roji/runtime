// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Transactions.DtcProxyShim.DTCInterfaces;

namespace System.Transactions.Oletx
{
    internal static class NativeMethods
    {
        private const int RetryInterval = 50;  // in milliseconds
        private const int MaxRetryCount = 100;

        // TODO: Is LibraryImport possible here? UnmanagedType.Interface doesn't seem to be supported.
        // https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms678898(v=vs.85)
        [DllImport(Interop.Libraries.Xolehlp, CharSet = CharSet.Unicode)]
        internal static extern void DtcGetTransactionManagerExW(
            [MarshalAs(UnmanagedType.LPWStr)] string? pszHost, // TODO: Is this the right marshaling for tchar*?
            [MarshalAs(UnmanagedType.LPWStr)] string? pszTmName,
            in Guid riid,
            int grfOptions, // TODO: Enum?
            object? pvConfigPararms,
            [MarshalAs(UnmanagedType.Interface)] out ITransactionDispenser ppvObject);

        internal static int S_OK = 0;
        internal static int E_FAIL = -2147467259;  // 0x80004005, -2147467259
        internal static int XACT_S_READONLY = 315394;  // 0x0004D002, 315394
        internal static int XACT_S_SINGLEPHASE = 315401;  // 0x0004D009, 315401
        internal static int XACT_E_ABORTED = -2147168231;  // 0x8004D019, -2147168231
        internal static int XACT_E_NOTRANSACTION = -2147168242;  // 0x8004D00E, -2147168242
        internal static int XACT_E_CONNECTION_DOWN = -2147168228;  // 0x8004D01C, -2147168228
        internal static int XACT_E_REENLISTTIMEOUT = -2147168226;  // 0x8004D01E, -2147168226
        internal static int XACT_E_RECOVERYALREADYDONE = -2147167996;  // 0x8004D104, -2147167996
        internal static int XACT_E_TMNOTAVAILABLE = -2147168229; // 0x8004d01b, -2147168229
        internal static int XACT_E_INDOUBT = -2147168234; // 0x8004d016,
        internal static int XACT_E_ALREADYINPROGRESS = -2147168232; // x08004d018,
        internal static int XACT_E_TOOMANY_ENLISTMENTS = -2147167999; // 0x8004d101
        internal static int XACT_E_PROTOCOL = -2147167995; // 8004d105
        internal static int XACT_E_FIRST = -2147168256; // 0x8004D000
        internal static int XACT_E_LAST = -2147168215; // 0x8004D029
        internal static int XACT_E_NOTSUPPORTED = -2147168241; // 0x8004D00F
        internal static int XACT_E_NETWORK_TX_DISABLED = -2147168220; // 0x8004D024

        internal static void Retry(Action action)
        {
            var nRetries = MaxRetryCount;

            while (nRetries > 0)
            {
                try
                {
                    action();
                    return;
                }
                catch (COMException e) when (e.ErrorCode == XACT_E_ALREADYINPROGRESS)
                {
                    Thread.Sleep(RetryInterval);
                    nRetries--;
                }
            }
        }
    }

    internal enum ShimNotificationType
    {
        None = 0,
        Phase0RequestNotify = 1,
        VoteRequestNotify = 2,
        PrepareRequestNotify = 3,
        CommitRequestNotify = 4,
        AbortRequestNotify = 5,
        CommittedNotify = 6,
        AbortedNotify = 7,
        InDoubtNotify = 8,
        EnlistmentTmDownNotify = 9,
        ResourceManagerTmDownNotify = 10
    }

    internal enum OletxPrepareVoteType
    {
        ReadOnly = 0,
        SinglePhase = 1,
        Prepared = 2,
        Failed = 3,
        InDoubt = 4
    }

    internal enum OletxTransactionOutcome
    {
        NotKnownYet = 0,
        Committed = 1,
        Aborted = 2
    }

    internal enum OletxTransactionIsolationLevel : long
    {
        ISOLATIONLEVEL_UNSPECIFIED = -1,
        ISOLATIONLEVEL_CHAOS = 0x10,
        ISOLATIONLEVEL_READUNCOMMITTED = 0x100,
        ISOLATIONLEVEL_BROWSE = 0x100,
        ISOLATIONLEVEL_CURSORSTABILITY = 0x1000,
        ISOLATIONLEVEL_READCOMMITTED = 0x1000,
        ISOLATIONLEVEL_REPEATABLEREAD = 0x10000,
        ISOLATIONLEVEL_SERIALIZABLE = 0x100000,
        ISOLATIONLEVEL_ISOLATED = 0x100000
    }

    [Flags]
    internal enum OletxTransactionIsoFlags : ulong
    {
        ISOFLAG_NONE = 0,
        ISOFLAG_RETAIN_COMMIT_DC = 1,
        ISOFLAG_RETAIN_COMMIT = 2,
        ISOFLAG_RETAIN_COMMIT_NO = 3,
        ISOFLAG_RETAIN_ABORT_DC = 4,
        ISOFLAG_RETAIN_ABORT = 8,
        ISOFLAG_RETAIN_ABORT_NO = 12,
        ISOFLAG_RETAIN_DONTCARE = ISOFLAG_RETAIN_COMMIT_DC | ISOFLAG_RETAIN_ABORT_DC,
        ISOFLAG_RETAIN_BOTH = ISOFLAG_RETAIN_COMMIT | ISOFLAG_RETAIN_ABORT,
        ISOFLAG_RETAIN_NONE = ISOFLAG_RETAIN_COMMIT_NO | ISOFLAG_RETAIN_ABORT_NO,
        ISOFLAG_OPTIMISTIC = 16,
        ISOFLAG_READONLY = 32
    }

    internal enum OletxXacttc : uint
    {
        XACTTC_NONE = 0,
        XACTTC_SYNC_PHASEONE = 1,
        XACTTC_SYNC_PHASETWO = 2,
        XACTTC_SYNC = 2,
        XACTTC_ASYNC_PHASEONE = 4,
        XACTTC_ASYNC = 4
    }

    internal enum OletxXactRm : uint
    {
        XACTRM_OPTIMISTICLASTWINS = 1,
        XACTRM_NOREADONLYPREPARES = 2
    }

    internal enum OletxTransactionStatus
    {
        OLETX_TRANSACTION_STATUS_NONE = 0,
        OLETX_TRANSACTION_STATUS_OPENNORMAL = 0x1,
        OLETX_TRANSACTION_STATUS_OPENREFUSED = 0x2,
        OLETX_TRANSACTION_STATUS_PREPARING = 0x4,
        OLETX_TRANSACTION_STATUS_PREPARED = 0x8,
        OLETX_TRANSACTION_STATUS_PREPARERETAINING = 0x10,
        OLETX_TRANSACTION_STATUS_PREPARERETAINED = 0x20,
        OLETX_TRANSACTION_STATUS_COMMITTING = 0x40,
        OLETX_TRANSACTION_STATUS_COMMITRETAINING = 0x80,
        OLETX_TRANSACTION_STATUS_ABORTING = 0x100,
        OLETX_TRANSACTION_STATUS_ABORTED = 0x200,
        OLETX_TRANSACTION_STATUS_COMMITTED = 0x400,
        OLETX_TRANSACTION_STATUS_HEURISTIC_ABORT = 0x800,
        OLETX_TRANSACTION_STATUS_HEURISTIC_COMMIT = 0x1000,
        OLETX_TRANSACTION_STATUS_HEURISTIC_DAMAGE = 0x2000,
        OLETX_TRANSACTION_STATUS_HEURISTIC_DANGER = 0x4000,
        OLETX_TRANSACTION_STATUS_FORCED_ABORT = 0x8000,
        OLETX_TRANSACTION_STATUS_FORCED_COMMIT = 0x10000,
        OLETX_TRANSACTION_STATUS_INDOUBT = 0x20000,
        OLETX_TRANSACTION_STATUS_CLOSED = 0x40000,
        OLETX_TRANSACTION_STATUS_OPEN = 0x3,
        OLETX_TRANSACTION_STATUS_NOTPREPARED = 0x7ffc3,
        OLETX_TRANSACTION_STATUS_ALL = 0x7ffff
    }

    internal enum OletxTransactionHeuristic : uint
    {
        XACTHEURISTIC_ABORT = 1,
        XACTHEURISTIC_COMMIT = 2,
        XACTHEURISTIC_DAMAGE = 3,
        XACTHEURISTIC_DANGER = 4
    }

    internal enum OletxXactStat
    {
        XACTSTAT_NONE = 0,
        XACTSTAT_OPENNORMAL = 0x1,
        XACTSTAT_OPENREFUSED = 0x2,
        XACTSTAT_PREPARING = 0x4,
        XACTSTAT_PREPARED = 0x8,
        XACTSTAT_PREPARERETAINING = 0x10,
        XACTSTAT_PREPARERETAINED = 0x20,
        XACTSTAT_COMMITTING = 0x40,
        XACTSTAT_COMMITRETAINING = 0x80,
        XACTSTAT_ABORTING = 0x100,
        XACTSTAT_ABORTED = 0x200,
        XACTSTAT_COMMITTED = 0x400,
        XACTSTAT_HEURISTIC_ABORT = 0x800,
        XACTSTAT_HEURISTIC_COMMIT = 0x1000,
        XACTSTAT_HEURISTIC_DAMAGE = 0x2000,
        XACTSTAT_HEURISTIC_DANGER = 0x4000,
        XACTSTAT_FORCED_ABORT = 0x8000,
        XACTSTAT_FORCED_COMMIT = 0x10000,
        XACTSTAT_INDOUBT = 0x20000,
        XACTSTAT_CLOSED = 0x40000,
        XACTSTAT_OPEN = 0x3,
        XACTSTAT_NOTPREPARED = 0x7ffc3,
        XACTSTAT_ALL = 0x7ffff
    }

    [ComVisible(false)]
    internal struct OletxXactTransInfo
    {
        internal Guid uow;
        internal OletxTransactionIsolationLevel isoLevel;
        internal OletxTransactionIsoFlags isoFlags;
        internal int grfTCSupported;
        internal int grfRMSupported;
        internal int grfTCSupportedRetaining;
        internal int grfRMSupportedRetaining;

        // This structure is only ever filled in by a call to the DTC proxy.  But if we don't have this constructor,
        // the compiler complains with a warning because the fields are never initialized away from their default value.
        // So we added this constructor to get rid of the warning.  But since the structure is only ever filled in by
        // unmanaged code through a proxy call, FXCop complains that this internal method is never called.
        internal OletxXactTransInfo(Guid guid, OletxTransactionIsolationLevel isoLevel)
        {
            this.uow = guid;
            this.isoLevel = isoLevel;
            this.isoFlags = OletxTransactionIsoFlags.ISOFLAG_NONE;
            this.grfTCSupported = 0;
            this.grfRMSupported = 0;
            this.grfTCSupportedRetaining = 0;
            this.grfRMSupportedRetaining = 0;
        }
    }

    internal interface IVoterBallotShim
    {
        void Vote(bool voteYes);
    }

    internal interface IPhase0EnlistmentShim
    {
        void Unenlist();

        void Phase0Done(bool voteYes);
    }

    internal interface IEnlistmentShim
    {
        void PrepareRequestDone(OletxPrepareVoteType voteType);

        void CommitRequestDone();

        void AbortRequestDone();
    }

    internal interface ITransactionShim
    {
        void Commit();

        void Abort();

        void GetITransactionNative([MarshalAs(UnmanagedType.Interface)] out IDtcTransaction transactionNative);

        void Export(byte[] whereabouts, out byte[] cookieBuffer);

        void CreateVoter(OletxPhase1VolatileEnlistmentContainer managedIdentifier, out IVoterBallotShim voterBallotShim);

        byte[] GetPropagationToken();

        void Phase0Enlist(object managedIdentifier, out IPhase0EnlistmentShim phase0EnlistmentShim);

        void GetTransaction(out ITransaction transaction);
    }

    internal interface IResourceManagerShim
    {
        void Enlist(ITransactionShim transactionShim, OletxEnlistment managedIdentifier, out IEnlistmentShim enlistmentShim);

        void Reenlist(uint prepareInfoSize, byte[] prepareInfo, out OletxTransactionOutcome outcome);

        void ReenlistComplete();
    }

    internal interface IDtcProxyShimFactory
    {
        void ConnectToProxy(
            string? nodeName,
            Guid resourceManagerIdentifier,
            object managedIdentifier,
            out bool nodeNameMatches,
            out byte[] whereaboutsBuffer,
            out IResourceManagerShim resourceManagerShim);

        public void GetNotification(
            out object? managedIdentifier,
            out ShimNotificationType shimNotificationType,
            out bool isSinglePhase,
            out bool abortingHint,
            out bool releaseLock,
            out byte[]? prepareInfo);

        void ReleaseNotificationLock();

        void BeginTransaction(
            uint timeout,
            OletxTransactionIsolationLevel isolationLevel,
            object? managedIdentifier,
            out Guid transactionIdentifier,
            out ITransactionShim transactionShim);

        void CreateResourceManager(
            Guid resourceManagerIdentifier,
            OletxResourceManager managedIdentifier,
            out IResourceManagerShim resourceManagerShim);

        void Import(
            byte[] cookie,
            OutcomeEnlistment managedIdentifier,
            out Guid transactionIdentifier,
            out OletxTransactionIsolationLevel isolationLevel,
            out ITransactionShim transactionShim);

        void ReceiveTransaction(
            byte[] propgationToken,
            OutcomeEnlistment managedIdentifier,
            out Guid transactionIdentifier,
            out OletxTransactionIsolationLevel isolationLevel,
            out ITransactionShim transactionShim);
    }
}
