///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that represents a union of multiple data windows. 
    /// <para/>
    /// The view is parameterized by two or more data windows. From an external viewpoint, 
    /// the view retains all events that is in any of the data windows (a union).
    /// </summary>
    public class UnionView
        : ViewSupport
        , LastPostObserver
        , CloneableView
        , StoppableView
        , DataWindowView
        , ViewDataVisitableContainer
        , ViewContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly UnionViewFactory _unionViewFactory;
        private readonly EventType _eventType;
        private readonly View[] _views;
        private readonly EventBean[][] _oldEventsPerView;
        private readonly RefCountedSet<EventBean> _unionWindow;
        private readonly IList<EventBean> _removalEvents = new List<EventBean>();
    
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
        public UnionView(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            UnionViewFactory factory,
            EventType eventType,
            IList<View> viewList)
        {
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _unionViewFactory = factory;
            _eventType = eventType;
            _views = viewList.ToArray();
            _unionWindow = new RefCountedSet<EventBean>();
            _oldEventsPerView = new EventBean[viewList.Count][];
            
            for (int i = 0; i < viewList.Count; i++)
            {
                var view = new LastPostObserverView(i);
                _views[i].RemoveAllViews();
                _views[i].AddView(view);
                view.Observer = this;
            }
    
            // recover
            for (int i = 0; i < _views.Length; i++)
            {
                var viewSnapshot = _views[i].GetEnumerator();
                while (viewSnapshot.MoveNext())
                {
                    _unionWindow.Add(viewSnapshot.Current);
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
            using (Instrument.With(
                i => i.QViewProcessIRStream(this, _unionViewFactory.ViewName, newData, oldData),
                i => i.AViewProcessIRStream()))
            {
                OneEventCollection oldDataColl = null;
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

                    // remove from union
                    foreach (EventBean oldEvent in oldData)
                    {
                        _unionWindow.RemoveAll(oldEvent);
                    }

                    oldDataColl = new OneEventCollection();
                    oldDataColl.Add(oldData);
                }

                // add new event to union
                if (newData != null)
                {
                    foreach (EventBean newEvent in newData)
                    {
                        _unionWindow.Add(newEvent, _views.Length);
                    }

                    // new events must go to all views
                    // old events, such as when removing from a named window, get removed from all views
                    _isHasRemovestreamData = false; // changed by observer logic to indicate new data
                    _isRetainObserverEvents = true; // enable retain logic in observer
                    try
                    {
                        foreach (View view in _views)
                        {
                            view.Update(newData, null);
                        }
                    }
                    finally
                    {
                        _isRetainObserverEvents = false;
                    }

                    // see if any child view has removed any events.
                    // if there was an insert stream, handle pushed-out events
                    if (_isHasRemovestreamData)
                    {
                        IList<EventBean> removedEvents = null;

                        // process each buffer
                        for (int i = 0; i < _oldEventsPerView.Length; i++)
                        {
                            if (_oldEventsPerView[i] == null)
                            {
                                continue;
                            }

                            EventBean[] viewOldData = _oldEventsPerView[i];
                            _oldEventsPerView[i] = null; // clear entry

                            // remove events for union, if the last event was removed then add it
                            foreach (EventBean old in viewOldData)
                            {
                                bool isNoMoreRef = _unionWindow.Remove(old);
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
                            foreach (EventBean oldItem in removedEvents)
                            {
                                oldDataColl.Add(oldItem);
                            }
                        }
                    }
                }

                if (HasViews)
                {
                    // indicate new and, possibly, old data
                    EventBean[] oldEvents = oldDataColl != null ? oldDataColl.ToArray() : null;
                    Instrument.With(
                        i => i.QViewIndicate(this, _unionViewFactory.ViewName, newData, oldEvents),
                        i => i.AViewIndicate(),
                        () => UpdateChildren(newData, oldEvents));
                }
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
            foreach (EventBean old in oldEvents)
            {
                bool isNoMoreRef = _unionWindow.Remove(old);
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
                EventBean[] removed = removedEvents.ToArray();
                Instrument.With(
                    i => i.QViewIndicate(this, _unionViewFactory.ViewName, null, removed),
                    i => i.AViewIndicate(),
                    () => UpdateChildren(null, removed));
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
