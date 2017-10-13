///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "first" and "last" aggregation function with index.
    /// </summary>
    public class AggregationAccessorFirstLastIndexWEval : AggregationAccessor
    {
        private readonly int _streamNum;
        private readonly ExprEvaluator _childNode;
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluator _indexNode;
        private readonly int _constant;
        private readonly bool _isFirst;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        /// <param name="indexNode">index expression</param>
        /// <param name="constant">constant index</param>
        /// <param name="isFirst">true if returning first, false for returning last</param>
        public AggregationAccessorFirstLastIndexWEval(int streamNum, ExprEvaluator childNode, ExprEvaluator indexNode, int constant, bool isFirst)
        {
            _streamNum = streamNum;
            _childNode = childNode;
            _indexNode = indexNode;
            _eventsPerStream = new EventBean[streamNum + 1];
            _constant = constant;
            _isFirst = isFirst;
        }

        public object GetValue(AggregationState state, EvaluateParams evalParams)
        {
            var bean = GetBean(state);
            if (bean == null) {
                return null;
            }
            _eventsPerStream[_streamNum] = bean;
            return _childNode.Evaluate(new EvaluateParams(_eventsPerStream, true, null));
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams) {
            var bean = GetBean(state);
            if (bean == null) {
                return null;
            }
            return Collections.SingletonList(bean);
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams) {
            var value = GetValue(state, evalParams);
            if (value == null) {
                return null;
            }
            return Collections.SingletonList(value);
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EvaluateParams evalParams) {
            return GetBean(state);
        }
    
        private EventBean GetBean(AggregationState state) {
            EventBean bean;
            var index = _constant;
            if (index == -1) {
                var result = _indexNode.Evaluate(new EvaluateParams(null, true, null));
                if ((result == null) || (!(result is int))) {
                    return null;
                }
                index = result.AsInt();
            }
            if (_isFirst) {
                bean = ((AggregationStateLinear) state).GetFirstNthValue(index);
            }
            else {
                bean = ((AggregationStateLinear) state).GetLastNthValue(index);
            }
            return bean;
        }
    }
}
