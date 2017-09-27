///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents an intersection of multiple data windows.
    /// <para />The view is parameterized by two or more data windows. From an external viewpoint, the
    /// view retains all events that is in all of the data windows at the same time (an intersection)
    /// and removes all events that leave any of the data windows.
    /// </summary>
    public class IntersectDefaultView
        : ViewSupport
        , LastPostObserver
        , CloneableView
        , StoppableView
        , DataWindowView
        , IntersectViewMarker
        , ViewDataVisitableContainer
        , ViewContainer
    {
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;
        private readonly IntersectViewFactory _factory;
        protected readonly View[] mViews;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="factory">the view _factory</param>
        /// <param name="viewList">the list of data window views</param>
        public IntersectDefaultView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _factory = factory;
            mViews = viewList.ToArray();

            for (int i = 0; i < viewList.Count; i++)
            {
                LastPostObserverView view = new LastPostObserverView(i);
                mViews[i].RemoveAllViews();
                mViews[i].AddView(view);
                view.Observer = this;
            }
        }

        public View[] ViewContained
        {
            get { return mViews; }
        }

        public View CloneView()
        {
            return _factory.MakeView(AgentInstanceViewFactoryContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QViewProcessIRStream(this, _factory.ViewName, newData, oldData);
            }

            IntersectDefaultViewLocalState localState = _factory.GetDefaultViewLocalStatePerThread();

            if (newData != null)
            {
                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                localState.HasRemovestreamData = false; // changed by observer logic to indicate new data
                localState.IsRetainObserverEvents = true; // enable retain logic in observer
                try
                {
                    foreach (View view in mViews)
                    {
                        view.Update(newData, oldData);
                    }
                }
                finally
                {
                    localState.IsRetainObserverEvents = false;
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (localState.HasRemovestreamData)
                {
                    localState.RemovalEvents.Clear();

                    // process each buffer
                    for (int i = 0; i < localState.OldEventsPerView.Length; i++)
                    {
                        if (localState.OldEventsPerView[i] == null)
                        {
                            continue;
                        }

                        EventBean[] viewOldData = localState.OldEventsPerView[i];
                        localState.OldEventsPerView[i] = null; // clear entry

                        // add each event to the set of events removed
                        foreach (EventBean oldEvent in viewOldData)
                        {
                            localState.RemovalEvents.Add(oldEvent);
                        }

                        localState.IsDiscardObserverEvents = true;
                        try
                        {
                            for (int j = 0; j < mViews.Length; j++)
                            {
                                if (i != j)
                                {
                                    mViews[j].Update(null, viewOldData);
                                }
                            }
                        }
                        finally
                        {
                            localState.IsDiscardObserverEvents = false;
                        }
                    }

                    oldData = localState.RemovalEvents.ToArray();
                }

                // indicate new and, possibly, old data
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, newData, oldData);
                }
                UpdateChildren(newData, oldData);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AViewIndicate();
                }
            }

            // handle remove stream
            else if (oldData != null)
            {
                localState.IsDiscardObserverEvents = true; // disable reaction logic in observer
                try
                {
                    foreach (View view in mViews)
                    {
                        view.Update(null, oldData);
                    }
                }
                finally
                {
                    localState.IsDiscardObserverEvents = false;
                }

                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QViewIndicate(this, _factory.ViewName, null, oldData);
                }
                UpdateChildren(null, oldData);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AViewIndicate();
                }
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AViewProcessIRStream();
            }
        }

        public override EventType EventType
        {
            get { return _factory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return mViews[0].GetEnumerator();
        }

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            IntersectDefaultViewLocalState localState = _factory.GetDefaultViewLocalStatePerThread();

            if ((oldEvents == null) || (localState.IsDiscardObserverEvents))
            {
                return;
            }

            if (localState.IsRetainObserverEvents)
            {
                localState.OldEventsPerView[streamId] = oldEvents;
                localState.HasRemovestreamData = true;
                return;
            }

            // remove old data from all other views
            localState.IsDiscardObserverEvents = true;
            try
            {
                for (int i = 0; i < mViews.Length; i++)
                {
                    if (i != streamId)
                    {
                        mViews[i].Update(null, oldEvents);
                    }
                }
            }
            finally
            {
                localState.IsDiscardObserverEvents = false;
            }

            UpdateChildren(null, oldEvents);
        }

        public void Stop()
        {
            foreach (View view in mViews)
            {
                if (view is StoppableView)
                {
                    ((StoppableView)view).Stop();
                }
            }
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            VisitViewContained(viewDataVisitor, _factory, mViews);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException();
        }

        public static void VisitViewContained(
            ViewDataVisitorContained viewDataVisitor,
            ViewFactory viewFactory,
            View[] views)
        {
            viewDataVisitor.VisitPrimary(viewFactory.ViewName, views.Length);
            for (int i = 0; i < views.Length; i++)
            {
                viewDataVisitor.VisitContained(i, views[i]);
            }
        }

        public ViewFactory ViewFactory
        {
            get { return _factory; }
        }
    }
} // end of namespace