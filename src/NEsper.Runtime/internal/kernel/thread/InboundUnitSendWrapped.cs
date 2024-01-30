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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly EventBean _eventBean;
        private readonly EPRuntimeEventProcessWrapped _runtime;
        private readonly EPServicesEvaluation _services;

        public InboundUnitSendWrapped(
            EventBean eventBean,
            EPRuntimeEventProcessWrapped runtime,
            EPServicesEvaluation services)
        {
            this._eventBean = eventBean;
            this._runtime = runtime;
            this._services = services;
        }

        public void Run()
        {
            try {
                _runtime.ProcessWrappedEvent(_eventBean);
            }
            catch (Exception e) {
                _services.ExceptionHandlingService.HandleInboundPoolException(_runtime.URI, e, _eventBean);
                Log.Error("Unexpected error processing wrapped event: " + e.Message, e);
            }
        }
    }
} // end of namespace