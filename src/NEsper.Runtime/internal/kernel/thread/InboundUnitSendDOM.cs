///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
	/// <summary>
	/// Inbound unit for DOM events.
	/// </summary>
	public class InboundUnitSendDOM : InboundUnitRunnable
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly XmlNode theEvent;
		private readonly string eventTypeName;
		private readonly EPRuntimeEventProcessWrapped runtime;
		private readonly EPServicesEvaluation services;

		public InboundUnitSendDOM(
			XmlNode theEvent,
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
				EventBean eventBean = services.EventTypeResolvingBeanFactory.AdapterForXMLDOM(theEvent, eventTypeName);
				runtime.ProcessWrappedEvent(eventBean);
			}
			catch (Exception e) {
				services.ExceptionHandlingService.HandleInboundPoolException(runtime.URI, e, theEvent);
				log.Error("Unexpected error processing DOM event: " + e.Message, e);
			}
		}
	}
} // end of namespace
