///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.events.property
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Interface for a property of an event of type BeanEventType.
    /// Properties are designed to handle the different types of properties for such events:
    /// indexed, mapped, simple, nested, or a combination of those.
    /// </summary>
    public interface Property
    {
        /// <summary>
        /// Returns the property type.
        /// </summary>
        /// <param name="eventType">is the event type representing the object</param>
        /// <param name="eventAdapterService">for event adapters</param>
        /// <returns>
        /// property type class
        /// </returns>
        Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService);
    
        /// <summary>
        /// Returns the property type plus its generic type parameter, if any.
        /// </summary>
        /// <param name="eventType">is the event type representing the object</param>
        /// <param name="eventAdapterService">for event adapters</param>
        /// <returns>
        /// type and generic descriptor
        /// </returns>
        GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType, EventAdapterService eventAdapterService);

        /// <summary>
        /// Returns value getter for the property of an event of the given event type.
        /// </summary>
        /// <param name="eventType">is the type of event to make a getter for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <returns>
        /// fast property value getter for property
        /// </returns>
        EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService);

        ObjectArrayEventPropertyGetter GetGetterObjectArray(IDictionary<string, int> indexPerProperty, IDictionary<string, object> nestableTypes, EventAdapterService eventAdapterService);

        /// <summary>
        /// Returns the property type for use with Map event representations.
        /// </summary>
        /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
        /// <param name="eventAdapterService">for resolving further map event types that are property types</param>
        /// <returns>
        /// property type @param optionalMapPropTypes
        /// </returns>
        Type GetPropertyTypeMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService);
    
        /// <summary>
        /// Returns the getter-method for use with Map event representations.
        /// </summary>
        /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
        /// <param name="eventAdapterService">for resolving further map event types that are property types</param>
        /// <returns>
        /// getter for maps
        /// </returns>
        MapEventPropertyGetter GetGetterMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService);
    
        /// <summary>
        /// Returns the property type for use with DOM event representations.
        /// </summary>
        /// <param name="complexProperty">a element-within-element type definition</param>
        /// <param name="eventAdapterService">for resolving further element event types if defined</param>
        /// <returns>
        /// property type
        /// </returns>
        SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService);

        /// <summary>
        /// Returns the getter-method for use with XML DOM event representations.
        /// </summary>
        /// <param name="complexProperty">a element-within-element type definition</param>
        /// <param name="eventAdapterService">for resolving or creating further event types that are property types</param>
        /// <param name="xmlEventType">the event type</param>
        /// <param name="propertyExpression">the full property expression</param>
        /// <returns>
        /// getter
        /// </returns>
        EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType xmlEventType, String propertyExpression);

        /// <summary>
        /// Returns the getter-method for use with XML DOM event representations.
        /// </summary>
        /// <returns>
        /// getter
        /// </returns>
        EventPropertyGetterSPI GetGetterDOM();

        /// <summary>
        /// Write the EPL-representation of the property.
        /// </summary>
        /// <param name="writer">to write to</param>
        void ToPropertyEPL(TextWriter writer);
    
        /// <summary>
        /// Return a String-array of atomic property names.
        /// </summary>
        /// <returns>
        /// array of atomic names in a property expression
        /// </returns>
        String[] ToPropertyArray();

        /// <summary>
        /// Returns true for dynamic properties.
        /// </summary>
        /// <returns>
        /// false for not-dynamic properties, true for dynamic properties.
        /// </returns>
        bool IsDynamic { get; }

        string PropertyNameAtomic { get; }
    }
}
