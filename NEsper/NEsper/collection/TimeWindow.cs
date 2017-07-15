///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// Container for events per time slot. The time is provided as long milliseconds by 
    /// client classes. Events are for a specified timestamp and the implementation creates 
    /// and adds the event to a slot for that timestamp. Events can be expired from the 
    /// window via the expireEvents method when their timestamp is before (or less then) 
    /// an expiry timestamp passed in. Expiry removes the event from the window. The window 
    /// allows iteration through its contents. It is assumed that the timestamp passed to the 
    /// add method is ascending. The window is backed by a collection reflecting the timestamp 
    /// order rather then any sorted map or linked hash map for performance reasons.
    /// </summary>
    public sealed class TimeWindow : IEnumerable<EventBean>
    {
        private ArrayDeque<TimeWindowPair> _window;
        private IDictionary<EventBean, TimeWindowPair> _reverseIndex;
        private int _size;

        /// <summary>Ctor. </summary>
        /// <param name="isSupportRemoveStream">
        /// true to indicate the time window should support effective removal of events
        /// in the window based on the remove stream events received, or false to not accomodate removal at all
        /// </param>
        public TimeWindow(bool isSupportRemoveStream)
        {
            _window = new ArrayDeque<TimeWindowPair>();

            if (isSupportRemoveStream)
            {
                _reverseIndex = new Dictionary<EventBean, TimeWindowPair>();
            }
        }

        /// <summary>Adjust expiry dates. </summary>
        /// <param name="delta">delta to adjust for</param>
        public void Adjust(long delta)
        {
            foreach (var data in _window)
            {
                data.Timestamp = data.Timestamp + delta;
            }
        }

        /// <summary>Adds event to the time window for the specified timestamp. </summary>
        /// <param name="timestamp">the time slot for the event</param>
        /// <param name="bean">event to add</param>
        public void Add(long timestamp, EventBean bean)
        {
            // Empty window
            if (_window.IsEmpty())
            {
                var pairX = new TimeWindowPair(timestamp, bean);
                _window.Add(pairX);

                if (_reverseIndex != null)
                {
                    _reverseIndex[bean] = pairX;
                }
                _size = 1;
                return;
            }

            TimeWindowPair lastPair = _window.Last;

            // Windows last timestamp matches the one supplied
            if (lastPair.Timestamp == timestamp)
            {
                if (lastPair.EventHolder is IList<EventBean>)
                {
                    var list = (IList<EventBean>) lastPair.EventHolder;
                    list.Add(bean);
                }
                else if (lastPair.EventHolder == null)
                {
                    lastPair.EventHolder = bean;
                }
                else
                {
                    var existing = (EventBean) lastPair.EventHolder;
                    IList<EventBean> list = new List<EventBean>(4);
                    list.Add(existing);
                    list.Add(bean);
                    lastPair.EventHolder = list;
                }
                if (_reverseIndex != null)
                {
                    _reverseIndex[bean] = lastPair;
                }
                _size++;
                return;
            }

            // Append to window
            var pair = new TimeWindowPair(timestamp, bean);
            if (_reverseIndex != null)
            {
                _reverseIndex[bean] = pair;
            }
            _window.Add(pair);
            _size++;
        }

        /// <summary>Removes the event from the window, if remove stream handling is enabled. </summary>
        /// <param name="theEvent">to remove</param>
        public void Remove(EventBean theEvent)
        {
            if (_reverseIndex == null)
            {
                throw new UnsupportedOperationException("TimeInMillis window does not accept event removal");
            }
            var pair = _reverseIndex.Get(theEvent);
            if (pair != null)
            {
                if (pair.EventHolder != null && pair.EventHolder.Equals(theEvent))
                {
                    pair.EventHolder = null;
                    _size--;
                }
                else if (pair.EventHolder != null)
                {
                    var list = (IList<EventBean>) pair.EventHolder;
                    var removed = list.Remove(theEvent);
                    if (removed)
                    {
                        _size--;
                    }
                }
                _reverseIndex.Remove(theEvent);
            }
        }

        /// <summary>
        /// Return and remove events in time-slots earlier (less) then the timestamp passed in, returning the list of events expired.
        /// </summary>
        /// <param name="expireBefore">is the timestamp from which on to keep events in the window</param>
        /// <returns>
        /// a list of events expired and removed from the window, or null if none expired
        /// </returns>
        public ArrayDeque<EventBean> ExpireEvents(long expireBefore)
        {
            if (_window.IsEmpty())
            {
                return null;
            }

            var pair = _window.First;

            // If the first entry's timestamp is after the expiry date, nothing to expire
            if (pair.Timestamp >= expireBefore)
            {
                return null;
            }

            var resultBeans = new ArrayDeque<EventBean>();

            // Repeat until the window is empty or the timestamp is above the expiry time
            do
            {
                if (pair.EventHolder != null)
                {
                    if (pair.EventHolder is EventBean)
                    {
                        resultBeans.Add((EventBean) pair.EventHolder);
                    }
                    else
                    {
                        resultBeans.AddAll((IList<EventBean>) pair.EventHolder);
                    }
                }

                _window.RemoveFirst();

                if (_window.IsEmpty())
                {
                    break;
                }

                pair = _window.First;
            } while (pair.Timestamp < expireBefore);

            if (_reverseIndex != null)
            {
                foreach (var expired in resultBeans)
                {
                    _reverseIndex.Remove(expired);
                }
            }

            _size -= resultBeans.Count;
            return resultBeans;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns event iterator.
        /// </summary>
        /// <returns>iterator over events currently in window</returns>
        public IEnumerator<EventBean> GetEnumerator()
        {
            return new TimeWindowEnumerator(_window);
        }

        /// <summary>
        /// Returns the oldest timestamp in the collection if there is at 
        /// least one entry, else it returns null if the window is empty.
        /// </summary>
        /// <value>null if empty, oldest timestamp if not empty</value>
        public long? OldestTimestamp
        {
            get
            {
                if (_window.IsEmpty())
                {
                    return null;
                }
                if (_window.First.EventHolder != null)
                {
                    return _window.First.Timestamp;
                }
                foreach (var pair in _window)
                {
                    if (pair.EventHolder != null)
                    {
                        return pair.Timestamp;
                    }
                }
                return null;
            }
        }

        /// <summary>Returns true if the window is currently empty. </summary>
        /// <returns>true if empty, false if not</returns>
        public bool IsEmpty()
        {
            return OldestTimestamp == null;
        }

        /// <summary>Returns the reverse index, for testing purposes. </summary>
        /// <value>reverse index</value>
        public IDictionary<EventBean, TimeWindowPair> ReverseIndex
        {
            get { return _reverseIndex; }
            set { _reverseIndex = value; }
        }

        public ArrayDeque<TimeWindowPair> Window
        {
            get { return _window; }
        }

        public void SetWindow(ArrayDeque<TimeWindowPair> window, int size)
        {
            _window = window;
            _size = size;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor, DataWindowViewFactory viewFactory)
        {
            viewDataVisitor.VisitPrimary(_window, false, viewFactory.ViewName, _size);
        }
    }
}