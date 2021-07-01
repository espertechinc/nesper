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
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;

namespace com.espertech.esper.common.@internal.collection
{
    public class EventUnderlyingCollection : ICollection<EventBean>, ICollection<object>
    {
        private ICollection<EventBean> _underlyingCollection;

        public EventUnderlyingCollection(FlexCollection flexCollection)
        {
            _underlyingCollection = flexCollection.EventBeanCollection;
        }
        
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

        #region ICollection<EventBean>
        
        IEnumerator<EventBean> IEnumerable<EventBean>.GetEnumerator()
        {
            return _underlyingCollection.GetEnumerator();
        }

        public void Add(EventBean item)
        {
            throw new NotSupportedException();
        }

        void ICollection<EventBean>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(EventBean item)
        {
            return _underlyingCollection.Contains(item);
        }

        public void CopyTo(
            EventBean[] array,
            int arrayIndex)
        {
            _underlyingCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(EventBean item)
        {
            throw new NotSupportedException();
        }

        int ICollection<EventBean>.Count => _underlyingCollection.Count();

        bool ICollection<EventBean>.IsReadOnly => true;
        
        #endregion
        
        #region ICollection<object>

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return _underlyingCollection
                .Select(i => i.Underlying)
                .GetEnumerator();
        }

        public void Add(object item)
        {
            throw new NotSupportedException();
        }

        void ICollection<object>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object item)
        {
            return _underlyingCollection
                .Select(i => i.Underlying)
                .Contains(item);
        }

        public void CopyTo(
            object[] array,
            int arrayIndex)
        {
            var tempArray = _underlyingCollection.Select(i => i.Underlying).ToArray();
            tempArray.CopyTo(array, arrayIndex);
        }

        public bool Remove(object item)
        {
            throw new NotSupportedException();
        }

        int ICollection<object>.Count => _underlyingCollection.Count;

        bool ICollection<object>.IsReadOnly => true;
        
        #endregion
    }
} // end of namespace