///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.support
{
    /// <summary>
    /// Contains a set of events to send to the runtime for testing along with a time
    /// for each theEvent. Each event has a string event id that can be obtained via the
    /// getParentEvent method.
    /// </summary>
    public class EventCollection : IEnumerable<KeyValuePair<String, Object>>
    {
        public readonly static String ON_START_EVENT_ID = "ON_START_ID";
    
        // Ordered map of string event id and event object
        // Events will be sent in the ordering maintained.
        private readonly IDictionary<String, Object> _testEvents;
    
        // Optional time for each event
        private readonly IDictionary<String, long?> _testEventTimes;
    
        public EventCollection(IDictionary<String, Object> testEvents,
                               IDictionary<String, long?> testEventTimes)
        {
            _testEvents = testEvents;
            _testEventTimes = testEventTimes;
        }
    
        public Object GetEvent(String eventId)
        {
            if (!_testEvents.ContainsKey(eventId))
            {
                throw new ArgumentException("Event id " + eventId + " not found in data set");
            }
            return _testEvents.Get(eventId);
        }

        public long? GetTime(String eventId)
        {
            return _testEventTimes.Get(eventId);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _testEvents.GetEnumerator();
        }
    }
}
