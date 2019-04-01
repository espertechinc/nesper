///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
	public class EventsAndSortKeysPair {
	    private readonly EventBean[] events;
	    private readonly object[] sortKeys;

	    public EventsAndSortKeysPair(EventBean[] events, object[] sortKeys) {
	        this.events = events;
	        this.sortKeys = sortKeys;
	    }

	    public EventBean[] GetEvents() {
	        return events;
	    }

	    public object[] GetSortKeys() {
	        return sortKeys;
	    }
	}
} // end of namespace