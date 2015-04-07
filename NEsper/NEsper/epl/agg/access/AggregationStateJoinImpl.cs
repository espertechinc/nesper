///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for joins.
    /// </summary>
    public class AggregationStateJoinImpl 
        : AggregationStateWithSize
        , AggregationStateLinear
    {
        protected int StreamId;
        protected LinkedHashMap<EventBean, int?> RefSet = new LinkedHashMap<EventBean, int?>();
        private EventBean[] _array;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamId">stream id</param>
        public AggregationStateJoinImpl(int streamId)
        {
            StreamId = streamId;
        }
    
        public void Clear() {
            RefSet.Clear();
            _array = null;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) {
                return;
            }
            _array = null;
            var value = RefSet.Get(theEvent);
            if (value == null)
            {
                RefSet.Put(theEvent, 1);
                return;
            }
    
            value++;
            RefSet.Put(theEvent, value);
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) {
                return;
            }
            _array = null;
    
            var value = RefSet.Get(theEvent);
            if (value == null)
            {
                return;
            }
    
            if (value == 1)
            {
                RefSet.Remove(theEvent);
                return;
            }
    
            value--;
            RefSet.Put(theEvent, value);
        }
    
        public EventBean GetFirstNthValue(int index) {
            if (index < 0) {
                return null;
            }
            if (RefSet.IsEmpty()) {
                return null;
            }
            if (index >= RefSet.Count) {
                return null;
            }
            if (_array == null) {
                InitArray();
            }
            return _array[index];
        }
    
        public EventBean GetLastNthValue(int index) {
            if (index < 0) {
                return null;
            }
            if (RefSet.IsEmpty()) {
                return null;
            }
            if (index >= RefSet.Count) {
                return null;
            }
            if (_array == null) {
                InitArray();
            }
            return _array[_array.Length - index - 1];
        }

        public EventBean FirstValue
        {
            get
            {
                if (RefSet.IsEmpty())
                {
                    return null;
                }
                return RefSet.First().Key;
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (RefSet.IsEmpty())
                {
                    return null;
                }
                if (_array == null)
                {
                    InitArray();
                }
                return _array[_array.Length - 1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (_array == null) {
                InitArray();
            }
            return _array.Cast<EventBean>().GetEnumerator();
        }

        public ICollection<EventBean> CollectionReadOnly
        {
            get
            {
                if (_array == null)
                {
                    InitArray();
                }
                return _array;
            }
        }

        public int Count
        {
            get { return RefSet.Count; }
        }

        private void InitArray()
        {
            var events = RefSet.Keys;
            _array = events.ToArray();
        }
    }
}
