///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    /// Represents an indexed property or array property, ie. an 'value' property with read method getValue(int index)
    /// or a 'array' property via read method getArray() returning an array.
    /// </summary>
    public class IndexedProperty : PropertyBase,
        PropertyWithIndex
    {
        private int _index;

        public IndexedProperty(string propertyName) : base(propertyName)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="index">is the index to use to access the property value</param>
        public IndexedProperty(
            string propertyName,
            int index) : base(propertyName)
        {
            _index = index;
        }

        public override bool IsDynamic => false;

        public override string[] ToPropertyArray()
        {
            return new string[] { PropertyNameAtomic };
        }

        /// <summary>
        /// Returns index for indexed access.
        /// </summary>
        /// <value>index value</value>
        public int Index => _index;

        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (propertyDesc is { IsIndexedReadMethod: true }) {
                return new KeyedMethodPropertyGetter(
                    propertyDesc.ReadMethod,
                    _index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            // Try the array as a simple property
            propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            var returnType = propertyDesc.ReturnType;
            if (returnType.IsArray) {
                return GetGetterFromArray(
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    propertyDesc);
            }

            if (returnType.IsGenericList()) {
                return GetGetterFromList(
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    propertyDesc);
            }

            if (returnType.IsGenericEnumerable() || returnType.IsImplementsInterface(typeof(System.Collections.IEnumerable))) {
                return GetGetterFromEnumerable(
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    propertyDesc);
            }

            return null;
        }

        private EventPropertyGetterSPI GetGetterFromEnumerable(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            PropertyStem propertyDesc)
        {
            if (propertyDesc.AccessorProp != null) {
                return new IterablePropertyPropertyGetter(
                    propertyDesc.AccessorProp,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            if (propertyDesc.IsSimpleReadMethod) {
                return new IterableMethodPropertyGetter(
                    propertyDesc.ReadMethod,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            if (propertyDesc.AccessorField != null) {
                return new IterableFieldPropertyGetter(
                    propertyDesc.AccessorField,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            throw new EPRuntimeException("unable to determine property enumerator accessor");
        }

        private EventPropertyGetterSPI GetGetterFromList(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            PropertyStem propertyDesc)
        {
            var prop = propertyDesc.AccessorProp;
            if (prop != null) {
                return new ListPropertyPropertyGetter(prop, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            if (propertyDesc.IsSimpleReadMethod) {
                var method = propertyDesc.ReadMethod;
                return new ListMethodPropertyGetter(
                    method,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            var field = propertyDesc.AccessorField;
            if (field != null) {
                return new ListFieldPropertyGetter(field, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            throw new EPRuntimeException("unable to determine property list accessor");
        }

        private EventPropertyGetterSPI GetGetterFromArray(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            PropertyStem propertyDesc)
        {
            var prop = propertyDesc.AccessorProp;
            if (prop != null) {
                return new ArrayPropertyPropertyGetter(prop, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            if (propertyDesc.IsSimpleReadMethod) {
                var method = propertyDesc.ReadMethod;
                return new ArrayMethodPropertyGetter(
                    method,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            var field = propertyDesc.AccessorField;
            if (field != null) {
                return new ArrayFieldPropertyGetter(field, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            throw new EPRuntimeException("unable to determine property array accessor");
        }
        
        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var descriptor = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (descriptor is { IsIndexedReadMethod: true }) {
                return descriptor.ReturnType;
            }

            // Check if this is an method returning array which is a type of simple property
            descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null) {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (returnType.IsArray) {
                return returnType.GetComponentType();
            }

            if (returnType.IsGenericDictionary()) {
                // no-op since we do not treat dictionaries as indexable...
            } else if (returnType.IsGenericEnumerable() || returnType.IsImplementsInterface(typeof(System.Collections.IEnumerable))) {
                if (descriptor.AccessorProp != null) {
                    return TypeHelper.GetGenericPropertyType(descriptor.AccessorProp, false);
                }

                if (descriptor.ReadMethod != null) {
                    return TypeHelper.GetGenericReturnType(descriptor.ReadMethod, false);
                }

                if (descriptor.AccessorField != null) {
                    return TypeHelper.GetGenericFieldType(descriptor.AccessorField, false);
                }

                return null;
            }

            return null;
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                var innerType = typeBeanOrUnderlyings[0].EventType;
                if (innerType is MapEventType) {
                    return typeof(IDictionary<string, object>);
                }
            }
            else {
                if (type is Type { IsArray: true } typeClass) {
                    return typeClass.GetElementType();
                }
            }
                            
            return null;
        }

        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                var innerType = typeBeanOrUnderlyings[0].EventType;
                if (innerType is MapEventType) {
                    return new MapArrayPropertyGetter(
                        PropertyNameAtomic,
                        _index,
                        eventBeanTypedEventFactory,
                        innerType);
                }
            }
            else if (type is Type asType) {
                if (asType.IsArray) {
                    var componentType = asType.GetElementType();
                    // its an array
                    return new MapArrayPONOEntryIndexedPropertyGetter(
                        PropertyNameAtomic,
                        _index,
                        eventBeanTypedEventFactory,
                        beanEventTypeFactory,
                        componentType);
                }
            }

            return null;
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("[");
            writer.Write(_index);
            writer.Write("]");
        }

        public override EventPropertyGetterSPI GetterDOM => new DOMIndexedGetter(PropertyNameAtomic, _index, null);

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            foreach (var simple in complexProperty.SimpleElements) {
                if (!simple.IsArray) {
                    continue;
                }

                if (!simple.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                return new DOMIndexedGetter(PropertyNameAtomic, _index, null);
            }

            foreach (var complex in complexProperty.ComplexElements) {
                if (!complex.IsArray) {
                    continue;
                }

                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                return new DOMIndexedGetter(
                    PropertyNameAtomic,
                    _index,
                    new FragmentFactoryDOMGetter(eventBeanTypedEventFactory, eventType, propertyExpression));
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            foreach (var simple in complexProperty.SimpleElements) {
                if (!simple.IsArray) {
                    continue;
                }

                if (!simple.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                // return the simple as a non-array since an index is provided
                return new SchemaElementSimple(
                    simple.Name,
                    simple.Namespace,
                    simple.SimpleType,
                    simple.TypeName,
                    false,
                    simple.FractionDigits);
            }

            foreach (var complex in complexProperty.ComplexElements) {
                if (!complex.IsArray) {
                    continue;
                }

                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                // return the complex as a non-array since an index is provided
                return new SchemaElementComplex(
                    complex.Name,
                    complex.Namespace,
                    complex.Attributes,
                    complex.ComplexElements,
                    complex.SimpleElements,
                    false,
                    complex.OptionalSimpleType,
                    complex.OptionalSimpleTypeName);
            }

            return null;
        }

        /// <summary>
        /// Returns the index number for an indexed property expression.
        /// </summary>
        /// <param name="propertyName">property expression</param>
        /// <returns>index</returns>
        public static int GetIndex(string propertyName)
        {
            var start = propertyName.IndexOf('[');
            var end = propertyName.IndexOf(']');
            var indexStr = propertyName.Substring(start, end);
            return int.Parse(indexStr);
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out var propertyIndex)) {
                return null;
            }

            var type = nestableTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                var innerType = typeBeanOrUnderlyings[0].EventType;
                if (!(innerType is ObjectArrayEventType)) {
                    return null;
                }

                return new ObjectArrayArrayPropertyGetter(
                    propertyIndex,
                    _index,
                    eventBeanTypedEventFactory,
                    innerType);
            }
            
            if (type is Type asType && asType.IsArray) {
                var componentType = asType.GetElementType();
                // its an array
                return new ObjectArrayArrayPONOEntryIndexedPropertyGetter(
                    propertyIndex,
                    _index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    componentType);
            }

            return null;
        }
    }
} // end of namespace