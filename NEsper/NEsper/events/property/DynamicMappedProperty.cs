///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Represents a dynamic mapped property of a given name.
    /// <para>
    /// Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </para>
    /// </summary>
    public class DynamicMappedProperty
        : PropertyBase,
            DynamicProperty,
            PropertyWithKey
    {
        private readonly string _key;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="key">is the mapped access key</param>
        public DynamicMappedProperty(string propertyName, string key)
            : base(propertyName)
        {
            _key = key;
        }

        public override bool IsDynamic => true;

        public override string[] ToPropertyArray()
        {
            return new string[]
            {
                PropertyNameAtomic
            };
        }

        public override EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return new DynamicMappedPropertyGetter(PropertyNameAtomic, _key, eventAdapterService);
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return typeof (Object);
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(
            BeanEventType beanEventType,
            EventAdapterService eventAdapterService)
        {
            return GenericPropertyDesc.ObjectGeneric;
        }

        public override Type GetPropertyTypeMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventAdapterService eventAdapterService)
        {
            return typeof (Object);
        }

        public override MapEventPropertyGetter GetGetterMap(
            IDictionary<string, object> optionalMapPropTypes,
            EventAdapterService eventAdapterService)
        {
            return new MapMappedPropertyGetter(PropertyNameAtomic, _key);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("('");
            writer.Write(_key);
            writer.Write("')");
            writer.Write('?');
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            return new DOMMapGetter(PropertyNameAtomic, _key, null);
        }

        public override SchemaItem GetPropertyTypeSchema(
            SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService)
        {
            return null; // always returns Node
        }

        public override EventPropertyGetterSPI GetGetterDOM()
        {
            return new DOMMapGetter(PropertyNameAtomic, _key, null);
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, Object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            int propertyIndex;

            if (indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
            {
                return new ObjectArrayMappedPropertyGetter(propertyIndex, _key);
            }

            return null;
        }

        public string Key => _key;
    }
} // end of namespace
