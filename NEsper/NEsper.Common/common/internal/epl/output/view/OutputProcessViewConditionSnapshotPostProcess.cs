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
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionSnapshotPostProcess : OutputProcessViewConditionSnapshot
    {
        private readonly OutputStrategyPostProcess postProcessor;

        public OutputProcessViewConditionSnapshotPostProcess(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext,
            OutputStrategyPostProcess postProcessor)
            : base(
                resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied, parent,
                agentInstanceContext)
        {
            this.postProcessor = postProcessor;
        }

        public override void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> results)
        {
            if (child != null) {
                postProcessor.Output(forceUpdate, results, child);
            }
        }
    }
} // end of namespace