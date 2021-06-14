///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.dispatch;

namespace com.espertech.esper.runtime.@internal.kernel.updatedispatch
{
    /// <summary>
    ///     Convenience view for dispatching view updates received from a parent view to update listeners
    ///     via the dispatch service.
    /// </summary>
    public class UpdateDispatchViewBlockingSpin : UpdateDispatchViewBase
    {
        private UpdateDispatchFutureSpin currentFutureSpin;
        private readonly long msecTimeout;
        private readonly TimeSourceService timeSourceService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="msecTimeout">timeout for preserving dispatch order through blocking</param>
        /// <param name="statementResultService">handles result delivery</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="eventType">event type</param>
        public UpdateDispatchViewBlockingSpin(
            EventType eventType,
            StatementResultService statementResultService,
            DispatchService dispatchService,
            long msecTimeout,
            TimeSourceService timeSourceService)
            : base(eventType, statementResultService, dispatchService)
        {
            currentFutureSpin = new UpdateDispatchFutureSpin(timeSourceService); // use a completed future as a start
            this.msecTimeout = msecTimeout;
            this.timeSourceService = timeSourceService;
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            NewResult(new UniformPair<EventBean[]>(newData, oldData));
        }

        public override void NewResult(UniformPair<EventBean[]> result)
        {
            var dispatchTLEntry = statementResultService.DispatchTL.GetOrCreate();
            statementResultService.Indicate(result, dispatchTLEntry);
            if (!dispatchTLEntry.IsDispatchWaiting) {
                UpdateDispatchFutureSpin nextFutureSpin;
                lock (this) {
                    nextFutureSpin = new UpdateDispatchFutureSpin(this, currentFutureSpin, msecTimeout, timeSourceService);
                    currentFutureSpin = nextFutureSpin;
                }

                dispatchService.AddExternal(nextFutureSpin);
                dispatchTLEntry.IsDispatchWaiting = true;
            }
        }
    }
} // end of namespace