///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.view.internals
{
    /// <summary>A view that retains the last Update. </summary>
    public sealed class LastPostObserverView : ViewSupport, CloneableView
    {
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

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (_observer != null)
            {
                _observer.NewData(_streamId, newData, oldData);
            }
        }
    }
}