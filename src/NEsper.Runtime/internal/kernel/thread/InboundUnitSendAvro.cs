///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
	/// <summary>
	///     Inbound work unit processing a map event.
	/// </summary>
	public class InboundUnitSendAvro : InboundUnitRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string eventTypeName;

        private readonly object genericRecordDotData;
        private readonly EPRuntimeEventProcessWrapped runtime;
        private readonly EPServicesEvaluation services;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="genericRecordDotData">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="runtime">to process</param>
        /// <param name="services">services</param>
        public InboundUnitSendAvro(
            object genericRecordDotData,
            string eventTypeName,
            EPRuntimeEventProcessWrapped runtime,
            EPServicesEvaluation services)
        {
            this.eventTypeName = eventTypeName;
            this.genericRecordDotData = genericRecordDotData;
            this.runtime = runtime;
            this.services = services;
        }

        public void Run()
        {
            try {
                var eventBean = services.EventTypeResolvingBeanFactory.AdapterForAvro(genericRecordDotData, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                services.ExceptionHandlingService.HandleInboundPoolException(runtime.URI, e, genericRecordDotData);
                log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
} // end of namespace