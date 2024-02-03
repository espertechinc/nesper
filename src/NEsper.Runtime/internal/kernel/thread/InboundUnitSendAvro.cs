///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly string _eventTypeName;

        private readonly object _genericRecordDotData;
        private readonly EPRuntimeEventProcessWrapped _runtime;
        private readonly EPServicesEvaluation _services;

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
            this._eventTypeName = eventTypeName;
            this._genericRecordDotData = genericRecordDotData;
            this._runtime = runtime;
            this._services = services;
        }

        public void Run()
        {
            try {
                var eventBean = _services.EventTypeResolvingBeanFactory.AdapterForAvro(_genericRecordDotData, _eventTypeName);
                _runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                _services.ExceptionHandlingService.HandleInboundPoolException(_runtime.URI, e, _genericRecordDotData);
                Log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
} // end of namespace