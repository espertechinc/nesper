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
using com.espertech.esper.core.context.util;
using com.espertech.esper.events;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents an intersection of multiple data windows.
    /// <para>
    /// The view is parameterized by two or more data windows. From an external viewpoint, the
    /// view retains all events that is in all of the data windows at the same time (an intersection)
    /// and removes all events that leave any of the data windows.
    /// </para>
    /// <para>
    /// This special batch-version has the following logic:
    /// - only one batching view allowed as sub-view
    /// - all externally-received newData events are inserted into each view
    /// - all externally-received oldData events are removed from each view
    /// - any non-batch view has its newData output ignored
    /// - the single batch-view has its newData posted to child views, and removed from all non-batch views
    /// - all oldData events received from all non-batch views are removed from each view
    /// </para>
    /// </summary>
    public class IntersectBatchView
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
        public IntersectBatchView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            IList<View> viewList)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _factory = factory;
            _views = viewList.ToArray();

            for (int i = 0; i < viewList.Count; i++)
            {
                LastPostObserverView view = new LastPostObserverView(i);
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
            IntersectBatchViewLocalState localState = _factory.GetBatchViewLocalStatePerThread();

            // handle remove stream: post oldData to all views
            if (oldData != null && oldData.Length != 0)
            {
                try
                {
                    localState.IsIgnoreViewIRStream = true;
                    for (int i = 0; i < _views.Length; i++)
                    {
                        _views[i].Update(newData, oldData);
                    }
                }
                finally
                {
                    localState.IsIgnoreViewIRStream = false;
                }
            }

            if (newData != null)
            {
                // post to all non-batch views first to let them decide the remove stream, if any
                try
                {
                    localState.IsCaptureIRNonBatch = true;
                    for (int i = 0; i < _views.Length; i++)
                    {
                        if (i != _factory.BatchViewIndex)
                        {
                            _views[i].Update(newData, oldData);
                        }
                    }
                }
                finally
                {
                    localState.IsCaptureIRNonBatch = false;
                }

                // if there is any data removed from non-batch views, remove from all views
                // collect removed events
                localState.RemovedEvents.Clear();
                for (int i = 0; i < _views.Length; i++)
                {
                    if (localState.OldEventsPerView[i] != null)
                    {
                        for (int j = 0; j < _views.Length; j++)
                        {
                            if (i == j)
                            {
                                continue;
                            }
                            _views[j].Update(null, localState.OldEventsPerView[i]);

                            for (int k = 0; k < localState.OldEventsPerView[i].Length; k++)
                            {
                                localState.RemovedEvents.Add(localState.OldEventsPerView[i][k]);
                            }
                        }
                        localState.OldEventsPerView[i] = null;
                    }
                }

                // post only new events to the batch view that have not been removed
                EventBean[] newDataNonRemoved;
                if (_factory.IsAsymmetric())
                {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(
                        newData, localState.RemovedEvents, localState.NewEventsPerView);
                }
                else
                {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(newData, localState.RemovedEvents);
                }
                if (newDataNonRemoved != null)
                {
                    _views[_factory.BatchViewIndex].Update(newDataNonRemoved, null);
                }
            }
        }

        public override EventType EventType
        {
            get { return _factory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _views[_factory.BatchViewIndex].GetEnumerator();
        }

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            IntersectBatchViewLocalState localState = _factory.GetBatchViewLocalStatePerThread();

            if (localState.IsIgnoreViewIRStream)
            {
                return;
            }

            if (localState.IsCaptureIRNonBatch)
            {
                localState.OldEventsPerView[streamId] = oldEvents;
                if (_factory.IsAsymmetric())
                {
                    localState.NewEventsPerView[streamId] = newEvents;
                }
                return;
            }

            // handle case where irstream originates from view, i.e. timer-based
            if (streamId == _factory.BatchViewIndex)
            {
                UpdateChildren(newEvents, oldEvents);
                if (newEvents != null)
                {
                    try
                    {
                        localState.IsIgnoreViewIRStream = true;
                        for (int i = 0; i < _views.Length; i++)
                        {
                            if (i != streamId)
                            {
                                _views[i].Update(null, newEvents);
                            }
                        }
                    }
                    finally
                    {
                        localState.IsIgnoreViewIRStream = false;
                    }
                }
            }
            // post remove stream to all other views
            else
            {
                if (oldEvents != null)
                {
                    try
                    {
                        localState.IsIgnoreViewIRStream = true;
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
                        localState.IsIgnoreViewIRStream = false;
                    }
                }
            }
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
