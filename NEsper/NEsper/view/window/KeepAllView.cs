///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a keep-all data window that simply keeps all events added. It in 
    /// addition allows to remove events efficiently for the remove-stream events received 
    /// by the view.
    /// </summary>
    public class KeepAllView : ViewSupport, DataWindowView, CloneableView
    {
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;
        private readonly KeepAllViewFactory _keepAllViewFactory;
        private readonly LinkedHashSet<EventBean> _indexedEvents;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="keepAllViewFactory">for copying this view in a group-by</param>
        /// <param name="viewUpdatedCollection">for satisfying queries that select previous events in window order</param>
        public KeepAllView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext, KeepAllViewFactory keepAllViewFactory, ViewUpdatedCollection viewUpdatedCollection)
        {
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _keepAllViewFactory = keepAllViewFactory;
            _indexedEvents = new LinkedHashSet<EventBean>();
            _viewUpdatedCollection = viewUpdatedCollection;
        }

        public View CloneView()
        {
            return _keepAllViewFactory.MakeView(AgentInstanceViewFactoryContext);
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _indexedEvents.IsEmpty;
        }

        /// <summary>Returns the (optional) collection handling random access to window contents for prior or previous events. </summary>
        /// <value>buffer for events</value>
        public ViewUpdatedCollection ViewUpdatedCollection => _viewUpdatedCollection;

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _keepAllViewFactory.ViewName, newData, oldData); }

            if (newData != null)
            {
                foreach (EventBean newEvent in newData)
                {
                    _indexedEvents.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }
            }

            if (oldData != null)
            {
                foreach (EventBean anOldData in oldData)
                {
                    _indexedEvents.Remove(anOldData);
                    InternalHandleRemoved(anOldData);
                }
            }

            // Update event buffer for access by expressions, if any
            if (_viewUpdatedCollection != null)
            {
                _viewUpdatedCollection.Update(newData, oldData);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _keepAllViewFactory.ViewName, newData, oldData); }
            UpdateChildren(newData, oldData);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _indexedEvents.GetEnumerator();
        }

        public void InternalHandleAdded(EventBean newEvent)
        {
            // no action required
        }

        public void InternalHandleRemoved(EventBean oldEvent)
        {
            // no action required
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_indexedEvents, true, _keepAllViewFactory.ViewName, null);
        }

        public LinkedHashSet<EventBean> IndexedEvents => _indexedEvents;

        public ViewFactory ViewFactory => _keepAllViewFactory;
    }
}
