///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Xml.Linq;
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
        private readonly EPEventServiceImpl runtime;
        private readonly XElement theEvent;

        /// <summary>Ctor. </summary>
        /// <param name="theEvent">document</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="eventTypeName">type name</param>
        public InboundUnitSendLINQ(
            XElement theEvent,
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
                var eventBean = runtime.Services.EventTypeResolvingBeanFactory.AdapterForXML(theEvent, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                runtime.Services.ExceptionHandlingService.HandleInboundPoolException(runtime.RuntimeURI, e, theEvent);
                Log.Error("Unexpected error processing DOM event: " + e.Message, e);
            }
        }
    }
}