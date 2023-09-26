///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class AggregationMethodLinearFirstLastIndex : AggregationMultiFunctionAggregationMethod
    {
        private AggregationAccessorLinearType accessType;
        private int? optionalConstIndex;
        private ExprEvaluator optionalIndexEval;

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

            var target = GetBean(events);
            return target?.Underlying;
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

        public AggregationAccessorLinearType AccessType {
            get => accessType;
            set => accessType = value;
        }

        public int? OptionalConstIndex {
            get => optionalConstIndex;
            set => optionalConstIndex = value;
        }

        public ExprEvaluator OptionalIndexEval {
            get => optionalIndexEval;
            set => optionalIndexEval = value;
        }

        private EventBean GetBean(IList<EventBean> events)
        {
            int index;

            if (optionalConstIndex != null) {
                index = optionalConstIndex.Value;
            }
            else {
                var result = optionalIndexEval.Evaluate(null, true, null);
                if (result == null || !(result is int intResult)) {
                    return null;
                }

                index = intResult;
            }

            if (index < 0) {
                return null;
            }

            if (index >= events.Count) {
                return null;
            }

            if (accessType == AggregationAccessorLinearType.FIRST) {
                return events[index];
            }

            return events[events.Count - index - 1];
        }
    }
} // end of namespace