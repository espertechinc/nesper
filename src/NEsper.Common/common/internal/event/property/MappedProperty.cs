///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
    /// Represents a mapped property or array property, ie. an 'value' property with read method getValue(int index)
    /// or a 'array' property via read method getArray() returning an array.
    /// </summary>
    public class MappedProperty : PropertyBase,
        PropertyWithKey
    {
        private string _key;

        public MappedProperty(string propertyName) : base(propertyName)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "propertyName">is the property name of the mapped property</param>
        /// <param name = "key">is the key value to access the mapped property</param>
        public MappedProperty(
            string propertyName,
            string key)
            : base(propertyName)
        {
            _key = key;
        }

        public override bool IsDynamic => false;
        
        public override string[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }
        
        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            if (propertyDesc.IsMappedReadMethod) {
                return new KeyedMethodPropertyGetter(
                    propertyDesc.ReadMethod,
                    Key,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            // Try the as a simple property
            if (!propertyDesc.PropertyType.IsSimple()) {
                return null;
            }

            var returnType = propertyDesc.ReturnType;
            if (returnType.IsGenericStringDictionary()) {
                return GetGetterFromDictionary(
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory,
                    propertyDesc);
            }

            return null;
        }
                
        private EventPropertyGetterSPI GetGetterFromDictionary(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            PropertyStem propertyDesc)
        {
            if (propertyDesc.AccessorProp != null) {
                return new KeyedMapPropertyPropertyGetter(
                    propertyDesc.AccessorProp,
                    Key,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            if (propertyDesc.IsSimpleReadMethod) {
                return new KeyedMapMethodPropertyGetter(
                    propertyDesc.ReadMethod,
                    Key,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }


            if (propertyDesc.AccessorField != null) {
                var field = propertyDesc.AccessorField;
                return new KeyedMapFieldPropertyGetter(
                    field,
                    Key,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            throw new IllegalStateException($"unable to determine property getter for requested member");
        }
        
        public static void AssertGenericDictionary(Type t)
        {
            if (!t.IsMapped()) {
                throw new IllegalStateException("type is not a mapped value");
            }
        }

        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);

            if (propertyDesc == null) {
                return null;
            }

            if (propertyDesc.IsMappedReadMethod) {
                return propertyDesc.ReadMethod.ReturnType;
            }

            if (!propertyDesc.PropertyType.IsSimple()) {
                return null;
            }

            if (propertyDesc.AccessorProp != null) {
                var accessorPropType = propertyDesc.AccessorProp.PropertyType;
                AssertGenericDictionary(accessorPropType);
                return accessorPropType.GetDictionaryValueType();
            }

            if (propertyDesc.IsSimpleReadMethod) {
                var accessorPropType = propertyDesc.ReadMethod.ReturnType;
                AssertGenericDictionary(accessorPropType);
                return accessorPropType.GetDictionaryValueType();
            }

            if (propertyDesc.AccessorField != null) {
                var accessorFieldType = propertyDesc.AccessorField.FieldType;
                AssertGenericDictionary(accessorFieldType);
                return accessorFieldType.GetDictionaryValueType();
            }

            throw new IllegalStateException($"invalid property descriptor: {propertyDesc}");
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (optionalMapPropTypes == null) {
                return null;
            }

            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is Type asType && asType.IsGenericStringDictionary()) {
                return typeof(object);
            }

            return null; // Mapped properties are not allowed in non-dynamic form in a map
        }

        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (optionalMapPropTypes == null) {
                return null;
            }

            if (!optionalMapPropTypes.TryGetValue(PropertyNameAtomic, out var type)) {
                return null;
            }

            if (type is Type asType && asType.IsGenericStringDictionary()) {
                    return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
            }

            if (type is IDictionary<string, object>) {
                return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
            }

            return null;
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("('");
            writer.Write(_key);
            writer.Write("')");
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            foreach (var complex in complexProperty.ComplexElements) {
                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                foreach (var attribute in complex.Attributes) {
                    if (!attribute.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }
                }

                return new DOMMapGetter(PropertyNameAtomic, _key, null);
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            foreach (var complex in complexProperty.ComplexElements) {
                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                foreach (var attribute in complex.Attributes) {
                    if (!attribute.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }
                }

                return complex;
            }

            return null;
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out var index)) {
                return null;
            }

            var type = nestableTypes.Get(PropertyNameAtomic);
            if (type is Type asType && asType.IsGenericStringDictionary()) {
                return new ObjectArrayMappedPropertyGetter(index, Key);
            }

            if (type is IDictionary<string, object>) {
                return new ObjectArrayMappedPropertyGetter(index, Key);
            }

            return null;
        }

        public string Key => _key;

        public override EventPropertyGetterSPI GetterDOM => new DOMMapGetter(PropertyNameAtomic, _key, null);
    }
} // end of namespace