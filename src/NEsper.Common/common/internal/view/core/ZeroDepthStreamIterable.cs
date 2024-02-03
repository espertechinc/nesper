///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Event stream implementation that does not keep any window by itself of the events coming into the stream,
    ///     however is itself iterable and keeps the last event.
    /// </summary>
    public class ZeroDepthStreamIterable : EventStream
    {
        private EventBean lastInsertedEvent;
        private EventBean[] lastInsertedEvents;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">type of event</param>
        public ZeroDepthStreamIterable(EventType eventType)
        {
            EventType = eventType;
        }

        public virtual void Insert(EventBean theEvent)
        {
            // Get a new array created rather then re-use the old one since some client listeners
            // to this view may keep reference to the new data
            EventBean[] row = { theEvent };
            Child.Update(row, null);
            lastInsertedEvent = theEvent;
        }

        public virtual void Insert(EventBean[] events)
        {
            Child.Update(events, null);
            lastInsertedEvents = events;
        }

        public EventType EventType { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (lastInsertedEvents != null) {
                foreach (var eventBean in lastInsertedEvents) {
                    yield return eventBean;
                }
            }

            if (lastInsertedEvent != null) {
                yield return lastInsertedEvent;
            }
        }

        public View Child { get; set; }
    }
} // end of namespace