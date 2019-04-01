///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.util
{
    /// <summary>
    ///     A view that acts as an adapter between views and update listeners.
    ///     The view can be added to a parent view. When the parent view publishes data, the view will forward the
    ///     data to the UpdateListener implementation that has been supplied. If no UpdateListener has been supplied,
    ///     then the view will cache the last data published by the parent view.
    /// </summary>
    public class BufferView : ViewSupport
    {
        private readonly FlushedEventBuffer oldDataBuffer = new FlushedEventBuffer();
        private readonly int streamId;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamId">number of the stream for which the view buffers the generated events.</param>
        public BufferView(int streamId)
        {
            this.streamId = streamId;
        }

        public override EventType EventType => Parent.EventType;

        /// <summary>
        ///     Returns the buffer for new data.
        /// </summary>
        /// <returns>new data buffer</returns>
        public FlushedEventBuffer NewDataBuffer { get; } = new FlushedEventBuffer();

        /// <summary>
        ///     Set the observer for indicating new and old data.
        /// </summary>
        /// <value>to indicate new and old events</value>
        public BufferObserver Observer { get; set; }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            NewDataBuffer.Add(newData);
            oldDataBuffer.Add(oldData);
            Observer.NewData(streamId, NewDataBuffer, oldDataBuffer);
        }
    }
} // end of namespace