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
    public class OutputProcessViewDirectPostProcess : OutputProcessViewDirect
    {
        private readonly OutputStrategyPostProcess postProcessor;

        public OutputProcessViewDirectPostProcess(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            OutputStrategyPostProcess postProcessor)
            : base(agentInstanceContext, resultSetProcessor)
        {
            this.postProcessor = postProcessor;
        }

        protected void PostProcess(
            bool force,
            UniformPair<EventBean[]> newOldEvents,
            UpdateDispatchView childView)
        {
            postProcessor.Output(force, newOldEvents, childView);
        }
    }
} // end of namespace