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
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Buffer class for insert stream events only for use with unbound streams that inserts 
    /// data only, to serve up one or more prior events in the insert stream based on an index.
    /// <para/>
    /// Does not expect or care about the remove stream and simple keeps a rolling buffer of 
    /// new data events up to the maximum prior event we are asking for.
    /// </summary>
    public class PriorEventBufferUnbound : ViewUpdatedCollection, RandomAccessByIndex
    {
        private readonly int _maxSize;
        private readonly RollingEventBuffer _newEvents;
    
        /// <summary>Ctor. </summary>
        /// <param name="maxPriorIndex">is the highest prior-event index required by any expression</param>
        public PriorEventBufferUnbound(int maxPriorIndex)
        {
            _maxSize = maxPriorIndex + 1;
            _newEvents = new RollingEventBuffer(_maxSize);
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            // Post new data to rolling buffer starting with the oldest
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    EventBean newEvent = newData[i];
    
                    // Add new event
                    _newEvents.Add(newEvent);
                }
            }
        }
    
        public EventBean GetNewData(int index)
        {
            if (index >= _maxSize)
            {
                throw new ArgumentException("MapIndex " + index + " not allowed, max size is " + _maxSize);
            }
            return _newEvents[index];
        }
    
        public EventBean GetOldData(int index)
        {
            return null;
        }
    
        public void Destroy()
        {
            // No action required
        }
    
        public EventBean GetNewDataTail(int index)
        {
            // No requirement to index from end of current buffer
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowEnumerator()
        {
            // no requirement for window iterator support
            return null;
        }

        public ICollection<EventBean> WindowCollectionReadOnly
        {
            get { return null; }
        }

        public int WindowCount
        {
            get
            {
                // no requirement for count support
                return 0;
            }
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
