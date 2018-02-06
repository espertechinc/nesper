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
    /// Represents a dynamic indexed property of a given name.
    /// <para>
    /// Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </para>
    /// </summary>
    public class DynamicIndexedProperty
        : PropertyBase
        , DynamicProperty
        , PropertyWithIndex
    {
        private readonly int _index;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        /// <param name="index">is the index of the array or indexed property</param>
        public DynamicIndexedProperty(string propertyName, int index)
            : base(propertyName)
        {
            _index = index;
        }

        public override bool IsDynamic => true;

        public override string[] ToPropertyArray()
        {
            return new string[]
            {
                this.PropertyNameAtomic
            };
        }

        public override EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return new DynamicIndexedPropertyGetter(PropertyNameAtomic, _index, eventAdapterService);
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
            return new MapIndexedPropertyGetter(this.PropertyNameAtomic, _index);
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, Object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            int propertyIndex;
            if (indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
            {
                return new ObjectArrayIndexedPropertyGetter(propertyIndex, _index);
            }

            return null;
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write('[');
            writer.Write(_index);
            writer.Write(']');
            writer.Write('?');
        }

        public override EventPropertyGetterSPI GetGetterDOM(
            SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService,
            BaseXMLEventType eventType,
            string propertyExpression)
        {
            return new DOMIndexedGetter(PropertyNameAtomic, _index, null);
        }

        public override SchemaItem GetPropertyTypeSchema(
            SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService)
        {
            return null; // dynamic properties always return Node
        }

        public override EventPropertyGetterSPI GetGetterDOM()
        {
            return new DOMIndexedGetter(PropertyNameAtomic, _index, null);
        }

        public int Index => _index;
    }
} // end of namespace
