///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Provides relative access to insert stream events for certain window.
    /// </summary>
    public class IStreamRelativeAccess : RelativeAccessByEventNIndex, ViewUpdatedCollection
    {
        private readonly IDictionary<EventBean, int> _indexPerEvent;
        private EventBean[] _lastNewData;
        private readonly IStreamRelativeAccessUpdateObserver _updateObserver;
    
        /// <summary>Ctor. </summary>
        /// <param name="updateObserver">is invoked when updates are received</param>
        public IStreamRelativeAccess(IStreamRelativeAccessUpdateObserver updateObserver)
        {
            _updateObserver = updateObserver;
            _indexPerEvent = new Dictionary<EventBean, int>();
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            _updateObserver.Updated(this, newData);
            _indexPerEvent.Clear();
            _lastNewData = newData;
    
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    _indexPerEvent.Put(newData[i], i);
                }
            }
        }
    
        public EventBean GetRelativeToEvent(EventBean theEvent, int prevIndex)
        {
            if (_lastNewData == null)
            {
                return null;
            }
    
            if (prevIndex == 0)
            {
                return theEvent;
            }

            int indexIncoming;

            if (!_indexPerEvent.TryGetValue(theEvent, out indexIncoming))
            {
                return null;
            }
    
            if (prevIndex > indexIncoming)
            {
                return null;
            }
    
            int relativeIndex = indexIncoming - prevIndex;
            if ((relativeIndex < _lastNewData.Length) && (relativeIndex >= 0))
            {
                return _lastNewData[relativeIndex];
            }
            return null;
        }
    
        public EventBean GetRelativeToEnd(int prevIndex)
        {
            if (_lastNewData == null)
            {
                return null;
            }
    
            if (prevIndex < _lastNewData.Length && prevIndex >= 0)
            {
                return _lastNewData[prevIndex];
            }
            return null;
        }
    
        public IEnumerator<EventBean> GetWindowToEvent()
        {
            return Enumerable.Reverse(_lastNewData).GetEnumerator();
        }
    
        public ICollection<EventBean> GetWindowToEventCollReadOnly()
        {
            return _lastNewData;
        }
    
        public int GetWindowToEventCount()
        {
            if (_lastNewData == null) {
                return 0;
            }
            return _lastNewData.Length;
        }
    
        public void Destroy()
        {
            // No action required
        }

        public int NumEventsInsertBuf => _indexPerEvent.Count;
    }

    public interface IStreamRelativeAccessUpdateObserver
    {
        /// <summary>
        /// For indicating that the collection has been updated.
        /// <param name="iStreamRelativeAccess">is the collection</param>
        /// <param name="newData">is the new data available</param>
        /// </summary>
        void Updated(RelativeAccessByEventNIndex iStreamRelativeAccess, EventBean[] newData);
    }

    public class ProxyIStreamRelativeAccessUpdateObserver : IStreamRelativeAccessUpdateObserver
    {
        public Action<RelativeAccessByEventNIndex, EventBean[]> ProcUpdated;

        public ProxyIStreamRelativeAccessUpdateObserver()
        {
        }

        public ProxyIStreamRelativeAccessUpdateObserver(Action<RelativeAccessByEventNIndex, EventBean[]> procUpdated)
        {
            ProcUpdated = procUpdated;
        }

        public void Updated(RelativeAccessByEventNIndex iStreamRelativeAccess, EventBean[] newData)
        {
            ProcUpdated.Invoke(iStreamRelativeAccess, newData);
        }
    }
}
