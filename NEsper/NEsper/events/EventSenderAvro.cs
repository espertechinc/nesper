///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for avro-backed events.
    /// <para>
    /// Allows sending only event objects of type GenericData.Record, does not check contents. Any other event object generates an error.
    /// </para>
    /// </summary>
    public class EventSenderAvro : EventSender
    {
        private readonly EPRuntimeEventSender _runtimeEventSender;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _eventType;
        private readonly ThreadingService _threadingService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="eventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventAdapterService">for event bean creation</param>
        public EventSenderAvro(
            EPRuntimeEventSender runtimeEventSender,
            EventType eventType,
            EventAdapterService eventAdapterService,
            ThreadingService threadingService)
        {
            _runtimeEventSender = runtimeEventSender;
            _eventType = eventType;
            _threadingService = threadingService;
            _eventAdapterService = eventAdapterService;
        }

        public void SendEvent(Object theEvent)
        {
            EventBean eventBean = _eventAdapterService.AdapterForTypedAvro(theEvent, _eventType);

            if ((ThreadingOption.IsThreadingEnabled) && (_threadingService.IsInboundThreading))
            {
                _threadingService.SubmitInbound(new InboundUnitSendWrapped(eventBean, _runtimeEventSender));
            }
            else
            {
                _runtimeEventSender.ProcessWrappedEvent(eventBean);
            }
        }

        public void Route(Object theEvent)
        {
            if (!(theEvent.GetType().IsArray))
            {
                throw new EPException(
                    "Unexpected event object of type " + theEvent.GetType().FullName + ", expected Object[]");
            }
            EventBean eventBean = _eventAdapterService.AdapterForTypedAvro(theEvent, _eventType);
            _runtimeEventSender.RouteEventBean(eventBean);
        }
    }
} // end of namespace
