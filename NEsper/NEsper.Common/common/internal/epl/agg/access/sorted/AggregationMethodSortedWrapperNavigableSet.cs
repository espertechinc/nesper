///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperDictionary; //immutableException;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperNavigableSet : ISet<object>
	{
		private readonly SortedSet<object> sorted;

		public AggregationMethodSortedWrapperNavigableSet(SortedSet<object> sorted)
		{
			this.sorted = sorted;
		}

		public IComparer<object> Comparator => sorted.Comparer;

		public object First => sorted.Min;

		public object Last => sorted.Max;

		public int Count => sorted.Count;

		public bool IsReadOnly => true;

		public bool Contains(object o)
		{
			return sorted.Contains(o);
		}

		void ICollection<object>.Add(object item)
		{
			throw ImmutableException();
		}

		bool ISet<object>.Add(object item)
		{
			throw ImmutableException();
		}

		public bool Remove(object o)
		{
			throw ImmutableException();
		}

		public void Clear()
		{
			throw ImmutableException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return sorted.GetEnumerator();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return sorted.GetEnumerator();
		}

		public object Lower(object o)
		{
			return sorted.Lower(o);
		}

		public object Floor(object o)
		{
			return sorted.Floor(o);
		}

		public object Ceiling(object o)
		{
			return sorted.Ceiling(o);
		}

		public object Higher(object o)
		{
			return sorted.Higher(o);
		}

		public object PollFirst()
		{
			throw ImmutableException();
		}

		public object PollLast()
		{
			throw ImmutableException();
		}

		public NavigableSet<object> DescendingSet()
		{
			return new AggregationMethodSortedWrapperNavigableSet(sorted.DescendingSet());
		}

		public IEnumerator<object> DescendingIterator()
		{
			return new UnmodifiableIterator<object>(sorted.DescendingIterator());
		}

		public NavigableSet<object> SubSet(
			object fromElement,
			bool fromInclusive,
			object toElement,
			bool toInclusive)
		{
			return new AggregationMethodSortedWrapperNavigableSet(sorted.SubSet(fromElement, fromInclusive, toElement, toInclusive));
		}

		public NavigableSet<object> HeadSet(
			object toElement,
			bool inclusive)
		{
			return new AggregationMethodSortedWrapperNavigableSet(sorted.HeadSet(toElement, inclusive));
		}

		public NavigableSet<object> TailSet(
			object fromElement,
			bool inclusive)
		{
			return new AggregationMethodSortedWrapperNavigableSet(sorted.TailSet(fromElement, inclusive));
		}

		public SortedSet<object> SubSet(
			object fromElement,
			object toElement)
		{
			return new AggregationMethodSortedWrapperSortedSet(sorted.SubSet(fromElement, toElement));
		}

		public SortedSet<object> HeadSet(object toElement)
		{
			return new AggregationMethodSortedWrapperSortedSet(sorted.HeadSet(toElement));
		}

		public SortedSet<object> TailSet(object fromElement)
		{
			return new AggregationMethodSortedWrapperSortedSet(sorted.TailSet(fromElement));
		}
	}
} // end of namespace
