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

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearFirstLastIndex : AggregationMultiFunctionTableReader
    {
        private AggregationAccessorLinearType accessType;
        private int? optionalConstIndex;
        private ExprEvaluator optionalIndexEval;

        public void SetAccessType(AggregationAccessorLinearType accessType)
        {
            this.accessType = accessType;
        }

        public void SetOptionalConstIndex(int? optionalConstIndex)
        {
            this.optionalConstIndex = optionalConstIndex;
        }

        public void SetOptionalIndexEval(ExprEvaluator optionalIndexEval)
        {
            this.optionalIndexEval = optionalIndexEval;
        }

        public object GetValue(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum, eventsPerStream, isNewData, exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            var target = GetBean(events);
            if (target == null) {
                return null;
            }

            return target.Underlying;
        }

        public ICollection<object> GetValueCollectionEvents(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<object> GetValueCollectionScalar(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetValueEventBean(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        private EventBean GetBean(IList<EventBean> events)
        {
            int index;
            if (optionalConstIndex != null) {
                index = optionalConstIndex;
            }
            else {
                object result = optionalIndexEval.Evaluate(null, true, null);
                if (result == null || !(result is int?)) {
                    return null;
                }

                index = (int?) result;
            }

            if (index < 0) {
                return null;
            }

            if (index >= events.Count) {
                return null;
            }

            if (accessType == AggregationAccessorLinearType.FIRST) {
                return events.Get(index);
            }

            return events.Get(events.Count - index - 1);
        }
    }
} // end of namespace