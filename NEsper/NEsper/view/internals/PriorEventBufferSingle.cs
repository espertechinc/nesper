///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Buffers view-posted insert stream (new data) and remove stream (old data) events 
    /// for use with serving prior results in these streams, for a single prior event. 
    /// <para/>
    /// Buffers only exactly those events in new data and old data that are being asked for via
    /// the 2 or more 'prior' functions that specify different indexes. For 
    /// example "select Prior(2, price), Prior(1, price)" results in on buffer instance handling 
    /// both the need to the immediatly prior (1) and the 2-events-ago event (2). 
    /// <para/>
    /// As all views are required to post new data and post old data that removes the new data to 
    /// subsequent views, this buffer can be attached to all views and should not result in a memory leak.
    /// <para/>
    /// When the buffer receives old data (rstream) events it removes the prior events to the rstream
    /// events from the buffer the next time it receives a post (not immediatly) to allow queries to 
    /// the buffer.
    /// </summary>
    public class PriorEventBufferSingle : ViewUpdatedCollection, RelativeAccessByEventNIndex
    {
        private readonly int _priorEventIndex;
        private readonly IDictionary<EventBean, EventBean> _priorEventMap;
        private readonly RollingEventBuffer _newEvents;
        private EventBean[] _lastOldData;
    
        /// <summary>Ctor. </summary>
        /// <param name="priorEventIndex">is the number-of-events prior to the current event we are interested in</param>
        public PriorEventBufferSingle(int priorEventIndex)
        {
            _priorEventIndex = priorEventIndex;
            // Construct a rolling buffer of new data for holding max index + 1 (position 1 requires 2 events to keep)
            _newEvents = new RollingEventBuffer(priorEventIndex + 1);
            _priorEventMap = new Dictionary<EventBean, EventBean>();
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
    
                    EventBean priorEvent = _newEvents.Get(_priorEventIndex);
                    _priorEventMap.Put(newEvent, priorEvent);
                }
            }
    
            // Save old data to be removed next time we get posted results
            _lastOldData = oldData;
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData, PriorEventBufferChangeCaptureSingle captureSingle)
        {
            // Remove last old data posted in previous post
            if (_lastOldData != null)
            {
                for (int i = 0; i < _lastOldData.Length; i++)
                {
                    EventBean oldDataItem = _lastOldData[i];
                    _priorEventMap.Remove(oldDataItem);
                    captureSingle.Removed(oldDataItem);
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
    
                    EventBean priorEvent = _newEvents.Get(_priorEventIndex);
                    _priorEventMap.Put(newEvent, priorEvent);
                    captureSingle.Added(newEvent, priorEvent);
                }
            }
    
            // Save old data to be removed next time we get posted results
            _lastOldData = oldData;
        }
    
        // Users are assigned an index
        public EventBean GetRelativeToEvent(EventBean theEvent, int priorToIndex)
        {
            if (priorToIndex != 0)
            {
                throw new ArgumentException("Single prior event buffer takes only a given index of zero");
            }
            return _priorEventMap.Get(theEvent);
        }
    
        public EventBean GetRelativeToEnd(int index)
        {
            // No requirement to index from end of current buffer
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowToEvent()
        {
            // no requirement for window iterator support
            return null;
        }
    
        public int GetWindowToEventCount()
        {
            // no requirement for count support
            return 0;
        }
    
        public ICollection<EventBean> GetWindowToEventCollReadOnly() {
            return null;
        }

        public IDictionary<EventBean, EventBean> PriorEventMap
        {
            get { return _priorEventMap; }
        }

        public RollingEventBuffer NewEvents
        {
            get { return _newEvents; }
        }

        public void Destroy()
        {
            // No action required
        }

        public int NumEventsInsertBuf
        {
            get { return _newEvents.Count; }
        }
    }
}
