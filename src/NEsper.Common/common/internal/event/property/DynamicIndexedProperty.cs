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
    ///     Represents a dynamic indexed property of a given name.
    ///     <para />
    ///     Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </summary>
    public class DynamicIndexedProperty : PropertyBase,
        DynamicProperty,
        PropertyWithIndex
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="index">is the index of the array or indexed property</param>
        public DynamicIndexedProperty(
            string propertyName,
            int index)
            : base(propertyName)
        {
            Index = index;
        }

        public override bool IsDynamic => true;

        public override EventPropertyGetterSPI GetterDOM => new DOMIndexedGetter(PropertyNameAtomic, Index, null);

        public int Index { get; }

        public override string[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }

        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (!eventType.Stem.IsPublicFields) {
                var propertyInfo = eventType.UnderlyingType.GetProperty(PropertyNameAtomic);
                if (propertyInfo != null && propertyInfo.CanRead) {
                }

                return new DynamicIndexedPropertyGetterByMethodOrProperty(
                    PropertyNameAtomic,
                    Index,
                    eventBeanTypedEventFactory,
                    beanEventTypeFactory);
            }

            return new DynamicIndexedPropertyGetterByField(
                PropertyNameAtomic,
                Index,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public override Type GetPropertyType(
            BeanEventType eventType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return typeof(object);
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
            return new MapIndexedPropertyGetter(PropertyNameAtomic, Index);
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


            return new ObjectArrayIndexedPropertyGetter(propertyIndex, Index);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write('[');
            writer.Write(Convert.ToString(Index));
            writer.Write(']');
            writer.Write('?');
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            return new DOMIndexedGetter(PropertyNameAtomic, Index, null);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty)
        {
            return null; // dynamic properties always return Node
        }
    }
} // end of namespace