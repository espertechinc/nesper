///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;

namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    ///     Interface for a property of an event of type BeanEventType (JavaBean event). Properties are designed to
    ///     handle the different types of properties for such events: indexed, mapped, simple, nested, or a combination of
    ///     those.
    /// </summary>
    public interface Property
    {
        /// <summary>
        ///     Returns the getter-method for use with XML DOM event representations.
        /// </summary>
        /// <value>getter</value>
        EventPropertyGetterSPI GetterDOM { get; }

        /// <summary>
        ///     Returns true for dynamic properties.
        /// </summary>
        /// <value>false for not-dynamic properties, true for dynamic properties.</value>
        bool IsDynamic { get; }

        string PropertyNameAtomic { get; }

        /// <summary>
        ///     Returns the property type.
        /// </summary>
        /// <param name="eventType">is the event type representing the JavaBean</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>property type class</returns>
        Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory);

        /// <summary>
        ///     Returns the property type plus its generic type parameter, if any.
        /// </summary>
        /// <param name="eventType">is the event type representing the JavaBean</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>type and generic descriptor</returns>
        GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory);

        /// <summary>
        ///     Returns value getter for the property of an event of the given event type.
        /// </summary>
        /// <param name="eventType">is the type of event to make a getter for</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>fast property value getter for property</returns>
        EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        /// <summary>
        ///     Returns the property type for use with Map event representations.
        /// </summary>
        /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>property type @param optionalMapPropTypes</returns>
        Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory);

        /// <summary>
        ///     Returns the getter-method for use with Map event representations.
        /// </summary>
        /// <param name="optionalMapPropTypes">a map-within-map type definition, if supplied, or null if not supplied</param>
        /// <param name="eventBeanTypedEventFactory">for resolving further map event types that are property types</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>getter for maps</returns>
        MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        /// <summary>
        ///     Returns the property type for use with DOM event representations.
        /// </summary>
        /// <param name="complexProperty">a element-within-element type definition</param>
        /// <returns>property type</returns>
        SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty);

        /// <summary>
        ///     Returns the getter-method for use with XML DOM event representations.
        /// </summary>
        /// <param name="complexProperty">a element-within-element type definition</param>
        /// <param name="eventBeanTypedEventFactory">for resolving or creating further event types that are property types</param>
        /// <param name="xmlEventType">the event type</param>
        /// <param name="propertyExpression">the full property expression</param>
        /// <returns>getter</returns>
        EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType xmlEventType,
            string propertyExpression);

        /// <summary>
        ///     Write the EPL-representation of the property.
        /// </summary>
        /// <param name="writer">to write to</param>
        void ToPropertyEPL(TextWriter writer);

        /// <summary>
        ///     Return a String-array of atomic property names.
        /// </summary>
        /// <returns>array of atomic names in a property expression</returns>
        string[] ToPropertyArray();
    }
} // end of namespace