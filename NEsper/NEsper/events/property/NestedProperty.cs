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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events.property
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// This class represents a nested property, each nesting level made up of a property instance that 
    /// can be of type indexed, mapped or simple itself. 
    /// <para /> 
    /// The syntax for nested properties is as follows. 
    /// <pre>a.n a[1].n a('1').n </pre>
    /// </summary>
    public class NestedProperty : Property
    {
        /// <summary>Ctor. </summary>
        /// <param name="properties">is the list of Property instances representing each nesting level</param>
        public NestedProperty(IList<Property> properties)
        {
            Properties = properties;
        }

        /// <summary>Returns the list of property instances making up the nesting levels. </summary>
        /// <value>list of Property instances</value>
        public IList<Property> Properties { get; private set; }

        public bool IsDynamic
        {
            get { return Properties.Any(property => property.IsDynamic); }
        }

        public EventPropertyGetterSPI GetGetter(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            var getters = new List<EventPropertyGetter>();

            Property lastProperty = null;
            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                var property = it.Next();
                lastProperty = property;
                var getter = property.GetGetter(eventType, eventAdapterService);
                if (getter == null)
                {
                    return null;
                }

                if (it.HasNext())
                {
                    var clazz = property.GetPropertyType(eventType, eventAdapterService);
                    if (clazz == null)
                    {
                        // if the property is not valid, return null
                        return null;
                    }
                    // Map cannot be used to further nest as the type cannot be determined
                    if (clazz == typeof(Map))
                    {
                        return null;
                    }
                    if (clazz.IsArray)
                    {
                        return null;
                    }
                    eventType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                        clazz.FullName, clazz, false, false, false);
                }
                getters.Add(getter);
            }

            var finalPropertyType = lastProperty.GetPropertyTypeGeneric(eventType, eventAdapterService);
            return new NestedPropertyGetter(getters, eventAdapterService, finalPropertyType.GenericType, finalPropertyType.Generic);
        }

        public Type GetPropertyType(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            Type result = null;

            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                var property = it.Next();
                result = property.GetPropertyType(eventType, eventAdapterService);

                if (result == null)
                {
                    // property not found, return null
                    return null;
                }

                if (it.HasNext())
                {
                    // Map cannot be used to further nest as the type cannot be determined
                    if (result == typeof(Map))
                    {
                        return null;
                    }

                    if (result.IsArray || result.IsPrimitive || result.IsBuiltinDataType())
                    {
                        return null;
                    }

                    eventType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(result.Name, result, false, false, false);
                }
            }

            return result;
        }

        public GenericPropertyDesc GetPropertyTypeGeneric(BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            GenericPropertyDesc result = null;

            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                var property = it.Next();
                result = property.GetPropertyTypeGeneric(eventType, eventAdapterService);

                if (result == null)
                {
                    // property not found, return null
                    return null;
                }

                if (it.HasNext())
                {
                    // Map cannot be used to further nest as the type cannot be determined
                    if (result.GenericType == typeof(Map))
                    {
                        return null;
                    }

                    if (result.GenericType.IsArray)
                    {
                        return null;
                    }

                    eventType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                        result.GenericType.FullName,
                        result.GenericType,
                        false, false, false);
                }
            }

            return result;
        }

        public Type GetPropertyTypeMap(Map optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                count++;
                var property = it.Next();
                var propertyBase = (PropertyBase)property;
                var propertyName = propertyBase.PropertyNameAtomic;

                Object nestedType = null;
                if (currentDictionary != null)
                {
                    nestedType = currentDictionary.Get(propertyName);
                }

                if (nestedType == null)
                {
                    if (property is DynamicProperty)
                    {
                        return typeof(Object);
                    }
                    else
                    {
                        return null;
                    }
                }

                if (!it.HasNext())
                {
                    if (nestedType is Type)
                    {
                        return ((Type)nestedType).GetBoxedType();
                    }
                    if (nestedType is Map)
                    {
                        return typeof(Map);
                    }
                }

                if (ReferenceEquals(nestedType, typeof(Map)))
                {
                    return typeof(Object);
                }

                if (nestedType is Type)
                {
                    var pocoType = (Type)nestedType;
                    if (!pocoType.IsArray)
                    {
                        var beanType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(pocoType.Name, pocoType, false, false, false);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps).GetBoxedType();
                    }
                    else if (property is IndexedProperty)
                    {
                        var componentType = pocoType.GetElementType();
                        var beanType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(componentType.Name, componentType, false, false, false);
                        var remainingProps = ToPropertyEPL(Properties, count);
                        return beanType.GetPropertyType(remainingProps).GetBoxedType();
                    }
                }

                if (nestedType is String)       // property type is the name of a map event type
                {
                    var nestedName = nestedType.ToString();
                    var isArray = EventTypeUtility.IsPropertyArray(nestedName);
                    if (isArray)
                    {
                        nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                    }

                    var innerType = eventAdapterService.GetEventTypeByName(nestedName);
                    if (innerType == null)
                    {
                        return null;
                    }

                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps).GetBoxedType();
                }
                else if (nestedType is EventType)       // property type is the name of a map event type
                {
                    var innerType = (EventType)nestedType;
                    var remainingProps = ToPropertyEPL(Properties, count);
                    return innerType.GetPropertyType(remainingProps).GetBoxedType();
                }
                else
                {
                    if (!(nestedType is Map))
                    {
                        var message = "Nestable map type configuration encountered an unexpected value type of '"
                            + nestedType.GetType() + " for property '" + propertyName + "', expected Class, typeof(Map) or IDictionary<String, Object> as value type";
                        throw new PropertyAccessException(message);
                    }
                }

                currentDictionary = (Map)nestedType;
            }
            throw new IllegalStateException("Unexpected end of nested property");
        }

        public MapEventPropertyGetter GetGetterMap(Map optionalMapPropTypes, EventAdapterService eventAdapterService)
        {
            var getters = new List<EventPropertyGetterSPI>();
            var currentDictionary = optionalMapPropTypes;

            var count = 0;
            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                count++;
                var property = it.Next();

                // manufacture a getter for getting the item out of the map
                var getter = property.GetGetterMap(currentDictionary, eventAdapterService);
                if (getter == null)
                {
                    return null;
                }
                getters.Add(getter);

                var @base = (PropertyBase)property;
                var propertyName = @base.PropertyNameAtomic;

                // For the next property if there is one, check how to property type is defined
                if (!it.HasNext())
                {
                    continue;
                }

                if (currentDictionary != null)
                {
                    // check the type that this property will return
                    var propertyReturnType = currentDictionary.Get(propertyName);

                    if (propertyReturnType == null)
                    {
                        currentDictionary = null;
                    }
                    if (propertyReturnType != null)
                    {
                        if (propertyReturnType is Map)
                        {
                            currentDictionary = (Map)propertyReturnType;
                        }
                        else if (ReferenceEquals(propertyReturnType, typeof(Map)))
                        {
                            currentDictionary = null;
                        }
                        else if (propertyReturnType is String)
                        {
                            var nestedName = propertyReturnType.ToString();
                            var isArray = EventTypeUtility.IsPropertyArray(nestedName);
                            if (isArray)
                            {
                                nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                            }

                            var innerType = eventAdapterService.GetEventTypeByName(nestedName);
                            if (innerType == null)
                            {
                                return null;
                            }

                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI) innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null)
                            {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else if (propertyReturnType is EventType)
                        {
                            var innerType = (EventType)propertyReturnType;
                            var remainingProps = ToPropertyEPL(Properties, count);
                            var getterInner = ((EventTypeSPI) innerType).GetGetterSPI(remainingProps);
                            if (getterInner == null)
                            {
                                return null;
                            }

                            getters.Add(getterInner);
                            break; // the single getter handles the rest
                        }
                        else
                        {
                            // treat the return type of the map property as an object
                            var returnType = (Type)propertyReturnType;
                            if (!returnType.IsArray)
                            {
                                var beanType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                                    returnType.Name, returnType, false, false, false);
                                var remainingProps = ToPropertyEPL(Properties, count);
                                var getterInner = beanType.GetGetterSPI(remainingProps);
                                if (getterInner == null)
                                {
                                    return null;
                                }
                                getters.Add(getterInner);
                                break; // the single getter handles the rest
                            }
                            else
                            {
                                var componentType = returnType.GetElementType();
                                var beanType = eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                                    componentType.Name, componentType, false, false, false);
                                var remainingProps = ToPropertyEPL(Properties, count);
                                var getterInner = beanType.GetGetterSPI(remainingProps);
                                if (getterInner == null)
                                {
                                    return null;
                                }
                                getters.Add(getterInner);
                                break; // the single pono getter handles the rest
                            }
                        }
                    }
                }
            }

            var hasNonmapGetters = false;
            for (var i = 0; i < getters.Count; i++)
            {
                if (!(getters[i] is MapEventPropertyGetter))
                {
                    hasNonmapGetters = true;
                }
            }
            if (!hasNonmapGetters)
            {
                return new MapNestedPropertyGetterMapOnly(getters, eventAdapterService);
            }
            else
            {
                return new MapNestedPropertyGetterMixedType(getters, eventAdapterService);
            }
        }

        public ObjectArrayEventPropertyGetter GetGetterObjectArray(IDictionary<string, int> indexPerProperty, IDictionary<string, object> nestableTypes, EventAdapterService eventAdapterService)
        {
            throw new UnsupportedOperationException("Object array nested property getter not implemented as not implicitly nestable");
        }

        public void ToPropertyEPL(TextWriter writer)
        {
            var delimiter = "";
            foreach (var property in Properties)
            {
                writer.Write(delimiter);
                property.ToPropertyEPL(writer);
                delimiter = ".";
            }
        }

        public String[] ToPropertyArray()
        {
            var propertyNames = new List<String>();
            foreach (var property in Properties)
            {
                var nested = property.ToPropertyArray();
                propertyNames.AddAll(nested);
            }
            return propertyNames.ToArray();
        }

        public EventPropertyGetterSPI GetGetterDOM()
        {
            var getters = new List<EventPropertyGetter>();

            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                var property = it.Next();
                var getter = property.GetGetterDOM();
                if (getter == null)
                {
                    return null;
                }

                getters.Add(getter);
            }

            return new DOMNestedPropertyGetter(getters, null);
        }

        public EventPropertyGetterSPI GetGetterDOM(SchemaElementComplex parentComplexProperty, EventAdapterService eventAdapterService, BaseXMLEventType eventType, String propertyExpression)
        {
            var getters = new List<EventPropertyGetter>();

            var complexElement = parentComplexProperty;

            for (var it = Properties.EnumerateWithLookahead(); it.HasNext(); )
            {
                var property = it.Next();
                var getter = property.GetGetterDOM(complexElement, eventAdapterService, eventType, propertyExpression);
                if (getter == null)
                {
                    return null;
                }

                if (it.HasNext())
                {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement, eventAdapterService);
                    if (childSchemaItem == null)
                    {
                        // if the property is not valid, return null
                        return null;
                    }

                    if ((childSchemaItem is SchemaItemAttribute) || (childSchemaItem is SchemaElementSimple))
                    {
                        return null;
                    }

                    complexElement = (SchemaElementComplex)childSchemaItem;

                    if (complexElement.IsArray)
                    {
                        if ((property is SimpleProperty) || (property is DynamicSimpleProperty))
                        {
                            return null;
                        }
                    }
                }

                getters.Add(getter);
            }

            return new DOMNestedPropertyGetter(getters, new FragmentFactoryDOMGetter(eventAdapterService, eventType, propertyExpression));
        }

        public SchemaItem GetPropertyTypeSchema(SchemaElementComplex parentComplexProperty, EventAdapterService eventAdapterService)
        {
            Property lastProperty = null;
            var complexElement = parentComplexProperty;

            for (var en = Properties.EnumerateWithLookahead(); en.HasNext(); )
            {
                var property = en.Next();
                lastProperty = property;

                if (en.HasNext())
                {
                    var childSchemaItem = property.GetPropertyTypeSchema(complexElement, eventAdapterService);
                    if (childSchemaItem == null)
                    {
                        // if the property is not valid, return null
                        return null;
                    }

                    if ((childSchemaItem is SchemaItemAttribute) || (childSchemaItem is SchemaElementSimple))
                    {
                        return null;
                    }

                    complexElement = (SchemaElementComplex)childSchemaItem;
                }
            }

            return lastProperty.GetPropertyTypeSchema(complexElement, eventAdapterService);
        }

        public string PropertyNameAtomic
        {
            get { throw new UnsupportedOperationException("Nested properties do not provide an atomic property name"); }
        }

        private static String ToPropertyEPL(IList<Property> property, int startFromIndex)
        {
            var delimiter = "";
            var writer = new StringWriter();
            for (var i = startFromIndex; i < property.Count; i++)
            {
                writer.Write(delimiter);
                property[i].ToPropertyEPL(writer);
                delimiter = ".";
            }
            return writer.ToString();
        }
    }
}
