///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

using static com.espertech.esper.common.@internal.context.util.StatementAgentInstanceLockConstants;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Simple read-write lock based on <seealso cref="ReentrantReadWriteLock" /> that associates a
    /// name with the lock and logs  read/write locking and unlocking.
    /// </summary>
    public class StatementAgentInstanceLockRWLogging : IReaderWriterLock
    {
        private static readonly ILog LOCK_LOG = LogManager.GetLogger(AuditPath.LOCK_LOG);

        private const string WRITE = "write";
        private const string READ = "read ";

        private readonly IReaderWriterLock _lock;
        private readonly string _lockId;
        private readonly string _statementName;
        private readonly int _cpid;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isFair">true if a fair lock, false if not</param>
        /// <param name="statementName">statement name</param>
        /// <param name="cpid">context partition id</param>
        public StatementAgentInstanceLockRWLogging(
            bool isFair,
            string statementName,
            int cpid)
        {
            if (isFair) {
                _lock = new FairReaderWriterLock();
            }
            else {
                _lock = new SlimReaderWriterLock();
            }

            _lockId = $"RWLock@{Guid.NewGuid()}";
            _statementName = statementName;
            _cpid = cpid;
        }

                public bool IsWriterLockHeld {
            get => _lock.IsWriterLockHeld;
        }

#if DEBUG
        public bool Trace {
            get => _lock.Trace;
            set => _lock.Trace = value;
        }
#endif

        public IDisposable AcquireReadLock()
        {
            Output(ACQUIRE_TEXT, READ, null);
            var lockDisposable = _lock.AcquireReadLock();
            Output(ACQUIRED_TEXT, READ, null);

            return new TrackedDisposable(
                () => {
                    Output(RELEASE_TEXT, READ, null);
                    lockDisposable.Dispose();
                    lockDisposable = null;
                    Output(RELEASED_TEXT, READ, null);
                }
            );
        }
        
        public IDisposable AcquireWriteLock()
        {
            Output(ACQUIRE_TEXT, WRITE, null);
            var lockDisposable = _lock.AcquireWriteLock();
            Output(ACQUIRED_TEXT, WRITE, null);
            
            return new TrackedDisposable(
                () => {
                    Output(RELEASE_TEXT, WRITE, null);
                    lockDisposable.Dispose();
                    lockDisposable = null;
                    Output(RELEASED_TEXT, WRITE, null);
                }
            );
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
            Output(ACQUIRE_TEXT, WRITE, lockWaitDuration);
            var lockDisposable = _lock.AcquireWriteLock(lockWaitDuration);
            Output(ACQUIRED_TEXT, WRITE, lockWaitDuration);
            
            return new TrackedDisposable(
                () => {
                    Output(RELEASE_TEXT, WRITE, lockWaitDuration);
                    lockDisposable.Dispose();
                    lockDisposable = null;
                    Output(RELEASED_TEXT, WRITE, lockWaitDuration);
                }
            );
        }

        public void ReleaseWriteLock()
        {
            Output(RELEASE_TEXT, WRITE, null);
            _lock.ReleaseWriteLock();
            Output(RELEASED_TEXT, WRITE, null);
        }

        public ILockable ReadLock => throw new NotSupportedException();
        public ILockable WriteLock => throw new NotSupportedException();
        
        
        private void Output(
            string action,
            string lockType,
            TimeSpan? timeoutMSec)
        {
            LOCK_LOG.Info(
                "{}{} {} stmt '{}' cpid {} timeoutMSec {} isWriteLocked {}",
                action,
                lockType,
                _lockId,
                _statementName,
                _cpid,
                timeoutMSec,
                _lock.IsWriterLockHeld);
        }

        public override string ToString()
        {
            return GetType().CleanName();
        }
    }
} // end of namespace