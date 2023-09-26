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
        private readonly RandomAccessByIndexObserver _updateObserver;
        private EventBean[] _cache;
        private int _cacheFilledTo;

        private IEnumerator<object> _iterator;

        private IOrderedDictionary<object, object> _sortedEvents;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="updateObserver">for indicating updates to</param>
        public IStreamSortRankRandomAccessImpl(RandomAccessByIndexObserver updateObserver)
        {
            _updateObserver = updateObserver;
        }

        /// <summary>
        ///     Refreshes the random access data with the updated information.
        /// </summary>
        /// <param name="sortedEvents">is the sorted window contents</param>
        /// <param name="currentSize">is the current size of the window</param>
        /// <param name="maxSize">is the maximum size of the window</param>
        public void Refresh(
            IOrderedDictionary<object, object> sortedEvents,
            int currentSize,
            int maxSize)
        {
            _updateObserver.Updated(this);
            _sortedEvents = sortedEvents;
            WindowCount = currentSize;

            _iterator = null;
            _cacheFilledTo = 0;
            if (_cache == null || _cache.Length < maxSize) {
                _cache = new EventBean[maxSize];
            }
        }

        public EventBean GetNewData(int index)
        {
            if (_iterator == null) {
                _iterator = _sortedEvents.Values.GetEnumerator();
            }

            // if asking for more then the sorted window currently holds, return no data
            if (index >= WindowCount) {
                return null;
            }

            // If we have it in cache, serve from cache
            if (index < _cacheFilledTo) {
                return _cache[index];
            }

            // Load more into cache
            while (true) {
                if (_cacheFilledTo == WindowCount) {
                    break;
                }

                if (!_iterator.MoveNext()) {
                    break;
                }

                var entry = _iterator.Current;
                if (entry is IList<EventBean> events) {
                    foreach (var theEvent in events) {
                        _cache[_cacheFilledTo] = theEvent;
                        _cacheFilledTo++;
                    }
                }
                else {
                    var theEvent = (EventBean)entry;
                    _cache[_cacheFilledTo] = theEvent;
                    _cacheFilledTo++;
                }

                if (_cacheFilledTo > index) {
                    break;
                }
            }

            // If we have it in cache, serve from cache
            if (index <= _cacheFilledTo) {
                return _cache[index];
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

            if (index < _cacheFilledTo && index >= 0) {
                return _cache[_cacheFilledTo - index - 1];
            }

            return null;
        }

        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            InitCache();
            return new ArrayEventEnumerator(_cache, _cacheFilledTo);
        }

        public ICollection<EventBean> WindowCollectionReadOnly {
            get {
                InitCache();
                return new ArrayMaxEventCollectionRO(_cache, _cacheFilledTo);
            }
        }

        public int WindowCount { get; private set; }

        private void InitCache()
        {
            if (_iterator == null) {
                _iterator = _sortedEvents.Values.GetEnumerator();
            }

            // Load more into cache
            while (true) {
                if (_cacheFilledTo == WindowCount) {
                    break;
                }

                if (!_iterator.MoveNext()) {
                    break;
                }

                var entry = _iterator.Current;
                if (entry is IList<EventBean> events) {
                    foreach (var theEvent in events) {
                        _cache[_cacheFilledTo] = theEvent;
                        _cacheFilledTo++;
                    }
                }
                else {
                    var theEvent = (EventBean)entry;
                    _cache[_cacheFilledTo] = theEvent;
                    _cacheFilledTo++;
                }
            }
        }
    }
} // end of namespace