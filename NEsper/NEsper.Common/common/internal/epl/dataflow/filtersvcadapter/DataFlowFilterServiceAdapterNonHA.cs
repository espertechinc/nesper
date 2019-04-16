///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter
{
    public class DataFlowFilterServiceAdapterNonHA : DataFlowFilterServiceAdapter
    {
        public static readonly DataFlowFilterServiceAdapterNonHA INSTANCE = new DataFlowFilterServiceAdapterNonHA();

        private readonly IDictionary<FilterHandleCallback, EPStatementHandleCallbackFilter> handlesPerOp =
            new Dictionary<FilterHandleCallback, EPStatementHandleCallbackFilter>();

        private DataFlowFilterServiceAdapterNonHA()
        {
        }

        public void AddFilterCallback(
            FilterHandleCallback filterHandleCallback,
            AgentInstanceContext agentInstanceContext,
            EventType eventType,
            FilterValueSetParam[][] @params,
            int filterCallbackId)
        {
            var handle = new EPStatementHandleCallbackFilter(agentInstanceContext.EpStatementAgentInstanceHandle, filterHandleCallback);
            agentInstanceContext.FilterService.Add(eventType, @params, handle);
            handlesPerOp.Put(filterHandleCallback, handle);
        }

        public void RemoveFilterCallback(
            FilterHandleCallback filterHandleCallback,
            AgentInstanceContext agentInstanceContext,
            EventType eventType,
            FilterValueSetParam[][] @params,
            int filterCallbackId)
        {
            var handle = handlesPerOp.Delete(filterHandleCallback);
            if (handle == null) {
                return;
            }

            agentInstanceContext.FilterService.Remove(handle, eventType, @params);
        }
    }
} // end of namespace