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
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for map-backed events. 
    /// <para>
    /// Allows sending only event objects of type map, does not check map contents. 
    /// Any other event object generates an error.
    /// </para>
    /// </summary>
    public class EventSenderObjectArray : EventSender
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly ObjectArrayEventType _objectArrayEventType;
        private readonly EPRuntimeEventSender _runtimeEventSender;
        private readonly ThreadingService _threadingService;

        /// <summary>Ctor. </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="objectArrayEventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventAdapterService">for event bean creation</param>
        public EventSenderObjectArray(EPRuntimeEventSender runtimeEventSender,
                                      ObjectArrayEventType objectArrayEventType,
                                      EventAdapterService eventAdapterService,
                                      ThreadingService threadingService)
        {
            _runtimeEventSender = runtimeEventSender;
            _objectArrayEventType = objectArrayEventType;
            _threadingService = threadingService;
            _eventAdapterService = eventAdapterService;
        }

        #region EventSender Members

        public void SendEvent(Object theEvent)
        {
            if (!(theEvent.GetType().IsArray))
            {
                throw new EPException(string.Format("Unexpected event object of type {0}, expected {1}", 
                    theEvent.GetType().GetCleanName(), typeof(object[]).GetCleanName()));
            }

            var arr = (Object[]) theEvent;
            EventBean objectArrayEvent = _eventAdapterService.AdapterForTypedObjectArray(arr, _objectArrayEventType);

            if ((ThreadingOption.IsThreadingEnabledValue) && (_threadingService.IsInboundThreading))
            {
                _threadingService.SubmitInbound(new InboundUnitSendWrapped(objectArrayEvent, _runtimeEventSender).Run);
            }
            else
            {
                _runtimeEventSender.ProcessWrappedEvent(objectArrayEvent);
            }
        }

        public void Route(Object theEvent)
        {
            if (!(theEvent.GetType().IsArray))
            {
                throw new EPException("Unexpected event object of type " + theEvent.GetType().Name + ", expected Object[]");
            }
            var arr = (Object[]) theEvent;
            EventBean objectArrayEvent = _eventAdapterService.AdapterForTypedObjectArray(arr, _objectArrayEventType);
            _runtimeEventSender.RouteEventBean(objectArrayEvent);
        }

        #endregion
    }
}