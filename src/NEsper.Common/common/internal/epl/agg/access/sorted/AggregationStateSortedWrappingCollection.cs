///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationStateSortedWrappingCollection : ICollection<EventBean>
    {
        private readonly IOrderedDictionary<object, object> _sorted;

        public AggregationStateSortedWrappingCollection(
            IOrderedDictionary<object, object> sorted,
            int count)
        {
            _sorted = sorted;
            Count = count;
        }

        public bool Remove(EventBean item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }

        public bool IsReadOnly => true;

        public IEnumerator<EventBean> GetEnumerator()
        {
            return new AggregationStateSortedEnumerator(_sorted, false);
        }

        public void Add(EventBean item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Contains(EventBean item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(
            EventBean[] array,
            int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public object[] ToArray()
        {
            throw new UnsupportedOperationException("Partial implementation");
        }

        public object[] ToArray(object[] a)
        {
            throw new UnsupportedOperationException("Partial implementation");
        }

        public bool Contains(object o)
        {
            throw new UnsupportedOperationException("Partial implementation");
        }

        public void Add(object o)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Remove(object o)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }
    }
}