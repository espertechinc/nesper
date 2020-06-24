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

using com.espertech.esper.common.client;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedWrapperNavigableMap;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedWrapperCollection : ICollection<ICollection<EventBean>> {
	    private readonly ICollection<object> _values;

	    public AggregationMethodSortedWrapperCollection(ICollection<object> values) {
	        _values = values;
	    }

	    public int Count => _values.Count;

	    public IEnumerator<ICollection<EventBean>> GetEnumerator()
	    {
		    return new AggregationMethodSortedWrapperValueEnumerator(_values.GetEnumerator());
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }

#if false
	    public object[] ToArray() {
	        ICollection<EventBean>[] collections = new ICollection[values.Count];
	        int index = 0;
	        foreach (object value in values) {
	            collections[index++] = AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(value);
	        }
	        return collections;
	    }

	    public <T> T[] ToArray(T[] a) {
	        return (T[]) ToArray();
	    }
#endif

		public void Add(ICollection<EventBean> item)
		{
			throw ImmutableException();
		}

		public bool Contains(ICollection<EventBean> item)
		{
			throw ContainsNotSupported();
		}

		public bool Remove(ICollection<EventBean> item)
		{
			throw ImmutableException();
		}

	    public void Clear()
	    {
	        throw ImmutableException();
	    }

	    public void CopyTo(
		    ICollection<EventBean>[] array,
		    int arrayIndex)
	    {
		    throw new NotImplementedException();
	    }

	    public bool IsReadOnly => true;

	    public void ForEach(Consumer<ICollection<EventBean>> action)
	    {
		    foreach (object value in _values) {
			    action.Invoke(AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(value));
		    }
	    }
	}
} // end of namespace
