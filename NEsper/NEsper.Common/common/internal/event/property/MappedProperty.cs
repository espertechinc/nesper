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
    ///     Represents a mapped property or array property, ie. an 'value' property with read method getValue(int index)
    ///     or a 'array' property via read method getArray() returning an array.
    /// </summary>
    public class MappedProperty : PropertyBase,
        PropertyWithKey
    {
        public MappedProperty(string propertyName) : base(propertyName)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name of the mapped property</param>
        /// <param name="key">is the key value to access the mapped property</param>
        public MappedProperty(string propertyName, string key) : base(propertyName)
        {
            Key = key;
        }

        public override bool IsDynamic => false;

        public override EventPropertyGetterSPI GetterDOM => new DOMMapGetter(PropertyNameAtomic, Key, null);

        /// <summary>
        ///     Returns the key value for mapped access.
        /// </summary>
        /// <returns>key value</returns>
        public string Key { get; }

        public override string[] ToPropertyArray()
        {
            return new string[] {this.PropertyNameAtomic};
        }

        // EventPropertyGetterAndMapped
        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null) {
                var method = propertyDesc.ReadMethod;
                return new KeyedMethodPropertyGetter(method, Key, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            // Try the array as a simple property
            propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            var returnType = propertyDesc.ReturnType;
            if (!returnType.IsGenericStringDictionary()) {
                return null;
            }

            if (propertyDesc.ReadMethod != null) {
                var method = propertyDesc.ReadMethod;
                return new KeyedMapMethodPropertyGetter(method, Key, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            var field = propertyDesc.AccessorField;
            return new KeyedMapFieldPropertyGetter(field, Key, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null) {
                return propertyDesc.ReadMethod.ReturnType;
            }

            // Check if this is an method returning array which is a type of simple property
            var descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null) {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (!TypeHelper.IsImplementsInterface(returnType, typeof(IDictionary<string, object>))) {
                return null;
            }

            if (descriptor.ReadMethod != null) {
                return TypeHelper.GetGenericReturnTypeMap(descriptor.ReadMethod, false);
            }

            if (descriptor.AccessorField != null) {
                return TypeHelper.GetGenericFieldTypeMap(descriptor.AccessorField, false);
            }

            return null;
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null) {
                return new GenericPropertyDesc(propertyDesc.ReadMethod.ReturnType);
            }

            // Check if this is an method returning array which is a type of simple property
            var descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null) {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (!TypeHelper.IsImplementsInterface(returnType, typeof(IDictionary<string, object>))) {
                return null;
            }

            if (descriptor.ReadMethod != null) {
                var genericType = TypeHelper.GetGenericReturnTypeMap(descriptor.ReadMethod, false);
                return new GenericPropertyDesc(genericType);
            }

            if (descriptor.AccessorField != null) {
                var genericType = TypeHelper.GetGenericFieldTypeMap(descriptor.AccessorField, false);
                return new GenericPropertyDesc(genericType);
            }

            return null;
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes, 
            BeanEventTypeFactory beanEventTypeFactory)
        {
            object type = DictionaryExtensions.Get(optionalMapPropTypes, this.PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is Type asType) {
                if (asType.IsGenericStringDictionary()) {
                    return typeof(object);
                }
            }

            return null; // Mapped properties are not allowed in non-dynamic form in a map
        }

        // MapEventPropertyGetterAndMapped 
        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is Type asType) {
                if (asType.IsGenericStringDictionary()) { 
                    return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
                }
            }

            if (type is IDictionary<string, object>) {
                return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
            }

            return null;
        }

        public override void ToPropertyEPL(StringWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("('");
            writer.Write(Key);
            writer.Write("')");
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            foreach (SchemaElementComplex complex in complexProperty.Children) {
                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                foreach (var attribute in complex.Attributes) {
                    if (!attribute.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)) {
                    }
                }

                return new DOMMapGetter(PropertyNameAtomic, Key, null);
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            foreach (SchemaElementComplex complex in complexProperty.Children) {
                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                foreach (var attribute in complex.Attributes) {
                    if (!string.Equals(attribute.Name, "id", StringComparison.InvariantCultureIgnoreCase)) {
                    }
                }

                return complex;
            }

            return null;
        }

        // ObjectArrayEventPropertyGetterAndMapped
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
            if (type is Type asType) {
                if (asType.IsGenericStringDictionary()) {
                    return new ObjectArrayMappedPropertyGetter(index, Key);
                }
            }

            if (type is IDictionary<string, object>) {
                return new ObjectArrayMappedPropertyGetter(index, Key);
            }

            return null;
        }
    }
} // end of namespace