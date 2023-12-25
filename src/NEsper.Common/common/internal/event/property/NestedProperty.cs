///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
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
    /// This class represents a nested property, each nesting level made up of a property instance that
    /// can be of type indexed, mapped or simple itself.
    /// <para />The syntax for nested properties is as follows.
    /// a.n
    /// a[1].n
    /// a('1').n
    /// </summary>
    public class NestedProperty : Property
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="properties">is the list of Property instances representing each nesting level</param>
        public NestedProperty(IList<Property> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Returns the list of property instances making up the nesting levels.
        /// </summary>
        /// <value>list of Property instances</value>
        public IList<Property> Properties { get; }

        public bool IsDynamic {
            get {
                foreach (var property in Properties) {
                    if (property.IsDynamic) {
                        return true;
                    }
                }

                return false;
            }
        }

        public EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();

            Property lastProperty = null;
            var publicFields = eventType.Stem.IsPublicFields;

            var properties = Properties;
            var propertiesCount = properties.Count;

            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = properties[ii];
                lastProperty = property;
                EventPropertyGetter getter = property.GetGetter(
                    eventType,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
                if (getter == null) {
                    return null;
                }

                if (ii < propertiesCount - 1) {
                    var clazz = property.GetPropertyType(eventType, beanEventTypeFactory);
                    if (clazz == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    // Map cannot be used to further nest as the type cannot be determined
                    if (clazz.IsGenericDictionary()) {
                        return null;
                    }

                    if (clazz.IsArray) {
                        return null;
                    }

                    eventType = beanEventTypeFactory.GetCreateBeanType(clazz, publicFields);
                }

                getters.Add(getter);
            }

            var finalPropertyType = lastProperty.GetPropertyType(eventType, beanEventTypeFactory);
            return new NestedPropertyGetter(
                getters,
                eventBeanTypedEventFactory,
                finalPropertyType,
                beanEventTypeFactory);
        }

        public Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            Type result = null;
            var boxed = false;

            var properties = Properties;
            var propertiesCount = properties.Count;
            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = properties[ii];
                boxed |= !(property is SimpleProperty);
                result = property.GetPropertyType(eventType, beanEventTypeFactory);

                if (result == null) {
                    // property not found, return null
                    return null;
                }

                if (ii < propertiesCount - 1) {
                    // Map cannot be used to further nest as the type cannot be determined
                    var typeClass = result;
                    if (typeClass == typeof(IDictionary<string, object>) ||
                        typeClass.IsArray ||
                        typeClass.IsPrimitive ||
                        typeClass.IsBuiltinDataType()) {
                        return null;
                    }

                    var publicFields = eventType.Stem.IsPublicFields;
                    eventType = beanEventTypeFactory.GetCreateBeanType(result, publicFields);
                }
            }

            if (result == null) {
                return null;
            }

            if (!boxed || result.CanNotBeNull()) {
                return result;
            }

            return result.GetBoxedType();
        }

        public Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            var propertiesCount = Properties.Count;
            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = Properties[ii];
                count++;
                var theBase = (PropertyBase)property;
                var propertyName = theBase.PropertyNameAtomic;

                object nestedType = null;
                if (currentDictionary != null) {
                    nestedType = currentDictionary.Get(propertyName);
                }

                if (nestedType == null) {
                    if (property is DynamicProperty) {
                        return typeof(object);
                    }
                    else {
                        return null;
                    }
                }

                if (ii < (propertiesCount - 1)) {
                    if (nestedType is Type nestedTypeType) {
                        return nestedTypeType;
                    }

                    if (nestedType is IDictionary<string, object>) {
                        return typeof(IDictionary<string, object>);
                    }
                }

                if (nestedType is Type type && type == typeof(IDictionary<string, object>)) {
                    return typeof(object);
                }

                if (nestedType is Type ponoType) {
                    if (!ponoType.IsArray) {
                        var beanType = beanEventTypeFactory.GetCreateBeanType(ponoType, false);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps);
                    }
                    else if (property is IndexedProperty) {
                        Type componentType = ponoType.GetComponentType();
                        var beanType = beanEventTypeFactory.GetCreateBeanType(componentType, false);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps);
                    }
                }

                if (nestedType is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                    var innerType = typeBeanOrUnderlying.EventType;
                    if (innerType == null) {
                        return null;
                    }

                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }

                if (nestedType is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                    var innerType = typeBeanOrUnderlyings[0].EventType;
                    if (innerType == null) {
                        return null;
                    }

                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }
                else if (nestedType is EventType innerType) {
                    // property type is the name of a map event type
                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }
                else {
                    if (!(nestedType is IDictionary<string, object>)) {
                        var message = "Nestable map type configuration encountered an unexpected value type of '" +
                                      nestedType.GetType() +
                                      "' for property '" +
                                      propertyName +
                                      "', expected Class, Map.class or Map<String, Object> as value type";
                        throw new PropertyAccessException(message);
                    }
                }

                currentDictionary = (IDictionary<string, object>)nestedType;
            }

            throw new IllegalStateException("Unexpected end of nested property");
        }

        public MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            IList<EventPropertyGetterSPI> getters = new List<EventPropertyGetterSPI>();
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            
            var propertiesCount = Properties.Count;
            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = Properties[ii];
                count++;

                // manufacture a getter for getting the item out of the map
                EventPropertyGetterSPI getter = property.GetGetterMap(
                    currentDictionary,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
                if (getter == null) {
                    return null;
                }

                getters.Add(getter);

                var theBase = (PropertyBase)property;
                var propertyName = theBase.PropertyNameAtomic;

                // For the next property if there is one, check how to property type is defined
                if (ii == (propertiesCount - 1)) {
                    continue;
                }

                if (currentDictionary != null) {
                    // check the type that this property will return
                    var propertyReturnType = currentDictionary.Get(propertyName);

                    if (propertyReturnType == null) {
                        currentDictionary = null;
                    }

                    if (propertyReturnType != null) {
                        if (propertyReturnType is IDictionary<string, object> mapReturnType) {
                            currentDictionary = mapReturnType;
                        }
                        else if (propertyReturnType is Type type &&
                                 type == typeof(IDictionary<string, object>)) {
                            currentDictionary = null;
                        }
                        else if (propertyReturnType is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                            var innerType = typeBeanOrUnderlying.EventType;
                            if (innerType == null) {
                                return null;
                            }

                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI)innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else if (propertyReturnType is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                            var innerType = typeBeanOrUnderlyings[0].EventType;
                            if (innerType == null) {
                                return null;
                            }

                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI)innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else if (propertyReturnType is EventType innerType) {
                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI)innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else {
                            // treat the return type of the map property as a object
                            var ponoType = (Type)propertyReturnType;
                            if (!ponoType.IsArray) {
                                var beanType = beanEventTypeFactory.GetCreateBeanType(ponoType, false);
                                var remainingProps = ToPropertyEPL(Properties, count);
                                var getterInner = beanType.GetGetterSPI(remainingProps);
                                if (getterInner == null) {
                                    return null;
                                }

                                getters.Add(getterInner);
                                break; // the single getter handles the rest
                            }
                            else {
                                Type componentType = ponoType.GetComponentType();
                                var beanType = beanEventTypeFactory.GetCreateBeanType(componentType, false);
                                var remainingProps = ToPropertyEPL(Properties, count);
                                var getterInner = beanType.GetGetterSPI(remainingProps);
                                if (getterInner == null) {
                                    return null;
                                }

                                getters.Add(getterInner);
                                break; // the single getter handles the rest
                            }
                        }
                    }
                }
            }

            var hasNonmapGetters = false;
            for (var i = 0; i < getters.Count; i++) {
                if (!(getters[i] is MapEventPropertyGetter)) {
                    hasNonmapGetters = true;
                }
            }

            if (!hasNonmapGetters) {
                return new MapNestedPropertyGetterMapOnly(getters, eventBeanTypedEventFactory);
            }
            else {
                return new MapNestedPropertyGetterMixedType(getters);
            }
        }

        public void ToPropertyEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var property in Properties) {
                writer.Write(delimiter);
                property.ToPropertyEPL(writer);
                delimiter = ".";
            }
        }

        public string[] ToPropertyArray()
        {
            IList<string> propertyNames = new List<string>();
            foreach (var property in Properties) {
                var nested = property.ToPropertyArray();
                propertyNames.AddAll(Arrays.AsList(nested));
            }

            return propertyNames.ToArray();
        }

        public EventPropertyGetterSPI GetterDOM {
            get {
                IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();

                foreach (var property in Properties) {
                    var getter = property.GetterDOM;
                    if (getter == null) {
                        return null;
                    }

                    getters.Add(getter);
                }

                return new DOMNestedPropertyGetter(getters, null);
            }
        }

        public EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex parentComplexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();

            var complexElement = parentComplexProperty;

            var propertiesCount = Properties.Count;
            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = Properties[ii];
                EventPropertyGetter getter = property.GetGetterDOM(
                    complexElement,
                    eventBeanTypedEventFactory,
                    eventType,
                    propertyExpression);
                if (getter == null) {
                    return null;
                }

                if (ii < (propertiesCount - 1)) {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement);
                    if (childSchemaItem == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    if (childSchemaItem is SchemaItemAttribute || childSchemaItem is SchemaElementSimple) {
                        return null;
                    }

                    complexElement = (SchemaElementComplex)childSchemaItem;

                    if (complexElement.IsArray) {
                        if (property is SimpleProperty || property is DynamicSimpleProperty) {
                            return null;
                        }
                    }
                }

                getters.Add(getter);
            }

            return new DOMNestedPropertyGetter(
                getters,
                new FragmentFactoryDOMGetter(eventBeanTypedEventFactory, eventType, propertyExpression));
        }

        public SchemaItem GetPropertyTypeSchema(SchemaElementComplex parentComplexProperty)
        {
            Property lastProperty = null;
            var complexElement = parentComplexProperty;

            var propertiesCount = Properties.Count;
            for (var ii = 0; ii < propertiesCount; ii++) {
                var property = Properties[ii];
                lastProperty = property;

                if (ii < (propertiesCount - 1)) {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement);
                    if (childSchemaItem == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    if (childSchemaItem is SchemaItemAttribute || childSchemaItem is SchemaElementSimple) {
                        return null;
                    }

                    complexElement = (SchemaElementComplex)childSchemaItem;
                }
            }

            return lastProperty.GetPropertyTypeSchema(complexElement);
        }

        public ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            throw new UnsupportedOperationException(
                "Object array nested property getter not implemented as not implicitly nestable");
        }

        public string PropertyNameAtomic =>
            throw new UnsupportedOperationException("Nested properties do not provide an atomic property name");

        private static string ToPropertyEPL(
            IList<Property> property,
            int startFromIndex)
        {
            var delimiter = "";
            var writer = new StringWriter();
            for (var i = startFromIndex; i < property.Count; i++) {
                writer.Write(delimiter);
                property[i].ToPropertyEPL(writer);
                delimiter = ".";
            }

            return writer.ToString();
        }
    }
} // end of namespace