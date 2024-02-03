///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    /// <summary>
    ///     Event sender for json-backed events.
    ///     <para>
    ///         Allows sending only event objects of type string, does not check contents. Any other event object generates an error.
    ///     </para>
    /// </summary>
    public class EventSenderJsonImpl : EventSenderJson
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly JsonEventType _eventType;
        private readonly EPRuntimeEventProcessWrapped _runtimeEventSender;
        private readonly ThreadingCommon _threadingService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="eventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
        public EventSenderJsonImpl(
            EPRuntimeEventProcessWrapped runtimeEventSender,
            JsonEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ThreadingCommon threadingService)
        {
            _runtimeEventSender = runtimeEventSender;
            _eventType = eventType;
            _threadingService = threadingService;
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public void SendEvent(object theEvent)
        {
            var underlying = GetUnderlying(theEvent);
            var eventBean = _eventBeanTypedEventFactory.AdapterForTypedJson(underlying, _eventType);

            if (_threadingService.IsInboundThreading) {
                _threadingService.SubmitInbound(eventBean, _runtimeEventSender);
            }
            else {
                _runtimeEventSender.ProcessWrappedEvent(eventBean);
            }
        }

        public void RouteEvent(object theEvent)
        {
            var eventBean = _eventBeanTypedEventFactory.AdapterForTypedJson(GetUnderlying(theEvent), _eventType);
            _runtimeEventSender.RouteEventBean(eventBean);
        }

        public object Parse(string json)
        {
            return _eventType.Parse(json);
        }

        private object GetUnderlying(object theEvent)
        {
            if (theEvent is string theString) {
                return _eventType.Parse(theString);
            }

            if (theEvent == null || !(theEvent.GetType() == _eventType.UnderlyingType)) {
                throw new EPException(
                    "Unexpected event object of type '" +
                    (theEvent == null ? "(null)" : theEvent.GetType().Name) +
                    "', expected a Json-formatted string-type value");
            }

            return theEvent;
        }
    }
} // end of namespace