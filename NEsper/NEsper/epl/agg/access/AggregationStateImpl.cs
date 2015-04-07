///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateImpl
        : AggregationStateWithSize
        , AggregationStateLinear
    {
        protected int StreamId;
        protected List<EventBean> Events = new List<EventBean>();

        /// <summary>Ctor. </summary>
        /// <param name="streamId">stream id</param>
        public AggregationStateImpl(int streamId)
        {
            StreamId = streamId;
        }

        public void Clear()
        {
            Events.Clear();
        }

        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[StreamId];
            if (theEvent == null)
            {
                return;
            }
            Events.Remove(theEvent);
        }

        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[StreamId];
            if (theEvent == null)
            {
                return;
            }
            Events.Add(theEvent);
        }

        public EventBean GetFirstNthValue(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= Events.Count)
            {
                return null;
            }
            return Events[index];
        }

        public EventBean GetLastNthValue(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= Events.Count)
            {
                return null;
            }
            return Events[Events.Count - index - 1];
        }

        public EventBean FirstValue
        {
            get
            {
                if (Events.IsEmpty())
                {
                    return null;
                }
                return Events[0];
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (Events.IsEmpty())
                {
                    return null;
                }
                return Events[Events.Count - 1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return Events.GetEnumerator();
        }

        public ICollection<EventBean> CollectionReadOnly
        {
            get { return Events; }
        }

        public int Count
        {
            get { return Events.Count; }
        }
    }
}
