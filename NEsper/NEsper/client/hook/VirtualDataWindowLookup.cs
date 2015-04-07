///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.hook
{
    /// <summary>Represents a lookup strategy object that an EPL statement that queries a virtual data window obtains to perform read operations into the virtual data window. <para />An instance is associated to each EPL statement querying (join, subquery, on-action etc.) the virtual data window. <para />Optimally an implementation returns only those rows matching the complete lookup context filtered field information.* <para />It is legal for an implementation to return rows that are not matching lookup context filter field information. Such rows are removed by where-clause criteria, when provided. </summary>
    public interface VirtualDataWindowLookup {
    
        /// <summary>Invoked by an EPL statement that queries a virtual data window to perform a lookup. <para />Keys passed are the actual query lookup values. For range lookups, the key passed is an instance of <seealso cref="VirtualDataWindowKeyRange" />. <para />Key values follow <seealso cref="VirtualDataWindowLookupContext" />. <para />EventsPerStream contains the events participating in the subquery or join. It is not necessary to use eventsPerStream and the events are provided for additional information. Please consider eventsPerStream for Esper internal use. </summary>
        /// <param name="keys">lookup values</param>
        /// <param name="eventsPerStream">input events for the lookup</param>
        /// <returns>set of events</returns>
        ISet<EventBean> Lookup(Object[] keys, EventBean[] eventsPerStream);
    }
}
