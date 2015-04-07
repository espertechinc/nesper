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
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupAllLocalGroupByFactory : AggregationServiceFactory
    {
        protected readonly bool IsJoin;
        private readonly object _groupKeyBinding;
        private readonly AggregationLocalGroupByPlan _localGroupByPlan;

        public AggSvcGroupAllLocalGroupByFactory(
            bool join,
            AggregationLocalGroupByPlan localGroupByPlan,
            object groupKeyBinding)
        {
            IsJoin = join;
            _localGroupByPlan = localGroupByPlan;
            _groupKeyBinding = groupKeyBinding;
        }

        public AggregationService MakeService(
            AgentInstanceContext agentInstanceContext,
            MethodResolutionService methodResolutionService)
        {
            return new AggSvcGroupAllLocalGroupBy(methodResolutionService, IsJoin, _localGroupByPlan, _groupKeyBinding);
        }
    }
} // end of namespace