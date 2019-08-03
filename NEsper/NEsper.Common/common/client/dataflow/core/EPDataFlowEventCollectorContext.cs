///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     For use with <seealso cref="EPDataFlowEventCollector" /> provides collection context.
    ///     <para />
    ///     Do not retain handles to this instance as its contents may change.
    /// </summary>
    public class EPDataFlowEventCollectorContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="sender">for sending events to the event bus</param>
        /// <param name="@event">to process</param>
        public EPDataFlowEventCollectorContext(
            EventServiceSendEventCommon sender,
            object @event)
        {
            Sender = sender;
            Event = @event;
        }

        /// <summary>
        ///     Returns the event.
        /// </summary>
        /// <returns>event</returns>
        public object Event { get; set; }

        /// <summary>
        ///     Returns the emitter for the event bus.
        /// </summary>
        /// <returns>emitter</returns>
        public EventServiceSendEventCommon Sender { get; }
    }
} // end of namespace