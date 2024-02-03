///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.client.collection;

namespace com.espertech.esper.common.@internal.collection
{
    public class EventUnderlyingCollection<T> : ICollection<T>
    {
        private readonly ICollection<EventBean> _underlyingCollection;

        public EventUnderlyingCollection(ICollection<EventBean> events)
        {
            _underlyingCollection = events;
        }

        /// <summary>
        /// Returns an enumeration of the UNDERLYING event data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _underlyingCollection
                .Select(i => i.Underlying)
                .GetEnumerator();
        }

        #region ICollection<T>

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _underlyingCollection
                .Select(i => (T) i.Underlying)
                .GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _underlyingCollection
                .Select(i => i.Underlying)
                .Contains(item);
        }

        public void CopyTo(
            T[] array,
            int arrayIndex)
        {
            var tempArray = _underlyingCollection.Select(i => i.Underlying).ToArray();
            tempArray.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        int ICollection<T>.Count => _underlyingCollection.Count;

        bool ICollection<T>.IsReadOnly => true;

        #endregion
    }
} // end of namespace