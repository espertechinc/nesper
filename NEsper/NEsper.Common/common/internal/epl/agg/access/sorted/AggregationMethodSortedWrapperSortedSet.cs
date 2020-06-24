///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using java.util.function;
using java.util.stream;


import static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperNavigableMap.immutableException;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperSortedSet : SortedSet<object> {
	    private readonly SortedSet<object> sorted;

	    public AggregationMethodSortedWrapperSortedSet(SortedSet<object> sorted) {
	        this.sorted = sorted;
	    }

	    public Comparator<? super object> Comparator() {
	        return sorted.Comparator();
	    }

	    public object First() {
	        return sorted.First();
	    }

	    public object Last() {
	        return sorted.Last();
	    }

	    public int Size() {
	        return sorted.Count;
	    }

	    public bool IsEmpty() {
	        return sorted.IsEmpty();
	    }

	    public bool Contains(object o) {
	        return sorted.Contains(o);
	    }

	    public object[] ToArray() {
	        return sorted.ToArray();
	    }

	    public <T> T[] ToArray(T[] a) {
	        return sorted.ToArray();
	    }

	    public bool Add(object o) {
	        throw ImmutableException();
	    }

	    public bool Remove(object o) {
	        throw ImmutableException();
	    }

	    public bool ContainsAll(ICollection<?> c) {
	        return sorted.ContainsAll(c);
	    }

	    public bool AddAll(ICollection<?> c) {
	        throw ImmutableException();
	    }

	    public bool RetainAll(ICollection<?> c) {
	        throw ImmutableException();
	    }

	    public bool RemoveAll(ICollection<?> c) {
	        throw ImmutableException();
	    }

	    public void Clear() {
	        throw ImmutableException();
	    }

	    public Spliterator<object> Spliterator() {
	        return sorted.Spliterator();
	    }

	    public bool RemoveIf(Predicate<? super object> filter) {
	        throw ImmutableException();
	    }

	    public Stream<object> Stream() {
	        return sorted.Stream();
	    }

	    public Stream<object> ParallelStream() {
	        return sorted.ParallelStream();
	    }

	    public void ForEach(Consumer<? super object> action) {
	        sorted.ForEach(action);
	    }

	    public IEnumerator<object> Iterator() {
	        return new UnmodifiableIterator(sorted.Iterator());
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
