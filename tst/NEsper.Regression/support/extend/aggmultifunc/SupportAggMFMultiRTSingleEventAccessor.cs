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
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTSingleEventAccessor : AggregationMultiFunctionAccessor
    {
        public object GetValue(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = GetEnumerableEvent(state, null, true, null);
            if (@event == null) {
                return null;
            }

            return @event.Underlying;
        }

        public ICollection<EventBean> GetEnumerableEvents(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetEnumerableEvent(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ((SupportAggMFMultiRTSingleEventState) state).Event;
        }

        public ICollection<object> GetEnumerableScalar(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
} // end of namespace