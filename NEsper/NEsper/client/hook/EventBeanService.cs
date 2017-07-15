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
        EventType GetExistsTypeByName(string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given bean (Object, POJO) using the class of the object to determine the Bean-only event type (not for Map/Object-Array/Avro/XML events)
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <returns>event bean</returns>
        EventBean AdapterForBean(Object theEvent);
    
        /// <summary>
        /// Construct an event bean for a given bean (Object, POJO) and given the Bean-event-type
        /// </summary>
        /// <param name="bean">event underlying</param>
        /// <param name="eventType">event type (Bean only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedBean(Object bean, EventType eventType);
    
        /// <summary>
        /// Construct an event bean for a given Avro GenericData.Record using the event type name to look up the Avro event type
        /// </summary>
        /// <param name="avroGenericDataDotRecord">event underlying</param>
        /// <param name="eventTypeName">name of the Avro event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForAvro(Object avroGenericDataDotRecord, string eventTypeName);
    
        /// <summary>
        /// Construct an event bean for a given Avro GenericData.Record and given the Avro-event-type
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
        EventBean AdapterForTypedDOM(XmlNode node, EventType eventType);

        /// <summary>
        /// Construct an event bean for a given XML-DOM using the node root node name to look up the XML-DOM event type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <returns>event bean</returns>
        EventBean AdapterForDOM(XNode node);

        /// <summary>
        /// Construct an event bean for a given Node and given the XML-event-type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <param name="eventType">event type (XML only)</param>
        /// <returns>event bean</returns>
        EventBean AdapterForTypedDOM(XNode node, EventType eventType);

        /// <summary>
        /// Construct an event bean for a given object and event type, wherein it is assumed that the object matches the event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventType">event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForType(Object theEvent, EventType eventType);
    }
} // end of namespace
