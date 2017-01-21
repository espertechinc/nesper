///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// A view that retains the last Update.
    /// </summary>
    public sealed class LastPostObserverView 
        : View
        , CloneableView
    {
        private Viewable _parent;
        private readonly int _streamId;
        private LastPostObserver _observer;

        /// <summary>Ctor. </summary>
        /// <param name="streamId">number of the stream for which the view buffers the generated events.</param>
        public LastPostObserverView(int streamId)
        {
            _streamId = streamId;
        }

        /// <summary>Set an observer. </summary>
        /// <value>to be called when results are available</value>
        public LastPostObserver Observer
        {
            set { _observer = value; }
        }

        public View CloneView()
        {
            return new LastPostObserverView(_streamId);
        }

        public EventType EventType
        {
            get { return _parent.EventType; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return _parent.GetEnumerator();
        }

        public Viewable Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public View AddView(View view)
        {
            throw new UnsupportedOperationException();
        }

        public View[] Views
        {
            get { return new View[0]; }
        }

        public bool RemoveView(View view)
        {
            throw new UnsupportedOperationException();
        }

        public void RemoveAllViews()
        {
            throw new UnsupportedOperationException();
        }

        public bool HasViews
        {
            get { return false; }
        }

        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (_observer != null)
            {
                _observer.NewData(_streamId, newData, oldData);
            }
        }
    }
}