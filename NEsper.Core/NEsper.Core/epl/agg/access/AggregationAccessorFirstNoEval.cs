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
    /// Represents the aggregation accessor that provides the result for the "first" aggregation function without index.
    /// </summary>
    public class AggregationAccessorFirstNoEval : AggregationAccessor {
        public readonly static AggregationAccessorFirstNoEval INSTANCE = new AggregationAccessorFirstNoEval();

        public object GetValue(AggregationState state, EvaluateParams evalParams)
        {
            EventBean bean = ((AggregationStateLinear) state).FirstValue;
            if (bean == null) {
                return null;
            }
            return bean.Underlying;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams) {
            EventBean bean = ((AggregationStateLinear) state).FirstValue;
            if (bean == null) {
                return null;
            }
            return Collections.SingletonList(bean);
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams)
        {
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
