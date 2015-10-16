///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// This view is a very simple view presenting the last event posted by the parent view 
    /// to any subviews. Only the very last event object is kept by this view. The Update 
    /// method invoked by the parent view supplies new data in an object array, of which 
    /// the view keeps the very last instance as the 'last' or newest event. The view 
    /// always has the same schema as the parent view and attaches to anything, and accepts 
    /// no parameters. Thus if 5 pieces of new data arrive, the child view receives 5 elements 
    /// of new data and also 4 pieces of old data which is the first 4 elements of new data.
    /// i.e. New data elements immediatly gets to be old data elements. Old data received from
    /// parent is not handled, it is ignored. We thus post old data as follows: last event is
    /// not null + new data from index zero to Count-1, where Count is the index of the last element in
    /// new data
    /// </summary>
    public class LastElementView
        : ViewSupport
        , CloneableView
        , DataWindowView
    {
        private readonly LastElementViewFactory _viewFactory;

        /// <summary>
        /// The last new element posted from a parent view.
        /// </summary>
        private EventBean _lastEvent;

        public LastElementView(LastElementViewFactory viewFactory)
        {
            this._viewFactory = viewFactory;
        }
    
        public View CloneView()
        {
            return new LastElementView(_viewFactory);
        }

        public override EventType EventType
        {
            get
            {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, LastElementViewFactory.NAME, newData, oldData);}
            OneEventCollection oldDataToPost = null;
    
            if ((newData != null) && (newData.Length != 0))
            {
                if (_lastEvent != null)
                {
                    oldDataToPost = new OneEventCollection();
                    oldDataToPost.Add(_lastEvent);
                }
                if (newData.Length > 1)
                {
                    for (int i = 0; i < newData.Length - 1; i++)
                    {
                        if (oldDataToPost == null)
                        {
                            oldDataToPost = new OneEventCollection();
                        }
                        oldDataToPost.Add(newData[i]);
                    }
                }
                _lastEvent = newData[newData.Length - 1];
            }
    
            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    if (oldData[i] == _lastEvent)
                    {
                        if (oldDataToPost == null)
                        {
                            oldDataToPost = new OneEventCollection();
                        }
                        oldDataToPost.Add(oldData[i]);
                        _lastEvent = null;
                    }
                }
            }
    
            // If there are child views, fireStatementStopped Update method
            if (HasViews)
            {
                if ((oldDataToPost != null) && (!oldDataToPost.IsEmpty()))
                {
                    EventBean[] oldDataArray = oldDataToPost.ToArray();
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, LastElementViewFactory.NAME, newData, oldDataArray);}
                    UpdateChildren(newData, oldDataArray);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
                }
                else
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, LastElementViewFactory.NAME, newData, null);}
                    UpdateChildren(newData, null);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_lastEvent != null)
            {
                yield return _lastEvent;
            }
        }
    
        public override String ToString()
        {
            return GetType().FullName;
        }
    
        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_lastEvent, LastElementViewFactory.NAME);
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
}
