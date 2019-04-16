///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Collection for internal use similar to the MatchedEventMap class in the client package
    ///     that holds the one or more events that could match any defined event expressions.
    ///     The optional tag value supplied when an event expression is created is used as a key for placing
    ///     matching event objects into this collection.
    /// </summary>
    public class MatchedEventMapImpl : MatchedEventMap
    {
        /// <summary>
        ///     Constructor creates an empty collection of events.
        /// </summary>
        /// <param name="meta">metadata</param>
        public MatchedEventMapImpl(MatchedEventMapMeta meta)
        {
            Meta = meta;
            MatchingEvents = new object[meta.TagsPerIndex.Length];
        }

        public MatchedEventMapImpl(
            MatchedEventMapMeta meta,
            object[] matches)
        {
            Meta = meta;
            MatchingEvents = matches;
        }

        /// <summary>
        ///     Add an event to the collection identified by the given tag.
        /// </summary>
        /// <param name="tag">is an identifier to retrieve the event from</param>
        /// <param name="theEvent">is the event object or array of event object to be added</param>
        public void Add(
            int tag,
            object theEvent)
        {
            MatchingEvents[tag] = theEvent;
        }

        /// <summary>
        ///     Returns a map containing the events where the key is the event tag string and the value is the event
        ///     instance.
        /// </summary>
        /// <returns>Hashtable containing event instances</returns>
        public object[] MatchingEvents { get; }

        /// <summary>
        ///     Returns a single event instance given the tag identifier, or null if the tag could not be located.
        /// </summary>
        /// <param name="tag">is the identifier to look for</param>
        /// <returns>event instances for the tag</returns>
        public EventBean GetMatchingEvent(int tag)
        {
            return (EventBean) MatchingEvents[tag];
        }

        public object GetMatchingEventAsObject(int tag)
        {
            return MatchingEvents[tag];
        }

        /// <summary>
        ///     Merge the state of an other match event structure into this one by adding all entries
        ///     within the MatchedEventMap to this match event.
        /// </summary>
        /// <param name="other">is the other instance to merge in.</param>
        public void Merge(MatchedEventMap other)
        {
            if (!(other is MatchedEventMapImpl)) {
                throw new UnsupportedOperationException("Merge requires same types");
            }

            var otherImpl = (MatchedEventMapImpl) other;
            for (var i = 0; i < MatchingEvents.Length; i++) {
                if (otherImpl.MatchingEvents[i] == null) {
                    continue;
                }

                MatchingEvents[i] = otherImpl.MatchingEvents[i];
            }
        }

        public MatchedEventMapMeta Meta { get; }

        public EventBean GetMatchingEventByTag(string resultEventAsName)
        {
            var obj = GetMatchingEventAsObjectByTag(resultEventAsName);
            return (EventBean) obj;
        }

        public object GetMatchingEventAsObjectByTag(string key)
        {
            var index = Meta.GetTagFor(key);
            if (index == -1) {
                return null;
            }

            return MatchingEvents[index];
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            var count = 0;

            for (var i = 0; i < MatchingEvents.Length; i++) {
                buffer.Append(" (");
                buffer.Append(count++);
                buffer.Append(") ");
                buffer.Append("tag=");
                buffer.Append(Meta.TagsPerIndex[i]);
                buffer.Append("  event=");
                buffer.Append(MatchingEvents[i]);
            }

            return buffer.ToString();
        }

        /// <summary>
        ///     Make a shallow copy of this collection.
        /// </summary>
        /// <returns>shallow copy</returns>
        public MatchedEventMap ShallowCopy()
        {
            if (MatchingEvents.Length == 0) {
                return this;
            }

            var copy = new object[MatchingEvents.Length];
            if (MatchingEvents.Length > 1) {
                Array.Copy(MatchingEvents, 0, copy, 0, MatchingEvents.Length);
            }
            else {
                copy[0] = MatchingEvents[0];
            }

            return new MatchedEventMapImpl(Meta, copy);
        }

        public IDictionary<string, object> MatchingEventsAsMap {
            get {
                IDictionary<string, object> map = new Dictionary<string, object>();
                for (var i = 0; i < Meta.TagsPerIndex.Length; i++) {
                    if (MatchingEvents[i] == null) {
                        continue;
                    }

                    map.Put(Meta.TagsPerIndex[i], MatchingEvents[i]);
                }

                return map;
            }
        }
    }
} // end of namespace