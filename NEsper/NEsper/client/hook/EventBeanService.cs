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

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Services for obtaining <seealso cref="EventType" /> information and constructing <seealso cref="EventBean" /> events.
    /// </summary>
    public interface EventBeanService
    {
        /// <summary>
        /// Look up an event type by name,
        /// </summary>
        /// <param name="eventTypeName">to look up</param>
        /// <returns>event type or null if not found</returns>
        EventType GetEventTypeByName(string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given object using the class of the object to
        /// determine the event type (not for Map/Object-Array/Avro/XML events)
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <returns>event bean</returns>
        EventBean AdapterForObject(Object theEvent);
    
        /// <summary>
        /// Construct an event bean for a given object and given the event-type
        /// </summary>
        /// <param name="bean">event underlying</param>
        /// <param name="eventType">event type (Bean only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedObject(Object bean, EventType eventType);
    
        /// <summary>
        /// Construct an event bean for a given Avro GenericRecord using the event type name to look up the Avro event type
        /// </summary>
        /// <param name="avroGenericDataDotRecord">event underlying</param>
        /// <param name="eventTypeName">name of the Avro event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForAvro(Object avroGenericDataDotRecord, string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given Avro GenericRecord and given the Avro-event-type
        /// </summary>
        /// <param name="avroGenericDataDotRecord">event underlying</param>
        /// <param name="eventType">event type (Avro only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedAvro(Object avroGenericDataDotRecord, EventType eventType);
    
        /// <summary>
        /// Construct an event bean for a given Map using the event type name to look up the Map event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventTypeName">name of the Map event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForMap(IDictionary<string, Object> theEvent, string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given Map and given the Map-event-type
        /// </summary>
        /// <param name="properties">event underlying</param>
        /// <param name="eventType">event type (Map only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedMap(IDictionary<string, Object> properties, EventType eventType);
    
        /// <summary>
        /// Construct an event bean for a given Object-Array using the event type name to look up the Object-Array event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventTypeName">name of the Object-Array event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForObjectArray(Object[] theEvent, string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given Object-Array and given the Object-Array-event-type
        /// </summary>
        /// <param name="props">event underlying</param>
        /// <param name="eventType">event type (Object-array only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedObjectArray(Object[] props, EventType eventType);

        /// <summary>
        /// Returns an adapter for the LINQ element that exposes it's data as event
        /// properties for use in statements.
        /// </summary>
        /// <param name="element">is the element to wrap</param>
        /// <returns>
        /// event wrapper for document
        /// </returns>
        EventBean AdapterForDOM(XElement element);

        /// <summary>
        /// Construct an event bean for a given XML-DOM using the node root node name to look up the XML-DOM event type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <returns>event bean</returns>
        EventBean AdapterForDOM(XmlNode node);

        /// <summary>
        /// Construct an event bean for a given Node and given the XML-event-type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <param name="eventType">event type (XML only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedDOM(XObject node, EventType eventType);
        
        /// <summary>
        /// Construct an event bean for a given Node and given the XML-event-type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <param name="eventType">event type (XML only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedDOM(XmlNode node, EventType eventType);

        /// <summary>
        /// Construct an event bean for a given object and event type, wherein it is assumed that the object matches the event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventType">event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForType(Object theEvent, EventType eventType);
    }
} // end of namespace
