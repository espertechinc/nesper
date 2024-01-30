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
	public class InboundUnitSendObjectArray : InboundUnitRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly string _eventTypeName;
        private readonly object[] _properties;
        private readonly EPRuntimeEventProcessWrapped _runtime;
        private readonly EPServicesEvaluation _services;

        public InboundUnitSendObjectArray(
            object[] properties,
            string eventTypeName,
            EPRuntimeEventProcessWrapped runtime,
            EPServicesEvaluation services)
        {
            this._properties = properties;
            this._eventTypeName = eventTypeName;
            this._runtime = runtime;
            this._services = services;
        }

        public void Run()
        {
            try {
                var eventBean = _services.EventTypeResolvingBeanFactory.AdapterForObjectArray(_properties, _eventTypeName);
                _runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e) {
                _services.ExceptionHandlingService.HandleInboundPoolException(_runtime.URI, e, _properties);
                Log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
} // end of namespace