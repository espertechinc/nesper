///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
    /// <summary>
    /// Provides random access into a rank-window's data.
    /// </summary>
    public class IStreamSortRankRandomAccess : RandomAccessByIndex
    {
        private readonly RandomAccessByIndexObserver _updateObserver;

        private OrderedDictionary<Object, Object> _sortedEvents;
        private int _currentSize;
    
        private IEnumerator<Object> _enumerator;
        private EventBean[] _cache;
        private int _cacheFilledTo;
    
        /// <summary>Ctor. </summary>
        /// <param name="updateObserver">for indicating updates to</param>
        public IStreamSortRankRandomAccess(RandomAccessByIndexObserver updateObserver)
        {
            _updateObserver = updateObserver;
        }
    
        /// <summary>Refreshes the random access data with the updated information. </summary>
        /// <param name="sortedEvents">is the sorted window contents</param>
        /// <param name="currentSize">is the current size of the window</param>
        /// <param name="maxSize">is the maximum size of the window</param>
        public void Refresh(OrderedDictionary<Object, Object> sortedEvents, int currentSize, int maxSize)
        {
            _updateObserver.Updated(this);
            _sortedEvents = sortedEvents;
            _currentSize = currentSize;
    
            _enumerator = null;
            _cacheFilledTo = 0;
            if (_cache == null || _cache.Length < maxSize)
            {
                _cache = new EventBean[maxSize];
            }
        }
    
        public EventBean GetNewData(int index)
        {
            if (_enumerator == null)
            {
                _enumerator = _sortedEvents.Values.GetEnumerator();
            }
    
            // if asking for more then the sorted window currently holds, return no data
            if (index >= _currentSize)
            {
                return null;
            }
    
            // If we have it in cache, serve from cache
            if (index < _cacheFilledTo)
            {
                return _cache[index];
            }
    
            // Load more into cache
            while(true)
            {
                if (_cacheFilledTo == _currentSize)
                {
                    break;
                }
                if (!_enumerator.MoveNext())
                {
                    break;
                }

                var entry = _enumerator.Current;
                if (entry is IList<EventBean>) {
                    var events = (IList<EventBean>) entry;
                    foreach (var theEvent in events)
                    {
                        _cache[_cacheFilledTo] = theEvent;
                        _cacheFilledTo++;
                    }
                }
                else
                {
                    var theEvent = (EventBean) entry;
                    _cache[_cacheFilledTo] = theEvent;
                    _cacheFilledTo++;
                }
    
                if (_cacheFilledTo > index)
                {
                    break;
                }
            }
    
            // If we have it in cache, serve from cache
            if (index <= _cacheFilledTo)
            {
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
    
            if ((index < _cacheFilledTo) && (index >= 0))
            {
                return _cache[_cacheFilledTo - index - 1];
            }
    
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            InitCache();
            return _cache.Take(_cacheFilledTo).GetEnumerator();
        }

        public ICollection<EventBean> WindowCollectionReadOnly
        {
            get
            {
                InitCache();
                return _cache.Take(_cacheFilledTo).ToArray();
                //return new ArrayMaxEventCollectionRO(_cache, _cacheFilledTo);
            }
        }

        public int WindowCount
        {
            get { return _currentSize; }
        }

        private void InitCache()
        {
    
            if (_enumerator == null)
            {
                _enumerator = _sortedEvents.Values.GetEnumerator();
            }
    
            // Load more into cache
            while(true)
            {
                if (_cacheFilledTo == _currentSize)
                {
                    break;
                }

                if (!_enumerator.MoveNext())
                {
                    break;
                }

                var entry = _enumerator.Current;
                if (entry is IList<EventBean>) {
                    var events = (IList<EventBean>) entry;
                    foreach (EventBean theEvent in events)
                    {
                        _cache[_cacheFilledTo] = theEvent;
                        _cacheFilledTo++;
                    }
                }
                else {
                    var theEvent = (EventBean) entry;
                    _cache[_cacheFilledTo] = theEvent;
                    _cacheFilledTo++;
                }
            }
        }
    }
}
