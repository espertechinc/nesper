///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorGroupedOutputAllGroupRepsImpl : ResultSetProcessorGroupedOutputAllGroupReps {

	    private readonly IDictionary<object, EventBean[]> groupRepsView = new LinkedHashMap<object, EventBean[]>();

	    public object Put(object mk, EventBean[] array) {
	        return groupRepsView.Push(mk, array);
	    }

	    public void Remove(object key) {
	        groupRepsView.Remove(key);
	    }

	    public IEnumerator<KeyValuePair<object, EventBean[]>> EntryIterator() {
	        return groupRepsView.GetEnumerator();
	    }

	    public void Destroy() {
	        // no action required
	    }
	}
} // end of namespace
