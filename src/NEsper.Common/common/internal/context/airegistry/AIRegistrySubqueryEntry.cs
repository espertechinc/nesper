///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistrySubqueryEntry
    {
        public AIRegistrySubqueryEntry(
            AIRegistrySubselectLookup lookupStrategies,
            AIRegistryAggregation aggregationServices,
            AIRegistryPriorEvalStrategy priorEvalStrategies,
            AIRegistryPreviousGetterStrategy previousGetterStrategies)
        {
            LookupStrategies = lookupStrategies;
            AggregationServices = aggregationServices;
            PriorEvalStrategies = priorEvalStrategies;
            PreviousGetterStrategies = previousGetterStrategies;
        }

        public AIRegistrySubselectLookup LookupStrategies { get; }

        public AIRegistryAggregation AggregationServices { get; }

        public AIRegistryPriorEvalStrategy PriorEvalStrategies { get; }

        public AIRegistryPreviousGetterStrategy PreviousGetterStrategies { get; }

        public void DeassignService(int agentInstanceId)
        {
            LookupStrategies.DeassignService(agentInstanceId);
            AggregationServices?.DeassignService(agentInstanceId);

            PriorEvalStrategies?.DeassignService(agentInstanceId);

            PreviousGetterStrategies?.DeassignService(agentInstanceId);
        }

        public void Assign(
            int agentInstanceId,
            SubordTableLookupStrategy lookupStrategy,
            AggregationService aggregationService,
            PriorEvalStrategy priorEvalStrategy,
            PreviousGetterStrategy previousGetterStrategy)
        {
            LookupStrategies.AssignService(agentInstanceId, lookupStrategy);
            AggregationServices?.AssignService(agentInstanceId, aggregationService);

            PriorEvalStrategies?.AssignService(agentInstanceId, priorEvalStrategy);

            PreviousGetterStrategies?.AssignService(agentInstanceId, previousGetterStrategy);
        }
    }
} // end of namespace