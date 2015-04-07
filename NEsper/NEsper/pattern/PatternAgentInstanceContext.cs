///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Contains handles to implementations of services needed by evaluation nodes.
    /// </summary>
    public class PatternAgentInstanceContext
    {
        public PatternAgentInstanceContext(PatternContext patternContext, AgentInstanceContext agentInstanceContext, bool hasConsumingFilter) {
            PatternContext = patternContext;
            AgentInstanceContext = agentInstanceContext;
            ConsumptionHandler = hasConsumingFilter ? new EvalFilterConsumptionHandler() : null;
        }

        public PatternContext PatternContext { get; private set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }

        public EvalFilterConsumptionHandler ConsumptionHandler { get; private set; }

        public StatementContext StatementContext
        {
            get { return AgentInstanceContext.StatementContext; }
        }
    }
}
