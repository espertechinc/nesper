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
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPriorEvalStrategyMap : AIRegistryPriorEvalStrategy
    {
        private readonly IDictionary<int, PriorEvalStrategy> services;

        protected internal AIRegistryPriorEvalStrategyMap()
        {
            services = new Dictionary<int, PriorEvalStrategy>();
        }

        public void AssignService(int serviceId, PriorEvalStrategy priorEvalStrategy)
        {
            services.Put(serviceId, priorEvalStrategy);
        }

        public void DeassignService(int serviceId)
        {
            services.Remove(serviceId);
        }

        public int InstanceCount => services.Count;

        public EventBean GetSubstituteEvent(
            EventBean originalEvent, bool isNewData, int constantIndexNumber, int relativeIndex,
            ExprEvaluatorContext exprEvaluatorContext, int streamNum)
        {
            return services.Get(exprEvaluatorContext.AgentInstanceId).GetSubstituteEvent(
                originalEvent, isNewData, constantIndexNumber, relativeIndex, exprEvaluatorContext, streamNum);
        }
    }
} // end of namespace