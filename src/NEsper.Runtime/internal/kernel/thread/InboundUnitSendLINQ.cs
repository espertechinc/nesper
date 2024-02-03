///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Xml.Linq;

using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Inbound unit for LINQ XML events.
    /// </summary>
    public class InboundUnitSendLINQ : InboundUnitRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string eventTypeName;
        private readonly XElement theEvent;
        private readonly EPRuntimeEventProcessWrapped runtime;
        private readonly EPServicesEvaluation services;

        /// <summary>Ctor. </summary>
        /// <param name="theEvent">document</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="eventTypeName">type name</param>
        public InboundUnitSendLINQ(
            XElement theEvent,
            string eventTypeName,
            EPRuntimeEventProcessWrapped runtime,
            EPServicesEvaluation services)
        {
            this.theEvent = theEvent;
            this.eventTypeName = eventTypeName;
            this.runtime = runtime;
            this.services = services;
        }

        public void Run()
        {
            try {
                var eventBean = services.EventTypeResolvingBeanFactory.AdapterForXML(theEvent, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                services.ExceptionHandlingService.HandleInboundPoolException(runtime.URI, e, theEvent);
                Log.Error("Unexpected error processing Json event: " + e.Message, e);
            }
        }
    }
}