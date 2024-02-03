///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.method.poll;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class HistoricalEventViewableMethodFactory : HistoricalEventViewableFactoryBase
    {
        public string ConfigurationName { get; set; }

        public MethodTargetStrategyFactory TargetStrategy { get; set; }

        public MethodConversionStrategy ConversionStrategy { get; set; }

        public override void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
        }

        public override HistoricalEventViewable Activate(AgentInstanceContext agentInstanceContext)
        {
            var strategy = new PollExecStrategyMethod(TargetStrategy.Make(agentInstanceContext), ConversionStrategy);
            return new HistoricalEventViewableMethod(this, strategy, agentInstanceContext);
        }
    }
} // end of namespace