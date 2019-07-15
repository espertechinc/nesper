///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    /// <summary>
    ///     Contains a set of events to send to the runtime for testing along with a time for each event.
    ///     Each event has a string event id that can be obtained via the getParentEvent method.
    /// </summary>
    public class EventCollection : IEnumerable<KeyValuePair<string, object>>
    {
        public const string ON_START_EVENT_ID = "ON_START_ID";

        // Ordered map of string event id and event object
        // Events will be sent in the ordering maintained.
        private readonly IDictionary<string, object> testEvents;

        // Optional time for each event
        private readonly IDictionary<string, long> testEventTimes;

        public EventCollection(
            IDictionary<string, object> testEvents,
            IDictionary<string, long> testEventTimes)
        {
            this.testEvents = testEvents;
            this.testEventTimes = testEventTimes;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return testEvents.GetEnumerator();
        }

        public object GetEvent(string eventId)
        {
            if (!testEvents.ContainsKey(eventId)) {
                throw new ArgumentException("Event Id " + eventId + " not found in data set");
            }

            return testEvents.Get(eventId);
        }

        public bool TryGetTime(
            string eventId,
            out long time)
        {
            return testEventTimes.TryGetValue(eventId, out time);
        }

        public long? GetTime(string eventId)
        {
            if (testEventTimes.TryGetValue(eventId, out var time)) {
                return time;
            }

            return null;
        }
    }
} // end of namespace