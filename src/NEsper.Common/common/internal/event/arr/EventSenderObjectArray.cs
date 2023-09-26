///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Event sender for map-backed events.
    ///     <para />
    ///     Allows sending only event objects of type map, does not check map contents. Any other event object generates an
    ///     error.
    /// </summary>
    public class EventSenderObjectArray : EventSender
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly ObjectArrayEventType objectArrayEventType;
        private readonly EPRuntimeEventProcessWrapped runtimeEventSender;
        private readonly ThreadingCommon threadingService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="objectArrayEventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
        public EventSenderObjectArray(
            EPRuntimeEventProcessWrapped runtimeEventSender,
            ObjectArrayEventType objectArrayEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ThreadingCommon threadingService)
        {
            this.runtimeEventSender = runtimeEventSender;
            this.objectArrayEventType = objectArrayEventType;
            this.threadingService = threadingService;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public void SendEvent(object theEvent)
        {
            if (!theEvent.GetType().IsArray) {
                throw new EPException(
                    "Unexpected event object of type " + theEvent.GetType().CleanName() + ", expected Object[]");
            }

            var arr = (object[])theEvent;
            EventBean objectArrayEvent =
                eventBeanTypedEventFactory.AdapterForTypedObjectArray(arr, objectArrayEventType);

            if (threadingService.IsInboundThreading) {
                threadingService.SubmitInbound(objectArrayEvent, runtimeEventSender);
            }
            else {
                runtimeEventSender.ProcessWrappedEvent(objectArrayEvent);
            }
        }

        public void RouteEvent(object theEvent)
        {
            if (!theEvent.GetType().IsArray) {
                throw new EPException(
                    "Unexpected event object of type " + theEvent.GetType().CleanName() + ", expected Object[]");
            }

            var arr = (object[])theEvent;
            EventBean objectArrayEvent =
                eventBeanTypedEventFactory.AdapterForTypedObjectArray(arr, objectArrayEventType);
            runtimeEventSender.RouteEventBean(objectArrayEvent);
        }
    }
} // end of namespace