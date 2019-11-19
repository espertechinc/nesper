///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Inbound unit for unwrapped events.
    /// </summary>
    public class InboundUnitSendEvent : InboundUnitRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InboundUnitSendEvent));

        private readonly string eventTypeName;
        private readonly EPEventServiceImpl runtime;
        private readonly object theEvent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="theEvent">to process</param>
        /// <param name="runtime">to process event</param>
        /// <param name="eventTypeName">type name</param>
        public InboundUnitSendEvent(
            object theEvent,
            string eventTypeName,
            EPEventServiceImpl runtime)
        {
            this.theEvent = theEvent;
            this.runtime = runtime;
            this.eventTypeName = eventTypeName;
        }

        public void Run()
        {
            try {
                var eventBean = runtime.Services.EventTypeResolvingBeanFactory.AdapterForBean(theEvent, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception ex) {
                runtime.Services.ExceptionHandlingService.HandleInboundPoolException(runtime.RuntimeURI, ex, theEvent);
                log.Error("Unexpected error processing unwrapped event: " + ex.Message, ex);
            }
        }
    }
} // end of namespace