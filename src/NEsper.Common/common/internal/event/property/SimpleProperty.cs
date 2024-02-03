///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    /// Represents a simple property of a given name.
    /// </summary>
    public class SimpleProperty : PropertyBase,
        PropertySimple
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "propertyName">is the property name</param>
        public SimpleProperty(string propertyName) : base(propertyName)
        {
        }

        public override string[] ToPropertyArray()
        {
            return new[] {
                PropertyNameAtomic
            };
        }

        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            if (!propertyDesc.PropertyType.IsSimple()) {
                return null;
            }

            return eventType.GetGetterSPI(PropertyNameAtomic);
        }

        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            return propertyDesc.ReturnType;
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            var def = optionalMapPropTypes?.Get(PropertyNameAtomic);
            if (def == null) {
                return null;
            }

            if (def is Type clazzType) {
                return clazzType;
            }
            else if (def is IDictionary<string, object>) {
                return typeof(IDictionary<string, object>);
            }
            else if (def is TypeBeanOrUnderlying underlying) {
                var eventType = underlying.EventType;
                return eventType.UnderlyingType;
            }
            else if (def is TypeBeanOrUnderlying[] underlyings) {
                var eventType = underlyings[0].EventType;
                return TypeHelper.GetArrayType(eventType.UnderlyingType);
            }
            else if (def is EventType type) {
                return type.UnderlyingType;
            }
            else if (def is EventType[] eventType) {
                return TypeHelper.GetArrayType(eventType[0].UnderlyingType);
            }

            var message = "Nestable map type configuration encountered an unexpected value type of '" +
                          def.GetType() +
                          "' for property '" +
                          PropertyNameAtomic +
                          "', expected Map or Class";
            throw new PropertyAccessException(message);
        }

        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            var def = optionalMapPropTypes?.Get(PropertyNameAtomic);
            if (def == null) {
                return null;
            }

            if (def is EventType eventType) {
                return new MapEventBeanPropertyGetter(PropertyNameAtomic, eventType.UnderlyingType);
            }

            return new MapPropertyGetterDefaultNoFragment(PropertyNameAtomic, eventBeanTypedEventFactory);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType xmlEventType,
            string propertyExpression)
        {
            foreach (var attribute in complexProperty.Attributes) {
                if (attribute.Name.Equals(PropertyNameAtomic)) {
                    return new DOMSimpleAttributeGetter(PropertyNameAtomic);
                }
            }

            foreach (var simple in complexProperty.SimpleElements) {
                if (simple.Name.Equals(PropertyNameAtomic)) {
                    return new DOMComplexElementGetter(PropertyNameAtomic, null, simple.IsArray);
                }
            }

            foreach (var complex in complexProperty.ComplexElements) {
                var complexFragmentFactory = new FragmentFactoryDOMGetter(
                    eventBeanTypedEventFactory,
                    xmlEventType,
                    propertyExpression);
                if (complex.Name.Equals(PropertyNameAtomic)) {
                    return new DOMComplexElementGetter(PropertyNameAtomic, complexFragmentFactory, complex.IsArray);
                }
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            return SchemaUtil.FindPropertyMapping(complexProperty, PropertyNameAtomic);
        }

        public override bool IsDynamic => false;

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (nestableTypes == null) {
                return null;
            }

            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out var propertyIndex)) {
                return null;
            }

            return new ObjectArrayPropertyGetterDefaultObjectArray(propertyIndex, null, eventBeanTypedEventFactory);
        }

        public override EventPropertyGetterSPI GetterDOM => new DOMAttributeAndElementGetter(PropertyNameAtomic);
    }
} // end of namespace