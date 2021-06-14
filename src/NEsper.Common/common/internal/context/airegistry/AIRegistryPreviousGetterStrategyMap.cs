///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPreviousGetterStrategyMap : AIRegistryPreviousGetterStrategy
    {
        private readonly IDictionary<int, PreviousGetterStrategy> services;

        protected internal AIRegistryPreviousGetterStrategyMap()
        {
            services = new Dictionary<int, PreviousGetterStrategy>();
        }

        public void AssignService(
            int serviceId,
            PreviousGetterStrategy previousGetterStrategy)
        {
            services.Put(serviceId, previousGetterStrategy);
        }

        public void DeassignService(int serviceId)
        {
            services.Remove(serviceId);
        }

        public int InstanceCount => services.Count;

        public PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx)
        {
            return services.Get(ctx.AgentInstanceId).GetStrategy(ctx);
        }
    }
} // end of namespace