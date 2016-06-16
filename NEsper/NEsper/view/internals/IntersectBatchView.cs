///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.events;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents an intersection of multiple data windows. 
    /// <para/>
    /// The view is parameterized by two or more data windows. From an external viewpoint, 
    /// the view retains all events that is in all of the data windows at the same time 
    /// (an intersection) and removes all events that leave any of the data windows. 
    /// <para/>
    /// This special batch-version has the following logic: 
    /// - only one batching view allowed as sub-view 
    /// - all externally-received newData events are inserted into each view 
    /// - all externally-received oldData events are removed from each view 
    /// - any non-batch view has its newData output ignored 
    /// - the single batch-view has its newData posted to child views, and removed from all non-batch views 
    /// - all oldData events received from all non-batch views are removed from each view
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly IntersectViewFactory _intersectViewFactory;
        private readonly EventType _eventType;
        private readonly View[] _views;
        private readonly int _batchViewIndex;
        private readonly EventBean[][] _oldEventsPerView;
        private readonly EventBean[][] _newEventsPerView;
        private readonly ISet<EventBean> _removedEvents = new LinkedHashSet<EventBean>();
        private readonly bool _hasAsymetric;
    
        private bool _captureIrNonBatch;
        private bool _ignoreViewIrStream;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="factory">the view factory</param>
        /// <param name="eventType">the parent event type</param>
        /// <param name="viewList">the list of data window views</param>
        /// <param name="viewFactories">view factories</param>
        /// <param name="hasAsymetric">if set to <c>true</c> [has asymetric].</param>
        public IntersectBatchView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            EventType eventType,
            IList<View> viewList,
            IList<ViewFactory> viewFactories,
            bool hasAsymetric)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _intersectViewFactory = factory;
            _eventType = eventType;
            _views = viewList.ToArray();
            _oldEventsPerView = new EventBean[viewList.Count][];
            _newEventsPerView = new EventBean[viewList.Count][];
            _hasAsymetric = hasAsymetric;
    
            // determine index of batch view
            _batchViewIndex = -1;
            for (int i = 0; i < viewFactories.Count; i++) {
                if (viewFactories[i] is DataWindowBatchingViewFactory) {
                    _batchViewIndex = i;
                }
            }
            if (_batchViewIndex == -1) {
                throw new IllegalStateException("Failed to find batch data window view");
            }
    
            for (int i = 0; i < viewList.Count; i++) {
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
            return _intersectViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // handle remove stream:post oldData to all views
            if (oldData != null && oldData.Length != 0)
            {
                try
                {
                    _ignoreViewIrStream = true;
                    for (int i = 0; i < _views.Length; i++)
                    {
                        _views[i].Update(newData, oldData);
                    }
                }
                finally
                {
                    _ignoreViewIrStream = false;
                }
            }

            if (newData != null)
            {
                // post to all non-batch views first to let them decide the remove stream, if any
                try
                {
                    _captureIrNonBatch = true;
                    for (int i = 0; i < _views.Length; i++)
                    {
                        if (i != _batchViewIndex)
                        {
                            _views[i].Update(newData, oldData);
                        }
                    }
                }
                finally
                {
                    _captureIrNonBatch = false;
                }

                // if there is any data removed from non-batch views, remove from all views
                // collect removed events
                _removedEvents.Clear();
                for (int i = 0; i < _views.Length; i++)
                {
                    if (_oldEventsPerView[i] != null)
                    {
                        for (int j = 0; j < _views.Length; j++)
                        {
                            if (i == j)
                            {
                                continue;
                            }
                            _views[j].Update(null, _oldEventsPerView[i]);

                            for (int k = 0; k < _oldEventsPerView[i].Length; k++)
                            {
                                _removedEvents.Add(_oldEventsPerView[i][k]);
                            }
                        }
                        _oldEventsPerView[i] = null;
                    }
                }

                // post only new events to the batch view that have not been removed
                EventBean[] newDataNonRemoved;
                if (_hasAsymetric)
                {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(newData, _removedEvents, _newEventsPerView);
                }
                else
                {
                    newDataNonRemoved = EventBeanUtility.GetNewDataNonRemoved(newData, _removedEvents);
                }
                if (newDataNonRemoved != null)
                {
                    _views[_batchViewIndex].Update(newDataNonRemoved, null);
                }
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _views[_batchViewIndex].GetEnumerator();
        }

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            if (_ignoreViewIrStream)
            {
                return;
            }

            if (_captureIrNonBatch)
            {
                _oldEventsPerView[streamId] = oldEvents;
                if (_hasAsymetric)
                {
                    _newEventsPerView[streamId] = newEvents;
                }
                return;
            }

            // handle case where irstream originates from view, i.e. timer-based
            if (streamId == _batchViewIndex)
            {
                UpdateChildren(newEvents, oldEvents);
                if (newEvents != null)
                {
                    try
                    {
                        _ignoreViewIrStream = true;
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
                        _ignoreViewIrStream = false;
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
                        _ignoreViewIrStream = true;
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
                        _ignoreViewIrStream = false;
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
            IntersectView.VisitViewContained(viewDataVisitor, _intersectViewFactory, _views);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException();
        }

        public ViewFactory ViewFactory
        {
            get { return _intersectViewFactory; }
        }
    }
}
