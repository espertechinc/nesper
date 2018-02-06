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
    /// Represents a dynamic simple property of a given name.
    /// <para>
    /// Dynamic properties always exist, have an Object type and are resolved to a method during runtime.
    /// </para>
    /// </summary>
    public class DynamicSimpleProperty : PropertyBase, DynamicProperty, PropertySimple
    {
        /// <summary>L
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        public DynamicSimpleProperty(string propertyName)
            : base(propertyName)
        {
        }

        public override EventPropertyGetterSPI GetGetter(BeanEventType eventType,
            EventAdapterService eventAdapterService)
        {
            return new DynamicSimplePropertyGetter(PropertyNameAtomic, eventAdapterService);
        }

        public override bool IsDynamic
        {
            get { return true; }
        }

        public override string[] ToPropertyArray()
        {
            return new string[] {this.PropertyNameAtomic};
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            return typeof(Object);
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType beanEventType,
            EventAdapterService eventAdapterService)
        {
            return GenericPropertyDesc.ObjectGeneric;
        }

        public override Type GetPropertyTypeMap(IDictionary<string, object> optionalMapPropTypes,
            EventAdapterService eventAdapterService)
        {
            return typeof(Object);
        }

        public override MapEventPropertyGetter GetGetterMap(IDictionary<string, object> optionalMapPropTypes,
            EventAdapterService eventAdapterService)
        {
            return new MapDynamicPropertyGetter(PropertyNameAtomic);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService, BaseXMLEventType eventType, string propertyExpression)
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM()
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty,
            EventAdapterService eventAdapterService)
        {
            return null; // always returns Node
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(IDictionary<string, int> indexPerProperty,
            IDictionary<string, Object> nestableTypes, EventAdapterService eventAdapterService)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (nestableTypes == null)
            {
                return new ObjectArrayDynamicPropertyGetter(PropertyNameAtomic);
            }

            int propertyIndex;
            if (indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
            {
                return new ObjectArrayPropertyGetterDefaultObjectArray(propertyIndex, null, eventAdapterService);
            }

            return new ObjectArrayDynamicPropertyGetter(PropertyNameAtomic);
        }
    }
} // end of namespace
