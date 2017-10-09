///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.view
{
    /// <summary>
    /// Unit test class for view testing that : the EventStream interface to which views 
    /// can be attached as child views. The schema class is passed in during construction. 
    /// The stream behaves much like a lenght window in that it keeps a reference to the 
    /// last X inserted events in the past. The Update method reflects new events added 
    /// and events pushed out of the window. This is useful for view testing of views that 
    /// use the oldData values supplied in the Update method.
    /// </summary>
    public class SupportStreamImpl : EventStream
    {
        private readonly EventType _eventType;
        private readonly int _depth;
    
        private readonly LinkedList<EventBean> _events;
        private View[] _childViews;
    
        public SupportStreamImpl(Type clazz, int depth)
        {
            _eventType = SupportEventTypeFactory.CreateBeanType(clazz);
            _depth = depth;

            _events = new LinkedList<EventBean>();
            _childViews = ViewSupport.EMPTY_VIEW_ARRAY;
        }
    
        /// <summary>Set a single event to the stream </summary>
        /// <param name="theEvent"></param>
        public void Insert(EventBean theEvent)
        {
            _events.AddLast(theEvent);
    
            EventBean[] oldEvents = null;
            if (_events.Count > _depth)
            {
                oldEvents = new EventBean[] { _events.PopFront() };
            }
    
            foreach (View child in _childViews)
            {
                child.Update(new EventBean[] {theEvent}, oldEvents);
            }
        }
    
        /// <summary>Set a bunch of events to the stream </summary>
        /// <param name="eventArray"></param>
        public void Insert(EventBean[] eventArray)
        {
            foreach (EventBean theEvent in eventArray)
            {
                _events.AddLast(theEvent);
            }
    
            EventBean[] oldEvents = null;
            int expiredCount = _events.Count - _depth;
            if (expiredCount > 0)
            {
                oldEvents = new EventBean[expiredCount];
                for (int i = 0; i < expiredCount; i++)
                {
                    oldEvents[i] = _events.PopFront();
                }
            }
    
            foreach (View child in _childViews)
            {
                child.Update(eventArray, oldEvents);
            }
        }

        public object this[long index]
        {
            get
            {
                if ((index > int.MaxValue) || (index < int.MinValue))
                {
                    throw new ArgumentException("MapIndex not within int range supported by this implementation");
                }
                return _events.Skip((int) index).First();
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
            Log.Info(".iterator Not yet implemented");
            return null;
        }
    
        public View AddView(View view)
        {
            _childViews = ViewSupport.AddView(_childViews, view);
            view.Parent = this;
            return view;
        }

        public View[] Views
        {
            get { return _childViews; }
        }

        public bool RemoveView(View view)
        {
            int index = ViewSupport.FindViewIndex(_childViews, view);
            if (index == -1) {
                return false;
            }
            _childViews = ViewSupport.RemoveView(_childViews, index);
            view.Parent = null;
            return true;
        }
    
        public void RemoveAllViews()
        {
            _childViews = ViewSupport.EMPTY_VIEW_ARRAY;
        }

        public bool HasViews
        {
            get { return _childViews.Length > 0; }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
