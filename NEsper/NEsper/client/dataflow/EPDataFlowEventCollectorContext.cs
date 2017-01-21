///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// For use with <seealso cref="EPDataFlowEventCollector" /> provides collection context. 
    /// <para />
    /// Do not retain handles to this instance as its contents may change.
    /// </summary>
    public class EPDataFlowEventCollectorContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="eventBusCollector">for sending events to the event bus</param>
        /// <param name="theEvent">to process</param>
        public EPDataFlowEventCollectorContext(EventBusCollector eventBusCollector, Object theEvent) {
            EventBusCollector = eventBusCollector;
            Event = theEvent;
        }

        /// <summary>Returns the event. </summary>
        /// <value>event</value>
        public object Event { get; set; }

        /// <summary>Returns the emitter for the event bus. </summary>
        /// <value>emitter</value>
        public EventBusCollector EventBusCollector { get; private set; }
    }
}
