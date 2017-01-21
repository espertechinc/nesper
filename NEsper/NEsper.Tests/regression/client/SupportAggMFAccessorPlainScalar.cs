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
    public class SupportAggMFAccessorPlainScalar : AggregationAccessor
    {
        public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return ((SupportAggMFStatePlainScalar) state).LastValue;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerSteam, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}
