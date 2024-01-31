///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
	/// Inbound unit for unwrapped events.
	/// </summary>
	public class InboundUnitSendEvent : InboundUnitRunnable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object _theEvent;
		private readonly string _eventTypeName;
		private readonly EPRuntimeEventProcessWrapped _runtime;
		private readonly EPServicesEvaluation _services;

		public InboundUnitSendEvent(
			object theEvent,
			string eventTypeName,
			EPRuntimeEventProcessWrapped runtime,
			EPServicesEvaluation services)
		{
			this._theEvent = theEvent;
			this._eventTypeName = eventTypeName;
			this._runtime = runtime;
			this._services = services;
		}

		public void Run()
		{
			try {
				EventBean eventBean = _services.EventTypeResolvingBeanFactory.AdapterForBean(_theEvent, _eventTypeName);
				_runtime.ProcessWrappedEvent(eventBean);
			}
			catch (Exception ex) {
				_services.ExceptionHandlingService.HandleInboundPoolException(_runtime.URI, ex, _theEvent);
				Log.Error("Unexpected error processing unwrapped event: " + ex.Message, ex);
			}
		}
	}
} // end of namespace
