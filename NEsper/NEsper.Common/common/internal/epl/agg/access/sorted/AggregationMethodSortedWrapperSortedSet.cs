///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperNavigableMap;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperSortedSet : SortedSet<object> {
	    private readonly SortedSet<object> sorted;

	    public AggregationMethodSortedWrapperSortedSet(SortedSet<object> sorted) {
	        this.sorted = sorted;
	    }

	    public IComparer Comparator() {
	        return sorted.Comparator();
	    }

	    public object First() {
	        return sorted.First();
	    }

	    public object Last() {
	        return sorted.Last();
	    }

	    public SortedSet<object> SubSet(object fromElement, object toElement) {
	        return new AggregationMethodSortedWrapperSortedSet(sorted.SubSet(fromElement, toElement));
	    }

	    public SortedSet<object> HeadSet(object toElement) {
	        return new AggregationMethodSortedWrapperSortedSet(sorted.HeadSet(toElement));
	    }

	    public SortedSet<object> TailSet(object fromElement) {
	        return new AggregationMethodSortedWrapperSortedSet(sorted.TailSet(fromElement));
	    }
	}
} // end of namespace
