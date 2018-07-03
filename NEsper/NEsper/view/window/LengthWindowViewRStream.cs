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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a moving window extending the specified number of elements into the past, allowing 
    /// in addition to remove events efficiently for remove-stream events received by the view.
    /// </summary>
    public class LengthWindowViewRStream : ViewSupport, DataWindowView, CloneableView
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly LengthWindowViewFactory _lengthWindowViewFactory;
        private readonly int _size;
        private readonly LinkedHashSet<EventBean> _indexedEvents;

        /// <summary>
        /// Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="lengthWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="size">is the specified number of elements into the past</param>
        public LengthWindowViewRStream(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext, LengthWindowViewFactory lengthWindowViewFactory, int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("Illegal argument for size of length window");
            }
    
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _lengthWindowViewFactory = lengthWindowViewFactory;
            _size = size;
            _indexedEvents = new LinkedHashSet<EventBean>();
        }
    
        public View CloneView()
        {
            return _lengthWindowViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }
    
        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _indexedEvents.IsEmpty;
        }

        /// <summary>Returns the size of the length window. </summary>
        /// <value>size of length window</value>
        public int Size => _size;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _lengthWindowViewFactory.ViewName, newData, oldData);}
    
            EventBean[] expiredArr = null;
            if (oldData != null)
            {
                foreach (EventBean anOldData in oldData)
                {
                    _indexedEvents.Remove(anOldData);
                    InternalHandleRemoved(anOldData);
                }

                expiredArr = oldData;
            }
    
            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null)
            {
                foreach (EventBean newEvent in newData) {
                    _indexedEvents.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }
            }
    
            // Check for any events that get pushed out of the window
            int expiredCount = _indexedEvents.Count - _size;
            if (expiredCount > 0)
            {
                expiredArr = _indexedEvents.Take(expiredCount).ToArray();
                foreach (EventBean anExpired in expiredArr)
                {
                    _indexedEvents.Remove(anExpired);
                    InternalHandleExpired(anExpired);
                }
            }
    
            // If there are child views, call Update method
            if (HasViews)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _lengthWindowViewFactory.ViewName, newData, expiredArr);}
                UpdateChildren(newData, expiredArr);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }
    
        public void InternalHandleExpired(EventBean oldData)
        {
            // no action required
        }
    
        public void InternalHandleRemoved(EventBean expiredData)
        {
            // no action required
        }
    
        public void InternalHandleAdded(EventBean newData)
        {
            // no action required
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _indexedEvents.GetEnumerator();
        }
    
        public override String ToString()
        {
            return GetType().FullName + " size=" + _size;
        }
    
        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_indexedEvents, true, _lengthWindowViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _lengthWindowViewFactory;
    }
}
