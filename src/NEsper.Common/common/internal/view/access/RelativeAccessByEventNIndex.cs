///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    /// Provides access to prior events given an event from which to count back, and an index to look at.
    /// </summary>
    public interface RelativeAccessByEventNIndex
    {
        /// <summary>
        /// Returns the prior event to the given event counting back the number of events as supplied by index.
        /// </summary>
        /// <param name="theEvent"></param>
        /// <param name="index">is the number of events to go back</param>
        /// <returns>event</returns>
        EventBean GetRelativeToEvent(
            EventBean theEvent,
            int index);

        EventBean GetRelativeToEnd(int index);

        IEnumerator<EventBean> WindowToEvent { get; }

        ICollection<EventBean> WindowToEventCollReadOnly { get; }

        int WindowToEventCount { get; }
    }
}