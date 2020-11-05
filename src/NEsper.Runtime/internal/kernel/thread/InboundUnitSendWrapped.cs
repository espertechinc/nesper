///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Inbound unit for wrapped events.
    /// </summary>
    public class InboundUnitSendWrapped : InboundUnitRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventBean eventBean;
        private readonly EPRuntimeEventProcessWrapped runtime;
        private readonly EPServicesEvaluation services;

        public InboundUnitSendWrapped(
            EventBean eventBean,
            EPRuntimeEventProcessWrapped runtime,
            EPServicesEvaluation services)
        {
            this.eventBean = eventBean;
            this.runtime = runtime;
            this.services = services;
        }

        public void Run()
        {
            try {
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                services.ExceptionHandlingService.HandleInboundPoolException(runtime.URI, e, eventBean);
                log.Error("Unexpected error processing wrapped event: " + e.Message, e);
            }
        }
    }
} // end of namespace