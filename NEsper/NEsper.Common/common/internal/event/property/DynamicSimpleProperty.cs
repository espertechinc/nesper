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
    ///     Represents a dynamic simple property of a given name.
    ///     <para />
    ///     Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </summary>
    public class DynamicSimpleProperty : PropertyBase,
        DynamicProperty,
        PropertySimple
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        public DynamicSimpleProperty(string propertyName)
            : base(propertyName)
        {
        }

        public override bool IsDynamic => true;

        public override EventPropertyGetterSPI GetterDOM => new DOMAttributeAndElementGetter(PropertyNameAtomic);

        public override EventPropertyGetterSPI GetGetter(
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new DynamicSimplePropertyGetter(
                PropertyNameAtomic, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        public override string[] ToPropertyArray()
        {
            return new[] {PropertyNameAtomic};
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
            return new MapDynamicPropertyGetter(PropertyNameAtomic);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
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
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (nestableTypes == null) {
                return new ObjectArrayDynamicPropertyGetter(PropertyNameAtomic);
            }

            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out var propertyIndex)) {
                return new ObjectArrayDynamicPropertyGetter(PropertyNameAtomic);
            }

            return new ObjectArrayPropertyGetterDefaultObjectArray(propertyIndex, null, eventBeanTypedEventFactory);
        }
    }
} // end of namespace