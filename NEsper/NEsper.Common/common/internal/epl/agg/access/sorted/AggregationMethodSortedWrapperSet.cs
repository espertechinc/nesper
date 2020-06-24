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
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperNavigableMap;
using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregatorAccessSortedImpl; //checkedPayloadGetCollEvents;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperSet : ISet<KeyValuePair<object, ICollection<EventBean>>>
	{
		// Assumes the underlying entry set is naturally sorted, this means that enumeration
		// must traverse according to some underlying comparer.
		private readonly ISet<KeyValuePair<object, object>> _entrySet;

		public AggregationMethodSortedWrapperSet(ISet<KeyValuePair<object, object>> entrySet)
		{
			_entrySet = entrySet;
		}

		public bool IsReadOnly => true;

		public int Count => _entrySet.Count;

		public IEnumerator<KeyValuePair<object, ICollection<EventBean>>> GetEnumerator()
		{
			return AggregationMethodSortedWrapperEntryEnumerator.For(_entrySet.GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(KeyValuePair<object, ICollection<EventBean>> item)
		{
			throw ContainsNotSupported();
		}

		public bool Add(KeyValuePair<object, ICollection<EventBean>> objectCollectionEntry)
		{
			throw ImmutableException();
		}

		void ICollection<KeyValuePair<object, ICollection<EventBean>>>.Add(KeyValuePair<object, ICollection<EventBean>> item)
		{
			throw ImmutableException();
		}

		public bool Remove(KeyValuePair<object, ICollection<EventBean>> item)
		{
			throw ImmutableException();
		}

		public void Clear()
		{
			throw ImmutableException();
		}

		public void CopyTo(
			KeyValuePair<object, ICollection<EventBean>>[] array,
			int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public void ExceptWith(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public void IntersectWith(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool IsProperSubsetOf(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool IsProperSupersetOf(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool IsSupersetOf(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool Overlaps(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public bool SetEquals(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public void SymmetricExceptWith(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public void UnionWith(IEnumerable<KeyValuePair<object, ICollection<EventBean>>> other)
		{
			throw new NotImplementedException();
		}

		public KeyValuePair<object, ICollection<EventBean>>[] ToArray()
		{
			return _entrySet
				.Select(v => ToEntry(v))
				.ToArray();
		}
		
		private KeyValuePair<object, ICollection<EventBean>> ToEntry(KeyValuePair<object, object> entry)
		{
			return new KeyValuePair<object, ICollection<EventBean>>(
				entry.Key,
				CheckedPayloadGetCollEvents(entry.Value));
		}
	}
} // end of namespace
