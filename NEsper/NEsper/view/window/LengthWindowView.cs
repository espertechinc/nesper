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
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a moving window extending the specified number of elements into the past.
    /// </summary>
    public class LengthWindowView : ViewSupport, DataWindowView, CloneableView
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly LengthWindowViewFactory _lengthWindowViewFactory;
        private readonly int _size;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;
        private readonly ArrayDeque<EventBean> _events = new ArrayDeque<EventBean>();

        /// <summary>
        /// Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="lengthWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="size">is the specified number of elements into the past</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must Update when receiving events</param>
        public LengthWindowView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            LengthWindowViewFactory lengthWindowViewFactory,
            int size,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            if (size < 1)
            {
                throw new ArgumentException("Illegal argument for size of length window");
            }
    
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _lengthWindowViewFactory = lengthWindowViewFactory;
            _size = size;
            _viewUpdatedCollection = viewUpdatedCollection;
        }
    
        public View CloneView()
        {
            return _lengthWindowViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }
    
        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _events.IsEmpty();
        }

        /// <summary>Returns the size of the length window. </summary>
        /// <value>size of length window</value>
        public int Size => _size;

        /// <summary>Returns the (optional) collection handling random access to window contents for prior or previous events. </summary>
        /// <value>buffer for events</value>
        public ViewUpdatedCollection ViewUpdatedCollection => _viewUpdatedCollection;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _lengthWindowViewFactory.ViewName, newData, oldData); }

            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null)
            {
                for (int ii = 0; ii < newData.Length; ii++)
                {
                    _events.Add(newData[ii]);
                }
            }

            // Check for any events that get pushed out of the window
            int expiredCount = _events.Count - _size;
            EventBean[] expiredArr = null;
            if (expiredCount > 0)
            {
                expiredArr = new EventBean[expiredCount];
                for (int i = 0; i < expiredCount; i++)
                {
                    expiredArr[i] = _events.RemoveFirst();
                }
            }

            // Update event buffer for access by expressions, if any
            if (_viewUpdatedCollection != null)
            {
                _viewUpdatedCollection.Update(newData, expiredArr);
            }

            // If there are child views, call Update method
            if (HasViews)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _lengthWindowViewFactory.ViewName, newData, expiredArr); }
                UpdateChildren(newData, expiredArr);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _events.GetEnumerator();
        }
    
        public override String ToString()
        {
            return GetType().FullName + " size=" + _size;
        }
    
        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_events, true, _lengthWindowViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _lengthWindowViewFactory;
    }
}
