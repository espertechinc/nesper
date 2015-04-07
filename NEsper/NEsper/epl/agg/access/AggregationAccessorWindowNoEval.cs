///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowNoEval : AggregationAccessor
    {
        private readonly Type _componentType;
    
        public AggregationAccessorWindowNoEval(Type componentType) {
            this._componentType = componentType;
        }
    
        public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = ((AggregationStateLinear) state);
            if (linear.Count == 0) {
                return null;
            }
            var count = 0;
            var array = Array.CreateInstance(_componentType, linear.Count);
            foreach(var bean in linear) {
                array.SetValue(bean.Underlying, count++);
            }
            return array;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = ((AggregationStateLinear) state);
            if (linear.Count == 0) {
                return null;
            }
            return linear.CollectionReadOnly;
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = ((AggregationStateLinear) state);
            if (linear.Count == 0) {
                return null;
            }
            var values = new List<object>(linear.Count);
            foreach (var bean in linear) {
                values.Add(bean.Underlying);
            }
            return values;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    }
}
