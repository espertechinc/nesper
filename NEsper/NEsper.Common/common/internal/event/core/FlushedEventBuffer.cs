///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Buffer for events - accumulates events until flushed.
    /// </summary>
    public class FlushedEventBuffer
    {
        private readonly List<EventBean[]> _remainEvents =
            new List<EventBean[]>();

        private int _remainEventsCount;

        /// <summary>Add an event array to buffer.</summary>
        /// <param name="events">to add</param>
        public void Add(EventBean[] events)
        {
            if (events != null) {
                _remainEvents.Add(events);
                _remainEventsCount += events.Length;
            }
        }

        /// <summary>
        ///     Get the events currently buffered. Returns null if the buffer is empty. Flushes the buffer.
        /// </summary>
        /// <returns>array of events in buffer or null if empty</returns>
        public EventBean[] GetAndFlush()
        {
            if (_remainEventsCount == 0) {
                return null;
            }

            var index = 0;
            var flattened = new EventBean[_remainEventsCount];
            var remainEventsLength = _remainEvents.Count;
            for (var ii = 0; ii < remainEventsLength; ii++) {
                var eventArray = _remainEvents[ii];
                eventArray.CopyTo(flattened, index);
                index += eventArray.Length;
            }

            _remainEvents.Clear();
            _remainEventsCount = 0;

            return flattened;
        }

        /// <summary>EmptyFalse buffer.</summary>
        public void Flush()
        {
            _remainEvents.Clear();
            _remainEventsCount = 0;
        }
    }
} // End of namespace