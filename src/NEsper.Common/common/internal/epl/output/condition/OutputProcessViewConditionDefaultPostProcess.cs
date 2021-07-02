///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    /// A view that prepares output events, batching incoming
    /// events and invoking the result set processor as necessary.
    /// <para />Handles output rate limiting or stabilizing.
    /// </summary>
    public class OutputProcessViewConditionDefaultPostProcess : OutputProcessViewConditionDefault
    {
        private readonly OutputStrategyPostProcess postProcessor;

        public OutputProcessViewConditionDefaultPostProcess(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext,
            OutputStrategyPostProcess postProcessor,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
            : base(
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied,
                parent,
                agentInstanceContext,
                eventTypes,
                stateMgmtSettings)
        {
            this.postProcessor = postProcessor;
        }

        protected override void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (child != null) {
                postProcessor.Output(forceUpdate, results, child);
            }
        }
    }
} // end of namespace