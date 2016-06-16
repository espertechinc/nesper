///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// <para/>
    /// The view is parameterized by two or more data windows. From an external viewpoint, 
    /// the view retains all events that is in all of the data windows at the same time 
    /// (an intersection) and removes all events that leave any of the data windows.
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
        private readonly IntersectViewFactory _intersectViewFactory;
        private readonly EventType _eventType;
        private readonly View[] _views;
        private readonly EventBean[][] _oldEventsPerView;
        private readonly ICollection<EventBean> _removalEvents = new HashSet<EventBean>();
        private readonly ArrayDeque<EventBean> _newEvents = new ArrayDeque<EventBean>();

        private EventBean[] _newDataChildView;
        private bool _isHasRemovestreamData;
        private bool _isRetainObserverEvents;
        private bool _isDiscardObserverEvents;
        private readonly ICollection<EventBean> _oldEvents = new HashSet<EventBean>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="factory">the view factory</param>
        /// <param name="eventType">the parent event type</param>
        /// <param name="viewList">the list of data window views</param>
        public IntersectAsymetricView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            IntersectViewFactory factory,
            EventType eventType,
            IList<View> viewList)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _intersectViewFactory = factory;
            _eventType = eventType;
            _views = viewList.ToArray();
            _oldEventsPerView = new EventBean[viewList.Count][];

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
            get { return Views; }
        }

        public View CloneView()
        {
            return _intersectViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _oldEvents.Clear();
            EventBean[] newDataPosted = null;

            // handle remove stream
            if (oldData != null)
            {
                _isDiscardObserverEvents = true; // disable reaction logic in observer
                try
                {
                    foreach (View view in _views)
                    {
                        view.Update(null, oldData);
                    }
                }
                finally
                {
                    _isDiscardObserverEvents = false;
                }

                for (int i = 0; i < oldData.Length; i++)
                {
                    _oldEvents.Add(oldData[i]);
                }
            }

            if (newData != null)
            {
                _removalEvents.Clear();

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                _isHasRemovestreamData = false; // changed by observer logic to indicate new data
                _isRetainObserverEvents = true; // enable retain logic in observer
                try
                {
                    foreach (View view in _views)
                    {
                        _newDataChildView = null;
                        view.Update(newData, oldData);

                        // first-X asymetric view post no insert stream for events that get dropped, remove these
                        if (_newDataChildView != null)
                        {
                            for (int i = 0; i < newData.Length; i++)
                            {
                                bool found = false;
                                for (int j = 0; j < _newDataChildView.Length; j++)
                                {
                                    if (_newDataChildView[i] == newData[i])
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    _removalEvents.Add(newData[i]);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < newData.Length; i++)
                            {
                                _removalEvents.Add(newData[i]);
                            }
                        }
                    }
                }
                finally
                {
                    _isRetainObserverEvents = false;
                }

                if (_removalEvents.IsNotEmpty())
                {
                    _isDiscardObserverEvents = true;
                    EventBean[] viewOldData = _removalEvents.ToArray();
                    try
                    {
                        for (int j = 0; j < _views.Length; j++)
                        {
                            _views[j].Update(null, viewOldData);
                        }
                    }
                    finally
                    {
                        _isDiscardObserverEvents = false;
                    }
                }

                // see if any child view has removed any events.
                // if there was an insert stream, handle pushed-out events
                if (_isHasRemovestreamData)
                {
                    // process each buffer
                    for (int i = 0; i < _oldEventsPerView.Length; i++)
                    {
                        if (_oldEventsPerView[i] == null)
                        {
                            continue;
                        }

                        EventBean[] viewOldData = _oldEventsPerView[i];
                        _oldEventsPerView[i] = null; // clear entry

                        // add each event to the set of events removed
                        foreach (EventBean oldEvent in viewOldData)
                        {
                            _removalEvents.Add(oldEvent);
                        }

                        _isDiscardObserverEvents = true;
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
                            _isDiscardObserverEvents = false;
                        }
                    }

                    _oldEvents.AddAll(_removalEvents);
                }

                _newEvents.Clear();
                for (int i = 0; i < newData.Length; i++)
                {
                    if (!_removalEvents.Contains(newData[i]))
                    {
                        _newEvents.Add(newData[i]);
                    }
                }

                if (_newEvents.IsNotEmpty())
                {
                    newDataPosted = _newEvents.ToArray();
                }

            }

            // indicate new and, possibly, old data
            EventBean[] oldDataPosted = null;
            if (_oldEvents.IsNotEmpty())
            {
                oldDataPosted = _oldEvents.ToArray();
            }
            if ((newDataPosted != null) || (oldDataPosted != null))
            {
                UpdateChildren(newDataPosted, oldDataPosted);
            }
            _oldEvents.Clear();
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _views[0].GetEnumerator();
        }

        public void NewData(int streamId, EventBean[] newEvents, EventBean[] oldEvents)
        {
            _newDataChildView = newEvents;

            if ((oldEvents == null) || (_isDiscardObserverEvents))
            {
                return;
            }

            if (_isRetainObserverEvents)
            {
                _oldEventsPerView[streamId] = oldEvents;
                _isHasRemovestreamData = true;
                return;
            }

            // remove old data from all other views
            _isDiscardObserverEvents = true;
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
                _isDiscardObserverEvents = false;
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
