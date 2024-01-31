///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Event sender for PONO object events.
    ///     <para />
    ///     Allows sending only event objects of the underlying type matching the event type, or
    ///     implementing the interface or extending the type. Any other event object generates an error.
    /// </summary>
    public class EventSenderBean : EventSender
    {
        private readonly BeanEventType _beanEventType;
        private readonly ISet<Type> _compatibleClasses;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EPRuntimeEventProcessWrapped _runtime;
        private readonly ThreadingCommon _threadingService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtime">for processing events</param>
        /// <param name="beanEventType">the event type</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="threadingService">for inbound threading</param>
        public EventSenderBean(
            EPRuntimeEventProcessWrapped runtime,
            BeanEventType beanEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ThreadingCommon threadingService)
        {
            _runtime = runtime;
            _beanEventType = beanEventType;
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _compatibleClasses = new HashSet<Type>();
            _threadingService = threadingService;
        }

        public void SendEvent(object theEvent)
        {
            if (theEvent == null) {
                throw new ArgumentNullException(nameof(theEvent), "No event object provided to sendEvent method");
            }

            var eventBean = GetEventBean(theEvent);

            // Process event
            if (_threadingService.IsInboundThreading) {
                _threadingService.SubmitInbound(eventBean, _runtime);
            }
            else {
                _runtime.ProcessWrappedEvent(eventBean);
            }
        }

        public void RouteEvent(object theEvent)
        {
            var eventBean = GetEventBean(theEvent);
            _runtime.RouteEventBean(eventBean);
        }

        private EventBean GetEventBean(object theEvent)
        {
            // type check
            if (theEvent.GetType() != _beanEventType.UnderlyingType) {
                lock (this) {
                    if (!_compatibleClasses.Contains(theEvent.GetType())) {
                        if (TypeHelper.IsSubclassOrImplementsInterface(
                                theEvent.GetType(),
                                _beanEventType.UnderlyingType)) {
                            _compatibleClasses.Add(theEvent.GetType());
                        }
                        else {
                            throw new EPException(
                                "Event object of type " +
                                theEvent.GetType().CleanName() +
                                " does not equal, extend or implement the type " +
                                _beanEventType.UnderlyingType.CleanName() +
                                " of event type '" +
                                _beanEventType.Name +
                                "'");
                        }
                    }
                }
            }

            return _eventBeanTypedEventFactory.AdapterForTypedObject(theEvent, _beanEventType);
        }
    }
} // end of namespace