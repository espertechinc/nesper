///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.util
{
    /// <summary>
    ///     For use by event sender for direct feed of wrapped events for processing.
    /// </summary>
    public interface EPRuntimeEventProcessWrapped
    {
        /// <summary>
        ///     Equivalent to the sendEvent method of EPRuntime, for use to process an known event.
        /// </summary>
        /// <param name="eventBean">is the event object wrapped by an event bean providing the event metadata</param>
        void ProcessWrappedEvent(EventBean eventBean);

        /// <summary>
        ///     For processing a routed event.
        /// </summary>
        /// <param name="theEvent">routed event</param>
        void RouteEventBean(EventBean theEvent);

        string URI { get; }
    }
} // end of namespace