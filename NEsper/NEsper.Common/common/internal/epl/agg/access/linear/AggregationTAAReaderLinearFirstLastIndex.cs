///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearFirstLastIndex : AggregationMultiFunctionTableReader
    {
        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            var target = GetBean(events);
            if (target == null) {
                return null;
            }

            return target.Underlying;
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

        public AggregationAccessorLinearType AccessType { get; set; }

        public int? OptionalConstIndex { get; set; }

        public ExprEvaluator OptionalIndexEval { get; set; }

        private EventBean GetBean(IList<EventBean> events)
        {
            int index;
            if (OptionalConstIndex != null) {
                index = OptionalConstIndex.Value;
            }
            else {
                var result = OptionalIndexEval.Evaluate(null, true, null);
                if (result == null || !result.IsInt()) {
                    return null;
                }

                index = result.AsInt32();
            }

            if (index < 0) {
                return null;
            }

            if (index >= events.Count) {
                return null;
            }

            if (AccessType == AggregationAccessorLinearType.FIRST) {
                return events[index];
            }

            return events[events.Count - index - 1];
        }
    }
} // end of namespace