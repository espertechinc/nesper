///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dispatch;
using com.espertech.esper.timer;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// UpdateDispatchFutureSpin can be added to a dispatch queue that is thread-local. It represents 
    /// is a stand-in for a future dispatching of a statement result to statement listeners. 
    /// <para/> 
    /// UpdateDispatchFutureSpin is aware of future and past dispatches: 
    ///     (newest) DF3   &lt;--&gt;   DF2  &lt;--&gt;  DF1  (oldest), 
    /// 
    /// and uses a spin lock to block if required
    /// </summary>
    public class UpdateDispatchFutureSpin : Dispatchable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly long _msecTimeout;
        private readonly TimeSourceService _timeSourceService;

        private readonly UpdateDispatchViewBlockingSpin _view;
        private UpdateDispatchFutureSpin _earlier;
        private volatile bool _isCompleted;

        /// <summary>Ctor. </summary>
        /// <param name="view">is the blocking dispatch view through which to execute a dispatch</param>
        /// <param name="earlier">is the older future</param>
        /// <param name="msecTimeout">is the timeout period to wait for listeners to complete a prior dispatch</param>
        /// <param name="timeSourceService">time source provider</param>
        public UpdateDispatchFutureSpin(UpdateDispatchViewBlockingSpin view,
                                        UpdateDispatchFutureSpin earlier,
                                        long msecTimeout,
                                        TimeSourceService timeSourceService)
        {
            _view = view;
            _earlier = earlier;
            _msecTimeout = msecTimeout;
            _timeSourceService = timeSourceService;
        }

        /// <summary>Ctor - use for the first future to indicate completion. </summary>
        /// <param name="timeSourceService">time source provider</param>
        public UpdateDispatchFutureSpin(TimeSourceService timeSourceService)
        {
            _isCompleted = true;
            _timeSourceService = timeSourceService;
        }

        #region Dispatchable Members

        public void Execute()
        {
            if (!_earlier._isCompleted)
            {
                long spinStartTime = _timeSourceService.GetTimeMillis();

                while (!_earlier._isCompleted)
                {
                    Thread.Yield();

                    long spinDelta = _timeSourceService.GetTimeMillis() - spinStartTime;
                    if (spinDelta > _msecTimeout)
                    {
                        Log.Info("Spin wait timeout exceeded in listener dispatch for statement '" + _view.StatementResultService.StatementName + "'");
                        break;
                    }
                }
            }

            _view.Execute();
            _isCompleted = true;

            _earlier = null;
        }

        #endregion

        /// <summary>Returns true if the dispatch completed for this future. </summary>
        /// <returns>true for completed, false if not</returns>
        public bool IsCompleted()
        {
            return _isCompleted;
        }
    }
}