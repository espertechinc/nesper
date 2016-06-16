///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.agg.service
{
	public class AggSvcGroupByLocalGroupByFactory : AggregationServiceFactory
    {
	    protected internal readonly bool Join;
        protected internal readonly AggregationLocalGroupByPlan LocalGroupByPlan;
        protected internal readonly object GroupKeyBinding;

	    public AggSvcGroupByLocalGroupByFactory(bool join, AggregationLocalGroupByPlan localGroupByPlan, object groupKeyBinding)
        {
	        Join = join;
	        LocalGroupByPlan = localGroupByPlan;
	        GroupKeyBinding = groupKeyBinding;
	    }

        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService, bool isSubquery, int? subqueryNumber)
        {
	        return new AggSvcGroupByLocalGroupBy(methodResolutionService, Join, LocalGroupByPlan, GroupKeyBinding);
	    }
	}
} // end of namespace
