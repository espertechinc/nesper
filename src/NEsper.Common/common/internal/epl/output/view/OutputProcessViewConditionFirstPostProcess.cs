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
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Handles output rate limiting for FIRST, only applicable with a having-clause and no group-by clause.
    ///     <para />
    ///     Without having-clause the order of processing won't matter therefore its handled by the
    ///     <seealso cref="OutputProcessViewConditionDefault" />. With group-by the <seealso cref="ResultSetProcessor" />
    ///     handles the per-group first criteria.
    /// </summary>
    public class OutputProcessViewConditionFirstPostProcess : OutputProcessViewConditionFirst
    {
        private readonly OutputStrategyPostProcess _postProcessor;

        public OutputProcessViewConditionFirstPostProcess(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext,
            OutputStrategyPostProcess postProcessor,
            StateMgmtSetting stateMgmtSetting)
            : base(
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied,
                parent,
                agentInstanceContext,
                stateMgmtSetting)
        {
            _postProcessor = postProcessor;
        }

        protected override void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (child != null) {
                _postProcessor.Output(forceUpdate, results, child);
            }
        }
    }
} // end of namespace