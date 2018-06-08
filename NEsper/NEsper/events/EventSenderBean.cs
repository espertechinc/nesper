///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.events.bean;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for object events.
    /// <para>
    /// Allows sending only event objects of the underlying type matching the event type, or
    /// implementing the interface or extending the type. Any other event object generates an error.
    /// </para>
    /// </summary>
    public class EventSenderBean : EventSender
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntimeEventSender _runtime;
        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly ISet<Type> _compatibleClasses;
        private readonly ThreadingService _threadingService;
        private readonly ILockable _iLock;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtime">for processing events</param>
        /// <param name="beanEventType">the event type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="lockManager">The lock manager.</param>
        public EventSenderBean(
            EPRuntimeEventSender runtime,
            BeanEventType beanEventType,
            EventAdapterService eventAdapterService,
            ThreadingService threadingService,
            ILockManager lockManager)
        {
            _iLock = lockManager.CreateLock(GetType());
            _runtime = runtime;
            _beanEventType = beanEventType;
            _eventAdapterService = eventAdapterService;
            _compatibleClasses = new HashSet<Type>();
            _threadingService = threadingService;
        }

        public void SendEvent(Object theEvent)
        {
            if (theEvent == null)
            {
                throw new ArgumentNullException("theEvent", "No event object provided to sendEvent method");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                if ((!(theEvent is CurrentTimeEvent)) || (ExecutionPathDebugLog.IsTimerDebugEnabled))
                {
                    Log.Debug(".sendEvent Processing event " + theEvent);
                }
            }

            EventBean eventBean = GetEventBean(theEvent);

            // Process event
            if ((ThreadingOption.IsThreadingEnabled) && (_threadingService.IsInboundThreading))
            {
                _threadingService.SubmitInbound(new InboundUnitSendWrapped(eventBean, _runtime).Run);
            }
            else
            {
                _runtime.ProcessWrappedEvent(eventBean);
            }
        }

        public void Route(Object theEvent)
        {
            EventBean eventBean = GetEventBean(theEvent);
            _runtime.RouteEventBean(eventBean);
        }

        private EventBean GetEventBean(Object theEvent)
        {
            // type check
            if (theEvent.GetType() != _beanEventType.UnderlyingType)
            {
                using (_iLock.Acquire())
                {
                    if (!_compatibleClasses.Contains(theEvent.GetType()))
                    {
                        if (TypeHelper.IsSubclassOrImplementsInterface(
                            theEvent.GetType(), _beanEventType.UnderlyingType))
                        {
                            _compatibleClasses.Add(theEvent.GetType());
                        }
                        else
                        {
                            throw new EPException(
                                "Event object of type " + theEvent.GetType().FullName +
                                " does not equal, extend or implement the type " + _beanEventType.UnderlyingType.FullName +
                                " of event type '" + _beanEventType.Name + "'");
                        }
                    }
                }
            }

            return _eventAdapterService.AdapterForTypedObject(theEvent, _beanEventType);
        }
    }
} // end of namespace
