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
    public class UpdateDispatchViewNonBlocking : UpdateDispatchViewBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="statementResultServiceImpl">handles result delivery</param>
        /// <param name="eventType">event type</param>
        public UpdateDispatchViewNonBlocking(
            EventType eventType,
            StatementResultService statementResultServiceImpl,
            DispatchService dispatchService)
            : base(eventType, statementResultServiceImpl, dispatchService)
        {
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
                dispatchService.AddExternal(this);
                dispatchTLEntry.IsDispatchWaiting = true;
            }
        }
    }
} // end of namespace