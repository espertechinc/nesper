///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Collection for internal use similar to the MatchedEventMap class in the client 
    /// package that holds the one or more events that could match any defined event expressions.
    /// The optional tag value supplied when an event expression is created is used as a key for
    /// placing matching event objects into this collection.
    /// </summary>
    public sealed class MatchedEventMapImpl : MatchedEventMap
    {
        private readonly Object[] _matches;

        /// <summary>Constructor creates an empty collection of events. </summary>
        /// <param name="meta">metadata</param>
        public MatchedEventMapImpl(MatchedEventMapMeta meta)
        {
            Meta = meta;
            _matches = new Object[meta.TagsPerIndex.Length];
        }

        public MatchedEventMapImpl(MatchedEventMapMeta meta, Object[] matches)
        {
            Meta = meta;
            _matches = matches;
        }

        /// <summary>Add an event to the collection identified by the given tag. </summary>
        /// <param name="tag">is an identifier to retrieve the event from</param>
        /// <param name="theEvent">is the event object or array of event object to be added</param>
        public void Add(int tag, Object theEvent)
        {
            _matches[tag] = theEvent;
        }

        /// <summary>Returns a map containing the events where the key is the event tag string and the value is the event instance. </summary>
        /// <value>Hashtable containing event instances</value>
        public object[] MatchingEvents
        {
            get { return _matches; }
        }

        /// <summary>Returns a single event instance given the tag identifier, or null if the tag could not be located. </summary>
        /// <param name="tag">is the identifier to look for</param>
        /// <returns>event instances for the tag</returns>
        public EventBean GetMatchingEvent(int tag)
        {
            return (EventBean)_matches[tag];
        }

        public Object GetMatchingEventAsObject(int tag)
        {
            return _matches[tag];
        }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            var count = 0;

            for (int i = 0; i < _matches.Length; i++)
            {
                buffer.Append(" (");
                buffer.Append(count++);
                buffer.Append(") ");
                buffer.Append("tag=");
                buffer.Append(Meta.TagsPerIndex[i]);
                buffer.Append("  event=");
                buffer.Append(_matches[i]);
            }

            return buffer.ToString();
        }

        /// <summary>Make a shallow copy of this collection. </summary>
        /// <returns>shallow copy</returns>
        public MatchedEventMap ShallowCopy()
        {
            if (_matches.Length == 0)
            {
                return this;
            }

            var copy = new Object[_matches.Length];
            if (_matches.Length > 1)
            {
                Array.Copy(_matches, 0, copy, 0, _matches.Length);
            }
            else
            {
                copy[0] = _matches[0];
            }
            return new MatchedEventMapImpl(Meta, copy);
        }

        /// <summary>Merge the state of an other match event structure into this one by adding all entries within the MatchedEventMap to this match event. </summary>
        /// <param name="other">is the other instance to merge in.</param>
        public void Merge(MatchedEventMap other)
        {
            if (!(other is MatchedEventMapImpl))
            {
                throw new UnsupportedOperationException("Merge requires same types");
            }

            var otherImpl = (MatchedEventMapImpl)other;
            for (int i = 0; i < _matches.Length; i++)
            {
                if (otherImpl._matches[i] == null)
                {
                    continue;
                }
                _matches[i] = otherImpl._matches[i];
            }
        }

        public IDictionary<string, object> MatchingEventsAsMap
        {
            get
            {
                IDictionary<String, Object> map = new Dictionary<String, Object>();
                for (int i = 0; i < Meta.TagsPerIndex.Length; i++)
                {
                    if (_matches[i] == null)
                    {
                        continue;
                    }
                    map.Put(Meta.TagsPerIndex[i], _matches[i]);
                }
                return map;
            }
        }

        public MatchedEventMapMeta Meta { get; private set; }

        public EventBean GetMatchingEventByTag(String resultEventAsName)
        {
            Object obj = GetMatchingEventAsObjectByTag(resultEventAsName);
            return (EventBean)obj;
        }

        public Object GetMatchingEventAsObjectByTag(String key)
        {
            int index = Meta.GetTagFor(key);
            if (index == -1)
            {
                return null;
            }
            return _matches[index];
        }
    }
}
