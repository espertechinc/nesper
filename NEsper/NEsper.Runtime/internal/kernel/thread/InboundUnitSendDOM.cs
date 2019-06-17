///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Inbound unit for DOM events.
    /// </summary>
    public class InboundUnitSendDOM : InboundUnitRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InboundUnitSendDOM));

        private readonly string eventTypeName;
        private readonly EPEventServiceImpl runtime;
        private readonly XmlNode theEvent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="theEvent">document</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="eventTypeName">type name</param>
        public InboundUnitSendDOM(
            XmlNode theEvent,
            string eventTypeName,
            EPEventServiceImpl runtime)
        {
            this.theEvent = theEvent;
            this.eventTypeName = eventTypeName;
            this.runtime = runtime;
        }

        public void Run()
        {
            try {
                var eventBean = runtime.Services.EventTypeResolvingBeanFactory.AdapterForXMLDOM(theEvent, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                runtime.Services.ExceptionHandlingService.HandleInboundPoolException(runtime.RuntimeURI, e, theEvent);
                Log.Error("Unexpected error processing DOM event: " + e.Message, e);
            }
        }
    }
} // end of namespace