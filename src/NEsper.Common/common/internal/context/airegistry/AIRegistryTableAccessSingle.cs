///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryTableAccessSingle : AIRegistryTableAccess
    {
        private ExprTableEvalStrategy service;

        public void AssignService(
            int num,
            ExprTableEvalStrategy subselectStrategy)
        {
            service = subselectStrategy;
        }

        public void DeassignService(int num)
        {
            service = null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return service.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }

        public AggregationRow GetAggregationRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return service.GetAggregationRow(eventsPerStream, isNewData, context);
        }

        public int InstanceCount => service == null ? 0 : 1;
    }
} // end of namespace