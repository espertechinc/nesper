///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Buffers view-posted insert stream (new data) and remove stream (old data) events for
    /// use with determining prior results in these streams, for multiple different prior events. 
    /// <para/>
    /// Buffers only exactly those events in new data and old data that are being asked for via 
    /// the 2 or more 'prior' functions that specify different indexes. For example "select Prior(2, price), Prior(1, price)" 
    /// results in on buffer instance handling both the need to the immediatly prior (1) and the 2-events-ago event (2). 
    /// <para/>
    /// As all views are required to post new data and post old data that removes the new data to subsequent views, this buffer 
    /// can be attached to all views and should not result in a memory leak. 
    /// <para/>
    /// When the buffer receives old data (rstream) events it removes the prior events to the rstream events from the buffer 
    /// the next time it receives a post (not immediatly) to allow queries to the buffer.
    /// </summary>
    public class PriorEventBufferMulti : ViewUpdatedCollection, RelativeAccessByEventNIndex
    {
        private readonly int _priorToIndexesSize;
        private readonly int[] _priorToIndexes;
        private readonly IDictionary<EventBean, EventBean[]> _priorEventMap;
        private readonly RollingEventBuffer _newEvents;
        private EventBean[] _lastOldData;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="priorToIndexSet">holds a list of prior-event indexes.
        /// <para/>For example, an array {0,4,6} means the current event, 4 events before the current event and 6 events before the current event.
        /// </param>
        public PriorEventBufferMulti(int[] priorToIndexSet)
        {
            // Determine the maximum prior index to retain
            int maxPriorIndex = 0;
            foreach (int priorIndex in priorToIndexSet)
            {
                if (priorIndex > maxPriorIndex)
                {
                    maxPriorIndex = priorIndex;
                }
            }
    
            // Copy the set of indexes into an array, sort in ascending order
            _priorToIndexesSize = priorToIndexSet.Length;
            _priorToIndexes = new int[_priorToIndexesSize];
            int count = 0;
            foreach (int priorIndex in priorToIndexSet)
            {
                _priorToIndexes[count++] = priorIndex;
            }

            _priorToIndexes.SortInPlace();
    
            // Construct a rolling buffer of new data for holding max index + 1 (position 1 requires 2 events to keep)
            _newEvents = new RollingEventBuffer(maxPriorIndex + 1);
            _priorEventMap = new Dictionary<EventBean, EventBean[]>();
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            // Remove last old data posted in previous post
            if (_lastOldData != null)
            {
                for (int i = 0; i < _lastOldData.Length; i++)
                {
                    _priorEventMap.Remove(_lastOldData[i]);
                }
            }
    
            // Post new data to rolling buffer starting with the oldest
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    EventBean newEvent = newData[i];
    
                    // Add new event
                    _newEvents.Add(newEvent);
    
                    // Save prior index events in array
                    EventBean[] priorEvents = new EventBean[_priorToIndexesSize];
                    for (int j = 0; j < _priorToIndexesSize; j++)
                    {
                        int priorIndex = _priorToIndexes[j];
                        priorEvents[j] = _newEvents.Get(priorIndex);
                    }
                    _priorEventMap.Put(newEvent, priorEvents);
                }
            }
    
            // Save old data to be removed next time we get posted results
            _lastOldData = oldData;
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData, PriorEventBufferChangeCaptureMulti capture)
        {
            // Remove last old data posted in previous post
            if (_lastOldData != null)
            {
                for (int i = 0; i < _lastOldData.Length; i++)
                {
                    EventBean oldDataItem = _lastOldData[i];
                    _priorEventMap.Remove(oldDataItem);
                    capture.Removed(oldDataItem);
                }
            }
    
            // Post new data to rolling buffer starting with the oldest
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    EventBean newEvent = newData[i];
    
                    // Add new event
                    _newEvents.Add(newEvent);
    
                    // Save prior index events in array
                    EventBean[] priorEvents = new EventBean[_priorToIndexesSize];
                    for (int j = 0; j < _priorToIndexesSize; j++)
                    {
                        int priorIndex = _priorToIndexes[j];
                        priorEvents[j] = _newEvents.Get(priorIndex);
                    }
                    _priorEventMap.Put(newEvent, priorEvents);
                    capture.Added(newEvent, priorEvents);
                }
            }
    
            // Save old data to be removed next time we get posted results
            _lastOldData = oldData;
        }
    
        public EventBean GetRelativeToEvent(EventBean theEvent, int priorToIndex)
        {
            if (priorToIndex >= _priorToIndexesSize)
            {
                throw new ArgumentException("MapIndex " + priorToIndex + " not allowed, max size is " + _priorToIndexesSize);
            }
            EventBean[] priorEvents = _priorEventMap.Get(theEvent);
            if (priorEvents == null)
            {
                throw new IllegalStateException("Event not currently in collection, event=" + theEvent);
            }
            return priorEvents[priorToIndex];
        }
    
        public EventBean GetRelativeToEnd(EventBean theEvent, int index)
        {
            // No requirements to return events related to the end of the current buffer
            return null;
        }
    
        public int GetWindowToEventCount(EventBean evalEvent)
        {
            // No requirements to return events related to the end of the current buffer
            return 0;
        }
    
        public IEnumerator<EventBean> GetWindowToEvent(Object evalEvent)
        {
            // No requirements to return events related to the end of the current buffer
            return null;  
        }
    
        public ICollection<EventBean> GetWindowToEventCollReadOnly(Object evalEvent)
        {
            // No requirements to return events related to the end of the current buffer
            return null;
        }
    
        public void Destroy()
        {
            // No action required
        }

        public IDictionary<EventBean, EventBean[]> PriorEventMap
        {
            get { return _priorEventMap; }
        }

        public RollingEventBuffer NewEvents
        {
            get { return _newEvents; }
        }

        public int NumEventsInsertBuf
        {
            get { return _newEvents.Count; }
        }
    }
}
