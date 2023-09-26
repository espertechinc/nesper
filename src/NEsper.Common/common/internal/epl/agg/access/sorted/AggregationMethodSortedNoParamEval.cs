///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedNoParamEval : AggregationMultiFunctionAggregationMethod
    {
        private readonly Func<AggregationStateSorted, object> _value;
        private readonly Func<AggregationStateSorted, EventBean> _event;
        private readonly Func<AggregationStateSorted, ICollection<EventBean>> _events;

        public AggregationMethodSortedNoParamEval(
            Func<AggregationStateSorted, object> value,
            Func<AggregationStateSorted, EventBean> @event,
            Func<AggregationStateSorted, ICollection<EventBean>> events)
        {
            _value = value;
            _event = @event;
            _events = events;
        }

        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var sorted = (AggregationStateSorted)row.GetAccessState(aggColNum);
            return _value.Invoke(sorted);
        }

        public ICollection<EventBean> GetValueCollectionEvents(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var sorted = (AggregationStateSorted)row.GetAccessState(aggColNum);
            return _events.Invoke(sorted);
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
            var sorted = (AggregationStateSorted)row.GetAccessState(aggColNum);
            return _event.Invoke(sorted);
        }
    }
} // end of namespace