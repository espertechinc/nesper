///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Base class for all future implementations
    /// </summary>
    internal abstract class FutureBase
    {
        public const int STATE_PENDING = 1;
        public const int STATE_RUNNING = 2;
        public const int STATE_FINISHED = 4;
        public const int STATE_CANCELED = 8;

        private Thread _execThread;
        private long _state;

        protected FutureBase()
        {
            _execThread = null;
            _state = STATE_PENDING;
        }

        /// <summary>
        /// Gets the exec thread.
        /// </summary>
        /// <value>The exec thread.</value>
        public Thread ExecThread => _execThread;

        /// <summary>
        /// Invokes the impl.
        /// </summary>
        protected abstract void InvokeImpl();

        protected bool CheckStateBit(long bit)
        {
            return (Interlocked.Read(ref _state) & bit) != 0;
        }

        protected long SetBitState(long bitstate)
        {
            return Interlocked.Exchange(ref _state, bitstate);
        }

        public bool IsCanceledOrFinished => CheckStateBit(STATE_CANCELED | STATE_FINISHED);
        public bool IsCanceled => CheckStateBit(STATE_CANCELED);
        public bool IsFinished => CheckStateBit(STATE_FINISHED);
        public bool IsRunning => CheckStateBit(STATE_RUNNING);
        public bool IsPending => CheckStateBit(STATE_PENDING);

        /// <summary>
        /// Attempts to cancel the future.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        public virtual bool Cancel(bool force)
        {
            // Attempt to cancel by marking using an interlocked comparison.  If the value comes
            // back false, then it indicates that we swapped the value successfully.
            var previousState = Interlocked.CompareExchange(ref _state, STATE_CANCELED, STATE_PENDING);
            if (previousState == STATE_PENDING) {
                return true;
            } else if ((previousState & STATE_CANCELED) == STATE_CANCELED) {
                return true; // canceled
            } else if (previousState == STATE_FINISHED) {
                return false; // executed, but not canceled
            }

            if (force) {
                Kill();

                for (int ii = 0; ii < 10; ii++) {
                    Thread.Sleep(100);

                    var currentState = Interlocked.Read(ref _state);
                    if (currentState == STATE_FINISHED) {
                        return false;
                    }
                    else if ((currentState & STATE_CANCELED) == STATE_CANCELED) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Invokes this instance.
        /// </summary>
        internal void Invoke()
        {
            // Prior to execute, we set the "cancel" state to non-cancelable.
            var previousState = Interlocked.CompareExchange(ref _state, STATE_RUNNING, STATE_PENDING);
            if (previousState == STATE_PENDING) {
                Interlocked.Exchange(ref _execThread, Thread.CurrentThread);
                try {
                    InvokeImpl();
                    Interlocked.Exchange(ref _state, STATE_FINISHED);
                }
                catch (ThreadInterruptedException) {
                    Log.Warn(".Invoke - Thread Interrupted");
                    Interlocked.Exchange(ref _state, STATE_CANCELED | STATE_FINISHED);
                }
                finally {
                    Interlocked.Exchange(ref _execThread, null);
                }
            }
        }

        /// <summary>
        /// Kills this instance.
        /// </summary>
        internal void Kill()
        {
            var tempThread = Interlocked.Exchange(ref _execThread, null);
            if (tempThread != null)
            {
                Log.Warn(".Kill - Forceably terminating future");
                tempThread.Interrupt();
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}