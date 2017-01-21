///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.map;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Implements a convertor for pattern partial results to events per stream.
    /// </summary>
    public class MatchedEventConvertorImpl
        : MatchedEventConvertor
    {
        private readonly IDictionary<String, Pair<EventType, String>> _arrayEventTypes;
        private readonly EventBean[] _eventsPerStream;
        private readonly IDictionary<String, Pair<EventType, String>> _filterTypes;
        private readonly MatchedEventMapMeta _matchedEventMapMeta;

        public MatchedEventMapMeta MatchedEventMapMeta
        {
            get { return _matchedEventMapMeta; }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filterTypes">the filter one-event types</param>
        /// <param name="arrayEventTypes">the filter many-event types</param>
        /// <param name="allTags">All tags.</param>
        /// <param name="eventAdapterService">for creating wrappers if required</param>
        public MatchedEventConvertorImpl(ICollection<KeyValuePair<string, Pair<EventType, string>>> filterTypes,
                                         ICollection<KeyValuePair<string, Pair<EventType, string>>> arrayEventTypes,
                                         IEnumerable<string> allTags,
                                         EventAdapterService eventAdapterService)
        {
            int size = filterTypes.Count;
            if (arrayEventTypes != null)
            {
                size += arrayEventTypes.Count;
            }

            _eventsPerStream = new EventBean[size];
            _filterTypes = new LinkedHashMap<String, Pair<EventType, String>>();
            _filterTypes.PutAll(filterTypes);
            _arrayEventTypes = new LinkedHashMap<String, Pair<EventType, String>>();
            if (arrayEventTypes != null)
            {
                _arrayEventTypes.PutAll(arrayEventTypes);
            }

            _matchedEventMapMeta = new MatchedEventMapMeta(allTags.ToArray(), _arrayEventTypes.IsNotEmpty());
        }

        public EventBean[] Convert(MatchedEventMap events)
        {
            int count = 0;
            foreach (var entry in _filterTypes)
            {
                EventBean theEvent = events.GetMatchingEventByTag(entry.Key);
                _eventsPerStream[count++] = theEvent;
            }
            if (_arrayEventTypes != null)
            {
                foreach (var entry in _arrayEventTypes)
                {
                    var eventArray = (EventBean[]) events.GetMatchingEventAsObjectByTag(entry.Key);
                    var map = new Dictionary<string, object>();
                    map.Put(entry.Key, eventArray);
                    EventBean theEvent = new MapEventBean(map, null);
                    _eventsPerStream[count++] = theEvent;
                }
            }
            return _eventsPerStream;
        }
    }
}
