///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.regression.client
{
    public class SupportAggMFAccessorSingleEvent : AggregationAccessor
    {
        public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = GetEnumerableEvent(state, null, true, null);
            if (@event == null) {
                return null;
            }
            return @event.Underlying;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerSteam, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return ((SupportAggMFStateSingleEvent) state).Event;
        }

        public ICollection<Object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}
