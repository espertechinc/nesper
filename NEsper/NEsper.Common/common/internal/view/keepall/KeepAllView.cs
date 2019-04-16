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
        internal readonly AgentInstanceContext agentInstanceContext;
        private readonly KeepAllViewFactory keepAllViewFactory;
        protected LinkedHashSet<EventBean> indexedEvents;
        protected ViewUpdatedCollection viewUpdatedCollection;

        public KeepAllView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            KeepAllViewFactory keepAllViewFactory,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
            this.keepAllViewFactory = keepAllViewFactory;
            indexedEvents = new LinkedHashSet<EventBean>();
            this.viewUpdatedCollection = viewUpdatedCollection;
        }

        public ViewFactory ViewFactory => keepAllViewFactory;

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => indexedEvents.IsEmpty();

        /// <summary>
        ///     Returns the (optional) collection handling random access to window contents for prior or previous events.
        /// </summary>
        /// <returns>buffer for events</returns>
        public ViewUpdatedCollection ViewUpdatedCollection => viewUpdatedCollection;

        public LinkedHashSet<EventBean> IndexedEvents => indexedEvents;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, keepAllViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(keepAllViewFactory, newData, oldData);

            if (newData != null) {
                foreach (var newEvent in newData) {
                    indexedEvents.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    indexedEvents.Remove(anOldData);
                    InternalHandleRemoved(anOldData);
                }
            }

            // update event buffer for access by expressions, if any
            if (viewUpdatedCollection != null) {
                viewUpdatedCollection.Update(newData, oldData);
            }

            agentInstanceContext.InstrumentationProvider.QViewIndicate(keepAllViewFactory, newData, oldData);
            child.Update(newData, oldData);
            agentInstanceContext.InstrumentationProvider.AViewIndicate();

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return indexedEvents.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(indexedEvents, true, keepAllViewFactory.ViewName, null);
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