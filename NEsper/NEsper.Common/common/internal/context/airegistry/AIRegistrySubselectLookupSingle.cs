///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistrySubselectLookupSingle : AIRegistrySubselectLookup
    {
        private SubordTableLookupStrategy service;

        public AIRegistrySubselectLookupSingle(LookupStrategyDesc strategyDesc)
        {
            StrategyDesc = strategyDesc;
        }

        public void AssignService(int num, SubordTableLookupStrategy subselectStrategy)
        {
            service = subselectStrategy;
        }

        public void DeassignService(int num)
        {
            service = null;
        }

        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            return service.Lookup(events, context);
        }

        public int InstanceCount => service == null ? 0 : 1;

        public string ToQueryPlan()
        {
            return StrategyDesc.LookupStrategy.GetName();
        }

        public LookupStrategyDesc StrategyDesc { get; }
    }
} // end of namespace