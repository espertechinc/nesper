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

using com.espertech.esper.client;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.events.property
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Represents a dynamic mapped property of a given name.
    /// <para>
    /// Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </para>
    /// </summary>
    public class DynamicMappedProperty 
        : PropertyBase
        , DynamicProperty
    {
        private readonly String _key;

        /// <summary>Ctor.</summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="key">is the mapped access key</param>
        public DynamicMappedProperty(String propertyName, String key)
            : base(propertyName)
        {
            _key = key;
        }

        public override bool IsDynamic
        {
            get { return true; }
        }

        public override String[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }

        public override EventPropertyGetter GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return new DynamicMappedPropertyGetter(PropertyNameAtomic, _key, eventAdapterService);
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return typeof(object);
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType beanEventType, EventAdapterService eventAdapterService)
        {
            return GenericPropertyDesc.ObjectGeneric;
        }

        public override Type GetPropertyTypeMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            return typeof(object);
        }

        public override MapEventPropertyGetter GetGetterMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
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

        public override EventPropertyGetter GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType eventType, String propertyExpression)
        {
            return new DOMMapGetter(PropertyNameAtomic, _key, null);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService)
        {
            return null;  // always returns Node
        }

        public override EventPropertyGetter GetGetterDOM()
        {
            return new DOMMapGetter(PropertyNameAtomic, _key, null);
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<String, int> indexPerProperty, 
            IDictionary<String, Object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            int propertyIndex;
            if (indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
                return new ObjectArrayMappedPropertyGetter(propertyIndex, _key);
            return null;
        }

        public string Key
        {
            get { return _key; }
        }
    }
} // End of namespace
