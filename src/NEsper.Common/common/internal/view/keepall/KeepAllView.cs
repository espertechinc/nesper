///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.keepall
{
    /// <summary>
    ///     This view is a keep-all data window that simply keeps all events added.
    ///     It in addition allows to remove events efficiently for the remove-stream events received by the view.
    /// </summary>
    public class KeepAllView : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly KeepAllViewFactory _keepAllViewFactory;
        private readonly LinkedHashSet<EventBean> _indexedEvents;
        private readonly ViewUpdatedCollection _viewUpdatedCollection;

        public KeepAllView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            KeepAllViewFactory keepAllViewFactory,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            _agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
            _keepAllViewFactory = keepAllViewFactory;
            _indexedEvents = new LinkedHashSet<EventBean>();
            _viewUpdatedCollection = viewUpdatedCollection;
        }

        public ViewFactory ViewFactory => _keepAllViewFactory;

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => _indexedEvents.IsEmpty();

        /// <summary>
        ///     Returns the (optional) collection handling random access to window contents for prior or previous events.
        /// </summary>
        /// <returns>buffer for events</returns>
        public ViewUpdatedCollection ViewUpdatedCollection => _viewUpdatedCollection;

        public LinkedHashSet<EventBean> IndexedEvents => _indexedEvents;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _keepAllViewFactory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_keepAllViewFactory, newData, oldData);

            if (newData != null) {
                foreach (var newEvent in newData) {
                    _indexedEvents.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    _indexedEvents.Remove(anOldData);
                    InternalHandleRemoved(anOldData);
                }
            }

            // update event buffer for access by expressions, if any
            _viewUpdatedCollection?.Update(newData, oldData);

            _agentInstanceContext.InstrumentationProvider.QViewIndicate(_keepAllViewFactory, newData, oldData);
            child.Update(newData, oldData);
            _agentInstanceContext.InstrumentationProvider.AViewIndicate();

            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _indexedEvents.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_indexedEvents, true, _keepAllViewFactory.ViewName, null);
        }

        public void InternalHandleAdded(EventBean newEvent)
        {
            // no action required
        }

        public void InternalHandleRemoved(EventBean oldEvent)
        {
            // no action required
        }
    }
} // end of namespace