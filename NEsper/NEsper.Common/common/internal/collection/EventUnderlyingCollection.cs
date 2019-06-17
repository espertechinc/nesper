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

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Iterator for an iterator of events returning the underlying itself.
    /// </summary>
    public class EventUnderlyingCollection : ICollection<object>
    {
        private readonly ICollection<EventBean> events;
        private ICollection<object> buf;

        public EventUnderlyingCollection(ICollection<EventBean> events)
        {
            this.events = events;
            throw new NotImplementedException("revisit why is this here");
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void ICollection<object>.Add(object item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(
            object[] array,
            int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
    }
} // end of namespace