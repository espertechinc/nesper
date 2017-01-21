///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;

    public class EventTypeUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(EventType target, String propertyName)
        {
            var descriptor = target.GetPropertyDescriptor(propertyName);
            if (descriptor != null)
            {
                return descriptor;
            }
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                return null;
            }
            // parse, can be an nested property
            var property = PropertyParser.ParseAndWalk(propertyName);
            if (property is PropertyBase)
            {
                return target.GetPropertyDescriptor(((PropertyBase) property).PropertyNameAtomic);
            }
            if (!(property is NestedProperty))
            {
                return null;
            }
            var nested = (NestedProperty) property;
            var properties = new LinkedList<Property>(nested.Properties);
            return GetNestablePropertyDescriptor(target, properties);
        }

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(EventType target, LinkedList<Property> stack)
        {
            var topProperty = stack.PopFront();
            if (stack.IsEmpty())
            {
                return target.GetPropertyDescriptor(((PropertyBase) topProperty).PropertyNameAtomic);
            }

            if (!(topProperty is SimpleProperty))
            {
                return null;
            }
            var simple = (SimpleProperty) topProperty;

            var fragmentEventType = target.GetFragmentType(simple.PropertyNameAtomic);
            if (fragmentEventType == null || fragmentEventType.FragmentType == null)
            {
                return null;
            }
            return GetNestablePropertyDescriptor(fragmentEventType.FragmentType, stack);
        }

        public static IDictionary<String, Object> BuildType(
            IList<ColumnDesc> columns,
            EventAdapterService eventAdapterService,
            ICollection<String> copyFrom,
            EngineImportService engineImportService)
        {
            IDictionary<String, Object> typing = new LinkedHashMap<String, Object>();
            ISet<String> columnNames = new HashSet<String>();
            foreach (var column in columns)
            {
                bool added = columnNames.Add(column.Name);
                if (!added)
                {
                    throw new ExprValidationException("Duplicate column name '" + column.Name + "'");
                }

                var columnType = BuildType(column, engineImportService);
                typing.Put(column.Name, columnType);
            }

            if (copyFrom != null && !copyFrom.IsEmpty())
            {
                foreach (var copyFromName in copyFrom)
                {
                    EventType type = eventAdapterService.GetEventTypeByName(copyFromName);
                    if (type == null)
                    {
                        throw new ExprValidationException("Type by name '" + copyFromName + "' could not be located");
                    }
                    MergeType(typing, type);
                }
            }
            return typing;
        }

        public static Object BuildType(ColumnDesc column, EngineImportService engineImportService)
        {
            if (column.Type == null)
            {
                return null;
            }

            if (column.IsPrimitiveArray)
            {
                var primitive = TypeHelper.GetPrimitiveTypeForName(column.Type);
                if (primitive != null)
                {
                    return Array.CreateInstance(primitive, 0).GetType();
                }
                throw new ExprValidationException("Type '" + column.Type + "' is not a primitive type");
            }

            var plain = TypeHelper.GetTypeForSimpleName(column.Type);
            if (plain != null)
            {
                if (column.IsArray)
                {
                    plain = Array.CreateInstance(plain, 0).GetType();
                }
                return plain;
            }

            // try imports first
            Type resolved = null;
            try
            {
                resolved = engineImportService.ResolveType(column.Type, false);
            }
            catch (EngineImportException e)
            {
                // expected
            }

            // resolve from classpath when not found
            if (resolved == null)
            {
                try
                {
                    resolved = TypeHelper.ResolveType(column.Type);
                }
                catch (TypeLoadException)
                {
                    // expected
                }
            }

            // Handle resolved classes here
            if (resolved != null)
            {
                if (column.IsArray)
                {
                    resolved = Array.CreateInstance(resolved, 0).GetType();
                }
                return resolved;
            }

            // Event types fall into here
            if (column.IsArray)
            {
                return column.Type + "[]";
            }
            return column.Type;
        }

        private static void MergeType(IDictionary<String, Object> typing, EventType typeToMerge)
        {
            foreach (var prop in typeToMerge.PropertyDescriptors)
            {
                var existing = typing.Get(prop.PropertyName);

                if (!prop.IsFragment)
                {
                    var assigned = prop.PropertyType;
                    if (existing != null && existing is Type)
                    {
                        if (((Type) existing).GetBoxedType() != assigned.GetBoxedType())
                        {
                            throw new ExprValidationException(
                                "Type by name '" + typeToMerge.Name + "' contributes property '" +
                                prop.PropertyName + "' defined as '" + TypeHelper.GetTypeNameFullyQualPretty(assigned) +
                                "' which overides the same property of type '" +
                                TypeHelper.GetTypeNameFullyQualPretty((Type) existing) + "'");
                        }
                    }
                    typing.Put(prop.PropertyName, prop.PropertyType);
                }
                else
                {
                    if (existing != null)
                    {
                        throw new ExprValidationException(
                            "Property by name '" + prop.PropertyName + "' is defined twice by adding type '" +
                            typeToMerge.Name + "'");
                    }

                    var fragment = typeToMerge.GetFragmentType(prop.PropertyName);
                    if (fragment == null)
                    {
                        continue;
                    }
                    if (fragment.IsIndexed)
                    {
                        typing.Put(
                            prop.PropertyName, new EventType[]
                            {
                                fragment.FragmentType
                            });
                    }
                    else
                    {
                        typing.Put(prop.PropertyName, fragment.FragmentType);
                    }
                }
            }
        }

        public static void ValidateTimestampProperties(
            EventType eventType,
            String startTimestampProperty,
            String endTimestampProperty)
        {
            if (startTimestampProperty != null)
            {
                if (eventType.GetGetter(startTimestampProperty) == null)
                {
                    throw new ConfigurationException(
                        "Declared start timestamp property name '" + startTimestampProperty + "' was not found");
                }
                var type = eventType.GetPropertyType(startTimestampProperty);
                if (!type.IsDateTime())
                {
                    throw new ConfigurationException(
                        "Declared start timestamp property '" + startTimestampProperty +
                        "' is expected to return a DateTime or long-typed value but returns '" + type.FullName + "'");
                }
            }

            if (endTimestampProperty != null)
            {
                if (startTimestampProperty == null)
                {
                    throw new ConfigurationException(
                        "Declared end timestamp property requires that a start timestamp property is also declared");
                }
                if (eventType.GetGetter(endTimestampProperty) == null)
                {
                    throw new ConfigurationException(
                        "Declared end timestamp property name '" + endTimestampProperty + "' was not found");
                }
                var type = eventType.GetPropertyType(endTimestampProperty);
                if (!type.IsDateTime())
                {
                    throw new ConfigurationException(
                        "Declared end timestamp property '" + endTimestampProperty +
                        "' is expected to return a DateTime or long-typed value but returns '" + type.FullName + "'");
                }
                var startType = eventType.GetPropertyType(startTimestampProperty);
                if (startType.GetBoxedType() != type.GetBoxedType())
                {
                    throw new ConfigurationException(
                        "Declared end timestamp property '" + endTimestampProperty +
                        "' is expected to have the same property type as the start-timestamp property '" +
                        startTimestampProperty + "'");
                }
            }
        }

        public static bool IsTypeOrSubTypeOf(EventType candidate, EventType superType)
        {
            if (Equals(candidate, superType))
            {
                return true;
            }

            if (candidate.SuperTypes != null)
            {
                return Enumerable.Contains(candidate.DeepSuperTypes, superType);
            }

            return false;
        }

        /// <summary>Determine among the Map-type properties which properties are Bean-type event type names, rewrites these as Class-type instead so that they are configured as native property and do not require wrapping, but may require unwrapping. </summary>
        /// <param name="typing">properties of map type</param>
        /// <param name="eventAdapterService">event adapter service</param>
        /// <returns>compiled properties, same as original unless Bean-type event type names were specified.</returns>
        public static IDictionary<String, Object> CompileMapTypeProperties(
            IDictionary<String, Object> typing,
            EventAdapterService eventAdapterService)
        {
            IDictionary<String, Object> compiled = new LinkedHashMap<String, Object>(typing);
            foreach (var specifiedEntry in typing)
            {
                var typeSpec = specifiedEntry.Value;
                var nameSpec = specifiedEntry.Key;
                if (!(typeSpec is String))
                {
                    continue;
                }

                var typeNameSpec = (String) typeSpec;
                var isArray = IsPropertyArray(typeNameSpec);
                if (isArray)
                {
                    typeNameSpec = GetPropertyRemoveArray(typeNameSpec);
                }

                EventType eventType = eventAdapterService.GetEventTypeByName(typeNameSpec);
                if (eventType == null || !(eventType is BeanEventType))
                {
                    continue;
                }

                var beanEventType = (BeanEventType) eventType;
                var underlyingType = beanEventType.UnderlyingType;
                if (isArray)
                {
                    underlyingType = TypeHelper.GetArrayType(underlyingType);
                }
                compiled.Put(nameSpec, underlyingType);
            }
            return compiled;
        }

        /// <summary>Returns true if the name indicates that the type is an array type. </summary>
        /// <param name="name">the property name</param>
        /// <returns>true if array type</returns>
        public static bool IsPropertyArray(String name)
        {
            return name.Trim().EndsWith("[]");
        }

        /// <summary>Returns the property name without the array type extension, if present. </summary>
        /// <param name="name">property name</param>
        /// <returns>property name with removed array extension name</returns>
        public static String GetPropertyRemoveArray(String name)
        {
            return name.RegexReplaceAll("\\[", "").RegexReplaceAll("\\]", "");
        }

        public static PropertySetDescriptor GetNestableProperties(
            IDictionary<String, Object> propertiesToAdd,
            EventAdapterService eventAdapterService,
            EventTypeNestableGetterFactory factory,
            EventType[] optionalSuperTypes)
        {
            var propertyNameList = new List<String>();
            var propertyDescriptors = new List<EventPropertyDescriptor>();
            var nestableTypes = new LinkedHashMap<String, Object>();
            var propertyItems = new Dictionary<String, PropertySetDescriptorItem>();

            // handle super-types first, such that the order of properties is well-defined from super-type to subtype
            if (optionalSuperTypes != null)
            {
                for (var i = 0; i < optionalSuperTypes.Length; i++)
                {
                    var superType = (BaseNestableEventType) optionalSuperTypes[i];
                    propertyNameList.AddRange(
                        superType.PropertyNames.Where(propertyName => !nestableTypes.ContainsKey(propertyName)));
                    propertyDescriptors.AddRange(
                        superType.PropertyDescriptors.Where(descriptor => !nestableTypes.ContainsKey(descriptor.PropertyName)));

                    propertyItems.PutAll(superType.PropertyItems);
                    nestableTypes.PutAll(superType.NestableTypes);
                }
            }

            nestableTypes.PutAll(propertiesToAdd);

            // Initialize getters and names array: at this time we do not care about nested types,
            // these are handled at the time someone is asking for them
            foreach (var entry in propertiesToAdd)
            {
                var name = entry.Key;
                if (name == null)
                {
                    throw new EPException("Invalid type configuration: property name is not a String-type value");
                }

                // handle types that are String values
                var entryValue = entry.Value;
                if (entryValue is String)
                {
                    var value = entryValue.ToString().Trim();
                    var clazz = TypeHelper.GetPrimitiveTypeForName(value);
                    if (clazz != null)
                    {
                        entryValue = clazz;
                    }
                }

                if (entryValue is Type)
                {
                    Type componentType = null;
                    Type classType = (Type)entryValue;

                    var isFragment = false;
                    var isIndexed = false;

                    if (classType.IsArray)
                    {
                        isIndexed = true;
                        componentType = classType.GetElementType();
                    }
                    else if (classType == typeof (XmlNode))
                    {
                    }
                    else if (classType == typeof (string))
                    {
                        isIndexed = true;
                        componentType = typeof (char);
                    }
                    else if (classType.IsGenericList())
                    {
                        isIndexed = true;

                        var genericType = classType.GetGenericType(0);
                        isFragment = genericType.IsFragmentableType();
                        if (genericType != null)
                        {
                            componentType = genericType;
                        }
                        else
                        {
                            componentType = typeof(Object);
                        }
                    }


                    var isMapped = classType.IsImplementsInterface<DataMap>();
                    if (isMapped)
                    {
                        componentType = typeof(object); // DataMap is string oriented keys and object oriented values
                    }

                    isFragment = classType.IsFragmentableType();
                    BeanEventType nativeFragmentType = null;
                    FragmentEventType fragmentType = null;
                    if (isFragment)
                    {
                        fragmentType = EventBeanUtility.CreateNativeFragmentType(classType, null, eventAdapterService);
                        if (fragmentType != null)
                        {
                            nativeFragmentType = (BeanEventType) fragmentType.FragmentType;
                        }
                        else
                        {
                            isFragment = false;
                        }
                    }
                    var getter = factory.GetGetterProperty(
                        name, nativeFragmentType, eventAdapterService);
                    var descriptor = new EventPropertyDescriptor(
                        name, classType, componentType, false, false, isIndexed, isMapped, isFragment);
                    var item = new PropertySetDescriptorItem(
                        descriptor, classType, getter, fragmentType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                // A null-type is also allowed
                if (entryValue == null)
                {
                    var getter = factory.GetGetterProperty(name, null, null);
                    var descriptor = new EventPropertyDescriptor(
                        name, null, null, false, false, false, false, false);
                    var item = new PropertySetDescriptorItem(descriptor, null, getter, null);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                // Add Map itself as a property
                if (entryValue is IDictionary<string, object>)
                {
                    var getter = factory.GetGetterProperty(name, null, null);
                    var descriptor = new EventPropertyDescriptor(
                        name, typeof(IDictionary<string, object>), null, false, false, false, true, false);
                    var item = new PropertySetDescriptorItem(
                        descriptor, typeof(IDictionary<string, object>), getter, null);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entryValue is EventType)
                {
                    // Add EventType itself as a property
                    var getter = factory.GetGetterEventBean(name);
                    var eventType = (EventType) entryValue;
                    var descriptor = new EventPropertyDescriptor(
                        name, eventType.UnderlyingType, null, false, false, false, false, true);
                    var fragmentEventType = new FragmentEventType(eventType, false, false);
                    var item = new PropertySetDescriptorItem(
                        descriptor, eventType.UnderlyingType.GetBoxedType(), getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entryValue is EventType[])
                {
                    // Add EventType array itself as a property, type is expected to be first array element
                    var eventType = ((EventType[]) entryValue)[0];
                    Object prototypeArray = Array.CreateInstance(eventType.UnderlyingType, 0);
                    var getter = factory.GetGetterEventBeanArray(name, eventType);
                    var descriptor = new EventPropertyDescriptor(
                        name, prototypeArray.GetType(), eventType.UnderlyingType, false, false, true, false, true);
                    var fragmentEventType = new FragmentEventType(eventType, true, false);
                    var item = new PropertySetDescriptorItem(
                        descriptor, prototypeArray.GetType().GetBoxedType(), getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entryValue is String)
                {
                    var propertyName = entryValue.ToString();
                    var isArray = IsPropertyArray(propertyName);
                    if (isArray)
                    {
                        propertyName = GetPropertyRemoveArray(propertyName);
                    }

                    // Add EventType itself as a property
                    EventType eventType = eventAdapterService.GetEventTypeByName(propertyName);
                    if (!(eventType is BaseNestableEventType) && !(eventType is BeanEventType))
                    {
                        throw new EPException(
                            "Nestable type configuration encountered an unexpected property type name '"
                            + entryValue + "' for property '" + name +
                            "', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
                    }

                    var underlyingType = eventType.UnderlyingType;
                    if (isArray)
                    {
                        underlyingType = Array.CreateInstance(underlyingType, 0).GetType();
                    }
                    EventPropertyGetter getter;
                    if (!isArray)
                    {
                        getter = factory.GetGetterBeanNested(name, eventType, eventAdapterService);
                    }
                    else
                    {
                        getter = factory.GetGetterBeanNestedArray(name, eventType, eventAdapterService);
                    }
                    var descriptor = new EventPropertyDescriptor(
                        name, underlyingType, null, false, false, isArray, false, true);
                    var fragmentEventType = new FragmentEventType(eventType, isArray, false);
                    var item = new PropertySetDescriptorItem(
                        descriptor, underlyingType.GetBoxedType(), getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                GenerateExceptionNestedProp(name, entryValue);
            }

            return new PropertySetDescriptor(propertyNameList, propertyDescriptors, propertyItems, nestableTypes);
        }

        private static void GenerateExceptionNestedProp(String name, Object value)
        {
            String clazzName = (value == null) ? "null" : value.GetType().Name;
            throw new EPException(
                "Nestable type configuration encountered an unexpected property type of '"
                + clazzName + "' for property '" + name +
                "', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
        }

        public static Type GetNestablePropertyType(
            String propertyName,
            IDictionary<String, PropertySetDescriptorItem> simplePropertyTypes,
            IDictionary<String, Object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            var item = simplePropertyTypes.Get(ASTUtil.UnescapeDot(propertyName));
            if (item != null)
            {
                return item.SimplePropertyType;
            }

            // see if this is a nested property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                // dynamic simple property
                if (propertyName.EndsWith("?"))
                {
                    return typeof (Object);
                }

                // parse, can be an indexed property
                Property property;
                try
                {
                    property = PropertyParser.ParseAndWalk(propertyName, false);
                }
                catch (Exception)
                {
                    // cannot parse property, return type
                    var propitem = simplePropertyTypes.Get(propertyName);
                    if (propitem != null)
                    {
                        return propitem.SimplePropertyType;
                    }
                    return null;
                }

                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    else if (type is EventType[])
                    {
                        return ((EventType[]) type)[0].UnderlyingType;
                    }
                    else if (type is String)
                    {
                        var propTypeName = type.ToString();
                        var isArray = IsPropertyArray(propTypeName);
                        if (isArray)
                        {
                            propTypeName = GetPropertyRemoveArray(propTypeName);
                        }
                        EventType innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                        return innerType.UnderlyingType;
                    }

                    var asType = type as Type;
                    if (asType == null)
                    {
                        return null;
                    }

                    if (asType == typeof (string))
                    {
                        return typeof(char);
                    }

                    var asGenericEnumType = asType.FindGenericInterface(typeof(IList<>));
                    if (asGenericEnumType != null)
                    {
                        return asGenericEnumType.GetGenericArguments()[0];
                    }

                    if (asType.IsArray)
                    {
                        // its an array
                        return asType.GetElementType();
                    }

                    return null;
                }
                else if (property is MappedProperty)
                {
                    var mappedProp = (MappedProperty) property;
                    var type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    var astype = type as Type;
                    if (astype != null)
                    {
                        if (astype.IsImplementsInterface(typeof (IDictionary<string, object>)))
                        {
                            return typeof (Object);
                        }
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is a object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
            // The property getters therefore act on

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?"))
            {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            var nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null)
            {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalk(propertyMap, false);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    // handle map-in-map case
                    if (type is String)
                    {
                        var propTypeName = type.ToString();
                        var isArray = IsPropertyArray(propTypeName);
                        if (isArray)
                        {
                            propTypeName = GetPropertyRemoveArray(propTypeName);
                        }
                        var innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        return innerType.GetPropertyType(propertyNested);
                    }
                        // handle eventtype[] in map
                    else if (type is EventType[])
                    {
                        var innerType = ((EventType[]) type)[0];
                        return innerType.GetPropertyType(propertyNested);
                    }
                        // handle array class in map case
                    else
                    {
                        var asType = type as Type;
                        if (asType == null)
                        {
                            return null;
                        }

                        var componentType = ((Type)type).GetElementType();
                        var asGenericListType = asType.FindGenericInterface(typeof(IList<>));
                        if (asGenericListType != null)
                        {
                            componentType = asGenericListType.GetGenericArguments()[0];
                        }
                        else if (asType == typeof (string))
                        {
                            componentType = typeof (char);
                        }
                        else if (asType.IsArray)
                        {
                        }
                        else
                        {
                            return null;
                        }
                        
                        var nestedEventType = eventAdapterService.AddBeanType(
                            componentType.Name, componentType, false, false, false);
                        return nestedEventType.GetPropertyType(propertyNested);
                    }
                }
                else if (property is MappedProperty)
                {
                    return null; // Since no type information is available for the property
                }
                else
                {
                    return null;
                }
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if (ReferenceEquals(nestedType, typeof (IDictionary<string, object>)))
            {
                var prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                return prop.GetPropertyTypeMap(null, eventAdapterService);
                    // we don't have a definition of the nested props
            }
            else if (nestedType is IDictionary<string, object>)
            {
                var prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                var nestedTypes = (IDictionary<string, object>)nestedType;
                return prop.GetPropertyTypeMap(nestedTypes, eventAdapterService);
            }
            else if (nestedType is Type)
            {
                var simpleClass = (Type) nestedType;
                if (simpleClass.IsBuiltinDataType())
                {
                    return null;
                }
                if (simpleClass.IsArray() && (simpleClass.GetElementType().IsBuiltinDataType() || simpleClass.GetElementType() == typeof(object)))
                {
                    return null;
                }
                var nestedEventType = eventAdapterService.AddBeanType(
                    simpleClass.FullName, simpleClass, false, false, false);
                return nestedEventType.GetPropertyType(propertyNested);
            }
            else if (nestedType is EventType)
            {
                var innerType = (EventType) nestedType;
                return innerType.GetPropertyType(propertyNested);
            }
            else if (nestedType is EventType[])
            {
                return null; // requires indexed property
            }
            else if (nestedType is String)
            {
                var nestedName = nestedType.ToString();
                var isArray = IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = GetPropertyRemoveArray(nestedName);
                }
                var innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is BaseNestableEventType))
                {
                    return null;
                }
                return innerType.GetPropertyType(propertyNested);
            }
            else
            {
                var message = "Nestable map type configuration encountered an unexpected value type of '"
                                 + nestedType.GetType() + " for property '" + propertyName +
                                 "', expected Class, typeof(Map) or Map<String, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        public static EventPropertyGetter GetNestableGetter(
            String propertyName,
            IDictionary<String, PropertySetDescriptorItem> propertyGetters,
            IDictionary<String, EventPropertyGetter> propertyGetterCache,
            IDictionary<String, Object> nestableTypes,
            EventAdapterService eventAdapterService,
            EventTypeNestableGetterFactory factory)
        {
            var cachedGetter = propertyGetterCache.Get(propertyName);
            if (cachedGetter != null)
            {
                return cachedGetter;
            }

            var unescapePropName = ASTUtil.UnescapeDot(propertyName);
            var item = propertyGetters.Get(unescapePropName);
            if (item != null)
            {
                var getter = item.PropertyGetter;
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }

            // see if this is a nested property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                var prop = PropertyParser.ParseAndWalk(propertyName);
                if (prop is DynamicProperty)
                {
                    var getterDyn = factory.GetPropertyProvidedGetter(
                        nestableTypes, propertyName, prop, eventAdapterService);
                    propertyGetterCache.Put(propertyName, getterDyn);
                    return getterDyn;
                }
                else if (prop is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) prop;
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    else if (type is EventType[])
                    {
                        var getterArr = factory.GetGetterIndexedEventBean(
                            indexedProp.PropertyNameAtomic, indexedProp.Index);
                        propertyGetterCache.Put(propertyName, getterArr);
                        return getterArr;
                    }
                    else if (type is String)
                    {
                        var nestedTypeName = type.ToString();
                        var isArray = IsPropertyArray(nestedTypeName);
                        if (isArray)
                        {
                            nestedTypeName = GetPropertyRemoveArray(nestedTypeName);
                        }
                        var innerType = eventAdapterService.GetEventTypeByName(nestedTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        EventPropertyGetter typeGetter;
                        if (!isArray)
                        {
                            typeGetter = factory.GetGetterBeanNested(
                                indexedProp.PropertyNameAtomic, innerType, eventAdapterService);
                        }
                        else
                        {
                            typeGetter = factory.GetGetterIndexedUnderlyingArray(
                                indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, innerType);
                        }
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    // handle map type name in map
                    var asType = type as Type;
                    if (asType == null)
                    {
                        return null;
                    }

                    if (asType == typeof (string))
                    {
                        var componentType = typeof (string);
                        var indexedGetter = factory.GetGetterIndexedPONO(indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, componentType);
                        propertyGetterCache.Put(propertyName, indexedGetter);
                        return indexedGetter;
                    }
                    
                    var asGenericListType = asType.FindGenericInterface(typeof(IList<>));
                    if (asGenericListType != null)
                    {
                        var componentType = asGenericListType.GetGenericArguments()[0];
                        var indexedGetter = factory.GetGetterIndexedPONO(indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, componentType);
                        propertyGetterCache.Put(propertyName, indexedGetter);
                        return indexedGetter;
                    }

                    if (asType.IsArray)
                    {
                        // its an array
                        var componentType = asType.GetElementType();
                        var indexedGetter = factory.GetGetterIndexedPONO(indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, componentType);
                        propertyGetterCache.Put(propertyName, indexedGetter);
                        return indexedGetter;
                    }

                    return null;
                }
                else if (prop is MappedProperty)
                {
                    var mappedProp = (MappedProperty) prop;
                    var type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    var asType = type as Type;
                    if (asType != null)
                    {
                        if (asType.IsImplementsInterface(typeof (IDictionary<string, object>)))
                        {
                            return factory.GetGetterMappedProperty(mappedProp.PropertyNameAtomic, mappedProp.Key);
                        }
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?"))
            {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            var nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null)
            {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalk(propertyMap);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    if (type is String)
                    {
                        var nestedTypeName = type.ToString();
                        var isArray = IsPropertyArray(nestedTypeName);
                        if (isArray)
                        {
                            nestedTypeName = GetPropertyRemoveArray(nestedTypeName);
                        }
                        EventType innerType = eventAdapterService.GetEventTypeByName(nestedTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        EventPropertyGetter typeGetter;
                        if (!isArray)
                        {
                            typeGetter = factory.GetGetterNestedEntryBean(
                                propertyMap, innerType.GetGetter(propertyNested), innerType, eventAdapterService);
                        }
                        else
                        {
                            typeGetter = factory.GetGetterNestedEntryBeanArray(
                                indexedProp.PropertyNameAtomic, indexedProp.Index, innerType.GetGetter(propertyNested),
                                innerType, eventAdapterService);
                        }
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is EventType[])
                    {
                        var componentType = ((EventType[]) type)[0];
                        var nestedGetter = componentType.GetGetter(propertyNested);
                        if (nestedGetter == null)
                        {
                            return null;
                        }
                        var typeGetter =
                            factory.GetGetterIndexedEntryEventBeanArrayElement(
                                indexedProp.PropertyNameAtomic, indexedProp.Index, nestedGetter);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else
                    {
                        var asType = type as Type;
                        if (asType == null)
                        {
                            return null;
                        }

                        var componentType = asType.GetElementType();
                        var asGenericListType = asType.FindGenericInterface(typeof(IList<>));
                        if (asGenericListType != null)
                        {
                            componentType = asGenericListType.GetGenericArguments()[0];
                        }
                        else if (asType == typeof (string))
                        {
                            componentType = typeof (char);
                        }
                        else if (asType.IsArray)
                        {
                        }
                        else
                        {
                            return null;
                        }

                        var nestedEventType = eventAdapterService.AddBeanType(
                            componentType.Name, componentType, false, false, false);
                        var nestedGetter = (BeanEventPropertyGetter) nestedEventType.GetGetter(propertyNested);
                        if (nestedGetter == null)
                        {
                            return null;
                        }

                        var propertyTypeGetter = nestedEventType.GetPropertyType(propertyNested);
                        // construct getter for nested property
                        var indexGetter = factory.GetGetterIndexedEntryPONO(
                            indexedProp.PropertyNameAtomic, indexedProp.Index, nestedGetter, eventAdapterService,
                            propertyTypeGetter);
                        propertyGetterCache.Put(propertyName, indexGetter);
                        return indexGetter;
                    }
                }
                else if (property is MappedProperty)
                {
                    return null; // Since no type information is available for the property
                }
                else
                {
                    return null;
                }
            }

            // The map contains another map, we resolve the property dynamically
            if (ReferenceEquals(nestedType, typeof(IDictionary<string, object>)))
            {
                var prop = PropertyParser.ParseAndWalk(propertyNested);
                var getterNestedMap = prop.GetGetterMap(null, eventAdapterService);
                if (getterNestedMap == null)
                {
                    return null;
                }
                var mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
                propertyGetterCache.Put(propertyName, mapGetter);
                return mapGetter;
            }
            else if (nestedType is IDictionary<string, object>)
            {
                var prop = PropertyParser.ParseAndWalk(propertyNested);
                var nestedTypes = (IDictionary<string, object>)nestedType;
                var getterNestedMap = prop.GetGetterMap(nestedTypes, eventAdapterService);
                if (getterNestedMap == null)
                {
                    return null;
                }
                var mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
                propertyGetterCache.Put(propertyName, mapGetter);
                return mapGetter;
            }
            else if (nestedType is Type)
            {
                // ask the nested class to resolve the property
                var simpleClass = (Type) nestedType;
                if (simpleClass.IsArray)
                {
                    return null;
                }
                var nestedEventType = eventAdapterService.AddBeanType(
                    simpleClass.FullName, simpleClass, false, false, false);
                var nestedGetter =
                    (BeanEventPropertyGetter) nestedEventType.GetGetter(propertyNested);
                if (nestedGetter == null)
                {
                    return null;
                }
                Type propertyType;
                Type propertyComponentType;
                EventPropertyDescriptor desc = nestedEventType.GetPropertyDescriptor(propertyNested);
                if (desc == null)
                {
                    propertyType = nestedEventType.GetPropertyType(propertyNested);
                    propertyComponentType = propertyType.IsArray ? propertyType.GetElementType() : propertyType.GetGenericType(0);
                }
                else
                {
                    propertyType = desc.PropertyType;
                    propertyComponentType = desc.PropertyComponentType;
                }
                // construct getter for nested property
                EventPropertyGetter getter = factory.GetGetterNestedPONOProp(
                    propertyMap, nestedGetter, eventAdapterService, propertyType, propertyComponentType);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType)
            {
                // ask the nested class to resolve the property
                var innerType = (EventType) nestedType;
                var nestedGetter = innerType.GetGetter(propertyNested);
                if (nestedGetter == null)
                {
                    return null;
                }

                // construct getter for nested property
                var getter = factory.GetGetterNestedEventBean(propertyMap, nestedGetter);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType[])
            {
                var typeArray = (EventType[]) nestedType;
                var beanArrGetter = factory.GetGetterEventBeanArray(propertyMap, typeArray[0]);
                propertyGetterCache.Put(propertyName, beanArrGetter);
                return beanArrGetter;
            }
            else if (nestedType is String)
            {
                var nestedName = nestedType.ToString();
                var isArray = IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = GetPropertyRemoveArray(nestedName);
                }
                var innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is BaseNestableEventType))
                {
                    return null;
                }
                var innerGetter = innerType.GetGetter(propertyNested);
                if (innerGetter == null)
                {
                    return null;
                }
                EventPropertyGetter outerGetter;
                if (!isArray)
                {
                    outerGetter = factory.GetGetterNestedEntryBean(
                        propertyMap, innerGetter, innerType, eventAdapterService);
                }
                else
                {
                    outerGetter = factory.GetGetterNestedEntryBeanArray(
                        propertyMap, 0, innerGetter, innerType, eventAdapterService);
                }
                propertyGetterCache.Put(propertyName, outerGetter);
                return outerGetter;
            }
            else
            {
                var message = "Nestable type configuration encountered an unexpected value type of '"
                                 + nestedType.GetType() + " for property '" + propertyName +
                                 "', expected Class, typeof(Map) or Map<String, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        public static LinkedHashMap<String, Object> ValidateObjectArrayDef(
            String[] propertyNames,
            Object[] propertyTypes)
        {
            if (propertyNames.Length != propertyTypes.Length)
            {
                throw new ConfigurationException(
                    "Number of property names and property types do not match, found " + propertyNames.Length +
                    " property names and " +
                    propertyTypes.Length + " property types");
            }

            // validate property names for no-duplicates
            ICollection<String> propertyNamesSet = new HashSet<String>();
            var propertyTypesMap = new LinkedHashMap<String, Object>();
            for (var i = 0; i < propertyNames.Length; i++)
            {
                var propertyName = propertyNames[i];
                if (propertyNamesSet.Contains(propertyName))
                {
                    // duplicate prop check
                    throw new ConfigurationException(
                        "Property '" + propertyName + "' is listed twice in the type definition");
                }
                propertyNamesSet.Add(propertyName);
                propertyTypesMap.Put(propertyName, propertyTypes[i]);
            }
            return propertyTypesMap;
        }

        public static EventType CreateNonVariantType(
            bool isAnonymous,
            CreateSchemaDesc spec,
            Attribute[] annotations,
            ConfigurationInformation configSnapshot,
            EventAdapterService eventAdapterService,
            EngineImportService engineImportService)
        {
            if (spec.AssignedType == AssignedType.VARIANT)
            {
                throw new IllegalStateException("Variant type is not allowed in this context");
            }

            EventType eventType;
            if (spec.Types.IsEmpty())
            {
                var useMap = EventRepresentationUtil.IsMap(annotations, configSnapshot, spec.AssignedType);
                var typing = BuildType(
                    spec.Columns, eventAdapterService, spec.CopyFrom, engineImportService);
                var compiledTyping = CompileMapTypeProperties(typing, eventAdapterService);

                ConfigurationEventTypeWithSupertype config;
                if (useMap)
                {
                    config = new ConfigurationEventTypeMap();
                }
                else
                {
                    config = new ConfigurationEventTypeObjectArray();
                }
                if (spec.Inherits != null)
                {
                    config.SuperTypes.AddAll(spec.Inherits);
                }
                config.StartTimestampPropertyName = spec.StartTimestampProperty;
                config.EndTimestampPropertyName = spec.EndTimestampProperty;

                if (useMap)
                {
                    if (isAnonymous)
                    {
                        eventType = eventAdapterService.CreateAnonymousMapType(spec.SchemaName, compiledTyping, true);
                    }
                    else
                    {
                        eventType = eventAdapterService.AddNestableMapType(
                            spec.SchemaName, compiledTyping, (ConfigurationEventTypeMap) config, false, false, true,
                            false, false);
                    }
                }
                else
                {
                    if (isAnonymous)
                    {
                        eventType = eventAdapterService.CreateAnonymousObjectArrayType(spec.SchemaName, compiledTyping);
                    }
                    else
                    {
                        eventType = eventAdapterService.AddNestableObjectArrayType(
                            spec.SchemaName, compiledTyping, (ConfigurationEventTypeObjectArray) config, false, false,
                            true, false, false, false, null);
                    }
                }
            }
            else
            {
                // type definition
                if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty())
                {
                    throw new ExprValidationException("Copy-from types are not allowed with class-provided types");
                }
                if (spec.Types.Count != 1)
                {
                    throw new IllegalStateException("Multiple types provided");
                }
                String typeName = spec.Types.First();
                try
                {
                    // use the existing configuration, if any, possibly adding the start and end timestamps
                    ConfigurationEventTypeLegacy config = eventAdapterService.GetTypeLegacyConfigs(typeName);
                    if (spec.StartTimestampProperty != null || spec.EndTimestampProperty != null)
                    {
                        if (config == null)
                        {
                            config = new ConfigurationEventTypeLegacy();
                        }
                        config.StartTimestampPropertyName = spec.StartTimestampProperty;
                        config.EndTimestampPropertyName = spec.EndTimestampProperty;
                        eventAdapterService.TypeLegacyConfigs = Collections.SingletonMap(typeName, config);
                    }
                    if (isAnonymous)
                    {
                        String className = spec.Types.First();
                        Type clazz;
                        try
                        {
                            clazz = engineImportService.ResolveType(className, false);
                        }
                        catch (EngineImportException e)
                        {
                            throw new ExprValidationException(
                                "Failed to resolve class '" + className + "': " + e.Message, e);
                        }
                        eventType = eventAdapterService.CreateAnonymousBeanType(spec.SchemaName, clazz);
                    }
                    else
                    {
                        eventType = eventAdapterService.AddBeanType(
                            spec.SchemaName, spec.Types.First(), false, false, false, true);
                    }
                }
                catch (EventAdapterException ex)
                {
                    Type clazz;
                    try
                    {
                        clazz = engineImportService.ResolveType(typeName, false);
                        if (isAnonymous)
                        {
                            eventType = eventAdapterService.CreateAnonymousBeanType(spec.SchemaName, clazz);
                        }
                        else
                        {
                            eventType = eventAdapterService.AddBeanType(spec.SchemaName, clazz, false, false, true);
                        }
                    }
                    catch (EngineImportException e)
                    {
                        Log.Debug("Engine import failed to resolve event type '" + typeName + "'");
                        throw ex;
                    }
                }
            }
            return eventType;
        }

        public static WriteablePropertyDescriptor FindWritable(
            String propertyName,
            ICollection<WriteablePropertyDescriptor> writables)
        {
            foreach (var writable in writables)
            {
                if (writable.PropertyName.Equals(propertyName))
                {
                    return writable;
                }
            }
            return null;
        }

        public static TimestampPropertyDesc ValidatedDetermineTimestampProps(
            EventType type,
            String startProposed,
            String endProposed,
            EventType[] superTypes)
        {
            // determine start&end timestamp as inherited
            var startTimestampPropertyName = startProposed;
            var endTimestampPropertyName = endProposed;

            if (superTypes != null && superTypes.Length > 0)
            {
                foreach (var superType in superTypes)
                {
                    if (superType.StartTimestampPropertyName != null)
                    {
                        if (startTimestampPropertyName != null &&
                            !startTimestampPropertyName.Equals(superType.StartTimestampPropertyName))
                        {
                            throw GetExceptionTimestampInherited(
                                "start", startTimestampPropertyName, superType.StartTimestampPropertyName, superType);
                        }
                        startTimestampPropertyName = superType.StartTimestampPropertyName;
                    }
                    if (superType.EndTimestampPropertyName != null)
                    {
                        if (endTimestampPropertyName != null &&
                            !endTimestampPropertyName.Equals(superType.EndTimestampPropertyName))
                        {
                            throw GetExceptionTimestampInherited(
                                "end", endTimestampPropertyName, superType.EndTimestampPropertyName, superType);
                        }
                        endTimestampPropertyName = superType.EndTimestampPropertyName;
                    }
                }
            }

            ValidateTimestampProperties(type, startTimestampPropertyName, endTimestampPropertyName);
            return new TimestampPropertyDesc(startTimestampPropertyName, endTimestampPropertyName);
        }

        private static EPException GetExceptionTimestampInherited(
            String tstype,
            String firstName,
            String secondName,
            EventType superType)
        {
            var message = "Event type declares " + tstype + " timestamp as property '" + firstName +
                             "' however inherited event type '" + superType.Name +
                             "' declares " + tstype + " timestamp as property '" + secondName + "'";
            return new EPException(message);
        }

        public static bool IsTypeOrSubTypeOf(String typeName, EventType sameTypeOrSubtype)
        {
            if (sameTypeOrSubtype.Name == typeName)
            {
                return true;
            }

            if (sameTypeOrSubtype.SuperTypes == null)
            {
                return false;
            }

            foreach (EventType superType in sameTypeOrSubtype.SuperTypes)
            {
                if (superType.Name == typeName)
                {
                    return true;
                }
            }
            return false;
        }

        public class TimestampPropertyDesc
        {
            public TimestampPropertyDesc(String start, String end)
            {
                Start = start;
                End = end;
            }

            public string Start { get; private set; }
            public string End { get; private set; }
        }
    }
}
