///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationMethodLinearFirstLast : AggregationMultiFunctionAggregationMethod
    {
        public AggregationAccessorLinearType AccessType { get; set; }

        public ExprEvaluator OptionalEvaluator { get; set; }

        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (IList<EventBean>)row.GetCollectionOfEvents(
                aggColNum,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            EventBean target;
            if (AccessType == AggregationAccessorLinearType.FIRST) {
                target = events[0];
            }
            else {
                target = events[^1];
            }

            if (OptionalEvaluator == null) {
                return target.Underlying;
            }

            EventBean[] eventsPerStreamBuf = { target };
            return OptionalEvaluator.Evaluate(eventsPerStreamBuf, isNewData, exprEvaluatorContext);
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

        public ICollection<EventBean> GetValueCollectionEvents(
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
            var events = (IList<EventBean>)row.GetCollectionOfEvents(
                aggColNum,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            if (AccessType == AggregationAccessorLinearType.FIRST) {
                return events[0];
            }

            return events[^1];
        }
    }
} // end of namespace