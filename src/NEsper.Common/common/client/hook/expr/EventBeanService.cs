///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.client.hook.expr
{
    /// <summary>
    /// Services for obtaining <seealso cref="EventType" /> information and constructing <seealso cref="EventBean" /> events.
    /// </summary>
    public interface EventBeanService : EventBeanTypedEventFactory
    {
        /// <summary>
        /// Construct an event bean for a given Map using the event type name to look up the Map event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventTypeName">name of the Map event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForMap(
            IDictionary<string, object> theEvent,
            string eventTypeName);

        /// <summary>
        /// Look up an event type by name,
        /// </summary>
        /// <param name="eventTypeName">to look up</param>
        /// <returns>event type or null if not found</returns>
        EventType GetExistsTypeByName(string eventTypeName);

        /// <summary>
        /// Construct an event bean for a given bean (Object, PONO) using the class of the object to determine the Bean-only event type (not for Map/Object-Array/Avro/XML events)
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventTypeName">event type name</param>
        /// <returns>event bean</returns>
        EventBean AdapterForBean(
            object theEvent,
            string eventTypeName);

        /// <summary>
        /// Construct an event bean for a given Avro GenericRecord using the event type name to look up the Avro event type
        /// </summary>
        /// <param name="avroGenericDataDotRecord">event underlying</param>
        /// <param name="eventTypeName">name of the Avro event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForAvro(
            object avroGenericDataDotRecord,
            string eventTypeName);

        /// <summary>
        /// Construct an event bean for a given Object-Array using the event type name to look up the Object-Array event type
        /// </summary>
        /// <param name="theEvent">event underlying</param>
        /// <param name="eventTypeName">name of the Object-Array event type</param>
        /// <returns>event bean</returns>
        EventBean AdapterForObjectArray(
            object[] theEvent,
            string eventTypeName);

        /// <summary>
        /// Construct an event bean for a given XML-DOM using the node root node name to look up the XML-DOM event type
        /// </summary>
        /// <param name="node">event underlying</param>
        /// <param name="eventTypeName">event type name</param>
        /// <returns>event bean</returns>
        EventBean AdapterForDOM(
            XmlNode node,
            string eventTypeName);
    }
} // end of namespace