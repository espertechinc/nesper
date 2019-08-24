///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Event sender for map-backed events.
    ///     <para />
    ///     Allows sending only event objects of type map, does not check map contents. Any other event object generates an
    ///     error.
    /// </summary>
    public class EventSenderMap : EventSender
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly MapEventType mapEventType;
        private readonly EPRuntimeEventProcessWrapped runtimeEventSender;
        private readonly ThreadingCommon threadingService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="mapEventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
        public EventSenderMap(
            EPRuntimeEventProcessWrapped runtimeEventSender,
            MapEventType mapEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ThreadingCommon threadingService)
        {
            this.runtimeEventSender = runtimeEventSender;
            this.mapEventType = mapEventType;
            this.threadingService = threadingService;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public void SendEvent(object theEvent)
        {
            if (!(theEvent is IDictionary<string, object>)) {
                throw new EPException(
                    "Unexpected event object of type " +
                    theEvent.GetType().GetCleanName() +
                    ", expected " +
                    typeof(IDictionary<string, object>).GetCleanName());
            }

            var map = (IDictionary<string, object>) theEvent;
            EventBean mapEvent = eventBeanTypedEventFactory.AdapterForTypedMap(map, mapEventType);

            if (threadingService.IsInboundThreading) {
                threadingService.SubmitInbound(mapEvent, runtimeEventSender);
            }
            else {
                runtimeEventSender.ProcessWrappedEvent(mapEvent);
            }
        }

        public void RouteEvent(object theEvent)
        {
            if (!(theEvent is IDictionary<string, object>)) {
                throw new EPException(
                    "Unexpected event object of type " +
                    theEvent.GetType().GetCleanName() +
                    ", expected " +
                    typeof(IDictionary<string, object>).GetCleanName());
            }

            var map = (IDictionary<string, object>) theEvent;
            EventBean mapEvent = eventBeanTypedEventFactory.AdapterForTypedMap(map, mapEventType);
            runtimeEventSender.RouteEventBean(mapEvent);
        }
    }
} // end of namespace