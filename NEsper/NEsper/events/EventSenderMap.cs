///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.events.map;
using com.espertech.esper.util;
using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for map-backed events.
    /// <para/>
    /// Allows sending only event objects of type map, does not check map contents. Any
    /// other event object generates an error.
    /// </summary>
    public class EventSenderMap : EventSender
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly MapEventType _mapEventType;
        private readonly EPRuntimeEventSender _runtimeEventSender;
        private readonly ThreadingService _threadingService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="mapEventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventAdapterService">for event bean creation</param>
        public EventSenderMap(EPRuntimeEventSender runtimeEventSender, MapEventType mapEventType,
                              EventAdapterService eventAdapterService, ThreadingService threadingService)
        {
            _runtimeEventSender = runtimeEventSender;
            _mapEventType = mapEventType;
            _threadingService = threadingService;
            _eventAdapterService = eventAdapterService;
        }

        public void SendEvent(Object theEvent)
        {
            if (!(theEvent is DataMap)) {
                throw new EPException(
                    string.Format(
                        "Unexpected event object of type {0}, expected {1}",
                        theEvent.GetType().GetCleanName(),
                        typeof(DataMap).GetCleanName()));
            }

            var map = (DataMap)theEvent;
            EventBean mapEvent = _eventAdapterService.AdapterForTypedMap(map, _mapEventType);

            if ((ThreadingOption.IsThreadingEnabledValue) && (_threadingService.IsInboundThreading))
            {
                _threadingService.SubmitInbound(() => _runtimeEventSender.ProcessWrappedEvent(mapEvent));
            }
            else
            {
                _runtimeEventSender.ProcessWrappedEvent(mapEvent);
            }
        }

        public void Route(Object theEvent)
        {
            if (!(theEvent is DataMap))
            {
                throw new EPException(
                    "Unexpected event object of type "
                    + Name.Clean(theEvent.GetType()) + ", expected "
                    + Name.Clean<DataMap>());
            }
            var map = (DataMap)theEvent;
            EventBean mapEvent = _eventAdapterService.AdapterForTypedMap(map, _mapEventType);
            _runtimeEventSender.RouteEventBean(mapEvent);
        }
    }
}
