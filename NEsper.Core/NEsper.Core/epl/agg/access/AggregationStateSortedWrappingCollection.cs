///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.access
{
    public class AggregationStateSortedWrappingCollection : ICollection<EventBean>
    {
        private readonly SortedDictionary<Object, Object> _sorted;
        private readonly int _count;
    
        public AggregationStateSortedWrappingCollection(SortedDictionary<Object, Object> sorted, int count)
        {
            _sorted = sorted;
            _count = count;
        }

        public bool Remove(EventBean item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsEmpty() {
            return Count == 0;
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return new AggregationStateSortedEnumerator(_sorted, false);
        }
    
        public Object[] ToArray()
        {
            throw new UnsupportedOperationException("Partial implementation");
        }
    
        public Object[] ToArray(Object[] a)
        {
            throw new UnsupportedOperationException("Partial implementation");
        }
    
        public bool Contains(Object o)
        {
            throw new UnsupportedOperationException("Partial implementation");
        }
    
        public void Add(Object o)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }
    
        public bool Remove(Object o)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public void Add(EventBean item)
        {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Contains(EventBean item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(EventBean[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
