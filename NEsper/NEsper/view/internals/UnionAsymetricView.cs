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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents a union of multiple data windows wherein at least one is asymetric:
    ///  it does not present a insert stream for each insert stream event received. 
    /// <para/>
    /// The view is parameterized by two or more data windows. From an external viewpoint, the 
    /// view retains all events that is in any of the data windows (a union).
    /// </summary>
    public class UnionAsymetricView 
        : ViewSupport
        , LastPostObserver
        , CloneableView
        , StoppableView
        , DataWindowView
        , ViewDataVisitableContainer
        , ViewContainer
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly UnionViewFactory _unionViewFactory;
        private readonly EventType _eventType;
        private readonly View[] _views;
        private readonly EventBean[][] _oldEventsPerView;
        private readonly RefCountedSet<EventBean> _unionWindow;
        private readonly IList<EventBean> _removalEvents = new List<EventBean>();
        private readonly ArrayDeque<EventBean> _newEvents = new ArrayDeque<EventBean>();
    
        private EventBean[] _newDataChildView;
        private bool _isHasRemovestreamData;
        private bool _isRetainObserverEvents;
        private bool _isDiscardObserverEvents;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="factory">the view factory</param>
        /// <param name="eventType">the parent event type</param>
        /// <param name="viewList">the list of data window views</param>
        public UnionAsymetricView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext, UnionViewFactory factory, EventType eventType, IList<View> viewList)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _unionViewFactory = factory;
            _eventType = eventType;
            _views = viewList.ToArray();
            _unionWindow = new RefCountedSet<EventBean>();
            _oldEventsPerView = new EventBean[viewList.Count][];
            
            for (var i = 0; i < viewList.Count; i++)
            {
                var view = new LastPostObserverView(i);
                _views[i].RemoveAllViews();
                _views[i].AddView(view);
                view.Observer = this;
            }
    
            // recover
            for (var i = 0; i < _views.Length; i++)
            {
                var viewSnapshot = _views[i].GetEnumerator();
                while(viewSnapshot.MoveNext())
                {
                    var theEvent = viewSnapshot.Current;
                    _unionWindow.Add(theEvent);
                }
            }
        }

        public View[] ViewContained
        {
            get { return _views; }
        }

        public View CloneView()
        {
            return _unionViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // handle remove stream
            OneEventCollection oldDataColl = null;
            EventBean[] newDataPosted = null;
            if (oldData != null)
            {
                _isDiscardObserverEvents = true; // disable reaction logic in observer

                try
                {
                    foreach (var view in _views)
                    {
                        view.Update(null, oldData);
                    }
                }
                finally
                {
                    _isDiscardObserverEvents = false;
                }

                // remove from union
                foreach (var oldEvent in oldData)
                {
                    _unionWindow.RemoveAll(oldEvent);
                }

                oldDataColl = new OneEventCollection();
                oldDataColl.Add(oldData);
            }

            // add new event to union
            if (newData != null)
            {
                var removedByView = new bool[newData.Length,_views.Length];
                foreach (var newEvent in newData)
                {
                    _unionWindow.Add(newEvent, _views.Length);
                }

                // new events must go to all views
                // old events, such as when removing from a named window, get removed from all views
                _isHasRemovestreamData = false; // changed by observer logic to indicate new data
                _isRetainObserverEvents = true; // enable retain logic in observer
                try
                {
                    for (var viewIndex = 0; viewIndex < _views.Length; viewIndex++)
                    {
                        var view = _views[viewIndex];
                        view.Update(newData, null);

                        // first-X asymetric view post no insert stream for events that get dropped, remove these
                        if (_newDataChildView != null)
                        {
                            for (var i = 0; i < newData.Length; i++)
                            {
                                var found = false;
                                for (var j = 0; j < _newDataChildView.Length; j++)
                                {
                                    if (_newDataChildView[i] == newData[i])
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    removedByView[i,viewIndex] = true;
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i < newData.Length; i++)
                            {
                                removedByView[i,viewIndex] = true;
                            }
                        }
                    }
                }
                finally
                {
                    _isRetainObserverEvents = false;
                }

                // determine removed events, those that have a "true" in the remove by view index for all views
                _removalEvents.Clear();
                for (var i = 0; i < newData.Length; i++)
                {
                    var allTrue = true;
                    for (var j = 0; j < _views.Length; j++)
                    {
                        if (!removedByView[i,j])
                        {
                            allTrue = false;
                            break;
                        }
                    }
                    if (allTrue)
                    {
                        _removalEvents.Add(newData[i]);
                        _unionWindow.RemoveAll(newData[i]);
                    }
                }

                // remove if any
                if (_removalEvents.IsNotEmpty())
                {
                    _isDiscardObserverEvents = true;
                    var viewOldData = _removalEvents.ToArray();
                    try
                    {
                        for (var j = 0; j < _views.Length; j++)
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
                    IList<EventBean> removedEvents = null;

                    // process each buffer
                    for (var i = 0; i < _oldEventsPerView.Length; i++)
                    {
                        if (_oldEventsPerView[i] == null)
                        {
                            continue;
                        }

                        var viewOldData = _oldEventsPerView[i];
                        _oldEventsPerView[i] = null; // clear entry

                        // remove events for union, if the last event was removed then add it
                        foreach (var old in viewOldData)
                        {
                            var isNoMoreRef = _unionWindow.Remove(old);
                            if (isNoMoreRef)
                            {
                                if (removedEvents == null)
                                {
                                    _removalEvents.Clear();
                                    removedEvents = _removalEvents;
                                }
                                removedEvents.Add(old);
                            }
                        }
                    }

                    if (removedEvents != null)
                    {
                        if (oldDataColl == null)
                        {
                            oldDataColl = new OneEventCollection();
                        }
                        foreach (var oldItem in removedEvents)
                        {
                            oldDataColl.Add(oldItem);
                        }
                    }
                }

                _newEvents.Clear();
                for (var i = 0; i < newData.Length; i++)
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
            if (HasViews && ((newDataPosted != null) || (oldDataColl != null)))
            {
                UpdateChildren(newDataPosted, oldDataColl != null ? oldDataColl.ToArray() : null);
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _unionWindow.Keys.GetEnumerator();
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

            // handle time-based removal
            IList<EventBean> removedEvents = null;

            // remove events for union, if the last event was removed then add it
            foreach (var old in oldEvents)
            {
                var isNoMoreRef = _unionWindow.Remove(old);
                if (isNoMoreRef)
                {
                    if (removedEvents == null)
                    {
                        _removalEvents.Clear();
                        removedEvents = _removalEvents;
                    }
                    removedEvents.Add(old);
                }
            }

            if (removedEvents != null)
            {
                var removed = removedEvents.ToArray();
                UpdateChildren(null, removed);
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
            IntersectDefaultView.VisitViewContained(viewDataVisitor, _unionViewFactory, _views);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            throw new UnsupportedOperationException();
        }
    
        public ViewFactory ViewFactory
        {
            get { return _unionViewFactory; }
        }
    }
}
