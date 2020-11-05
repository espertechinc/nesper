///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Simple collection that exposes a limited add-and-get interface and
    ///     that is optimized towards holding a single event, but can hold multiple
    ///     events. If more then one event is added, the class allocates a linked
    ///     list for additional events.
    /// </summary>
    public class OneEventCollection
    {
        /// <summary>
        ///     Gets the first event.
        /// </summary>
        /// <value>
        ///     The first event.
        /// </value>
        public EventBean FirstEvent { get; private set; }

        /// <summary>
        ///     Gets the additional events.
        /// </summary>
        /// <value>
        ///     The additional events.
        /// </value>
        public LinkedList<EventBean> AdditionalEvents { get; private set; }

        /// <summary>
        ///     Add an event to the collection.
        /// </summary>
        /// <param name="theEvent">is the event to add</param>
        public void Add(EventBean theEvent)
        {
            if (theEvent == null) {
                throw new ArgumentException("Null event not allowed");
            }

            if (FirstEvent == null) {
                FirstEvent = theEvent;
                return;
            }

            if (AdditionalEvents == null) {
                AdditionalEvents = new LinkedList<EventBean>();
            }

            AdditionalEvents.AddLast(theEvent);
        }

        /// <summary>
        ///     Returns true if the collection is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// <returns>true if empty, false if not</returns>
        public bool IsEmpty()
        {
            return FirstEvent == null;
        }

        /// <summary>
        ///     Returns an array holding the collected events.
        /// </summary>
        /// <returns>event array</returns>
        public EventBean[] ToArray()
        {
            if (FirstEvent == null) {
                return new EventBean[0];
            }

            if (AdditionalEvents == null) {
                return new[] {FirstEvent};
            }

            var events = new EventBean[1 + AdditionalEvents.Count];
            events[0] = FirstEvent;

            var count = 1;
            foreach (var theEvent in AdditionalEvents) {
                events[count] = theEvent;
                count++;
            }

            return events;
        }

        public void Add(EventBean[] events)
        {
            foreach (var ev in events) {
                Add(ev);
            }
        }

        public void Clear()
        {
            FirstEvent = null;
            AdditionalEvents?.Clear();
        }
    }
}