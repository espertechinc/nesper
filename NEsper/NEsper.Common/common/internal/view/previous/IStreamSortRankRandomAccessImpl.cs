///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.previous
{
    /// <summary>
    ///     Provides random access into a rank-window's data.
    /// </summary>
    public class IStreamSortRankRandomAccessImpl : RandomAccessByIndex,
        IStreamSortRankRandomAccess
    {
        private readonly RandomAccessByIndexObserver updateObserver;
        private EventBean[] cache;
        private int cacheFilledTo;

        private IEnumerator<object> iterator;

        private OrderedDictionary<object, object> sortedEvents;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="updateObserver">for indicating updates to</param>
        public IStreamSortRankRandomAccessImpl(RandomAccessByIndexObserver updateObserver)
        {
            this.updateObserver = updateObserver;
        }

        /// <summary>
        ///     Refreshes the random access data with the updated information.
        /// </summary>
        /// <param name="sortedEvents">is the sorted window contents</param>
        /// <param name="currentSize">is the current size of the window</param>
        /// <param name="maxSize">is the maximum size of the window</param>
        public void Refresh(
            OrderedDictionary<object, object> sortedEvents,
            int currentSize,
            int maxSize)
        {
            updateObserver.Updated(this);
            this.sortedEvents = sortedEvents;
            WindowCount = currentSize;

            iterator = null;
            cacheFilledTo = 0;
            if (cache == null || cache.Length < maxSize) {
                cache = new EventBean[maxSize];
            }
        }

        public EventBean GetNewData(int index)
        {
            if (iterator == null) {
                iterator = sortedEvents.Values.GetEnumerator();
            }

            // if asking for more then the sorted window currently holds, return no data
            if (index >= WindowCount) {
                return null;
            }

            // If we have it in cache, serve from cache
            if (index < cacheFilledTo) {
                return cache[index];
            }

            // Load more into cache
            while (true) {
                if (cacheFilledTo == WindowCount) {
                    break;
                }

                if (!iterator.MoveNext()) {
                    break;
                }

                var entry = iterator.Current;
                if (entry is IList<EventBean> events) {
                    foreach (var theEvent in events) {
                        cache[cacheFilledTo] = theEvent;
                        cacheFilledTo++;
                    }
                }
                else {
                    var theEvent = (EventBean) entry;
                    cache[cacheFilledTo] = theEvent;
                    cacheFilledTo++;
                }

                if (cacheFilledTo > index) {
                    break;
                }
            }

            // If we have it in cache, serve from cache
            if (index <= cacheFilledTo) {
                return cache[index];
            }

            return null;
        }

        public EventBean GetOldData(int index)
        {
            return null;
        }

        public EventBean GetNewDataTail(int index)
        {
            InitCache();

            if (index < cacheFilledTo && index >= 0) {
                return cache[cacheFilledTo - index - 1];
            }

            return null;
        }

        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            InitCache();
            return new ArrayMaxEventIterator(cache, cacheFilledTo);
        }

        public ICollection<EventBean> WindowCollectionReadOnly {
            get {
                InitCache();
                return new ArrayMaxEventCollectionRO(cache, cacheFilledTo);
            }
        }

        public int WindowCount { get; private set; }

        private void InitCache()
        {
            if (iterator == null) {
                iterator = sortedEvents.Values.GetEnumerator();
            }

            // Load more into cache
            while (true) {
                if (cacheFilledTo == WindowCount) {
                    break;
                }

                if (!iterator.MoveNext()) {
                    break;
                }

                var entry = iterator.Current;
                if (entry is IList<EventBean> events) {
                    foreach (var theEvent in events) {
                        cache[cacheFilledTo] = theEvent;
                        cacheFilledTo++;
                    }
                }
                else {
                    var theEvent = (EventBean) entry;
                    cache[cacheFilledTo] = theEvent;
                    cacheFilledTo++;
                }
            }
        }
    }
} // end of namespace