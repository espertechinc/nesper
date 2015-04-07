///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Base interface for providing access-aggregations, i.e. aggregations that mirror a data 
    /// window but group by the group-by clause and that do not mirror the data windows sorting
    /// policy.
    /// </summary>
    public interface AggregationAccess
    {
        /// <summary>Enter an event. </summary>
        /// <param name="eventsPerStream">all events in all streams, typically implementations pick the relevant stream's events to add</param>
        void ApplyEnter(EventBean[] eventsPerStream);
    
        /// <summary>Remove an event. </summary>
        /// <param name="eventsPerStream">all events in all streams, typically implementations pick the relevant stream's events to remove</param>
        void ApplyLeave(EventBean[] eventsPerStream);

        /// <summary>Returns the first (oldest) value entered. </summary>
        /// <value>first value</value>
        EventBean FirstValue { get; }

        /// <summary>Returns the newest (last) value entered. </summary>
        /// <value>last value</value>
        EventBean LastValue { get; }

        /// <summary>Counting from the first element to the last, returns the oldest (first) value entered for index zero and the n-th oldest value for index N. </summary>
        /// <param name="index">index</param>
        /// <returns>last value</returns>
        EventBean GetFirstNthValue(int index);
    
        /// <summary>Counting from the last element to the first, returns the newest (last) value entered for index zero and the n-th newest value for index N. </summary>
        /// <param name="index">index</param>
        /// <returns>last value</returns>
        EventBean GetLastNthValue(int index);
    
        /// <summary>Returns all events for the group. </summary>
        /// <returns>group event iterator</returns>
        IEnumerator<EventBean> GetEnumerator();
    
        /// <summary>Returns all events for the group. </summary>
        /// <returns>group event iterator</returns>
        ICollection<EventBean> CollectionReadOnly();

        /// <summary>Returns the number of events in the group. </summary>
        /// <value>size</value>
        int Count { get; }

        /// <summary>Clear all events in the group. </summary>
        void Clear();
    }
}
