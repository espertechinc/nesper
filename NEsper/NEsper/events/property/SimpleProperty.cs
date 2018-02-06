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
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Represents a simple property of a given name.
    /// </summary>
    public class SimpleProperty 
        : PropertyBase
        , PropertySimple
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">is the property name</param>
        public SimpleProperty(String propertyName)
            : base(propertyName)
        {
        }

        public override String[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }

        public override EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null)
            {
                return null;
            }
            if (!propertyDesc.PropertyType.Equals(EventPropertyType.SIMPLE))
            {
                return null;
            }
            return eventType.GetGetterSPI(PropertyNameAtomic);
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null)
            {
                return null;
            }
            return propertyDesc.ReturnType.GetBoxedType();
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null)
            {
                return null;
            }
            return propertyDesc.GetReturnTypeGeneric();
        }

        public override Type GetPropertyTypeMap(IDictionary<string, object> optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (optionalMapPropTypes == null)
            {
                return null;
            }
            var def = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (def == null)
            {
                return null;
            }
            if (def is Type)
            {
                return ((Type)def).GetBoxedType();
            }
            else if (def is IDictionary<string, object>)
            {
                return typeof(IDictionary<string, object>);
            }
            else if (def is String)
            {
                String propertyName = def.ToString();
                bool isArray = EventTypeUtility.IsPropertyArray(propertyName);
                if (isArray)
                {
                    propertyName = EventTypeUtility.GetPropertyRemoveArray(propertyName);
                }

                var eventType = eventAdapterService.GetEventTypeByName(propertyName);
                if (eventType is MapEventType)
                {
                    if (isArray)
                    {
                        return typeof(IDictionary<string, object>[]);
                    }
                    else
                    {
                        return typeof(IDictionary<string, object>);
                    }
                }

                if (eventType is ObjectArrayEventType)
                {
                    if (isArray)
                    {
                        return typeof(Object[][]);
                    }
                    else
                    {
                        return typeof(Object[]);
                    }
                }
            }
            String message = "Nestable map type configuration encountered an unexpected value type of '"
                + def.GetType() + " for property '" + PropertyNameAtomic + "', expected Map or Class";
            throw new PropertyAccessException(message);
        }

        public override MapEventPropertyGetter GetGetterMap(IDictionary<string, object> optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (optionalMapPropTypes == null)
            {
                return null;
            }
            var def = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (def == null)
            {
                return null;
            }

            return new MapPropertyGetterDefaultNoFragment(PropertyNameAtomic, eventAdapterService);
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM()
        {
            return new DOMAttributeAndElementGetter(PropertyNameAtomic);
        }

        public override EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType xmlEventType, String propertyExpression)
        {
            if (complexProperty.Attributes.Any(attribute => attribute.Name == PropertyNameAtomic))
            {
                return new DOMSimpleAttributeGetter(PropertyNameAtomic);
            }

            foreach (var simple in complexProperty.SimpleElements.Where(simple => simple.Name == PropertyNameAtomic))
            {
                return new DOMComplexElementGetter(PropertyNameAtomic, null, simple.IsArray);
            }

            foreach (SchemaElementComplex complex in complexProperty.ComplexElements)
            {
                var complexFragmentFactory = new FragmentFactoryDOMGetter(eventAdapterService, xmlEventType, propertyExpression);
                if (complex.Name == PropertyNameAtomic)
                {
                    return new DOMComplexElementGetter(PropertyNameAtomic, complexFragmentFactory, complex.IsArray);
                }
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService)
        {
            return SchemaUtil.FindPropertyMapping(complexProperty, PropertyNameAtomic);
        }

        public override bool IsDynamic
        {
            get { return false; }
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            // The simple, none-dynamic property needs a definition of the map contents else no property
            if (nestableTypes == null)
            {
                return null;
            }

            int propertyIndex;
            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
            {
                return null;
            }

            return new ObjectArrayPropertyGetterDefaultObjectArray(propertyIndex, null, eventAdapterService);
        }
    }
}
