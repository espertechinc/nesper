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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public abstract class AggregationAccessorMinMaxByBase : AggregationAccessor
    {
        private readonly bool _max;

        protected AggregationAccessorMinMaxByBase(bool max)
        {
            _max = max;
        }

        public abstract object GetValue(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public ICollection<EventBean> GetEnumerableEvents(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            EventBean bean = GetEnumerableEvent(state, eventsPerStream, isNewData, context);
            if (bean == null)
            {
                return null;
            }
            return Collections.SingletonList(bean);
        }

        public ICollection<object> GetEnumerableScalar(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetEnumerableEvent(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_max)
            {
                return ((AggregationStateSorted) state).LastValue;
            }
            else
            {
                return ((AggregationStateSorted) state).FirstValue;
            }
        }
    }
}
