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
using com.espertech.esper.common.@internal.statement.dispatch;

namespace com.espertech.esper.runtime.@internal.kernel.updatedispatch
{
    /// <summary>
    ///     Convenience view for dispatching view updates received from a parent view to update listeners
    ///     via the dispatch service.
    /// </summary>
    public class UpdateDispatchViewBlockingWait : UpdateDispatchViewBase
    {
        private UpdateDispatchFutureWait currentFutureWait;
        private readonly long msecTimeout;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="msecTimeout">timeout for preserving dispatch order through blocking</param>
        /// <param name="statementResultServiceImpl">handles result delivery</param>
        /// <param name="eventType">event type</param>
        public UpdateDispatchViewBlockingWait(
            EventType eventType,
            StatementResultService statementResultServiceImpl,
            DispatchService dispatchService,
            long msecTimeout)
            : base(eventType, statementResultServiceImpl, dispatchService)
        {
            currentFutureWait = new UpdateDispatchFutureWait(); // use a completed future as a start
            this.msecTimeout = msecTimeout;
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            NewResult(new UniformPair<EventBean[]>(newData, oldData));
        }

        public override void NewResult(UniformPair<EventBean[]> results)
        {
            var dispatchTLEntry = statementResultService.DispatchTL.GetOrCreate();
            statementResultService.Indicate(results, dispatchTLEntry);
            if (!dispatchTLEntry.IsDispatchWaiting) {
                UpdateDispatchFutureWait nextFutureWait;
                lock (this) {
                    nextFutureWait = new UpdateDispatchFutureWait(this, currentFutureWait, msecTimeout);
                    currentFutureWait.SetLater(nextFutureWait);
                    currentFutureWait = nextFutureWait;
                }

                dispatchService.AddExternal(nextFutureWait);
                dispatchTLEntry.IsDispatchWaiting = true;
            }
        }
    }
} // end of namespace