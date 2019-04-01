///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
            AIRegistrySubselectLookup lookupStrategies, AIRegistryAggregation aggregationServices,
            AIRegistryPriorEvalStrategy priorEvalStrategies, AIRegistryPreviousGetterStrategy previousGetterStrategies)
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
            if (AggregationServices != null) {
                AggregationServices.DeassignService(agentInstanceId);
            }

            if (PriorEvalStrategies != null) {
                PriorEvalStrategies.DeassignService(agentInstanceId);
            }

            if (PreviousGetterStrategies != null) {
                PreviousGetterStrategies.DeassignService(agentInstanceId);
            }
        }

        public void Assign(
            int agentInstanceId, SubordTableLookupStrategy lookupStrategy, AggregationService aggregationService,
            PriorEvalStrategy priorEvalStrategy, PreviousGetterStrategy previousGetterStrategy)
        {
            LookupStrategies.AssignService(agentInstanceId, lookupStrategy);
            if (AggregationServices != null) {
                AggregationServices.AssignService(agentInstanceId, aggregationService);
            }

            if (PriorEvalStrategies != null) {
                PriorEvalStrategies.AssignService(agentInstanceId, priorEvalStrategy);
            }

            if (PreviousGetterStrategies != null) {
                PreviousGetterStrategies.AssignService(agentInstanceId, previousGetterStrategy);
            }
        }
    }
} // end of namespace