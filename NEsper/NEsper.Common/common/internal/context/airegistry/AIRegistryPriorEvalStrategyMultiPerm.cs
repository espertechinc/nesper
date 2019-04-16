///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prior;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPriorEvalStrategyMultiPerm : AIRegistryPriorEvalStrategy
    {
        private readonly ArrayWrap<PriorEvalStrategy> services;

        protected internal AIRegistryPriorEvalStrategyMultiPerm()
        {
            services = new ArrayWrap<PriorEvalStrategy>(2);
        }

        public void AssignService(
            int serviceId,
            PriorEvalStrategy priorEvalStrategy)
        {
            AIRegistryUtil.CheckExpand(serviceId, services);
            services.Array[serviceId] = priorEvalStrategy;
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

        public EventBean GetSubstituteEvent(
            EventBean originalEvent,
            bool isNewData,
            int constantIndexNumber,
            int relativeIndex,
            ExprEvaluatorContext exprEvaluatorContext,
            int streamNum)
        {
            return services.Array[exprEvaluatorContext.AgentInstanceId].GetSubstituteEvent(
                originalEvent, isNewData, constantIndexNumber, relativeIndex, exprEvaluatorContext, streamNum);
        }
    }
} // end of namespace