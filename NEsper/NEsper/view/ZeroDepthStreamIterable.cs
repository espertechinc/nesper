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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Event stream implementation that does not keep any window by itself of the events 
    /// coming into the stream, however is itself iterable and keeps the last event.
    /// </summary>
    public sealed class ZeroDepthStreamIterable : EventStream
    {
        private View[] _children = ViewSupport.EMPTY_VIEW_ARRAY;
        private readonly EventType _eventType;
        private EventBean _lastInsertedEvent;
        private EventBean[] _lastInsertedEvents;

        /// <summary>Ctor. </summary>
        /// <param name="eventType">type of event</param>
        public ZeroDepthStreamIterable(EventType eventType)
        {
            _eventType = eventType;
        }

        public void Insert(EventBean[] events)
        {
            foreach (View childView in _children)
            {
                childView.Update(events, null);
            }

            _lastInsertedEvents = events;
        }

        public void Insert(EventBean theEvent)
        {
            // Get a new array created rather then re-use the old one since some client listeners
            // to this view may keep reference to the new data
            var row = new EventBean[]{theEvent};
            foreach (View childView in _children)
            {
                childView.Update(row, null);
            }

            _lastInsertedEvent = theEvent;
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (_lastInsertedEvents != null)
            {
                for (int ii = 0; ii < _lastInsertedEvents.Length; ii++)
                    yield return _lastInsertedEvents[ii];
            }
            else if (_lastInsertedEvent != null)
            {
                yield return _lastInsertedEvent;
            }
        }

        public View AddView(View view)
        {
            _children = ViewSupport.AddView(_children, view);
            view.Parent = this;
            return view;
        }

        public View[] Views
        {
            get { return _children; }
        }

        public bool RemoveView(View view)
        {
            int index = ViewSupport.FindViewIndex(_children, view);
            if (index == -1) {
                return false;
            }
            _children = ViewSupport.RemoveView(_children, index);
            view.Parent = null;
            return true;
        }

        public bool HasViews
        {
            get { return _children.Length > 0; }
        }

        public void RemoveAllViews()
        {
            _children = ViewSupport.EMPTY_VIEW_ARRAY;
        }
    }
}
