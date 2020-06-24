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

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregatorAccessSortedImpl; //checkedPayloadGetCollEvents;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperNavigableMap : IDictionary<object, ICollection<EventBean>> {
	    private readonly IDictionary<object, object> sorted;

	    internal AggregationMethodSortedWrapperNavigableMap(IDictionary<object, object> sorted) {
	        this.sorted = sorted;
	    }

	    public KeyValuePair<object, ICollection<EventBean>> LowerEntry(object key) {
	        return ToEntry(sorted.LowerEntry(key));
	    }

	    public object LowerKey(object key) {
	        return sorted.LowerKey(key);
	    }

	    public KeyValuePair<object, ICollection<EventBean>> FloorEntry(object key) {
	        return ToEntry(sorted.FloorEntry(key));
	    }

	    public object FloorKey(object key) {
	        return sorted.FloorKey(key);
	    }

	    public KeyValuePair<object, ICollection<EventBean>> CeilingEntry(object key) {
	        return ToEntry(sorted.CeilingEntry(key));
	    }

	    public object CeilingKey(object key) {
	        return sorted.CeilingKey(key);
	    }

	    public KeyValuePair<object, ICollection<EventBean>> HigherEntry(object key) {
	        return ToEntry(sorted.HigherEntry(key));
	    }

	    public object HigherKey(object key) {
	        return sorted.HigherKey(key);
	    }

	    public KeyValuePair<object, ICollection<EventBean>> FirstEntry() {
	        return ToEntry(sorted.FirstEntry());
	    }

	    public KeyValuePair<object, ICollection<EventBean>> LastEntry() {
	        return ToEntry(sorted.LastEntry());
	    }

	    public IDictionary<object, ICollection<EventBean>> DescendingMap() {
	        return new AggregationMethodSortedWrapperNavigableMap(sorted.DescendingMap());
	    }

	    public NavigableSet<object> NavigableKeySet() {
	        return new AggregationMethodSortedWrapperNavigableSet(sorted.NavigableKeySet());
	    }

	    public NavigableSet<object> DescendingKeySet() {
	        return new AggregationMethodSortedWrapperNavigableSet(sorted.DescendingKeySet());
	    }

	    public IDictionary<object, ICollection<EventBean>> SubMap(object fromKey, bool fromInclusive, object toKey, bool toInclusive) {
	        return new AggregationMethodSortedWrapperNavigableMap(sorted.SubMap(fromKey, fromInclusive, toKey, toInclusive));
	    }

	    public IDictionary<object, ICollection<EventBean>> HeadMap(object toKey, bool inclusive) {
	        return new AggregationMethodSortedWrapperNavigableMap(sorted.HeadMap(toKey, inclusive));
	    }

	    public IDictionary<object, ICollection<EventBean>> TailMap(object fromKey, bool inclusive) {
	        return new AggregationMethodSortedWrapperNavigableMap(sorted.TailMap(fromKey, inclusive));
	    }

	    public OrderedDictionary<object, ICollection<EventBean>> SubMap(object fromKey, object toKey) {
	        return new AggregationMethodSortedWrapperSortedMap(sorted.SubMap(fromKey, toKey));
	    }

	    public OrderedDictionary<object, ICollection<EventBean>> HeadMap(object toKey) {
	        return new AggregationMethodSortedWrapperSortedMap(sorted.HeadMap(toKey));
	    }

	    public OrderedDictionary<object, ICollection<EventBean>> TailMap(object fromKey) {
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

	    public int Count {
		    get { return sorted.Count; }
	    }

	    public bool IsEmpty() {
	        return sorted.IsEmpty();
	    }

	    public bool ContainsKey(object key) {
	        return sorted.ContainsKey(key);
	    }

	    public bool ContainsValue(object value) {
	        throw ContainsNotSupported();
	    }

	    public ICollection<EventBean> Get(object key) {
	        object value = sorted.Get(key);
	        return value == null ? null : CheckedPayloadGetCollEvents(value);
	    }

	    public ICollection<EventBean> Put(object key, ICollection<EventBean> value) {
	        throw ImmutableException();
	    }

	    public ICollection<EventBean> Remove(object key) {
	        throw ImmutableException();
	    }

	    public KeyValuePair<object, ICollection<EventBean>> PollFirstEntry() {
	        throw ImmutableException();
	    }

	    public KeyValuePair<object, ICollection<EventBean>> PollLastEntry() {
	        throw ImmutableException();
	    }

	    public void PutAll(IDictionary<ICollection<EventBean>> m) {
	        throw ImmutableException();
	    }

	    public void Clear() {
	        throw ImmutableException();
	    }

	    private KeyValuePair<object, ICollection<EventBean>> ToEntry(KeyValuePair<object, object> entry) {
	        return new AbstractMap.SimpleKeyValuePair<>(entry.Key, CheckedPayloadGetCollEvents(entry.Value));
	    }

	    internal static UnsupportedOperationException ImmutableException() {
	        return new UnsupportedOperationException("Mutation operations are not supported");
	    }

	    internal static UnsupportedOperationException ContainsNotSupported() {
	        throw new UnsupportedOperationException("Contains-method is not supported");
	    }
	}
} // end of namespace
