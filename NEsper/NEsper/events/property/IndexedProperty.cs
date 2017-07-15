///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;

using XLR8.CGLib;

using com.espertech.esper.client;
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
    /// Represents an indexed property or array property, ie. an 'value' property with read method getValue(int index)
    /// or a 'array' property via read method Array returning an array.
    /// </summary>
    public class IndexedProperty : PropertyBase, PropertyWithIndex
    {
        private readonly int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedProperty"/> class.
        /// </summary>
        /// <param name="propertyName">is the name of the property</param>
        public IndexedProperty(String propertyName)
            : base(propertyName)
        {
        }

        /// <summary> Ctor.</summary>
        /// <param name="propertyName">is the property name
        /// </param>
        /// <param name="index">is the index to use to access the property value
        /// </param>
        public IndexedProperty(String propertyName, int index)
            : base(propertyName)
        {
            _index = index;
        }

        /// <summary> Returns index for indexed access.</summary>
        /// <returns> index value
        /// </returns>
        public int Index
        {
            get { return _index; }
        }

        public override bool IsDynamic
        {
            get { return false; }
        }

        public override String[] ToPropertyArray()
        {
            return new[] { PropertyNameAtomic };
        }

        public override EventPropertyGetter GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            FastClass fastClass = eventType.FastClass;
            InternalEventPropDescriptor propertyDesc = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (propertyDesc != null)
            {
                if (fastClass != null)
                {
                    MethodInfo method = propertyDesc.ReadMethod;
                    FastMethod fastMethod = fastClass.GetMethod(method);
                    return new KeyedFastPropertyGetter(fastMethod, _index, eventAdapterService);
                }
                else
                {
                    return new KeyedMethodPropertyGetter(propertyDesc.ReadMethod, _index, eventAdapterService);
                }
            }

            // Try the array as a simple property
            propertyDesc = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (propertyDesc == null)
            {
                return null;
            }

            Type returnType = propertyDesc.ReturnType;
            if (returnType.IsArray)
            {
                if (propertyDesc.ReadMethod != null)
                {
                    MethodInfo method = propertyDesc.ReadMethod;
                    if (fastClass != null)
                    {
                        FastMethod fastMethod = fastClass.GetMethod(method);
                        return new ArrayFastPropertyGetter(fastMethod, _index, eventAdapterService);
                    }
                    else
                    {
                        return new ArrayMethodPropertyGetter(method, _index, eventAdapterService);
                    }
                }
                else
                {
                    FieldInfo field = propertyDesc.AccessorField;
                    return new ArrayFieldPropertyGetter(field, _index, eventAdapterService);
                }
            }
            else if (returnType.IsImplementsInterface(typeof(IList<object>)))
            {
                if (propertyDesc.ReadMethod != null)
                {
                    MethodInfo method = propertyDesc.ReadMethod;
                    if (fastClass != null)
                    {
                        FastMethod fastMethod = fastClass.GetMethod(method);
                        return new ListFastPropertyGetter(method, fastMethod, _index, eventAdapterService);
                    }
                    else
                    {
                        return new ListMethodPropertyGetter(method, _index, eventAdapterService);
                    }
                }
                else
                {
                    FieldInfo field = propertyDesc.AccessorField;
                    return new ListFieldPropertyGetter(field, _index, eventAdapterService);
                }
            }
            else if (returnType.IsImplementsInterface(typeof(IEnumerable)))
            {
                if (propertyDesc.ReadMethod != null)
                {
                    MethodInfo method = propertyDesc.ReadMethod;
                    if (fastClass != null)
                    {
                        FastMethod fastMethod = fastClass.GetMethod(method);
                        return new EnumerableFastPropertyGetter(method, fastMethod, _index, eventAdapterService);
                    }
                    else
                    {
                        return new EnumerableMethodPropertyGetter(method, _index, eventAdapterService);
                    }
                }
                else
                {
                    FieldInfo field = propertyDesc.AccessorField;
                    return new EnumerableFieldPropertyGetter(field, _index, eventAdapterService);
                }
            }

            return null;
        }

        public override GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType,
                                                                   EventAdapterService eventAdapterService)
        {
            InternalEventPropDescriptor descriptor = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (descriptor != null)
            {
                return new GenericPropertyDesc(descriptor.ReturnType);
            }

            // Check if this is an method returning array which is a type of simple property
            descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null)
            {
                return null;
            }

            Type returnType = descriptor.ReturnType;
            if (returnType.IsArray)
            {
                return new GenericPropertyDesc(returnType.GetElementType());
            }
            else if (returnType.IsImplementsInterface(typeof(IEnumerable)))
            {
                if (descriptor.ReadMethod != null)
                {
                    Type genericType = TypeHelper.GetGenericReturnType(descriptor.ReadMethod, false);
                    return new GenericPropertyDesc(genericType);
                }
                else if (descriptor.AccessorField != null)
                {
                    Type genericType = TypeHelper.GetGenericFieldType(descriptor.AccessorField, false);
                    return new GenericPropertyDesc(genericType);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public override Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var descriptor = eventType.GetIndexedProperty(PropertyNameAtomic);
            if (descriptor != null)
            {
                return descriptor.ReturnType;
            }

            // Check if this is an method returning array which is a type of simple property
            descriptor = eventType.GetSimpleProperty(PropertyNameAtomic);
            if (descriptor == null)
            {
                return null;
            }

            var returnType = descriptor.ReturnType;
            if (returnType.IsArray)
            {
                return returnType.GetElementType();
            }
            else if (returnType.IsImplementsInterface(typeof(IEnumerable)))
            {
                if (descriptor.ReadMethod != null)
                {
                    return TypeHelper.GetGenericReturnType(descriptor.ReadMethod, false);
                }
                else if (descriptor.AccessorField != null)
                {
                    return TypeHelper.GetGenericFieldType(descriptor.AccessorField, false);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public override Type GetPropertyTypeMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            var type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null)
            {
                return null;
            }
            if (type is String) // resolve a property that is a map event type
            {
                var nestedName = type.ToString();
                var isArray = EventTypeUtility.IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                }

                var innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is MapEventType))
                {
                    return null;
                }
                if (!isArray)
                {
                    return null; // must be declared as an index to use array notation
                }
                else
                {
                    return typeof(DataMap[]);
                }
            }
            else
            {
                if (!(type is Type))
                {
                    return null;
                }
                if (!((Type)type).IsArray)
                {
                    return null;
                }
                return ((Type)type).GetElementType().GetBoxedType();
            }
        }

        public override MapEventPropertyGetter GetGetterMap(DataMap optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            Object type = optionalMapPropTypes.Get(PropertyNameAtomic);
            if (type == null)
            {
                return null;
            }
            if (type is string) // resolve a property that is a map event type
            {
                String nestedName = type.ToString();
                bool isArray = EventTypeUtility.IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                }
                EventType innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is MapEventType))
                {
                    return null;
                }
                if (!isArray)
                {
                    return null; // must be declared as an array to use an indexed notation
                }
                else
                {
                    return new MapArrayPropertyGetter(PropertyNameAtomic, _index, eventAdapterService, innerType);
                }
            }
            else
            {
                if (!(type is Type))
                {
                    return null;
                }
                if (!((Type)type).IsArray)
                {
                    return null;
                }
                Type componentType = ((Type)type).GetElementType();
                // its an array
                return new MapArrayEntryIndexedPropertyGetter(PropertyNameAtomic, _index, eventAdapterService,
                                                                  componentType);
            }
        }

        public override void ToPropertyEPL(TextWriter writer)
        {
            writer.Write(PropertyNameAtomic);
            writer.Write("[");
            writer.Write(_index);
            writer.Write("]");
        }

        public override EventPropertyGetter GetGetterDOM()
        {
            return new DOMIndexedGetter(PropertyNameAtomic, _index, null);
        }

        public override EventPropertyGetter GetGetterDOM(SchemaElementComplex complexProperty,
                                                         EventAdapterService eventAdapterService,
                                                         BaseXMLEventType eventType,
                                                         String propertyExpression)
        {
            foreach (SchemaElementSimple simple in complexProperty.SimpleElements)
            {
                if (!simple.IsArray)
                {
                    continue;
                }
                if (simple.Name != PropertyNameAtomic)
                {
                    continue;
                }
                return new DOMIndexedGetter(PropertyNameAtomic, _index, null);
            }

            foreach (SchemaElementComplex complex in complexProperty.ComplexElements)
            {
                if (!complex.IsArray)
                {
                    continue;
                }
                if (complex.Name != PropertyNameAtomic)
                {
                    continue;
                }
                return new DOMIndexedGetter(PropertyNameAtomic, _index,
                                            new FragmentFactoryDOMGetter(eventAdapterService, eventType,
                                                                         propertyExpression));
            }

            return null;
        }

        public override SchemaItem GetPropertyTypeSchema(SchemaElementComplex complexProperty,
                                                         EventAdapterService eventAdapterService)
        {
            foreach (SchemaElementSimple simple in complexProperty.SimpleElements)
            {
                if (simple.Name != PropertyNameAtomic)
                {
                    continue;
                }

                if ((simple.IsArray) ||
                    (simple.SimpleType == XmlSchemaSimpleType.GetBuiltInSimpleType(XmlTypeCode.String)))
                {
                    // return the simple as a non-array since an index is provided
                    return new SchemaElementSimple(
                        simple.Name,
                        simple.Namespace,
                        simple.SimpleType,
                        simple.TypeName,
                        false,
                        simple.FractionDigits);
                }
            }

            foreach (SchemaElementComplex complex in complexProperty.ComplexElements)
            {
                if (complex.Name != PropertyNameAtomic)
                {
                    continue;
                }
                if (complex.IsArray)
                {
                    // return the complex as a non-array since an index is provided
                    return new SchemaElementComplex(
                        complex.Name,
                        complex.Namespace,
                        complex.Attributes,
                        complex.ComplexElements,
                        complex.SimpleElements, false,
                        null,
                        null);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the index number for an indexed property expression.
        /// </summary>
        /// <param name="propertyName">property expression</param>
        /// <returns>index</returns>

        public static int GetIndex(String propertyName)
        {
            int start = propertyName.IndexOf('[');
            int end = propertyName.IndexOf(']');
            String indexStr = propertyName.Substring(start, end - start);
            return Int32.Parse(indexStr);
        }

        public override ObjectArrayEventPropertyGetter GetGetterObjectArray(
            IDictionary<string, int> indexPerProperty,
            IDictionary<string, object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            int propertyIndex;
            if (!indexPerProperty.TryGetValue(PropertyNameAtomic, out propertyIndex))
            {
                return null;
            }

            var type = nestableTypes.Get(PropertyNameAtomic);
            if (type == null)
            {
                return null;
            }

            if (type is string) // resolve a property that is a map event type
            {
                var nestedName = type.ToString();
                var isArray = EventTypeUtility.IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                }
                EventType innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is MapEventType))
                {
                    return null;
                }
                if (!isArray)
                {
                    return null; // must be declared as an array to use an indexed notation
                }
                else
                {
                    return new ObjectArrayArrayPropertyGetter(propertyIndex, _index, eventAdapterService, innerType);
                }
            }
            else
            {
                if (!(type is Type))
                {
                    return null;
                }
                if (!((Type)type).IsArray())
                {
                    return null;
                }
                Type componentType = ((Type)type).GetElementType();
                // its an array
                return new ObjectArrayArrayPONOEntryIndexedPropertyGetter(
                    propertyIndex, _index, eventAdapterService, componentType);
            }
        }
    }
}
