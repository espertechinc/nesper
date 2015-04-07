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
    /// Represents a dynamic simple property of a given name.
    /// <para>
    /// Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </para>
    /// </summary>
    public class DynamicSimpleProperty 
        : PropertyBase
        , DynamicProperty
    {
        /// <summary>Ctor.</summary>
        /// <param name="propertyName">is the property name</param>
        public DynamicSimpleProperty(String propertyName)
            : base(propertyName)
        {
        }

        public override EventPropertyGetter GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return new DynamicSimplePropertyGetter(PropertyNameAtomic, eventAdapterService);
        }

        public override bool IsDynamic
        {
            get { return true; }
        }

        public override String[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
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
            return new MapDynamicPropertyGetter(PropertyNameAtomic);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
        }

        public override EventPropertyGetter GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType eventType, String propertyExpression)
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
        }

        public override EventPropertyGetter GetGetterDOM()
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService)
        {
            return null;    // always returns Node
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty, 
            IDictionary<string, object> nestableTypes, 
            EventAdapterService eventAdapterService)
        {
            return new ObjectArrayDynamicPropertyGetter(PropertyNameAtomic);
        }
    }
} // End of namespace
