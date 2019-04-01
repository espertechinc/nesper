///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.intersect;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.view.union
{
    /// <summary>
    ///     A view that represents a union of multiple data windows wherein at least one is asymetric:
    ///     it does not present a insert stream for each insert stream event received.
    ///     <para />
    ///     The view is parameterized by two or more data windows. From an external viewpoint, the
    ///     view retains all events that is in any of the data windows (a union).
    /// </summary>
    public class UnionAsymetricView : ViewSupport,
        LastPostObserver,
        AgentInstanceStopCallback,
        DataWindowView,
        ViewDataVisitableContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly ArrayDeque<EventBean> newEvents = new ArrayDeque<EventBean>();
        private readonly EventBean[][] oldEventsPerView;
        private readonly IList<EventBean> removalEvents = new List<EventBean>();
        protected internal readonly RefCountedSet<EventBean> unionWindow;
        protected internal readonly View[] views;
        private bool isDiscardObserverEvents;
        private bool isHasRemovestreamData;
        private bool isRetainObserverEvents;
        private EventBean[] newDataChildView;

        public UnionAsymetricView(
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
                for (; viewSnapshot.MoveNext();) {
                    var theEvent = viewSnapshot.Current;
                    unionWindow.Add(theEvent);
                }
            }
        }

        public UnionViewFactory ViewFactory { get; }

        public void Stop(AgentInstanceStopServices services)
        {
            foreach (var view in views) {
                if (view is AgentInstanceStopCallback) {
                    ((AgentInstanceStopCallback) view).Stop(services);
                }
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, ViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(ViewFactory, newData, oldData);

            // handle remove stream
            OneEventCollection oldDataColl = null;
            EventBean[] newDataPosted = null;
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
                var removedByView = new bool[newData.Length][]
                    .Fill(() => new bool[views.Length]);

                foreach (var newEvent in newData) {
                    unionWindow.Add(newEvent, views.Length);
                }

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                isHasRemovestreamData = false; // changed by observer logic to indicate new data
                isRetainObserverEvents = true; // enable retain logic in observer
                try {
                    for (var viewIndex = 0; viewIndex < views.Length; viewIndex++) {
                        var view = views[viewIndex];
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newData, null);
                        view.Update(newData, null);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();

                        // first-X asymetric view post no insert stream for events that get dropped, remove these
                        if (newDataChildView != null) {
                            for (var i = 0; i < newData.Length; i++) {
                                var found = false;
                                for (var j = 0; j < newDataChildView.Length; j++) {
                                    if (newDataChildView[i] == newData[i]) {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found) {
                                    removedByView[i][viewIndex] = true;
                                }
                            }
                        }
                        else {
                            for (var i = 0; i < newData.Length; i++) {
                                removedByView[i][viewIndex] = true;
                            }
                        }
                    }
                }
                finally {
                    isRetainObserverEvents = false;
                }

                // determine removed events, those that have a "true" in the remove by view index for all views
                removalEvents.Clear();
                for (var i = 0; i < newData.Length; i++) {
                    var allTrue = true;
                    for (var j = 0; j < views.Length; j++) {
                        if (!removedByView[i][j]) {
                            allTrue = false;
                            break;
                        }
                    }

                    if (allTrue) {
                        removalEvents.Add(newData[i]);
                        unionWindow.RemoveAll(newData[i]);
                    }
                }

                // remove if any
                if (!removalEvents.IsEmpty()) {
                    isDiscardObserverEvents = true;
                    var viewOldData = removalEvents.ToArray();
                    try {
                        for (var j = 0; j < views.Length; j++) {
                            agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, viewOldData);
                            views[j].Update(null, viewOldData);
                            agentInstanceContext.InstrumentationProvider.AViewIndicate();
                        }
                    }
                    finally {
                        isDiscardObserverEvents = false;
                    }
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

                newEvents.Clear();
                for (var i = 0; i < newData.Length; i++) {
                    if (!removalEvents.Contains(newData[i])) {
                        newEvents.Add(newData[i]);
                    }
                }

                if (!newEvents.IsEmpty()) {
                    newDataPosted = newEvents.ToArray();
                }
            }

            // indicate new and, possibly, old data
            if (Child != null && (newDataPosted != null || oldDataColl != null)) {
                var oldDataToPost = oldDataColl != null ? oldDataColl.ToArray() : null;
                agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newDataPosted, oldDataToPost);
                Child.Update(newDataPosted, oldDataToPost);
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

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            newDataChildView = newEvents;

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
                Child.Update(null, removed);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            IntersectDefaultView.VisitViewContained(viewDataVisitor, ViewFactory, views);
        }
    }
} // end of namespace