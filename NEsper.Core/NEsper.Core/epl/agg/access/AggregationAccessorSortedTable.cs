///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorSortedTable : AggregationAccessor
    {
        private readonly bool _max;
        private readonly Type _componentType;
        private readonly TableMetadata _tableMetadata;
    
        public AggregationAccessorSortedTable(bool max, Type componentType, TableMetadata tableMetadata) {
            _max = max;
            _componentType = componentType;
            _tableMetadata = tableMetadata;
        }

        public object GetValue(AggregationState state, EvaluateParams evalParams)
        {
            var sorted = (AggregationStateSorted) state;
            if (sorted.Count == 0) {
                return null;
            }
            var array = Array.CreateInstance(_componentType, sorted.Count);
    
            IEnumerator<EventBean> it;
            if (_max) {
                it = sorted.Reverse().GetEnumerator();
            }
            else {
                it = sorted.GetEnumerator();
            }
    
            int count = 0;
            while(it.MoveNext())
            {
                EventBean bean = it.Current;
                object und = _tableMetadata.EventToPublic.ConvertToUnd(bean, evalParams);
                array.SetValue(und, count++);
            }
            return array;
        }
    
        public ICollection<EventBean> GetEnumerableEvents(AggregationState state, EvaluateParams evalParams) {
            return ((AggregationStateSorted) state).CollectionReadOnly();
        }
    
        public ICollection<object> GetEnumerableScalar(AggregationState state, EvaluateParams evalParams) {
            return null;
        }
    
        public EventBean GetEnumerableEvent(AggregationState state, EvaluateParams evalParams) {
            return null;
        }
    }
}
