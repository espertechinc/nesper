///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorSortedNonTable : AggregationAccessor
    {
        private readonly bool _max;
        private readonly Type _componentType;

        public AggregationAccessorSortedNonTable(bool max, Type componentType)
        {
            _max = max;
            _componentType = componentType;
        }

        public object GetValue(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var sorted = (AggregationStateSorted) state;
            if (sorted.Count == 0)
            {
                return null;
            }
            var array = Array.CreateInstance(_componentType, sorted.Count);

            IEnumerator<EventBean> it;
            if (_max)
            {
                it = sorted.Reverse().GetEnumerator();
            }
            else
            {
                it = sorted.GetEnumerator();
            }

            int count = 0;
            while (it.MoveNext())
            {
                array.SetValue(it.Current.Underlying, count++);
            }
            return array;
        }

        public ICollection<EventBean> GetEnumerableEvents(
            AggregationState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ((AggregationStateSorted) state).CollectionReadOnly();
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
            return null;
        }
    }
}
