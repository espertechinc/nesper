///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Collector for send events into the event bus.
    /// </summary>
    public interface EventBusCollector
    {
        /// <summary>
        /// Send an event represented by a plain object to the event stream processing runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code, 
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// </para>
        /// </summary>
        /// <param name="object">is the event to sent to the runtime</param>
        /// <throws>com.espertech.esper.client.EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(Object @object);

        /// <summary>
        /// Send a map containing event property values to the event stream processing runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code
        /// to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// </para>
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type String while value scan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName.</param>
        /// <param name="eventTypeName">the name for the Map event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEvent(IDictionary<string,object> map, String eventTypeName);

        /// <summary>
        /// Send an object array containing event property values as array elements to the event stream processing runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code to avoid 
        /// the possibility of a stack overflow due to nested calls to sendEvent.
        /// </para>
        /// </summary>
        /// <param name="objectArray">object array that contains event property values.Your application must ensure that property values match the exact same order that the property names and types have been declared, and that the array length matches the number of properties declared.</param>
        /// <param name="eventTypeName">the name for the Object-array event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEvent(Object[] objectArray, String eventTypeName);

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code. to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// </para>
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(XmlNode node);

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para>
        /// Use the route method for sending events into the runtime from within UpdateListener code. to avoid the possibility of a stack overflow due to nested calls to sendEvent.
        /// </para>
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(XElement node);
    }
}
