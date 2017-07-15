///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowNoEval : AggregationAccessor {
        private readonly Type componentType;
    
        public AggregationAccessorWindowNoEval(Type componentType) {
            this.componentType = componentType;
        }
    
        public Object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            AggregationStateLinear linear = (AggregationStateLinear) state;
            if (linear.Count == 0) {
                return null;
            }
            Object array = Array.NewInstance(componentType, linear.Count);
            IEnumerator<EventBean> it = linear.GetEnumerator();
            int count = 0;
            for (; it.HasNext(); ) {
                EventBean bean = it.Next();
                Array.Set(array, count++, bean.Underlying);
            }
            return array;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            AggregationStateLinear linear = (AggregationStateLinear) state;
            if (linear.Count == 0) {
                return null;
            }
            return Linear.CollectionReadOnly();
        }
    
        public ICollection<Object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            AggregationStateLinear linear = (AggregationStateLinear) state;
            if (linear.Count == 0) {
                return null;
            }
            var values = new List<Object>(linear.Count);
            IEnumerator<EventBean> it = linear.GetEnumerator();
            for (; it.HasNext(); ) {
                EventBean bean = it.Next();
                values.Add(bean.Underlying);
            }
            return values;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    }
} // end of namespace
