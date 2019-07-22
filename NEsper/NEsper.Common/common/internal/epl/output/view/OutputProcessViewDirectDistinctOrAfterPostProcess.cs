///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.statement.dispatch;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfterPostProcess : OutputProcessViewDirectDistinctOrAfter
    {
        private readonly OutputStrategyPostProcess _postProcessor;

        public OutputProcessViewDirectDistinctOrAfterPostProcess(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewDirectDistinctOrAfterFactory parent,
            OutputStrategyPostProcess postProcessor)
            : base(
                agentInstanceContext,
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied,
                parent)
        {
            _postProcessor = postProcessor;
        }

        protected override void PostProcess(
            bool force,
            UniformPair<EventBean[]> newOldEvents,
            UpdateDispatchView childView)
        {
            _postProcessor.Output(force, newOldEvents, childView);
        }
    }
} // end of namespace