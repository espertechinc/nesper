///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPreviousGetterStrategyMultiPerm : AIRegistryPreviousGetterStrategy
    {
        private readonly ArrayWrap<PreviousGetterStrategy> services;

        protected internal AIRegistryPreviousGetterStrategyMultiPerm()
        {
            services = new ArrayWrap<PreviousGetterStrategy>(2);
        }

        public void AssignService(
            int serviceId,
            PreviousGetterStrategy previousGetterStrategy)
        {
            AIRegistryUtil.CheckExpand(serviceId, services);
            services.Array[serviceId] = previousGetterStrategy;
            InstanceCount++;
        }

        public void DeassignService(int serviceId)
        {
            if (serviceId >= services.Array.Length) {
                // possible since it may not have been assigned as there was nothing to assign
                return;
            }

            services.Array[serviceId] = null;
            InstanceCount--;
        }

        public int InstanceCount { get; private set; }

        public PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx)
        {
            return services.Array[ctx.AgentInstanceId].GetStrategy(ctx);
        }
    }
} // end of namespace