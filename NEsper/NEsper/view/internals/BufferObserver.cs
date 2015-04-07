///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.collection;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Observer interface to a stream publishing new and old events.
    /// </summary>
    public interface BufferObserver
    {
        /// <summary>
        /// Receive new and old events from a stream.
        /// </summary>
        /// <param name="streamId">the stream number sending the events</param>
        /// <param name="newEventBuffer">buffer for new events</param>
        /// <param name="oldEventBuffer">buffer for old events</param>
        void NewData(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer);
    }

    public delegate void BufferObserverDelegate(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer);

    public class ProxyBufferObserver : BufferObserver
    {
        private readonly BufferObserverDelegate m_delegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyBufferObserver"/> class.
        /// </summary>
        /// <param name="d">The d.</param>
        public ProxyBufferObserver(BufferObserverDelegate d)
        {
            m_delegate = d;
        }

        #region BufferObserver Members

        /// <summary>
        /// Receive new and old events from a stream.
        /// </summary>
        /// <param name="streamId">the stream number sending the events</param>
        /// <param name="newEventBuffer">buffer for new events</param>
        /// <param name="oldEventBuffer">buffer for old events</param>
        public void NewData(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer)
        {
            m_delegate(streamId, newEventBuffer, oldEventBuffer);
        }

        #endregion
    }
}
