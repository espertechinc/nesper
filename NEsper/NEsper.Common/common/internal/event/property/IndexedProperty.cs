///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
    ///     Represents an indexed property or array property, ie. an 'value' property with read method getValue(int index)
    ///     or a 'array' property via read method getArray() returning an array.
    /// </summary>
    public class IndexedProperty : PropertyBase,
        PropertyWithIndex
    {
        public IndexedProperty(string propertyName) : base(propertyName)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="index">is the index to use to access the property value</param>
        public IndexedProperty(string propertyName, int index) : base(propertyName)
        {
            Index = index;
        }

        public override bool IsDynamic => false;

        public override EventPropertyGetterSPI GetterDOM => new DOMIndexedGetter(PropertyNameAtomic, Index, null);

        /// <summary>
        ///     Returns index for indexed access.
        /// </summary>
        /// <returns>index value</returns>
        public int Index { get; }

        public override string[] ToPropertyArray()
        {
            return new[] {PropertyNameAtomic};
        }

        // EventPropertyGetterAndIndexed
        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyDesc = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (propertyDesc != null) {
                return new KeyedMethodPropertyGetter(
                    propertyDesc.ReadMethod, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            // Try the array as a simple property
            propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null) {
                return null;
            }

            var returnType = propertyDesc.ReturnType;
            if (returnType.IsArray) {
                if (propertyDesc.ReadMethod != null) {
                    var method = propertyDesc.ReadMethod;
                    return new ArrayMethodPropertyGetter(
                        method, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
                }

                var field = propertyDesc.AccessorField;
                return new ArrayFieldPropertyGetter(field, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            if (returnType.IsGenericList()) {
                if (propertyDesc.ReadMethod != null) {
                    var method = propertyDesc.ReadMethod;
                    return new ListMethodPropertyGetter(
                        method, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
                }

                var field = propertyDesc.AccessorField;
                return new ListFieldPropertyGetter(field, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            if (returnType.IsGenericEnumerable() || TypeHelper.IsImplementsInterface(returnType, typeof(IEnumerable))) {
                if (propertyDesc.ReadMethod != null) {
                    var method = propertyDesc.ReadMethod;
                    return new IterableMethodPropertyGetter(
                        method, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
                }

                var field = propertyDesc.AccessorField;
                return new IterableFieldPropertyGetter(field, Index, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            return null;
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType eventType, BeanEventTypeFactory beanEventTypeFactory)
        {
            var descriptor = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (descriptor != null) {
                return new GenericPropertyDesc(descriptor.ReturnType);
            }

            // Check if this is an method returning array which is a type of simple property
            descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null) {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (returnType.IsArray) {
                return new GenericPropertyDesc(returnType.GetElementType());
            }

            if (returnType.IsGenericEnumerable() || TypeHelper.IsImplementsInterface(returnType, typeof(IEnumerable))) {
                if (descriptor.ReadMethod != null) {
                    var genericType = TypeHelper.GetGenericReturnType(descriptor.ReadMethod, false);
                    return new GenericPropertyDesc(genericType);
                }

                if (descriptor.AccessorField != null) {
                    var genericType = TypeHelper.GetGenericFieldType(descriptor.AccessorField, false);
                    return new GenericPropertyDesc(genericType);
                }

                return null;
            }

            return null;
        }

        public override Type GetPropertyType(BeanEventType eventType, BeanEventTypeFactory beanEventTypeFactory)
        {
            var descriptor = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (descriptor != null) {
                return descriptor.ReturnType;
            }

            // Check if this is an method returning array which is a type of simple property
            descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null) {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (returnType.IsArray) {
                return returnType.GetElementType();
            }

            if (returnType.IsGenericEnumerable() || TypeHelper.IsImplementsInterface(returnType, typeof(IEnumerable))) {
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

            if (type is TypeBeanOrUnderlying[]) {
                var innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
                if (!(innerType is MapEventType)) {
                    return null;
                }

                return typeof(IDictionary<string, object>[]);
            }

            if (!(type is Type)) {
                return null;
            }

            if (!((Type) type).IsArray) {
                return null;
            }

            return ((Type) type).GetElementType();
        }

        // MapEventPropertyGetterAndIndexed
        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null) {
                return null;
            }

            if (type is TypeBeanOrUnderlying[]) {
                var innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
                if (!(innerType is MapEventType)) {
                    return null;
                }

                return new MapArrayPropertyGetter(PropertyNameAtomic, Index, eventBeanTypedEventFactory, innerType);
            }

            if (!(type is Type)) {
                return null;
            }

            if (!((Type) type).IsArray) {
                return null;
            }

            var componentType = ((Type) type).GetElementType();
            // its an array
            return new MapArrayPONOEntryIndexedPropertyGetter(
                PropertyNameAtomic, Index, eventBeanTypedEventFactory, beanEventTypeFactory, componentType);
        }

        public override void ToPropertyEPL(StringWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("[");
            writer.Write(Convert.ToString(Index));
            writer.Write("]");
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType, string propertyExpression)
        {
            foreach (var simple in complexProperty.SimpleElements) {
                if (!simple.IsArray) {
                    continue;
                }

                if (!simple.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                return new DOMIndexedGetter(PropertyNameAtomic, Index, null);
            }

            foreach (SchemaElementComplex complex in complexProperty.Children) {
                if (!complex.IsArray) {
                    continue;
                }

                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                return new DOMIndexedGetter(
                    PropertyNameAtomic, Index,
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
                    simple.Name, simple.Namespace, simple.XsSimpleType, simple.TypeName, false, simple.FractionDigits);
            }

            foreach (SchemaElementComplex complex in complexProperty.Children) {
                if (!complex.IsArray) {
                    continue;
                }

                if (!complex.Name.Equals(PropertyNameAtomic)) {
                    continue;
                }

                // return the complex as a non-array since an index is provided
                return new SchemaElementComplex(
                    complex.Name, complex.Namespace, complex.Attributes, complex.Children, complex.SimpleElements,
                    false, complex.OptionalSimpleType, complex.OptionalSimpleTypeName);
            }

            return null;
        }

        /// <summary>
        ///     Returns the index number for an indexed property expression.
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

        // ObjectArrayEventPropertyGetterAndIndexed 
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

            if (type is TypeBeanOrUnderlying[]) {
                var innerType = ((TypeBeanOrUnderlying[]) type)[0].EventType;
                if (!(innerType is ObjectArrayEventType)) {
                    return null;
                }

                return new ObjectArrayArrayPropertyGetter(propertyIndex, Index, eventBeanTypedEventFactory, innerType);
            }

            if (!(type is Type)) {
                return null;
            }

            if (!((Type) type).IsArray) {
                return null;
            }

            var componentType = ((Type) type).GetElementType();
            // its an array
            return new ObjectArrayArrayPONOEntryIndexedPropertyGetter(
                propertyIndex, Index, eventBeanTypedEventFactory, beanEventTypeFactory, componentType);
        }
    }
} // end of namespace