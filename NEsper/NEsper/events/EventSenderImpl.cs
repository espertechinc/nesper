///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for use with plug-in event representations.
    /// <para/>
    /// The implementation asks a list of event bean factoryies originating from
    /// plug-in event representations to each reflect on the event and generate an event bean.
    /// The first one to return an event bean wins.
    /// </summary>
    public class EventSenderImpl : EventSender
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<EventSenderURIDesc> _handlingFactories;
        private readonly EPRuntimeEventSender _epRuntime;
        private readonly ThreadingService _threadingService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="handlingFactories">list of factories</param>
        /// <param name="epRuntime">the runtime to use to process the event</param>
        /// <param name="threadingService">for inbound threading</param>
        public EventSenderImpl(List<EventSenderURIDesc> handlingFactories, EPRuntimeEventSender epRuntime, ThreadingService threadingService)
        {
            _handlingFactories = handlingFactories;
            _epRuntime = epRuntime;
            _threadingService = threadingService;
        }

        public void SendEvent(Object theEvent)
        {
            SendIn(theEvent, false);
        }

        public void Route(Object theEvent)
        {
            SendIn(theEvent, true);
        }

        private void SendIn(Object theEvent, bool isRoute)
        {
            // Ask each factory in turn to take care of it
            foreach (EventSenderURIDesc entry in _handlingFactories)
            {
                EventBean eventBean = null;

                try
                {
                    eventBean = entry.BeanFactory(theEvent, entry.ResolutionURI);
                }
                catch (Exception ex)
                {
                    Log.Warn("Unexpected exception thrown by plug-in event bean factory '" + entry.BeanFactory + "' processing event " + theEvent, ex);
                }

                if (eventBean != null)
                {
                    if (isRoute)
                    {
                        _epRuntime.RouteEventBean(eventBean);
                    }
                    else
                    {
                        if ((ThreadingOption.IsThreadingEnabled) && (_threadingService.IsInboundThreading))
                        {
                            _threadingService.SubmitInbound(() => _epRuntime.ProcessWrappedEvent(eventBean));
                        }
                        else
                        {
                            _epRuntime.ProcessWrappedEvent(eventBean);
                        }
                    }
                    return;
                }
            }
        }
    }
}
