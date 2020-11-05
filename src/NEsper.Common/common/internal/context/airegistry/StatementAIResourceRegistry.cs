///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class StatementAIResourceRegistry
    {
        public StatementAIResourceRegistry(
            AIRegistryAggregation agentInstanceAggregationService,
            AIRegistryPriorEvalStrategy[] agentInstancePriorEvalStrategies,
            IDictionary<int, AIRegistrySubqueryEntry> agentInstanceSubselects,
            IDictionary<int, AIRegistryTableAccess> agentInstanceTableAccesses,
            AIRegistryPreviousGetterStrategy[] agentInstancePreviousGetterStrategies,
            AIRegistryRowRecogPreviousStrategy agentInstanceRowRecogPreviousStrategy)
        {
            AgentInstanceAggregationService = agentInstanceAggregationService;
            AgentInstancePriorEvalStrategies = agentInstancePriorEvalStrategies;
            AgentInstanceSubselects = agentInstanceSubselects;
            AgentInstanceTableAccesses = agentInstanceTableAccesses;
            AgentInstancePreviousGetterStrategies = agentInstancePreviousGetterStrategies;
            AgentInstanceRowRecogPreviousStrategy = agentInstanceRowRecogPreviousStrategy;
        }

        public AIRegistryAggregation AgentInstanceAggregationService { get; }

        public AIRegistryPriorEvalStrategy[] AgentInstancePriorEvalStrategies { get; }

        public AIRegistryPreviousGetterStrategy[] AgentInstancePreviousGetterStrategies { get; }

        public AIRegistryRowRecogPreviousStrategy AgentInstanceRowRecogPreviousStrategy { get; }

        public IDictionary<int, AIRegistrySubqueryEntry> AgentInstanceSubselects { get; }

        public IDictionary<int, AIRegistryTableAccess> AgentInstanceTableAccesses { get; }

        public void Deassign(int agentInstanceId)
        {
            AgentInstanceAggregationService.DeassignService(agentInstanceId);
            if (AgentInstancePriorEvalStrategies != null) {
                foreach (var prior in AgentInstancePriorEvalStrategies) {
                    prior.DeassignService(agentInstanceId);
                }
            }

            if (AgentInstanceSubselects != null) {
                foreach (var entry in AgentInstanceSubselects) {
                    entry.Value.DeassignService(agentInstanceId);
                }
            }

            if (AgentInstancePreviousGetterStrategies != null) {
                foreach (var previous in AgentInstancePreviousGetterStrategies) {
                    previous.DeassignService(agentInstanceId);
                }
            }

            if (AgentInstanceTableAccesses != null) {
                foreach (var entry in AgentInstanceTableAccesses) {
                    entry.Value.DeassignService(agentInstanceId);
                }
            }
        }
    }
} // end of namespace