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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    ///     A view that represents an intersection of multiple data windows.
    ///     <para />
    ///     The view is parameterized by two or more data windows. From an external viewpoint, the
    ///     view retains all events that is in all of the data windows at the same time (an intersection)
    ///     and removes all events that leave any of the data windows.
    /// </summary>
    public class IntersectDefaultView : ViewSupport,
        LastPostObserver,
        AgentInstanceMgmtCallback,
        DataWindowView,
        ViewDataVisitableContainer,
        IntersectViewMarker
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly IntersectViewFactory factory;
        internal readonly View[] views;

        public IntersectDefaultView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
            this.factory = factory;
            views = viewList.ToArray();

            for (var i = 0; i < viewList.Count; i++) {
                var view = new LastPostObserverView(i);
                views[i].Child = view;
                view.Observer = this;
            }
        }

        public View[] ViewContained => views;

        public IntersectViewFactory ViewFactory => factory;

        public void Stop(AgentInstanceStopServices services)
        {
            foreach (var view in views) {
                (view as AgentInstanceMgmtCallback)?.Stop(services);
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            var localState = factory.DefaultViewLocalStatePerThread;

            if (newData != null) {
                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                localState.HasRemovestreamData = false; // changed by observer logic to indicate new data
                localState.IsRetainObserverEvents = true; // enable retain logic in observer
                try {
                    foreach (var view in views) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                        view.Update(newData, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    localState.IsRetainObserverEvents = false;
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (localState.HasRemovestreamData) {
                    localState.RemovalEvents.Clear();

                    // process each buffer
                    for (var i = 0; i < localState.OldEventsPerView.Length; i++) {
                        if (localState.OldEventsPerView[i] == null) {
                            continue;
                        }

                        var viewOldData = localState.OldEventsPerView[i];
                        localState.OldEventsPerView[i] = null; // clear entry

                        // add each event to the set of events removed
                        foreach (var oldEvent in viewOldData) {
                            localState.RemovalEvents.Add(oldEvent);
                        }

                        localState.IsDiscardObserverEvents = true;
                        try {
                            for (var j = 0; j < views.Length; j++) {
                                if (i != j) {
                                    agentInstanceContext.InstrumentationProvider.QViewIndicate(
                                        factory,
                                        null,
                                        viewOldData);
                                    views[j].Update(null, viewOldData);
                                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                                }
                            }
                        }
                        finally {
                            localState.IsDiscardObserverEvents = false;
                        }
                    }

                    oldData = localState.RemovalEvents.ToArray();
                }

                // indicate new and, possibly, old data
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                child.Update(newData, oldData);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }
            else if (oldData != null) {
                // handle remove stream
                localState.IsDiscardObserverEvents = true; // disable reaction logic in observer
                try {
                    foreach (var view in views) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldData);
                        view.Update(null, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    localState.IsDiscardObserverEvents = false;
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldData);
                child.Update(null, oldData);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType => factory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return views[0].GetEnumerator();
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
            var localState = factory.DefaultViewLocalStatePerThread;

            if (oldEvents == null || localState.IsDiscardObserverEvents) {
                return;
            }

            if (localState.IsRetainObserverEvents) {
                localState.OldEventsPerView[streamId] = oldEvents;
                localState.HasRemovestreamData = true;
                return;
            }

            // remove old data from all other views
            localState.IsDiscardObserverEvents = true;
            try {
                for (var i = 0; i < views.Length; i++) {
                    if (i != streamId) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldEvents);
                        views[i].Update(null, oldEvents);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
            }
            finally {
                localState.IsDiscardObserverEvents = false;
            }

            agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldEvents);
            child.Update(null, oldEvents);
            agentInstanceContext.InstrumentationProvider.AViewIndicate();
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            VisitViewContained(viewDataVisitor, factory, views);
        }

        public static void VisitViewContained(
            ViewDataVisitorContained viewDataVisitor,
            ViewFactory viewFactory,
            View[] views)
        {
            viewDataVisitor.VisitPrimary(viewFactory.GetType().GetSimpleName(), views.Length);
            for (var i = 0; i < views.Length; i++) {
                viewDataVisitor.VisitContained(i, views[i]);
            }
        }
        
        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace