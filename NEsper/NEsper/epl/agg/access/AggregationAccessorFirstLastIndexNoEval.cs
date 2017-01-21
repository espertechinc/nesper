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
    public class AggregationAccessorFirstLastIndexNoEval : AggregationAccessor
    {
        private readonly ExprEvaluator indexNode;
        private readonly int constant;
        private readonly bool isFirst;
    
        public AggregationAccessorFirstLastIndexNoEval(ExprEvaluator indexNode, int constant, bool first) {
            this.indexNode = indexNode;
            this.constant = constant;
            isFirst = first;
        }
    
        public object GetValue(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            EventBean bean = GetBean(state);
            if (bean == null) {
                return null;
            }
            return bean.Underlying;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            EventBean bean = GetBean(state);
            if (bean == null) {
                return null;
            }
            return Collections.SingletonList(bean);
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            object value = GetValue(state, eventsPerStream, isNewData, exprEvaluatorContext);
            if (value == null) {
                return null;
            }
            return Collections.SingletonList(value);
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return GetBean(state);
        }
    
        private EventBean GetBean(AggregationState state) {
            EventBean bean;
            int index = constant;
            if (index == -1) {
                object result = indexNode.Evaluate(new EvaluateParams(null, true, null));
                if ((result == null) || (!(result is int))) {
                    return null;
                }
                index = result.AsInt();
            }
            if (isFirst) {
                bean = ((AggregationStateLinear) state).GetFirstNthValue(index);
            }
            else {
                bean = ((AggregationStateLinear) state).GetLastNthValue(index);
            }
            return bean;
        }
    }
}
