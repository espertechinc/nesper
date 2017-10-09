///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "first" aggregation function without index.
    /// </summary>
    public class AggregationAccessorFirstWEval : AggregationAccessor
    {
        private readonly int _streamNum;
        private readonly ExprEvaluator _childNode;
        private readonly EventBean[] _eventsPerStream;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        public AggregationAccessorFirstWEval(int streamNum, ExprEvaluator childNode)
        {
            _streamNum = streamNum;
            _childNode = childNode;
            _eventsPerStream = new EventBean[streamNum + 1];
        }

        public object GetValue(AggregationState state, EvaluateParams evalParams)
        {
            EventBean bean = ((AggregationStateLinear) state).FirstValue;
            if (bean == null) {
                return null;
            }
            _eventsPerStream[_streamNum] = bean;
            return _childNode.Evaluate(new EvaluateParams(_eventsPerStream, true, null));
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams) {
            EventBean bean = ((AggregationStateLinear) state).FirstValue;
            if (bean == null) {
                return null;
            }
            return Collections.SingletonList(bean);
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams) {
            object value = GetValue(state, evalParams);
            if (value == null) {
                return null;
            }
            return Collections.SingletonList(value);
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EvaluateParams evalParams) {
            return ((AggregationStateLinear) state).FirstValue;
        }
    }
}
