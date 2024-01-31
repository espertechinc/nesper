///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryTableAccessMultiPerm : AIRegistryTableAccess
    {
        private readonly ArrayWrap<ExprTableEvalStrategy> strategies;

        protected internal AIRegistryTableAccessMultiPerm()
        {
            strategies = new ArrayWrap<ExprTableEvalStrategy>(8);
        }

        public void AssignService(
            int num,
            ExprTableEvalStrategy subselectStrategy)
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

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return strategies.Array[exprEvaluatorContext.AgentInstanceId]
                .Evaluate(
                    eventsPerStream,
                    isNewData,
                    exprEvaluatorContext);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return strategies.Array[context.AgentInstanceId]
                .EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return strategies.Array[context.AgentInstanceId].EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return strategies.Array[context.AgentInstanceId]
                .EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return strategies.Array[context.AgentInstanceId].EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }

        public AggregationRow GetAggregationRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return strategies
                .Array[context.AgentInstanceId]
                .GetAggregationRow(eventsPerStream, isNewData, context);
        }

        public int InstanceCount { get; private set; }
    }
} // end of namespace