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
    public interface AggregationStateLinear : IEnumerable<EventBean>
    {
        /// <summary>Returns the first (oldest) value entered. </summary>
        /// <value>first value</value>
        EventBean FirstValue { get; }

        /// <summary>Returns the newest (last) value entered. </summary>
        /// <value>last value</value>
        EventBean LastValue { get; }

        /// <summary>Returns the number of events in the group. </summary>
        /// <value>size</value>
        int Count { get; }

        /// <summary>
        ///     Counting from the first element to the last, returns the oldest (first) value entered for index zero and the
        ///     n-th oldest value for index Count.
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>last value</returns>
        EventBean GetFirstNthValue(int index);

        /// <summary>
        ///     Counting from the last element to the first, returns the newest (last) value entered for index zero and the
        ///     n-th newest value for index Count.
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>last value</returns>
        EventBean GetLastNthValue(int index);

        /// <summary>Returns all events for the group. </summary>
        /// <value>group event iterator</value>
        ICollection<EventBean> CollectionReadOnly { get; }
    }
}