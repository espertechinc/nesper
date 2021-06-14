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

namespace com.espertech.esper.common.@internal.@event.property
{
    /// <summary>
    ///     Represents a dynamic mapped property of a given name.
    ///     <para />
    ///     Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </summary>
    public class DynamicMappedProperty : PropertyBase,
        DynamicProperty,
        PropertyWithKey
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="key">is the mapped access key</param>
        public DynamicMappedProperty(
            string propertyName,
            string key)
            : base(propertyName)
        {
            Key = key;
        }

        public override bool IsDynamic => true;

        public override EventPropertyGetterSPI GetterDOM => new DOMMapGetter(PropertyNameAtomic, Key, null);

        public string Key { get; }

        public override string[] ToPropertyArray()
        {
            return new[] {PropertyNameAtomic};
        }

        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (!eventType.Stem.IsPublicFields) {
                // Determine if there is an "indexed" method matching the form GetXXX(int index)
                var underlyingType = eventType.UnderlyingType;
                
                
                
                var propertyInfo = eventType.UnderlyingType.GetProperty(PropertyNameAtomic);
                if (propertyInfo != null && propertyInfo.CanRead) {
                }

                return new DynamicMappedPropertyGetterByMethodOrProperty(
                    PropertyNameAtomic,
                    Key,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            return new DynamicMappedPropertyGetterByField(
                PropertyNameAtomic,
                Key,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return typeof(object);
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType beanEventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return GenericPropertyDesc.ObjectGeneric;
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return typeof(object);
        }

        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("('");
            writer.Write(Key);
            writer.Write("')");
            writer.Write('?');
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            return new DOMMapGetter(PropertyNameAtomic, Key, null);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            return null; // always returns Node
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

            return new ObjectArrayMappedPropertyGetter(propertyIndex, Key);
        }
    }
} // end of namespace