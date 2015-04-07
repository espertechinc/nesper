///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Event stream implementation that does not keep any window by itself of the events 
    /// coming into the stream, without the possibility to iterate the last event.
    /// </summary>
    public sealed class ZeroDepthStreamNoIterate : EventStream
    {
        private View[] _children = ViewSupport.EMPTY_VIEW_ARRAY;
        private readonly EventType _eventType;

        /// <summary>Ctor. </summary>
        /// <param name="eventType">type of event</param>
        public ZeroDepthStreamNoIterate(EventType eventType)
        {
            _eventType = eventType;
        }

        public void Insert(EventBean[] events)
        {
            var length = _children.Length;
            for (int ii = 0; ii < length; ii++)
            {
                _children[ii].Update(events, null);
            }
        }

        public void Insert(EventBean theEvent)
        {
            // Get a new array created rather then re-use the old one since some client listeners
            // to this view may keep reference to the new data
            var row = new EventBean[]{theEvent};
            var length = _children.Length;
            for (int ii = 0; ii < length; ii++)
            {
                _children[ii].Update(row, null);
            }
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
            return CollectionUtil.NULL_EVENT_ITERATOR;
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
