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
	/// Inbound unit for unwrapped events.
	/// </summary>
	public class InboundUnitSendEvent : InboundUnitRunnable
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object theEvent;
		private readonly string eventTypeName;
		private readonly EPRuntimeEventProcessWrapped runtime;
		private readonly EPServicesEvaluation services;

		public InboundUnitSendEvent(
			object theEvent,
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
				EventBean eventBean = services.EventTypeResolvingBeanFactory.AdapterForBean(theEvent, eventTypeName);
				runtime.ProcessWrappedEvent(eventBean);
			}
			catch (Exception ex) {
				services.ExceptionHandlingService.HandleInboundPoolException(runtime.URI, ex, theEvent);
				log.Error("Unexpected error processing unwrapped event: " + ex.Message, ex);
			}
		}
	}
} // end of namespace
