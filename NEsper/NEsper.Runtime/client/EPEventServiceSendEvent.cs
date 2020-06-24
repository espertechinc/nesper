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
    /// Service for processing events.
    /// <para />Use any of the route-event methods of <seealso cref="EPEventServiceRouteEvent" /> when listeners, subscribers or extension code
    /// process events.
    /// </summary>
    public interface EPEventServiceSendEvent
    {
        /// <summary>
        /// Send an object array containing event property values to the runtime.
        /// <para />Use the route method for sending events into the runtime from within UpdateListener code.
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventObjectArray(Object[], String)}.
        /// </summary>
        /// <param name="event">array that contains event property values. Your application must ensure that property values match the exact same order that the property names and types have been declared, and that the array length matches the number of properties declared.
        /// </param>
        /// <param name="eventTypeName">event type name</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEventObjectArray(object[] @event, string eventTypeName);

        /// <summary>
        /// Send an event represented by an object to the runtime.
        /// <para />Use the route method for sending events into the runtime from within UpdateListener code,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventBean(Object, String)}.
        /// </summary>
        /// <param name="event">is the event to sent to the runtime</param>
        /// <param name="eventTypeName">event type name</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEventBean(object @event, string eventTypeName);

        /// <summary>
        /// Send a map containing event property values to the runtime.
        /// <para />Use the route method for sending events into the runtime from within UpdateListener code.
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventMap(IDictionary, String)}).
        /// </summary>
        /// <param name="event">map that contains event property values. Keys are expected to be of type String while value scan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName.
        /// </param>
        /// <param name="eventTypeName">event type name</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEventMap(IDictionary<string, object> @event, string eventTypeName);

        /// <summary>
        /// Send an event represented by a DOM node to the runtime.
        /// <para />Use the route method for sending events into the runtime from within UpdateListener code.
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventXMLDOM(Node, String)}.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <param name="eventTypeName">event type name</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEventXMLDOM(XmlNode node, string eventTypeName);

        /// <summary>
        /// Send an event represented by a Avro GenericData.Record to the runtime.
        /// <para />Use the route method for sending events into the runtime from within UpdateListener code,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventAvro(Object, String)}}).
        /// </summary>
        /// <param name="avroGenericDataDotRecord">is the event to sent to the runtime</param>
        /// <param name="avroEventTypeName">event type name</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEventAvro(object avroGenericDataDotRecord, string avroEventTypeName);
        
        /// <summary>
        /// Send an event represented by a String JSON to the runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code,
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent
        /// (except with the outbound-threading configuration), see {@link EPEventServiceRouteEvent#routeEventJson(String, String)}}).
        /// </summary>
        /// <param name="json">is the event to sent to the runtime</param>
        /// <param name="jsonEventTypeName">event type name</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEventJson(string json, string jsonEventTypeName);
    }
} // end of namespace