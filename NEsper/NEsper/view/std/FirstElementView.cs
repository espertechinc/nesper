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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// View retaining the very first event. Any subsequent events received are simply 
    /// discarded and not entered into either insert or remove stream. Only the very first 
    /// event received is entered into the remove stream. <para/>The view thus never posts 
    /// a remove stream unless explicitly deleted from when used with a named window.
    /// </summary>
    public class FirstElementView : ViewSupport, CloneableView, DataWindowView
    {
        private readonly FirstElementViewFactory _viewFactory;

        /// <summary>The first new element posted from a parent view. </summary>
        private EventBean _firstEvent;

        public FirstElementView(FirstElementViewFactory viewFactory)
        {
            this._viewFactory = viewFactory;
        }

        public View CloneView()
        {
            return new FirstElementView(_viewFactory);
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, FirstElementViewFactory.NAME, newData, oldData);}
    
            EventBean[] newDataToPost = null;
            EventBean[] oldDataToPost = null;
    
            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    if (oldData[i] == _firstEvent)
                    {
                        oldDataToPost = new EventBean[] {_firstEvent};
                        _firstEvent = null;
                    }
                }
            }
    
            if ((newData != null) && (newData.Length != 0))
            {
                if (_firstEvent == null)
                {
                    _firstEvent = newData[0];
                    newDataToPost = new EventBean[] {_firstEvent};
                }
            }
    
            if ((HasViews) && ((newDataToPost != null) || (oldDataToPost != null)))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, FirstElementViewFactory.NAME, newDataToPost, oldDataToPost);}
                UpdateChildren(newDataToPost, oldDataToPost);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }
    
        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_firstEvent != null)
            {
                yield return _firstEvent;
            }
        }
    
        public override String ToString()
        {
            return GetType().FullName;
        }

        public EventBean FirstEvent
        {
            get { return _firstEvent; }
            set { _firstEvent = value; }
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_firstEvent, FirstElementViewFactory.NAME);
        }

        public ViewFactory ViewFactory
        {
            get { return _viewFactory; }
        }
    }
}
