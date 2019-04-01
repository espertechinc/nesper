///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Iterator for iterating over an array of events up to a given max number of events.
    /// </summary>
    public class ArrayMaxEventCollectionRO : ICollection<EventBean>
    {
        private readonly EventBean[] events;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="events">to iterate</param>
        /// <param name="maxNumEvents">max to iterate</param>
        public ArrayMaxEventCollectionRO(EventBean[] events, int maxNumEvents)
        {
            this.events = events;
            Count = maxNumEvents;
        }

        public int Count { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return new ArrayMaxEventIterator(events, Count);
        }

        public void Clear()
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public void Add(EventBean item)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Contains(EventBean item)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public void CopyTo(EventBean[] array, int arrayIndex)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Remove(EventBean item)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool IsReadOnly => true;

        public bool IsEmpty()
        {
            return Count == 0;
        }
    }
} // end of namespace