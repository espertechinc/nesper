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
    public class AggregationAccessorFirstLastIndexNoEval : AggregationAccessor
    {
        private readonly ExprEvaluator _indexNode;
        private readonly int _constant;
        private readonly bool _isFirst;
    
        public AggregationAccessorFirstLastIndexNoEval(ExprEvaluator indexNode, int constant, bool first)
        {
            _indexNode = indexNode;
            _constant = constant;
            _isFirst = first;
        }

        public object GetValue(AggregationState state, EvaluateParams evalParams)
        {
            EventBean bean = GetBean(state);
            if (bean == null) {
                return null;
            }
            return bean.Underlying;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams)
        {
            EventBean bean = GetBean(state);
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
            return GetBean(state);
        }
    
        private EventBean GetBean(AggregationState state) {
            EventBean bean;
            int index = _constant;
            if (index == -1) {
                object result = _indexNode.Evaluate(new EvaluateParams(null, true, null));
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
