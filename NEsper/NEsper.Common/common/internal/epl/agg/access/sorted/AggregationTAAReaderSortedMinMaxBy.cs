///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationTAAReaderSortedMinMaxBy : AggregationMultiFunctionTableReader
    {
        private bool max;

        public bool Max {
            get => max;
            set => max = value;
        }

        public AggregationTAAReaderSortedMinMaxBy SetMax(bool max)
        {
            this.max = max;
            return this;
        }

        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationStateSorted sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
            EventBean @event = GetEnumerableEvent(sorted, eventsPerStream, isNewData, exprEvaluatorContext);
            if (@event == null) {
                return null;
            }

            return @event.Underlying;
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
            AggregationStateSorted sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
            return GetEnumerableEvent(sorted, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private EventBean GetEnumerableEvent(
            AggregationStateSorted state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (max) {
                return state.LastValue;
            }
            else {
                return state.FirstValue;
            }
        }
    }
} // end of namespace