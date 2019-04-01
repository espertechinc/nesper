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
    ///     This class represents a nested property, each nesting level made up of a property instance that
    ///     can be of type indexed, mapped or simple itself.
    ///     <para />
    ///     The syntax for nested properties is as follows.
    ///     a.n
    ///     a[1].n
    ///     a('1').n
    /// </summary>
    public class NestedProperty : Property
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="properties">is the list of Property instances representing each nesting level</param>
        public NestedProperty(IList<Property> properties)
        {
            Properties = properties;
        }

        /// <summary>
        ///     Returns the list of property instances making up the nesting levels.
        /// </summary>
        /// <returns>list of Property instances</returns>
        public IList<Property> Properties { get; }

        public virtual bool IsDynamic {
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
            for (int ii = 0; ii < Properties.Count; ii++)
            {
                Property property = Properties[ii];
                lastProperty = property;
                EventPropertyGetter getter = property.GetGetter(
                    eventType, eventBeanTypedEventFactory, beanEventTypeFactory);
                if (getter == null) {
                    return null;
                }

                if (it.MoveNext()) {
                    var clazz = property.GetPropertyType(eventType, beanEventTypeFactory);
                    if (clazz == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    // Map cannot be used to further nest as the type cannot be determined
                    if (clazz == typeof(IDictionary<object, object>)) {
                        return null;
                    }

                    if (clazz.IsArray) {
                        return null;
                    }

                    eventType = beanEventTypeFactory.GetCreateBeanType(clazz);
                }

                getters.Add(getter);
            }

            var finalPropertyType = lastProperty.GetPropertyTypeGeneric(eventType, beanEventTypeFactory);
            return new NestedPropertyGetter(
                getters, eventBeanTypedEventFactory, finalPropertyType.Type, finalPropertyType.Generic,
                beanEventTypeFactory);
        }

        public Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            Type result = null;
            var boxed = false;

            for (int ii = 0; ii < Properties.Count; ii++)
            {
                Property property = Properties[ii];
                boxed |= !(property is SimpleProperty);
                result = property.GetPropertyType(eventType, beanEventTypeFactory);

                if (result == null) {
                    // property not found, return null
                    return null;
                }

                if (it.MoveNext()) {
                    // Map cannot be used to further nest as the type cannot be determined
                    if (result == typeof(IDictionary<object, object>)) {
                        return null;
                    }

                    if (result.IsArray || result.IsPrimitive || TypeHelper.IsJavaBuiltinDataType(result)) {
                        return null;
                    }

                    eventType = beanEventTypeFactory.GetCreateBeanType(result);
                }
            }

            return !boxed ? result : result.GetBoxedType();
        }

        public GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            GenericPropertyDesc result = null;

            for (int ii = 0; ii < Properties.Count; ii++)
            {
                Property property = Properties[ii];
                result = property.GetPropertyTypeGeneric(eventType, beanEventTypeFactory);

                if (result == null) {
                    // property not found, return null
                    return null;
                }

                if (it.MoveNext()) {
                    // Map cannot be used to further nest as the type cannot be determined
                    if (result.Type == typeof(IDictionary<object, object>)) {
                        return null;
                    }

                    if (result.Type.IsArray) {
                        return null;
                    }

                    eventType = beanEventTypeFactory.GetCreateBeanType(result.Type);
                }
            }

            return result;
        }

        public void ToPropertyEPL(StringWriter writer)
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
                propertyNames.AddAll(nested);
            }

            return propertyNames.ToArray();
        }

        public EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex parentComplexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType, 
            string propertyExpression)
        {
            IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();

            var complexElement = parentComplexProperty;

            for (int ii = 0; ii < Properties.Count; ii++)
            {
                Property property = Properties[ii];
                EventPropertyGetter getter = property.GetGetterDOM(
                    complexElement, eventBeanTypedEventFactory, eventType, propertyExpression);
                if (getter == null) {
                    return null;
                }

                if (it.MoveNext()) {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement);
                    if (childSchemaItem == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    if (childSchemaItem is SchemaItemAttribute || childSchemaItem is SchemaElementSimple) {
                        return null;
                    }

                    complexElement = (SchemaElementComplex) childSchemaItem;

                    if (complexElement.IsArray) {
                        if (property is SimpleProperty || property is DynamicSimpleProperty) {
                            return null;
                        }
                    }
                }

                getters.Add(getter);
            }

            return new DOMNestedPropertyGetter(
                getters, new FragmentFactoryDOMGetter(eventBeanTypedEventFactory, eventType, propertyExpression));
        }

        public SchemaItem GetPropertyTypeSchema(SchemaElementComplex parentComplexProperty)
        {
            Property lastProperty = null;
            var complexElement = parentComplexProperty;

            for (int ii = 0; ii < Properties.Count; ii++)
            {
                Property property = Properties[ii];
                lastProperty = property;

                if (it.MoveNext()) {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement);
                    if (childSchemaItem == null) {
                        // if the property is not valid, return null
                        return null;
                    }

                    if (childSchemaItem is SchemaItemAttribute || childSchemaItem is SchemaElementSimple) {
                        return null;
                    }

                    complexElement = (SchemaElementComplex) childSchemaItem;
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

        public virtual string PropertyNameAtomic =>
            throw new UnsupportedOperationException("Nested properties do not provide an atomic property name");

        public virtual Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes, 
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            for (int ii = 0 ; ii < Properties.Count ; ii++) { 
                count++;
                Property property = Properties[ii];
                var theBase = (PropertyBase) property;
                var propertyName = theBase.PropertyNameAtomic;

                object nestedType = null;
                if (currentDictionary != null) {
                    nestedType = currentDictionary.Get(propertyName);
                }

                if (nestedType == null) {
                    if (property is DynamicProperty) {
                        return typeof(object);
                    }

                    return null;
                }

                if (!it.MoveNext()) {
                    if (nestedType is Type) {
                        return (Type) nestedType;
                    }

                    if (nestedType is IDictionary<string, object>) {
                        return typeof(IDictionary<string, object>);
                    }
                }

                if (Equals(nestedType, typeof(IDictionary<string, object>))) {
                    return typeof(object);
                }

                if (nestedType is Type) {
                    var pojoClass = (Type) nestedType;
                    if (!pojoClass.IsArray) {
                        var beanType = beanEventTypeFactory.GetCreateBeanType(pojoClass);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps);
                    }

                    if (property is IndexedProperty) {
                        var componentType = pojoClass.GetElementType();
                        var beanType = beanEventTypeFactory.GetCreateBeanType(componentType);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps);
                    }
                }

                if (nestedType is TypeBeanOrUnderlying) {
                    var innerType = ((TypeBeanOrUnderlying) nestedType).EventType;
                    if (innerType == null) {
                        return null;
                    }

                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }

                if (nestedType is TypeBeanOrUnderlying[]) {
                    var innerType = ((TypeBeanOrUnderlying[]) nestedType)[0].EventType;
                    if (innerType == null) {
                        return null;
                    }

                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }

                if (nestedType is EventType) {
                    // property type is the name of a map event type
                    var innerType = (EventType) nestedType;
                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps);
                }

                if (!(nestedType is IDictionary<string, object>)) {
                    var message = "Nestable map type configuration encountered an unexpected value type of '"
                                  + nestedType.GetType() + "' for property '" + propertyName +
                                  "', expected Type or Map<string, object> as value type";
                    throw new PropertyAccessException(message);
                }

                currentDictionary = (IDictionary<string, object>) nestedType;
            }

            throw new IllegalStateException("Unexpected end of nested property");
        }

        public virtual MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            IList<EventPropertyGetterSPI> getters = new List<EventPropertyGetterSPI>();
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            for (int ii = 0; ii < Properties.Count; ii++)
            {
                count++;
                Property property = Properties[ii];

                // manufacture a getter for getting the item out of the map
                EventPropertyGetterSPI getter = property.GetGetterMap(
                    currentDictionary, eventBeanTypedEventFactory, beanEventTypeFactory);
                if (getter == null) {
                    return null;
                }

                getters.Add(getter);

                var theBase = (PropertyBase) property;
                var propertyName = theBase.PropertyNameAtomic;

                // For the next property if there is one, check how to property type is defined
                if (!it.MoveNext()) {
                    continue;
                }

                if (currentDictionary != null) {
                    // check the type that this property will return
                    var propertyReturnType = currentDictionary.Get(propertyName);

                    if (propertyReturnType == null) {
                        currentDictionary = null;
                    }

                    if (propertyReturnType != null) {
                        if (propertyReturnType is IDictionary<object, object>) {
                            currentDictionary = (IDictionary<object, object>) propertyReturnType;
                        }
                        else if (propertyReturnType.Equals(typeof(IDictionary<object, object>))) {
                            currentDictionary = null;
                        }
                        else if (propertyReturnType is TypeBeanOrUnderlying) {
                            var innerType = ((TypeBeanOrUnderlying) propertyReturnType).EventType;
                            if (innerType == null) {
                                return null;
                            }

                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI) innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else if (propertyReturnType is TypeBeanOrUnderlying[]) {
                            var innerType = ((TypeBeanOrUnderlying[]) propertyReturnType)[0].EventType;
                            if (innerType == null) {
                                return null;
                            }

                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI) innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else if (propertyReturnType is EventType) {
                            var innerType = (EventType) propertyReturnType;
                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI) innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null) {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else {
                            // treat the return type of the map property as an object
                            var pojoClass = (Type) propertyReturnType;
                            if (!pojoClass.IsArray) {
                                var beanType = beanEventTypeFactory.GetCreateBeanType(pojoClass);
                                var remainingProps = ToPropertyEPL(Properties, count);
                                var getterInner = beanType.GetGetterSPI(remainingProps);
                                if (getterInner == null) {
                                    return null;
                                }

                                getters.Add(getterInner);
                                break; // the single getter handles the rest
                            }
                            else {
                                var componentType = pojoClass.GetElementType();
                                var beanType = beanEventTypeFactory.GetCreateBeanType(componentType);
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

            return new MapNestedPropertyGetterMixedType(getters);
        }

        public EventPropertyGetterSPI GetterDOM {
            get {
                IList<EventPropertyGetter> getters = new List<EventPropertyGetter>();

                foreach (var property in Properties) {
                    EventPropertyGetter getter = property.GetterDOM;
                    if (getter == null) {
                        return null;
                    }

                    getters.Add(getter);
                }

                return new DOMNestedPropertyGetter(getters, null);
            }
        }

        private static string ToPropertyEPL(IList<Property> property, int startFromIndex)
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