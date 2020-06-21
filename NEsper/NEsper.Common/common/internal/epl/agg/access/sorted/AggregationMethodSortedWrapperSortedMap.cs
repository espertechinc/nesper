///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperDictionary;
using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregatorAccessSortedImpl;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperSortedMap : IDictionary<object, ICollection<EventBean>> {
	    private readonly SortedMap<object, object> sorted;

	    AggregationMethodSortedWrapperSortedMap(SortedMap<object, object> sorted) {
	        this.sorted = sorted;
	    }

	    public SortedMap<object, ICollection<EventBean>> SubMap(object fromKey, object toKey) {
	        return new AggregationMethodSortedWrapperSortedMap(sorted.SubMap(fromKey, toKey));
	    }

	    public SortedMap<object, ICollection<EventBean>> HeadMap(object toKey) {
	        return new AggregationMethodSortedWrapperSortedMap(sorted.HeadMap(toKey));
	    }

	    public SortedMap<object, ICollection<EventBean>> TailMap(object fromKey) {
	        return new AggregationMethodSortedWrapperSortedMap(sorted.TailMap(fromKey));
	    }

	    public object FirstKey() {
	        return sorted.FirstKey();
	    }

	    public object LastKey() {
	        return sorted.LastKey();
	    }

	    public ISet<object> KeySet() {
	        return Collections.UnmodifiableSet(sorted.KeySet());
	    }

	    public ICollection<ICollection<EventBean>> Values() {
	        return new AggregationMethodSortedWrapperCollection(sorted.Values());
	    }

	    public ISet<KeyValuePair<object, ICollection<EventBean>>> EntrySet() {
	        return new AggregationMethodSortedWrapperSet(sorted.EntrySet());
	    }


	    public ICollection<EventBean> Get(object key) {
	        object value = sorted.Get(key);
	        return value == null ? null : CheckedPayloadGetCollEvents(value);
	    }

	}
} // end of namespace
