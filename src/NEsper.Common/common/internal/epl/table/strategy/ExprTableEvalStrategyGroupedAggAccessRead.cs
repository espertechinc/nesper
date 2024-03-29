///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupedAggAccessRead : ExprTableEvalStrategyGroupedBase
    {
        public ExprTableEvalStrategyGroupedAggAccessRead(
            TableAndLockProviderGrouped provider,
            ExprTableEvalStrategyFactory factory)
            : base(provider, factory)
        {
        }

        public override object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggs = GetAggregationRow(eventsPerStream, isNewData, exprEvaluatorContext);
            if (aggs == null) {
                return null;
            }

            return Factory.AggregationMethod.GetValue(
                Factory.AggColumnNum,
                aggs,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
        }

        public override ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var aggs = GetAggregationRow(eventsPerStream, isNewData, context);
            if (aggs == null) {
                return null;
            }

            return Factory.AggregationMethod.GetValueCollectionEvents(
                Factory.AggColumnNum,
                aggs,
                eventsPerStream,
                isNewData,
                context);
        }

        public override EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var aggs = GetAggregationRow(eventsPerStream, isNewData, context);
            if (aggs == null) {
                return null;
            }

            return Factory.AggregationMethod.GetValueEventBean(
                Factory.AggColumnNum,
                aggs,
                eventsPerStream,
                isNewData,
                context);
        }

        public override ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var aggs = GetAggregationRow(eventsPerStream, isNewData, context);
            if (aggs == null) {
                return null;
            }

            return Factory.AggregationMethod.GetValueCollectionScalar(
                Factory.AggColumnNum,
                aggs,
                eventsPerStream,
                isNewData,
                context);
        }

        public override object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace