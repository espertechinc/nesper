///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly XmlNode _theEvent;
		private readonly string _eventTypeName;
		private readonly EPRuntimeEventProcessWrapped _runtime;
		private readonly EPServicesEvaluation _services;

		public InboundUnitSendDOM(
			XmlNode theEvent,
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
				EventBean eventBean = _services.EventTypeResolvingBeanFactory.AdapterForXMLDOM(_theEvent, _eventTypeName);
				_runtime.ProcessWrappedEvent(eventBean);
			}
			catch (Exception e) {
				_services.ExceptionHandlingService.HandleInboundPoolException(_runtime.URI, e, _theEvent);
				Log.Error("Unexpected error processing DOM event: " + e.Message, e);
			}
		}
	}
} // end of namespace
