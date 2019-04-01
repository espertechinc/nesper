///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    ///     Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateLinearImpl : AggregationStateWithSize, AggregationStateLinear
    {
        private readonly List<EventBean> _events = new List<EventBean>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamId">stream id</param>
        public AggregationStateLinearImpl(int streamId)
        {
            StreamId = streamId;
        }

        internal int StreamId;

        public IList<EventBean> Events => _events;

        public EventBean GetFirstNthValue(int index)
        {
            if (index < 0) return null;
            if (index >= _events.Count) return null;
            return _events[index];
        }

        public EventBean GetLastNthValue(int index)
        {
            if (index < 0) return null;
            if (index >= _events.Count) return null;
            return _events[_events.Count - index - 1];
        }

        public EventBean FirstValue => _events.IsEmpty() ? null : Events[0];

        public EventBean LastValue => _events.IsEmpty() ? null : _events[_events.Count - 1];

        public IEnumerator<EventBean> GetEnumerator()
        {
            return Events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ICollection<EventBean> CollectionReadOnly => _events;

        public virtual void Clear()
        {
            _events.Clear();
        }

        public virtual void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            _events.Remove(theEvent);
        }

        public virtual void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            _events.Add(theEvent);
        }

        public int Count => Events.Count;
    }
} // end of namespace