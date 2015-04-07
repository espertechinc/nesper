///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.stmt
{
    public class StatementAIResourceRegistry
    {
        public StatementAIResourceRegistry(AIRegistryAggregation agentInstanceAggregationService, AIRegistryExpr agentInstanceExprService)
        {
            AgentInstanceAggregationService = agentInstanceAggregationService;
            AgentInstanceExprService = agentInstanceExprService;
        }

        public AIRegistryExpr AgentInstanceExprService { get; private set; }

        public AIRegistryAggregation AgentInstanceAggregationService { get; private set; }

        public void Deassign(int agentInstanceIds)
        {
            AgentInstanceAggregationService.DeassignService(agentInstanceIds);
            AgentInstanceExprService.DeassignService(agentInstanceIds);
        }
    }
}
