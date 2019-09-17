///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly BeanEventType beanEventType;
        private readonly ISet<Type> compatibleClasses;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EPRuntimeEventProcessWrapped runtime;
        private readonly ThreadingCommon threadingService;

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
            this.runtime = runtime;
            this.beanEventType = beanEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            compatibleClasses = new HashSet<Type>();
            this.threadingService = threadingService;
        }

        public void SendEvent(object theEvent)
        {
            if (theEvent == null) {
                throw new ArgumentNullException(nameof(theEvent), "No event object provided to sendEvent method");
            }

            var eventBean = GetEventBean(theEvent);

            // Process event
            if (threadingService.IsInboundThreading) {
                threadingService.SubmitInbound(eventBean, runtime);
            }
            else {
                runtime.ProcessWrappedEvent(eventBean);
            }
        }

        public void RouteEvent(object theEvent)
        {
            var eventBean = GetEventBean(theEvent);
            runtime.RouteEventBean(eventBean);
        }

        private EventBean GetEventBean(object theEvent)
        {
            // type check
            if (theEvent.GetType() != beanEventType.UnderlyingType) {
                lock (this) {
                    if (!compatibleClasses.Contains(theEvent.GetType())) {
                        if (TypeHelper.IsSubclassOrImplementsInterface(
                            theEvent.GetType(),
                            beanEventType.UnderlyingType)) {
                            compatibleClasses.Add(theEvent.GetType());
                        }
                        else {
                            throw new EPException(
                                "Event object of type " +
                                theEvent.GetType().CleanName() +
                                " does not equal, extend or implement the type " +
                                beanEventType.UnderlyingType.CleanName() +
                                " of event type '" +
                                beanEventType.Name +
                                "'");
                        }
                    }
                }
            }

            return eventBeanTypedEventFactory.AdapterForTypedBean(theEvent, beanEventType);
        }
    }
} // end of namespace