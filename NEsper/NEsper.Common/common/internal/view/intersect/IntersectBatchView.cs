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
using com.espertech.esper.common.@internal.@event.core;
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
    ///     <para />
    ///     This special batch-version has the following logic:
    ///     - only one batching view allowed as sub-view
    ///     - all externally-received newData events are inserted into each view
    ///     - all externally-received oldData events are removed from each view
    ///     - any non-batch view has its newData output ignored
    ///     - the single batch-view has its newData posted to child views, and removed from all non-batch views
    ///     - all oldData events received from all non-batch views are removed from each view
    /// </summary>
    public class IntersectBatchView : ViewSupport,
        LastPostObserver,
        AgentInstanceMgmtCallback,
        DataWindowView,
        IntersectViewMarker,
        ViewDataVisitableContainer
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly IntersectViewFactory factory;
        internal readonly View[] views;

        public IntersectBatchView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.factory = factory;
            views = viewList.ToArray();

            for (var i = 0; i < viewList.Count; i++) {
                var view = new LastPostObserverView(i);
                views[i].Child = view;
                view.Observer = this;
            }
        }

        public View[] ViewContained => views;

        public ViewFactory ViewFactory => factory;

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
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            var localState = factory.BatchViewLocalStatePerThread;

            // handle remove stream: post oldData to all views
            if (oldData != null && oldData.Length != 0) {
                try {
                    localState.IsIgnoreViewIRStream = true;
                    for (var i = 0; i < views.Length; i++) {
                        var view = views[i];
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                        view.Update(newData, oldData);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
                finally {
                    localState.IsIgnoreViewIRStream = false;
                }
            }

            if (newData != null) {
                // post to all non-batch views first to let them decide the remove stream, if any
                try {
                    localState.IsCaptureIRNonBatch = true;
                    for (var i = 0; i < views.Length; i++) {
                        if (i != factory.BatchViewIndex) {
                            var view = views[i];
                            agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldData);
                            view.Update(newData, oldData);
                            agentInstanceContext.InstrumentationProvider.AViewIndicate();
                        }
                    }
                }
                finally {
                    localState.IsCaptureIRNonBatch = false;
                }

                // if there is any data removed from non-batch views, remove from all views
                // collect removed events
                localState.RemovedEvents.Clear();
                for (var i = 0; i < views.Length; i++) {
                    if (localState.OldEventsPerView[i] != null) {
                        for (var j = 0; j < views.Length; j++) {
                            if (i == j) {
                                continue;
                            }

                            var view = views[j];
                            var oldEvents = localState.OldEventsPerView[i];

                            agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldEvents);
                            view.Update(null, oldEvents);
                            agentInstanceContext.InstrumentationProvider.AViewIndicate();

                            for (var k = 0; k < localState.OldEventsPerView[i].Length; k++) {
                                localState.RemovedEvents.Add(localState.OldEventsPerView[i][k]);
                            }
                        }

                        localState.OldEventsPerView[i] = null;
                    }
                }

                // post only new events to the batch view that have not been removed
                EventBean[] newDataNonRemoved;
                if (factory.IsAsymetric) {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(
                        newData,
                        localState.RemovedEvents,
                        localState.NewEventsPerView);
                }
                else {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(newData, localState.RemovedEvents);
                }

                if (newDataNonRemoved != null) {
                    var view = views[factory.BatchViewIndex];
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newDataNonRemoved, null);
                    view.Update(newDataNonRemoved, null);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override EventType EventType => factory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return views[factory.BatchViewIndex].GetEnumerator();
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
            var localState = factory.BatchViewLocalStatePerThread;

            if (localState.IsIgnoreViewIRStream) {
                return;
            }

            if (localState.IsCaptureIRNonBatch) {
                localState.OldEventsPerView[streamId] = oldEvents;
                if (factory.IsAsymetric) {
                    localState.NewEventsPerView[streamId] = newEvents;
                }

                return;
            }

            // handle case where irstream originates from view, i.e. timer-based
            if (streamId == factory.BatchViewIndex) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newEvents, oldEvents);
                child.Update(newEvents, oldEvents);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                if (newEvents != null) {
                    try {
                        localState.IsIgnoreViewIRStream = true;
                        for (var i = 0; i < views.Length; i++) {
                            if (i != streamId) {
                                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, newEvents);
                                views[i].Update(null, newEvents);
                                agentInstanceContext.InstrumentationProvider.AViewIndicate();
                            }
                        }
                    }
                    finally {
                        localState.IsIgnoreViewIRStream = false;
                    }
                }
            }
            else {
                // post remove stream to all other views
                if (oldEvents != null) {
                    try {
                        localState.IsIgnoreViewIRStream = true;
                        for (var i = 0; i < views.Length; i++) {
                            if (i != streamId) {
                                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldEvents);
                                views[i].Update(null, oldEvents);
                                agentInstanceContext.InstrumentationProvider.AViewIndicate();
                            }
                        }
                    }
                    finally {
                        localState.IsIgnoreViewIRStream = false;
                    }
                }
            }
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            IntersectDefaultView.VisitViewContained(viewDataVisitor, factory, views);
        }
    }
} // end of namespace