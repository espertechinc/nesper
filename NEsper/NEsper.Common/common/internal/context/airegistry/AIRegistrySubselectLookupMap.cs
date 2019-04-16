///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistrySubselectLookupMap : AIRegistrySubselectLookup
    {
        private readonly IDictionary<int, SubordTableLookupStrategy> services;

        public AIRegistrySubselectLookupMap(LookupStrategyDesc strategyDesc)
        {
            StrategyDesc = strategyDesc;
            services = new Dictionary<int, SubordTableLookupStrategy>();
        }

        public void AssignService(
            int num,
            SubordTableLookupStrategy subselectStrategy)
        {
            services.Put(num, subselectStrategy);
        }

        public void DeassignService(int num)
        {
            services.Remove(num);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            return services.Get(context.AgentInstanceId).Lookup(events, context);
        }

        public int InstanceCount => services.Count;

        public string ToQueryPlan()
        {
            return StrategyDesc.LookupStrategy.GetName();
        }

        public LookupStrategyDesc StrategyDesc { get; }
    }
} // end of namespace