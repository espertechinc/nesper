///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>Implementation of access function for joins.</summary>
    public class AggregationStateLinearJoinImpl : AggregationStateWithSize, AggregationStateLinear
    {
        private readonly int _streamId;
        private EventBean[] _array;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamId">stream id</param>
        public AggregationStateLinearJoinImpl(int streamId)
        {
            _streamId = streamId;
        }

        public int StreamId => _streamId;

        public EventBean[] Array => _array;

        public IDictionary<EventBean, int> RefSet { get; } = new LinkedHashMap<EventBean, int>();

        public EventBean GetFirstNthValue(int index)
        {
            if (index < 0) return null;
            if (RefSet.IsEmpty()) return null;
            if (index >= RefSet.Count) return null;
            if (_array == null) InitArray();
            return _array[index];
        }

        public EventBean GetLastNthValue(int index)
        {
            if (index < 0) return null;
            if (RefSet.IsEmpty()) return null;
            if (index >= RefSet.Count) return null;
            if (_array == null) InitArray();
            return _array[_array.Length - index - 1];
        }

        public EventBean FirstValue
        {
            get
            {
                if (RefSet.IsEmpty()) return null;

                return RefSet.First().Key;
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (RefSet.IsEmpty()) return null;

                if (_array == null) InitArray();

                return _array[_array.Length - 1];
            }
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (_array == null) InitArray();

            return ((IEnumerable<EventBean>) _array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ICollection<EventBean> CollectionReadOnly
        {
            get
            {
                if (_array == null) InitArray();

                return _array.AsList();
            }
        }

        public void Clear()
        {
            RefSet.Clear();
            _array = null;
        }

        public virtual void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[_streamId];
            if (theEvent == null) return;
            AddEvent(theEvent);
        }

        public virtual void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[_streamId];
            if (theEvent == null) return;
            RemoveEvent(theEvent);
        }

        public int Count => RefSet.Count;

        protected void AddEvent(EventBean theEvent)
        {
            _array = null;

            if (!RefSet.TryGetValue(theEvent, out var value))
            {
                RefSet.Put(theEvent, 1);
                return;
            }

            value++;
            RefSet.Put(theEvent, value);
        }

        protected void RemoveEvent(EventBean theEvent)
        {
            _array = null;

            if (!RefSet.TryGetValue(theEvent, out var value)) return;

            if (value == 1)
            {
                RefSet.Remove(theEvent);
                return;
            }

            value--;
            RefSet.Put(theEvent, value);
        }

        private void InitArray()
        {
            var events = RefSet.Keys;
            _array = events.ToArray();
        }
    }
} // end of namespace