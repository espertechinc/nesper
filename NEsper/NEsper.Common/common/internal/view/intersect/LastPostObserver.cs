///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    ///     Observer interface to a stream publishing new and old events.
    /// </summary>
    public interface LastPostObserver
    {
        /// <summary>
        ///     Receive new and old events from a stream.
        /// </summary>
        /// <param name="streamId">the stream number sending the events</param>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        void NewData(
            int streamId,
            EventBean[] newEvents,
            EventBean[] oldEvents);
    }
} // end of namespace