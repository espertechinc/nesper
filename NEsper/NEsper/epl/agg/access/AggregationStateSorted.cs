///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.agg.access
{
    public interface AggregationStateSorted : IEnumerable<EventBean>
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

        /// <summary>Returns all events for the group. </summary>
        /// <returns>group event iterator</returns>
        ICollection<EventBean> CollectionReadOnly();

        IEnumerator<EventBean> GetReverseEnumerator();
    }
}