///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.avro
{
	/// <summary>
	/// Event sender for avro-backed events.
	/// <para />Allows sending only event objects of type GenericData.Record, does not check contents. Any other event object generates an error.
	/// </summary>
	public class EventSenderAvro : EventSender {
	    private readonly EPRuntimeEventProcessWrapped runtimeEventSender;
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
	    private readonly EventType eventType;
	    private readonly ThreadingCommon threadingService;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="runtimeEventSender">for processing events</param>
	    /// <param name="eventType">the event type</param>
	    /// <param name="threadingService">for inbound threading</param>
	    /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
	    public EventSenderAvro(EPRuntimeEventProcessWrapped runtimeEventSender, EventType eventType, EventBeanTypedEventFactory eventBeanTypedEventFactory, ThreadingCommon threadingService) {
	        this.runtimeEventSender = runtimeEventSender;
	        this.eventType = eventType;
	        this.threadingService = threadingService;
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	    }

	    public void SendEvent(object theEvent) {
	        EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedAvro(theEvent, eventType);

	        if (threadingService.IsInboundThreading) {
	            threadingService.SubmitInbound(eventBean, runtimeEventSender);
	        } else {
	            runtimeEventSender.ProcessWrappedEvent(eventBean);
	        }
	    }

	    public void RouteEvent(object theEvent) {
	        if (!(theEvent.GetType().IsArray)) {
	            throw new EPException("Unexpected event object of type " + theEvent.GetType().Name + ", expected Object[]");
	        }
	        EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedAvro(theEvent, eventType);
	        runtimeEventSender.RouteEventBean(eventBean);
	    }
	}
} // end of namespace