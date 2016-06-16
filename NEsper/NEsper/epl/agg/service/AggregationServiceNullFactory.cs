///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// A null object implementation of the AggregationService interface.
    /// </summary>
    public class AggregationServiceNullFactory : AggregationServiceFactory
    {
        public static AggregationServiceNullFactory AGGREGATION_SERVICE_NULL_FACTORY = new AggregationServiceNullFactory();

        private static readonly AggregationServiceNull AGGREGATION_SERVICE_NULL = new AggregationServiceNull();

        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService, bool isSubquery, int? subqueryNumber)
        {
            return AGGREGATION_SERVICE_NULL;
        }
    }
}
