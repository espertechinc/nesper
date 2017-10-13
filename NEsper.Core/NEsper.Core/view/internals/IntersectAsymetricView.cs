///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents an intersection of multiple data windows.
    /// <para />The view is parameterized by two or more data windows. From an external viewpoint, the
    /// view retains all events that is in all of the data windows at the same time (an intersection)
    /// and removes all events that leave any of the data windows.
    /// </summary>
    public class IntersectAsymetricView
        : ViewSupport
        , LastPostObserver
        , CloneableView
        , StoppableView
        , DataWindowView
        , IntersectViewMarker
        , ViewDataVisitableContainer
        , ViewContainer
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly IntersectViewFactory _factory;
        private readonly View[] _views;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="factory">the view factory</param>
        /// <param name="viewList">the list of data window views</param>
        public IntersectAsymetricView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _factory = factory;
            _views = viewList.ToArray();

            for (int i = 0; i < viewList.Count; i++)
            {
                var view = new LastPostObserverView(i);
                _views[i].RemoveAllViews();
                _views[i].AddView(view);
                view.Observer = this;
            }
        }

        public View[] ViewContained
        {
            get { return _views; }
        }

        public View CloneView()
        {
            return _factory.MakeView(_agentInstanceViewFactoryContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            IntersectAsymetricViewLocalState localState = _factory.GetAsymetricViewLocalStatePerThread();

            localState.OldEvents.Clear();
            EventBean[] newDataPosted = null;

            // handle remove stream
            if (oldData != null)
            {
                localState.IsDiscardObserverEvents = true; // disable reaction logic in observer
                try
                {
                    foreach (View view in _views)
                    {
                        view.Update(null, oldData);
                    }
                }
                finally
                {
                    localState.IsDiscardObserverEvents = false;
                }

                for (int i = 0; i < oldData.Length; i++)
                {
                    localState.OldEvents.Add(oldData[i]);
                }
            }

            if (newData != null)
            {
                localState.RemovalEvents.Clear();

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                localState.HasRemovestreamData = false; // changed by observer logic to indicate new data
                localState.IsRetainObserverEvents = true; // enable retain logic in observer
                try
                {
                    foreach (View view in _views)
                    {
                        localState.NewDataChildView = null;
                        view.Update(newData, oldData);

                        // first-X asymetric view post no insert stream for events that get dropped, remove these
                        if (localState.NewDataChildView != null)
                        {
                            for (int i = 0; i < newData.Length; i++)
                            {
                                bool found = false;
                                for (int j = 0; j < localState.NewDataChildView.Length; j++)
                                {
                                    if (localState.NewDataChildView[i] == newData[i])
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    localState.RemovalEvents.Add(newData[i]);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < newData.Length; i++)
                            {
                                localState.RemovalEvents.Add(newData[i]);
                            }
                        }
                    }
                }
                finally
                {
                    localState.IsRetainObserverEvents = false;
                }

                if (!localState.RemovalEvents.IsEmpty())
                {
                    localState.IsDiscardObserverEvents = true;
                    EventBean[] viewOldData = localState.RemovalEvents.ToArray();
                    try
                    {
                        for (int j = 0; j < _views.Length; j++)
                        {
                            _views[j].Update(null, viewOldData);
                        }
                    }
                    finally
                    {
                        localState.IsDiscardObserverEvents = false;
                    }
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (localState.HasRemovestreamData)
                {
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
                            for (int j = 0; j < _views.Length; j++)
                            {
                                if (i != j)
                                {
                                    _views[j].Update(null, viewOldData);
                                }
                            }
                        }
                        finally
                        {
                            localState.IsDiscardObserverEvents = false;
                        }
                    }

                    localState.OldEvents.AddAll(localState.RemovalEvents);
                }

                localState.NewEvents.Clear();
                for (int i = 0; i < newData.Length; i++)
                {
                    if (!localState.RemovalEvents.Contains(newData[i]))
                    {
                        localState.NewEvents.Add(newData[i]);
                    }
                }

                if (!localState.NewEvents.IsEmpty())
                {
                    newDataPosted = localState.NewEvents.ToArray();
                }

            }

            // indicate new and, possibly, old data
            EventBean[] oldDataPosted = null;
            if (!localState.OldEvents.IsEmpty())
            {
                oldDataPosted = localState.OldEvents.ToArray();
            }
            if ((newDataPosted != null) || (oldDataPosted != null))
            {
                UpdateChildren(newDataPosted, oldDataPosted);
            }
            localState.OldEvents.Clear();
        }

        public override EventType EventType
        {
            get { return _factory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _views[0].GetEnumerator();
        }

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            IntersectAsymetricViewLocalState localState = _factory.GetAsymetricViewLocalStatePerThread();
            localState.NewDataChildView = newEvents;

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
                for (int i = 0; i < _views.Length; i++)
                {
                    if (i != streamId)
                    {
                        _views[i].Update(null, oldEvents);
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
            foreach (var view in _views.OfType<StoppableView>())
            {
                view.Stop();
            }
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            IntersectDefaultView.VisitViewContained(viewDataVisitor, _factory, _views);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException();
        }

        public ViewFactory ViewFactory
        {
            get { return _factory; }
        }
    }
} // end of namespace
