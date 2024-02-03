///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedEventsBetweenEval : AggregationMultiFunctionAggregationMethod
    {
        private readonly ExprEvaluator fromKeyEval;
        private readonly ExprEvaluator fromInclusiveEval;
        private readonly ExprEvaluator toKeyEval;
        private readonly ExprEvaluator toInclusiveEval;
        private readonly Func<IDictionary<object, object>, object> value;
        private readonly Func<IDictionary<object, object>, ICollection<EventBean>> events;

        public AggregationMethodSortedEventsBetweenEval(
            ExprEvaluator fromKeyEval,
            ExprEvaluator fromInclusiveEval,
            ExprEvaluator toKeyEval,
            ExprEvaluator toInclusiveEval,
            Func<IDictionary<object, object>, object> value,
            Func<IDictionary<object, object>, ICollection<EventBean>> events)
        {
            this.fromKeyEval = fromKeyEval;
            this.fromInclusiveEval = fromInclusiveEval;
            this.toKeyEval = toKeyEval;
            this.toInclusiveEval = toInclusiveEval;
            this.value = value;
            this.events = events;
        }

        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var submap = GetSubmap(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
            if (submap == null) {
                return null;
            }

            return value.Invoke(submap);
        }

        public ICollection<EventBean> GetValueCollectionEvents(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var submap = GetSubmap(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
            if (submap == null) {
                return null;
            }

            return events.Invoke(submap);
        }

        public ICollection<object> GetValueCollectionScalar(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetValueEventBean(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        private IDictionary<object, object> GetSubmap(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var sorted = (AggregationStateSorted)row.GetAccessState(aggColNum);
            var fromKey = fromKeyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (fromKey == null) {
                return null;
            }

            var fromInclusive = fromInclusiveEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                .AsBoxedBoolean();
            if (fromInclusive == null) {
                return null;
            }

            var toKey = toKeyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (toKey == null) {
                return null;
            }

            var toInclusive = toInclusiveEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                .AsBoxedBoolean();
            if (toInclusive == null) {
                return null;
            }


            return sorted.Sorted.Between(fromKey, fromInclusive.Value, toKey, toInclusive.Value);
        }
    }
} // end of namespace