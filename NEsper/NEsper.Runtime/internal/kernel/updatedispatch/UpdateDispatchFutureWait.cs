///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.kernel.updatedispatch
{
    /// <summary>
    /// UpdateDispatchFutureWait can be added to a dispatch queue that is thread-local. It
    /// represents is a stand-in for a future dispatching of a statement result to statement
    /// listeners.
    /// <para />
    /// UpdateDispatchFutureWait is aware of future and past dispatches:
    ///     (newest) DF3   &lt;--&gt;   DF2  &lt;--&gt;  DF1  (oldest)
    /// </summary>
    public class UpdateDispatchFutureWait : Dispatchable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UpdateDispatchViewBlockingWait _view;
        private UpdateDispatchFutureWait _earlier;
        private UpdateDispatchFutureWait _later;
        private volatile bool _isCompleted;
        private readonly long _msecTimeout;

        /// <summary>Ctor. </summary>
        /// <param name="view">is the blocking dispatch view through which to execute a dispatch</param>
        /// <param name="earlier">is the older future</param>
        /// <param name="msecTimeout">is the timeout period to wait for listeners to complete a prior dispatch</param>
        public UpdateDispatchFutureWait(UpdateDispatchViewBlockingWait view, UpdateDispatchFutureWait earlier, long msecTimeout)
        {
            _view = view;
            _earlier = earlier;
            _msecTimeout = msecTimeout;
        }

        /// <summary>Ctor - use for the first future to indicate completion. </summary>
        public UpdateDispatchFutureWait()
        {
            _isCompleted = true;
        }

        /// <summary>Returns true if the dispatch completed for this future. </summary>
        /// <value>true for completed, false if not</value>
        public bool IsCompleted
        {
            get => _isCompleted;
            set => _isCompleted = value;
        }

        /// <summary>Hand a later future to the dispatch to use for indicating completion via notify. </summary>
        /// <param name="later">is the later dispatch</param>
        public UpdateDispatchFutureWait SetLater(UpdateDispatchFutureWait later)
        {
            _later = later;
            return this;
        }

        public void Execute()
        {
            if (!_earlier._isCompleted)
            {
                lock (this)
                {
                    if (!_earlier._isCompleted)
                    {
                        Monitor.Wait(this, (int) _msecTimeout);
                    }
                }
            }

            _view.Execute();
            _isCompleted = true;

            if (_later != null)
            {
                lock (_later)
                {
                    Monitor.Pulse(_later);
                }
            }
            _earlier = null;
            _later = null;
        }
    }
}