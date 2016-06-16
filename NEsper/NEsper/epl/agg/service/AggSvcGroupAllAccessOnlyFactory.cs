///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Aggregation service for use when only first/last/window aggregation functions are used an none other.
    /// </summary>
    public class AggSvcGroupAllAccessOnlyFactory : AggregationServiceFactory
    {
        protected internal readonly AggregationAccessorSlotPair[] Accessors;
        protected internal readonly AggregationStateFactory[] AccessAggSpecs;
        protected internal readonly bool IsJoin;
    
        public AggSvcGroupAllAccessOnlyFactory(AggregationAccessorSlotPair[] accessors, AggregationStateFactory[] accessAggSpecs, bool join)
        {
            Accessors = accessors;
            AccessAggSpecs = accessAggSpecs;
            IsJoin = join;
        }

        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService, bool isSubquery, int? subqueryNumber)
        {
            AggregationState[] states = methodResolutionService.NewAccesses(agentInstanceContext.AgentInstanceId, IsJoin, AccessAggSpecs, null);
            return new AggSvcGroupAllAccessOnlyImpl(Accessors, states, AccessAggSpecs);
        }
    }
}
