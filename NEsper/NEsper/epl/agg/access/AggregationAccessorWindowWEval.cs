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
    public class AggregationAccessorWindowWEval : AggregationAccessor {
        private readonly int streamNum;
        private readonly ExprEvaluator childNode;
        private readonly EventBean[] eventsPerStream;
        private readonly Type componentType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        /// <param name="componentType">type</param>
        public AggregationAccessorWindowWEval(int streamNum, ExprEvaluator childNode, Type componentType) {
            this.streamNum = streamNum;
            this.childNode = childNode;
            this.eventsPerStream = new EventBean[streamNum + 1];
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
                this.eventsPerStream[streamNum] = bean;
                Object value = childNode.Evaluate(this.eventsPerStream, true, null);
                Array.Set(array, count++, value);
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
                this.eventsPerStream[streamNum] = bean;
                Object value = childNode.Evaluate(this.eventsPerStream, true, null);
                values.Add(value);
            }
    
            return values;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    }
} // end of namespace
