///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Service for processing events that originate from listeners, subscribers or other extension code.
    /// </summary>
    public interface EPEventServiceRouteEvent
    {
        /// <summary>
        /// Route the event object back to the runtime for internal dispatching,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// The route event is processed just like it was sent to the runtime, that is any
        /// active expressions seeking that event receive it. The routed event has priority over other
        /// events sent to the runtime. In a single-threaded application the routed event is
        /// processed before the next event is sent to the runtime.
        /// <para />Note: when outbound-threading is enabled, the thread delivering to listeners
        /// is not the thread processing the original event. Therefore with outbound-threading
        /// enabled the sendEvent method should be used by listeners instead.
        /// </summary>
        /// <param name="event">to route internally for processing by the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        void RouteEventObjectArray(object[] @event, string eventTypeName);

        /// <summary>
        /// Route the event object back to the runtime for internal dispatching,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// The route event is processed just like it was sent to the runtime, that is any
        /// active expressions seeking that event receive it. The routed event has priority over other
        /// events sent to the runtime. In a single-threaded application the routed event is
        /// processed before the next event is sent to the runtime.
        /// <para />Note: when outbound-threading is enabled, the thread delivering to listeners
        /// is not the thread processing the original event. Therefore with outbound-threading
        /// enabled the sendEvent method should be used by listeners instead.
        /// </summary>
        /// <param name="event">to route internally for processing by the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        void RouteEventBean(object @event, string eventTypeName);

        /// <summary>
        /// Route the event object back to the runtime for internal dispatching,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// The route event is processed just like it was sent to the runtime, that is any
        /// active expressions seeking that event receive it. The routed event has priority over other
        /// events sent to the runtime. In a single-threaded application the routed event is
        /// processed before the next event is sent to the runtime.
        /// <para />Note: when outbound-threading is enabled, the thread delivering to listeners
        /// is not the thread processing the original event. Therefore with outbound-threading
        /// enabled the sendEvent method should be used by listeners instead.
        /// </summary>
        /// <param name="event">to route internally for processing by the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        void RouteEventMap(IDictionary<string, object> @event, string eventTypeName);

        /// <summary>
        /// Route the event object back to the runtime for internal dispatching,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// The route event is processed just like it was sent to the runtime, that is any
        /// active expressions seeking that event receive it. The routed event has priority over other
        /// events sent to the runtime. In a single-threaded application the routed event is
        /// processed before the next event is sent to the runtime.
        /// <para />Note: when outbound-threading is enabled, the thread delivering to listeners
        /// is not the thread processing the original event. Therefore with outbound-threading
        /// enabled the sendEvent method should be used by listeners instead.
        /// </summary>
        /// <param name="event">to route internally for processing by the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        void RouteEventXMLDOM(XmlNode @event, string eventTypeName);

        /// <summary>
        /// Route the event object back to the runtime for internal dispatching,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// The route event is processed just like it was sent to the runtime, that is any
        /// active expressions seeking that event receive it. The routed event has priority over other
        /// events sent to the runtime. In a single-threaded application the routed event is
        /// processed before the next event is sent to the runtime.
        /// <para />Note: when outbound-threading is enabled, the thread delivering to listeners
        /// is not the thread processing the original event. Therefore with outbound-threading
        /// enabled the sendEvent method should be used by listeners instead.
        /// </summary>
        /// <param name="avroGenericDataDotRecord">to route internally for processing by the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        void RouteEventAvro(object avroGenericDataDotRecord, string eventTypeName);
    }
} // end of namespace