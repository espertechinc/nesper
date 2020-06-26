///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.intersect;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.union
{
    /// <summary>
    ///     A view that represents a union of multiple data windows.
    ///     <para />
    ///     The view is parameterized by two or more data windows. From an external viewpoint, the
    ///     view retains all events that is in any of the data windows (a union).
    /// </summary>
    public class UnionView : ViewSupport,
        LastPostObserver,
        AgentInstanceMgmtCallback,
        DataWindowView,
        ViewDataVisitableContainer
    {
        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly EventBean[][] oldEventsPerView;
        private readonly IList<EventBean> removalEvents = new List<EventBean>();
        protected internal readonly RefCountedSet<EventBean> unionWindow;
        protected internal readonly View[] views;
        private bool isDiscardObserverEvents;

        private bool isHasRemovestreamData;
        private bool isRetainObserverEvents;

        public UnionView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            UnionViewFactory factory,
            IList<View> viewList)
        {
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
            ViewFactory = factory;
            views = viewList.ToArray();
            unionWindow = new RefCountedSet<EventBean>();
            oldEventsPerView = new EventBean[viewList.Count][];

            for (var i = 0; i < viewList.Count; i++) {
                var view = new LastPostObserverView(i);
                views[i].Child = view;
                view.Observer = this;
            }

            // recover
            for (var i = 0; i < views.Length; i++) {
                var viewSnapshot = views[i].GetEnumerator();
                while (viewSnapshot.MoveNext()) {
                    EventBean theEvent = viewSnapshot.Current;
                    unionWindow.Add(theEvent);
                }
            }
        }

        public View[] ViewContained => views;

        public UnionViewFactory ViewFactory { get; }

        public void Stop(AgentInstanceStopServices services)
        {
            foreach (var view in views) {
                if (view is AgentInstanceMgmtCallback) {
                    ((AgentInstanceMgmtCallback) view).Stop(services);
                }
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, ViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(ViewFactory, newData, oldData);

            OneEventCollection oldDataColl = null;
            if (oldData != null) {
                isDiscardObserverEvents = true; // disable reaction logic in observer

                try {
                    foreach (var view in views) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, oldData);
                        view.Update(null, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    isDiscardObserverEvents = false;
                }

                // remove from union
                foreach (var oldEvent in oldData) {
                    unionWindow.RemoveAll(oldEvent);
                }

                oldDataColl = new OneEventCollection();
                oldDataColl.Add(oldData);
            }

            // add new event to union
            if (newData != null) {
                foreach (var newEvent in newData) {
                    unionWindow.Add(newEvent, views.Length);
                }

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                isHasRemovestreamData = false; // changed by observer logic to indicate new data
                isRetainObserverEvents = true; // enable retain logic in observer
                try {
                    foreach (var view in views) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newData, null);
                        view.Update(newData, null);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    isRetainObserverEvents = false;
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (isHasRemovestreamData) {
                    IList<EventBean> removedEvents = null;

                    // process each buffer
                    for (var i = 0; i < oldEventsPerView.Length; i++) {
                        if (oldEventsPerView[i] == null) {
                            continue;
                        }

                        var viewOldData = oldEventsPerView[i];
                        oldEventsPerView[i] = null; // clear entry

                        // remove events for union, if the last event was removed then add it
                        foreach (var old in viewOldData) {
                            var isNoMoreRef = unionWindow.Remove(old);
                            if (isNoMoreRef) {
                                if (removedEvents == null) {
                                    removalEvents.Clear();
                                    removedEvents = removalEvents;
                                }

                                removedEvents.Add(old);
                            }
                        }
                    }

                    if (removedEvents != null) {
                        if (oldDataColl == null) {
                            oldDataColl = new OneEventCollection();
                        }

                        foreach (var oldItem in removedEvents) {
                            oldDataColl.Add(oldItem);
                        }
                    }
                }
            }

            if (child != null) {
                // indicate new and, possibly, old data
                var oldEvents = oldDataColl != null ? oldDataColl.ToArray() : null;
                agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newData, oldEvents);
                child.Update(newData, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType => ViewFactory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return unionWindow.Keys.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException("Must visit container");
        }

        public void NewData(
            int streamId,
            EventBean[] newEvents,
            EventBean[] oldEvents)
        {
            if (oldEvents == null || isDiscardObserverEvents) {
                return;
            }

            if (isRetainObserverEvents) {
                oldEventsPerView[streamId] = oldEvents;
                isHasRemovestreamData = true;
                return;
            }

            // handle time-based removal
            IList<EventBean> removedEvents = null;

            // remove events for union, if the last event was removed then add it
            foreach (var old in oldEvents) {
                var isNoMoreRef = unionWindow.Remove(old);
                if (isNoMoreRef) {
                    if (removedEvents == null) {
                        removalEvents.Clear();
                        removedEvents = removalEvents;
                    }

                    removedEvents.Add(old);
                }
            }

            if (removedEvents != null) {
                var removed = removedEvents.ToArray();
                agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, removed);
                child.Update(null, removed);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            IntersectDefaultView.VisitViewContained(viewDataVisitor, ViewFactory, views);
        }
        
        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace