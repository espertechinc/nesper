///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryRowRecogPreviousStrategyMultiPerm : RowRecogPreviousStrategy,
        AIRegistryRowRecogPreviousStrategy
    {
        private readonly ArrayWrap<RowRecogPreviousStrategy> services;

        protected internal AIRegistryRowRecogPreviousStrategyMultiPerm()
        {
            services = new ArrayWrap<RowRecogPreviousStrategy>(2);
        }

        public void AssignService(
            int serviceId,
            RowRecogPreviousStrategy service)
        {
            AIRegistryUtil.CheckExpand(serviceId, services);
            services.Array[serviceId] = service;
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

        public RowRecogStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            return services.Array[exprEvaluatorContext.AgentInstanceId].GetAccess(exprEvaluatorContext);
        }
    }
} // end of namespace