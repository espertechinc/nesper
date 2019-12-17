///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Inbound work unit processing a map event.
    /// </summary>
    public class InboundUnitSendMap : InboundUnitRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InboundUnitSendMap));

        private readonly DataMap map;
        private readonly string eventTypeName;
        private readonly EPEventServiceImpl runtime;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="map">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendMap(
            DataMap map,
            string eventTypeName,
            EPEventServiceImpl runtime)
        {
            this.eventTypeName = eventTypeName;
            this.map = map;
            this.runtime = runtime;
        }

        public void Run()
        {
            try {
                EventBean eventBean = runtime.Services.EventTypeResolvingBeanFactory.AdapterForMap(map, eventTypeName);
                runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                runtime.Services.ExceptionHandlingService.HandleInboundPoolException(runtime.RuntimeURI, e, map);
                log.Error("Unexpected error processing Map event: " + e.Message, e);
            }
        }
    }
} // end of namespace