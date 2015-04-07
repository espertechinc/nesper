///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.core.service;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Provided once by an <seealso cref="PlugInEventRepresentation"/> for any event type it creates.
    /// </summary>
    public interface PlugInEventTypeHandler
    {
        /// <summary>Returns the event type. </summary>
        /// <value>event type.</value>
        EventType EventType { get; }

        /// <summary>Returns a facility responsible for converting or wrapping event objects. </summary>
        /// <param name="runtimeEventSender">for sending events into the engine</param>
        /// <returns>sender</returns>
        EventSender GetSender(EPRuntimeEventSender runtimeEventSender);
    }

    public class ProxyPlugInEventTypeHandler : PlugInEventTypeHandler
    {
        public Func<EventType> EventTypeFunc { get; set; }
        public Func<EPRuntimeEventSender, EventSender> GetSenderFunc { get; set; }

        public EventType EventType
        {
            get { return EventTypeFunc(); }
        }

        public EventSender GetSender(EPRuntimeEventSender runtimeEventSender)
        {
            return GetSenderFunc(runtimeEventSender);
        }
    }
}
