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
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowWEval : AggregationAccessor
    {
        private readonly int _streamNum;
        private readonly ExprEvaluator _childNode;
        private readonly EventBean[] _eventsPerStream;
        private readonly Type _componentType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        /// <param name="componentType">type</param>
        public AggregationAccessorWindowWEval(int streamNum, ExprEvaluator childNode, Type componentType)
        {
            _streamNum = streamNum;
            _childNode = childNode;
            _eventsPerStream = new EventBean[streamNum + 1];
            _componentType = componentType;
        }
    
        public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = ((AggregationStateLinear) state);
            if (linear.Count == 0) {
                return null;
            }
            var array = Array.CreateInstance(_componentType, linear.Count);
            var count = 0;
            foreach(var bean in linear)
            {
                _eventsPerStream[_streamNum] = bean;
                var value = _childNode.Evaluate(new EvaluateParams(_eventsPerStream, true, null));
                array.SetValue(value, count++);
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
            foreach(EventBean bean in linear)
            {
                _eventsPerStream[_streamNum] = bean;
                object value = _childNode.Evaluate(new EvaluateParams(_eventsPerStream, true, null));
                values.Add(value);
            }
    
            return values;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}
