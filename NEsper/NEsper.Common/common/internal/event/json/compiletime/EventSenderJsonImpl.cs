///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	/// <summary>
	/// Event sender for json-backed events.
	/// <para />Allows sending only event objects of type string, does not check contents. Any other event object generates an error.
	/// </summary>
	public class EventSenderJsonImpl : EventSenderJson {
	    private readonly EPRuntimeEventProcessWrapped runtimeEventSender;
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
	    private readonly JsonEventType eventType;
	    private readonly ThreadingCommon threadingService;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="runtimeEventSender">for processing events</param>
	    /// <param name="eventType">the event type</param>
	    /// <param name="threadingService">for inbound threading</param>
	    /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
	    public EventSenderJsonImpl(EPRuntimeEventProcessWrapped runtimeEventSender, JsonEventType eventType, EventBeanTypedEventFactory eventBeanTypedEventFactory, ThreadingCommon threadingService) {
	        this.runtimeEventSender = runtimeEventSender;
	        this.eventType = eventType;
	        this.threadingService = threadingService;
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	    }

	    public void SendEvent(object theEvent) {
	        object underlying = GetUnderlying(theEvent);
	        EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedJson(underlying, eventType);

	        if (threadingService.IsInboundThreading) {
	            threadingService.SubmitInbound(eventBean, runtimeEventSender);
	        } else {
	            runtimeEventSender.ProcessWrappedEvent(eventBean);
	        }
	    }

	    public void RouteEvent(object theEvent) {
	        EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedJson(GetUnderlying(theEvent), eventType);
	        runtimeEventSender.RouteEventBean(eventBean);
	    }

	    public object Parse(string json) {
	        return eventType.Parse(json);
	    }

	    private object GetUnderlying(object theEvent) {
	        if (theEvent is string) {
	            return eventType.Parse((string) theEvent);
	        } else if (theEvent == null || !(theEvent.GetType() == eventType.UnderlyingType)) {
	            throw new EPException("Unexpected event object of type '" + (theEvent == null ? "(null)" : theEvent.GetType().Name) + "', expected a Json-formatted string-type value");
	        }
	        return theEvent;
	    }
	}
} // end of namespace
