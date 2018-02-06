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

using com.espertech.esper.compat.collections;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events.property
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Represents a mapped property or array property, ie. an 'value' property with read method 
    /// GetValue(int index) or a 'array' property via read method GetArray() returning an array.
    /// </summary>
    public class MappedProperty : PropertyBase, PropertyWithKey
    {
        private readonly String _key;

        public MappedProperty(String propertyName)
            : base(propertyName)
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="propertyName">is the property name of the mapped property</param>
        /// <param name="key">is the key value to access the mapped property</param>
        public MappedProperty(String propertyName, String key)
            : base(propertyName)
        {
            _key = key;
        }

        /// <summary>Returns the key value for mapped access. </summary>
        /// <value>key value</value>
        public string Key => _key;

        public override String[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }

        public override bool IsDynamic => false;

        public override EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            InternalEventPropDescriptor propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null)
            {
                var method = propertyDesc.ReadMethod;
                var fastClass = eventType.FastClass;
                if (fastClass != null)
                {
                    var fastMethod = fastClass.GetMethod(method);
                    return new KeyedFastPropertyGetter(fastMethod, _key, eventAdapterService);
                }
                else
                {
                    return new KeyedMethodPropertyGetter(method, _key, eventAdapterService);
                }
            }

            // Try the array as a simple property
            propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null)
            {
                return null;
            }

            var returnType = propertyDesc.ReturnType;
            // Unlike Java, CLR based maps are strongly typed ... the mechanics
            // for identifying and extracting the map contents are also different.
            if (!returnType.IsGenericStringDictionary())
            {
                return null;
            }

            if (propertyDesc.ReadMethod != null)
            {
                var fastClass = eventType.FastClass;
                var method = propertyDesc.ReadMethod;
                if (fastClass != null)
                {
                    var fastMethod = fastClass.GetMethod(method);
                    return new KeyedMapFastPropertyGetter(method, fastMethod, _key, eventAdapterService);
                }
                else
                {
                    return new KeyedMapMethodPropertyGetter(method, _key, eventAdapterService);
                }
            }
            else
            {
                var field = propertyDesc.AccessorField;
                return new KeyedMapFieldPropertyGetter(field, _key, eventAdapterService);
            }
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null)
            {
                return propertyDesc.ReadMethod.ReturnType;
            }

            // Check if this is an method returning array which is a type of simple property
            var descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null)
            {
                return null;
            }

            Type returnType = descriptor.ReturnType;
            if (!returnType.IsGenericStringDictionary())
            {
                return null;
            }

            if (descriptor.ReadMethod != null)
            {
                return TypeHelper.GetGenericReturnTypeMap(descriptor.ReadMethod, false);
            }

            if (descriptor.AccessorField != null)
            {
                return TypeHelper.GetGenericFieldTypeMap(descriptor.AccessorField, false);
            }

            return null;
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var propertyDesc = eventType.GetMappedProperty(PropertyNameAtomic);
            if (propertyDesc != null)
            {
                return new GenericPropertyDesc(propertyDesc.ReadMethod.ReturnType);
            }

            // Check if this is an method returning array which is a type of simple property
            var descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null)
            {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (!returnType.IsGenericStringDictionary())
            {
                return null;
            }
            if (descriptor.ReadMethod != null)
            {
                var genericType = TypeHelper.GetGenericReturnTypeMap(descriptor.ReadMethod, false);
                return new GenericPropertyDesc(genericType);
            }
            else if (descriptor.AccessorField != null)
            {
                var genericType = TypeHelper.GetGenericFieldTypeMap(descriptor.AccessorField, false);
                return new GenericPropertyDesc(genericType);
            }
            else
            {
                return null;
            }
        }

        public override Type GetPropertyTypeMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null)
            {
                return null;
            }
            if ((type is Type trueType) && (trueType.IsGenericStringDictionary()))
            {
                return typeof(Object);
            }
            return null;  // Mapped properties are not allowed in non-dynamic form in a map
        }

        public override MapEventPropertyGetter GetGetterMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null)
            {
                return null;
            }
            if (type is Type trueType)
            {
                if (trueType.IsGenericStringDictionary())
                {
                    return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
                }
            }
            if (type.GetType().IsGenericStringDictionary())
            {
                return new MapMappedPropertyGetter(PropertyNameAtomic, Key);
            }
            return null;
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("('");
            writer.Write(_key);
            writer.Write("')");
        }

        public override EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService, BaseXMLEventType eventType, String propertyExpression)
        {
            foreach (SchemaElementComplex complex in complexProperty.ComplexElements.Where(c => c.Name == PropertyNameAtomic))
            {
                foreach (SchemaItemAttribute attribute in complex.Attributes)
                {
                    if (!String.Equals(attribute.Name, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                return new DOMMapGetter(PropertyNameAtomic, _key, null);
            }

            return null;
        }

        public override EventPropertyGetterSPI GetGetterDOM()
        {
            return new DOMMapGetter(PropertyNameAtomic, _key, null);
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty, EventAdapterService eventAdapterService)
        {
            foreach (SchemaElementComplex complex in complexProperty.ComplexElements.Where(c => c.Name == PropertyNameAtomic))
            {
                foreach (SchemaItemAttribute attribute in complex.Attributes)
                {
                    if (!String.Equals(attribute.Name, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                return complex;
            }

            return null;
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            int index;

            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out index))
            {
                return null;
            }

            var type = nestableTypes.Get(PropertyNameAtomic);
            var typeAsType = type as Type;
            if (typeAsType != null)
            {
                if (typeAsType.IsGenericStringDictionary())
                {
                    return new ObjectArrayMappedPropertyGetter(index, Key);
                }
            }
            if (type is DataMap)
            {
                return new ObjectArrayMappedPropertyGetter(index, Key);
            }
            return null;
        }
    }
}
