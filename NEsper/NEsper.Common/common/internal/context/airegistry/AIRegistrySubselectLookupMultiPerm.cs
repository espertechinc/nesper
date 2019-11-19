///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistrySubselectLookupMultiPerm : AIRegistrySubselectLookup
    {
        private readonly ArrayWrap<SubordTableLookupStrategy> strategies;

        public AIRegistrySubselectLookupMultiPerm(LookupStrategyDesc strategyDesc)
        {
            StrategyDesc = strategyDesc;
            strategies = new ArrayWrap<SubordTableLookupStrategy>(10);
        }

        public void AssignService(
            int num,
            SubordTableLookupStrategy subselectStrategy)
        {
            AIRegistryUtil.CheckExpand(num, strategies);
            strategies.Array[num] = subselectStrategy;
            InstanceCount++;
        }

        public void DeassignService(int num)
        {
            strategies.Array[num] = null;
            InstanceCount--;
        }

        public ICollection<EventBean> Lookup(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            return strategies.Array[context.AgentInstanceId].Lookup(events, context);
        }

        public int InstanceCount { get; private set; }

        public string ToQueryPlan()
        {
            return StrategyDesc.LookupStrategy.GetName();
        }

        public LookupStrategyDesc StrategyDesc { get; }
    }
} // end of namespace