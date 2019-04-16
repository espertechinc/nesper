///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    /// A view that retains the last update.
    /// </summary>
    public class LastPostObserverView : ViewSupport
    {
        protected internal Viewable parent;
        protected internal readonly int streamId;
        protected internal LastPostObserver observer;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamId">number of the stream for which the view buffers the generated events.</param>
        public LastPostObserverView(int streamId)
        {
            this.streamId = streamId;
        }

        /// <summary>
        /// Set an observer.
        /// </summary>
        /// <value>to be called when results are available</value>
        public LastPostObserver Observer {
            get => this.observer;
            set => this.observer = value;
        }

        public override EventType EventType {
            get => parent.EventType;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return parent.GetEnumerator();
        }

        public override Viewable Parent {
            get => parent;
            set => parent = value;
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            observer?.NewData(streamId, newData, oldData);
        }
    }
} // end of namespace