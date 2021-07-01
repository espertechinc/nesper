///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Collection for internal use similar to the MatchedEventMap class in the client package
    ///     that holds the one or more events that could match any defined event expressions.
    ///     The optional tag value supplied when an event expression is created is used as a key for placing
    ///     matching event objects into this collection.
    /// </summary>
    public interface MatchedEventMap : MatchedEventMapMinimal
    {
#if INHERITED
        /// <summary>
        ///     Returns a map containing the events where the key is the event tag string and the value is the event
        ///     instance.
        /// </summary>
        /// <returns>Map containing event instances</returns>
        object[] MatchingEvents { get; }
#endif

        IDictionary<string, object> MatchingEventsAsMap { get; }

#if INHERITED
        MatchedEventMapMeta Meta { get; }
#endif

        /// <summary>
        ///     Add an event to the collection identified by the given tag.
        /// </summary>
        /// <param name="tag">is an identifier to retrieve the event from</param>
        /// <param name="theEvent">is the event object or array of event object to be added</param>
        void Add(
            int tag,
            object theEvent);

        /// <summary>
        ///     Returns a single event instance given the tag identifier, or null if the tag could not be located.
        /// </summary>
        /// <param name="tag">is the identifier to look for</param>
        /// <returns>event instances for the tag</returns>
        EventBean GetMatchingEvent(int tag);

        /// <summary>
        ///     Returns the object for the matching event, be it the event bean array or the event bean.
        /// </summary>
        /// <param name="tag">is the tag to return the object for</param>
        /// <returns>event bean or event bean array</returns>
        object GetMatchingEventAsObject(int tag);

        /// <summary>
        ///     Make a shallow copy of this collection.
        /// </summary>
        /// <returns>shallow copy</returns>
        MatchedEventMap ShallowCopy();

        /// <summary>
        ///     Merge the state of an other match event structure into this one by adding all entries
        ///     within the MatchedEventMap to this match event.
        /// </summary>
        /// <param name="other">is the other instance to merge in.</param>
        void Merge(MatchedEventMap other);

        EventBean GetMatchingEventByTag(string resultEventAsName);

        object GetMatchingEventAsObjectByTag(string key);
    }
} // end of namespace