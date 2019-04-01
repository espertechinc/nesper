///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dispatch;
using com.espertech.esper.timer;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Convenience view for dispatching view updates received from a parent view 
    /// to Update listeners via the dispatch service.
    /// </summary>
    public class UpdateDispatchViewBlockingSpin : UpdateDispatchViewBase
    {
        private UpdateDispatchFutureSpin _currentFutureSpin;
        private readonly long _msecTimeout;
        private readonly TimeSourceService _timeSourceService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementResultService">handles result delivery</param>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="msecTimeout">timeout for preserving dispatch order through blocking</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="threadLocalManager">The thread local manager.</param>
        public UpdateDispatchViewBlockingSpin(StatementResultService statementResultService, DispatchService dispatchService, long msecTimeout, TimeSourceService timeSourceService, IThreadLocalManager threadLocalManager)
            : base(statementResultService, dispatchService, threadLocalManager)
        {
            _currentFutureSpin = new UpdateDispatchFutureSpin(timeSourceService); // use a completed future as a start
            _msecTimeout = msecTimeout;
            _timeSourceService = timeSourceService;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            NewResult(new UniformPair<EventBean[]>(newData, oldData));
        }

        public override void NewResult(UniformPair<EventBean[]> result)
        {
            StatementResultService.Indicate(result);

            var isDispatchWaiting = IsDispatchWaiting;

            if (!isDispatchWaiting.Value)
            {
                UpdateDispatchFutureSpin nextFutureSpin;
                lock (this)
                {
                    nextFutureSpin = new UpdateDispatchFutureSpin(this, _currentFutureSpin, _msecTimeout, _timeSourceService);
                    _currentFutureSpin = nextFutureSpin;
                }
                DispatchService.AddExternal(nextFutureSpin);
                isDispatchWaiting.Value = true;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
