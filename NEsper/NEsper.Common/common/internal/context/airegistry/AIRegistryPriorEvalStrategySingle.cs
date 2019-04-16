///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prior;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryPriorEvalStrategySingle : AIRegistryPriorEvalStrategy
    {
        private PriorEvalStrategy service;

        public void AssignService(
            int serviceId,
            PriorEvalStrategy priorEvalStrategy)
        {
            service = priorEvalStrategy;
        }

        public void DeassignService(int serviceId)
        {
            service = null;
        }

        public int InstanceCount => service == null ? 0 : 1;

        public EventBean GetSubstituteEvent(
            EventBean originalEvent,
            bool isNewData,
            int constantIndexNumber,
            int relativeIndex,
            ExprEvaluatorContext exprEvaluatorContext,
            int streamNum)
        {
            return service.GetSubstituteEvent(
                originalEvent, isNewData, constantIndexNumber, relativeIndex, exprEvaluatorContext, streamNum);
        }
    }
} // end of namespace