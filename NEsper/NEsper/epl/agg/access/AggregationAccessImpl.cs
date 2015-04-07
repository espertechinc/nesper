///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationAccessImpl : AggregationAccess
    {
        internal int streamId;
        internal List<EventBean> events = new List<EventBean>();
    
        /// <summary>Ctor. </summary>
        /// <param name="streamId">stream id</param>
        public AggregationAccessImpl(int streamId)
        {
            this.streamId = streamId;
        }
    
        public virtual void Clear() {
            events.Clear();
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream)
        {
            EventBean theEvent = eventsPerStream[streamId];
            if (theEvent == null) {
                return;
            }
            events.Remove(theEvent);
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream)
        {
            EventBean theEvent = eventsPerStream[streamId];
            if (theEvent == null) {
                return;
            }
            events.Add(theEvent);
        }
    
        public EventBean GetFirstNthValue(int index)
        {
            if (index < 0) {
                return null;
            }
            if (index >= events.Count) {
                return null;
            }
            return events[index];
        }
    
        public EventBean GetLastNthValue(int index) {
            if (index < 0) {
                return null;
            }
            if (index >= events.Count) {
                return null;
            }
            return events[events.Count - index - 1];
        }

        public EventBean FirstValue
        {
            get
            {
                if (events.IsEmpty())
                {
                    return null;
                }
                return events[0];
            }
        }

        public EventBean LastValue
        {
            get
            {
                if (events.IsEmpty())
                {
                    return null;
                }
                return events[events.Count - 1];
            }
        }

        public IEnumerator<EventBean> GetEnumerator() {
            return events.GetEnumerator();
        }
    
        public ICollection<EventBean> CollectionReadOnly() {
            return events;
        }

        public int Count
        {
            get { return events.Count; }
        }
    }
}
