///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// Simple collection that exposes a limited add-and-get interface and
    /// that is optimized towards holding a single event, but can hold multiple
    /// events. If more then one event is added, the class allocates a linked
    /// list for additional events.
    /// </summary>
    public class OneEventCollection
    {
        private EventBean _firstEvent;
        private LinkedList<EventBean> _additionalEvents;

        /// <summary>
        /// Gets the first event.
        /// </summary>
        /// <value>
        /// The first event.
        /// </value>
        public EventBean FirstEvent
        {
            get { return _firstEvent; }
        }

        /// <summary>
        /// Gets the additional events.
        /// </summary>
        /// <value>
        /// The additional events.
        /// </value>
        public LinkedList<EventBean> AdditionalEvents
        {
            get { return _additionalEvents; }
        }

        /// <summary>
        /// Add an event to the collection.
        /// </summary>
        /// <param name="theEvent">is the event to add</param>
        public void Add(EventBean theEvent)
        {
            if (theEvent == null)
            {
                throw new ArgumentException("Null event not allowed");
            }
            
            if (_firstEvent == null)
            {
                _firstEvent = theEvent;
                return;
            }
            
            if (_additionalEvents == null)
            {
                _additionalEvents = new LinkedList<EventBean>();
            }
            _additionalEvents.AddLast(theEvent);
        }

        /// <summary>
        /// Returns true if the collection is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// <returns>true if empty, false if not</returns>
        public bool IsEmpty()
        {
            return _firstEvent == null;
        }

        /// <summary>
        /// Returns an array holding the collected events.
        /// </summary>
        /// <returns>event array</returns>
        public EventBean[] ToArray()
        {
            if (_firstEvent == null)
            {
                return new EventBean[0];
            }
    
            if (_additionalEvents == null)
            {
                return new[] {_firstEvent};
            }
    
            EventBean[] events = new EventBean[1 + _additionalEvents.Count];
            events[0] = _firstEvent;
    
            int count = 1;
            foreach (EventBean theEvent in _additionalEvents)
            {
                events[count] = theEvent;
                count++;
            }
    
            return events;
        }

        public void Add(EventBean[] events)
        {
            foreach (EventBean ev in events)
            {
                Add(ev);
            }
        }
    }
}
