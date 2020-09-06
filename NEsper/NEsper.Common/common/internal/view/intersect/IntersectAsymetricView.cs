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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    ///     A view that represents an intersection of multiple data windows.
    ///     <para />
    ///     The view is parameterized by two or more data windows. From an external viewpoint, the
    ///     view retains all events that is in all of the data windows at the same time (an intersection)
    ///     and removes all events that leave any of the data windows.
    /// </summary>
    public class IntersectAsymetricView : ViewSupport,
        LastPostObserver,
        AgentInstanceMgmtCallback,
        DataWindowView,
        ViewDataVisitableContainer,
        IntersectViewMarker
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly View[] views;

        public IntersectAsymetricView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            ViewFactory = factory;
            views = viewList.ToArray();

            for (var i = 0; i < viewList.Count; i++) {
                var view = new LastPostObserverView(i);
                views[i].Child = view;
                view.Observer = this;
            }
        }

        public View[] ViewContained => views;

        public IntersectViewFactory ViewFactory { get; }

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
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, ViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(ViewFactory, newData, oldData);

            var localState = ViewFactory.AsymetricViewLocalStatePerThread;

            localState.OldEvents.Clear();
            EventBean[] newDataPosted = null;

            // handle remove stream
            if (oldData != null) {
                localState.IsDiscardObserverEvents = true; // disable reaction logic in observer
                try {
                    foreach (var view in views) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, oldData);
                        view.Update(null, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    localState.IsDiscardObserverEvents = false;
                }

                for (var i = 0; i < oldData.Length; i++) {
                    localState.OldEvents.Add(oldData[i]);
                }
            }

            if (newData != null) {
                localState.RemovalEvents.Clear();

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                localState.HasRemovestreamData = false; // changed by observer logic to indicate new data
                localState.IsRetainObserverEvents = true; // enable retain logic in observer
                try {
                    foreach (var view in views) {
                        localState.NewDataChildView = null;

                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newData, oldData);
                        view.Update(newData, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();

                        // first-X asymetric view post no insert stream for events that get dropped, remove these
                        if (localState.NewDataChildView != null) {
                            for (var i = 0; i < newData.Length; i++) {
                                var found = false;
                                for (var j = 0; j < localState.NewDataChildView.Length; j++) {
                                    if (localState.NewDataChildView[i] == newData[i]) {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found) {
                                    localState.RemovalEvents.Add(newData[i]);
                                }
                            }
                        }
                        else {
                            for (var i = 0; i < newData.Length; i++) {
                                localState.RemovalEvents.Add(newData[i]);
                            }
                        }
                    }
                }
                finally {
                    localState.IsRetainObserverEvents = false;
                }

                if (!localState.RemovalEvents.IsEmpty()) {
                    localState.IsDiscardObserverEvents = true;
                    var viewOldData = localState.RemovalEvents.ToArray();
                    try {
                        for (var j = 0; j < views.Length; j++) {
                            var view = views[j];
                            agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, viewOldData);
                            view.Update(null, viewOldData);
                            agentInstanceContext.InstrumentationProvider.AViewIndicate();
                        }
                    }
                    finally {
                        localState.IsDiscardObserverEvents = false;
                    }
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (localState.HasRemovestreamData) {
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
                                    var view = views[j];
                                    agentInstanceContext.InstrumentationProvider.QViewIndicate(
                                        ViewFactory,
                                        null,
                                        viewOldData);
                                    view.Update(null, viewOldData);
                                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                                }
                            }
                        }
                        finally {
                            localState.IsDiscardObserverEvents = false;
                        }
                    }

                    localState.OldEvents.AddAll(localState.RemovalEvents);
                }

                localState.NewEvents.Clear();
                for (var i = 0; i < newData.Length; i++) {
                    if (!localState.RemovalEvents.Contains(newData[i])) {
                        localState.NewEvents.Add(newData[i]);
                    }
                }

                if (!localState.NewEvents.IsEmpty()) {
                    newDataPosted = localState.NewEvents.ToArray();
                }
            }

            // indicate new and, possibly, old data
            EventBean[] oldDataPosted = null;
            if (!localState.OldEvents.IsEmpty()) {
                oldDataPosted = localState.OldEvents.ToArray();
            }

            if (newDataPosted != null || oldDataPosted != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newDataPosted, oldDataPosted);
                child.Update(newDataPosted, oldDataPosted);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            localState.OldEvents.Clear();

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType => ViewFactory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return views[0].GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException("Cannot visit as Contained");
        }

        public void NewData(
            int streamId,
            EventBean[] newEvents,
            EventBean[] oldEvents)
        {
            var localState = ViewFactory.AsymetricViewLocalStatePerThread;
            localState.NewDataChildView = newEvents;

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
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, oldEvents);
                        views[i].Update(null, oldEvents);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
            }
            finally {
                localState.IsDiscardObserverEvents = false;
            }

            agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, null, oldEvents);
            child.Update(null, oldEvents);
            agentInstanceContext.InstrumentationProvider.AViewIndicate();
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