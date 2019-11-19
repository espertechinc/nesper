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
    ///     Inbound work unit processing a map event.
    /// </summary>
    public class InboundUnitSendObjectArray : InboundUnitRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InboundUnitSendObjectArray));

        private readonly string eventTypeName;
        private readonly object[] properties;
        private readonly EPEventServiceImpl runtime;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendObjectArray(
            object[] properties,
            string eventTypeName,
            EPEventServiceImpl runtime)
        {
            this.eventTypeName = eventTypeName;
            this.properties = properties;
            this.runtime = runtime;
        }

        public void Run()
        {
            try {
                var eventBean = runtime.Services.EventTypeResolvingBeanFactory.AdapterForObjectArray(properties, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                runtime.Services.ExceptionHandlingService.HandleInboundPoolException(runtime.RuntimeURI, e, properties);
                Log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
} // end of namespace