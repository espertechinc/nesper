///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Contains handles to implementations of services needed by evaluation nodes.
    /// </summary>
    public class PatternAgentInstanceContext
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly EvalFilterConsumptionHandler consumptionHandler;
        internal readonly Func<FilterSpecActivatable, FilterValueSetParam[][]> contextAddendumFunction;
        internal readonly PatternContext patternContext;

        public PatternAgentInstanceContext(
            PatternContext patternContext,
            AgentInstanceContext agentInstanceContext,
            bool hasConsumingFilter,
            Func<FilterSpecActivatable, FilterValueSetParam[][]> contextAddendumFunction)
        {
            this.patternContext = patternContext;
            this.agentInstanceContext = agentInstanceContext;
            this.contextAddendumFunction = contextAddendumFunction;

            if (hasConsumingFilter) {
                consumptionHandler = new EvalFilterConsumptionHandler();
            }
            else {
                consumptionHandler = null;
            }
        }

        public PatternContext PatternContext => patternContext;

        public EvalFilterConsumptionHandler ConsumptionHandler => consumptionHandler;

        public AgentInstanceContext AgentInstanceContext => agentInstanceContext;

        public StatementContext StatementContext => agentInstanceContext.StatementContext;

        public string StatementName => agentInstanceContext.StatementName;

        public FilterService FilterService => StatementContext.FilterService;

        public long Time => agentInstanceContext.SchedulingService.Time;

        public FilterValueSetParam[][] GetFilterAddendumForContextPath(FilterSpecActivatable filterSpec)
        {
            return contextAddendumFunction?.Invoke(filterSpec);
        }

        public AgentInstanceContext GetAgentInstanceContext()
        {
            return AgentInstanceContext;
        }
    }
} // end of namespace