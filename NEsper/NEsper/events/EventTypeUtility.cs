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

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    public class EventTypeUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static EventType RequireEventType(
            string aspectCamel,
            string aspectName,
            EventAdapterService eventAdapterService,
            string optionalEventTypeName)
        {
            if (optionalEventTypeName == null)
            {
                throw new ExprValidationException(
                    aspectCamel + " '" + aspectName +
                    "' returns EventBean-array but does not provide the event type name");
            }
            EventType eventType = eventAdapterService.GetEventTypeByName(optionalEventTypeName);
            if (eventType == null)
            {
                throw new ExprValidationException(
                    aspectCamel + " '" + aspectName + "' returns event type '" + optionalEventTypeName +
                    "' and the event type cannot be found");
            }
            return eventType;
        }

        public static Pair<EventType[], ICollection<EventType>> GetSuperTypesDepthFirst<T>(
            ConfigurationEventTypeWithSupertype optionalConfig,
            EventUnderlyingType representation,
            IDictionary<string, T> nameToTypeMap)
            where T : EventType
        {
            return GetSuperTypesDepthFirst(
                optionalConfig == null ? null : optionalConfig.SuperTypes, representation, nameToTypeMap);
        }

        public static Pair<EventType[], ICollection<EventType>> GetSuperTypesDepthFirst<T>(
            ICollection<string> superTypesSet,
            EventUnderlyingType representation,
            IDictionary<string, T> nameToTypeMap)
            where T : EventType
        {
            if (superTypesSet == null || superTypesSet.IsEmpty())
            {
                return new Pair<EventType[], ICollection<EventType>>(null, null);
            }

            var superTypes = new EventType[superTypesSet.Count];
            var deepSuperTypes = new LinkedHashSet<EventType>();

            int count = 0;
            foreach (string superName in superTypesSet)
            {
                EventType type = nameToTypeMap.Get(superName);
                if (type == null)
                {
                    throw new EventAdapterException("Supertype by name '" + superName + "' could not be found");
                }
                if (representation == EventUnderlyingType.MAP)
                {
                    if (!(type is MapEventType))
                    {
                        throw new EventAdapterException(
                            "Supertype by name '" + superName +
                            "' is not a Map, expected a Map event type as a supertype");
                    }
                }
                else if (representation == EventUnderlyingType.OBJECTARRAY)
                {
                    if (!(type is ObjectArrayEventType))
                    {
                        throw new EventAdapterException(
                            "Supertype by name '" + superName +
                            "' is not an Object-array type, expected a Object-array event type as a supertype");
                    }
                }
                else if (representation == EventUnderlyingType.AVRO)
                {
                    if (!(type is AvroSchemaEventType))
                    {
                        throw new EventAdapterException(
                            "Supertype by name '" + superName +
                            "' is not an Avro type, expected a Avro event type as a supertype");
                    }
                }
                else
                {
                    throw new IllegalStateException("Unrecognized enum " + representation);
                }
                superTypes[count++] = type;
                deepSuperTypes.Add(type);
                AddRecursiveSupertypes(deepSuperTypes, type);
            }

            var superTypesListDepthFirst = new List<EventType>(deepSuperTypes);
            superTypesListDepthFirst.Reverse();

            return new Pair<EventType[], ICollection<EventType>>(superTypes, new LinkedHashSet<EventType>(superTypesListDepthFirst));
        }

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(EventType target, string propertyName)
        {
            EventPropertyDescriptor descriptor = target.GetPropertyDescriptor(propertyName);
            if (descriptor != null)
            {
                return descriptor;
            }
            int index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                return null;
            }
            // parse, can be an nested property
            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is PropertyBase)
            {
                return target.GetPropertyDescriptor(((PropertyBase)property).PropertyNameAtomic);
            }
            if (!(property is NestedProperty))
            {
                return null;
            }
            var nested = (NestedProperty) property;
            var properties = new ArrayDeque<Property>(nested.Properties);
            return GetNestablePropertyDescriptor(target, properties);
        }

        public static EventPropertyDescriptor GetNestablePropertyDescriptor(EventType target, Deque<Property> stack)
        {
            Property topProperty = stack.RemoveFirst();
            if (stack.IsEmpty())
            {
                return target.GetPropertyDescriptor(((PropertyBase)topProperty).PropertyNameAtomic);
            }

            if (!(topProperty is SimpleProperty))
            {
                return null;
            }
            var simple = (SimpleProperty) topProperty;

            FragmentEventType fragmentEventType = target.GetFragmentType(simple.PropertyNameAtomic);
            if (fragmentEventType == null || fragmentEventType.FragmentType == null)
            {
                return null;
            }
            return GetNestablePropertyDescriptor(fragmentEventType.FragmentType, stack);
        }

        public static LinkedHashMap<string, Object> BuildType(
            IList<ColumnDesc> columns,
            EventAdapterService eventAdapterService,
            ICollection<string> copyFrom,
            EngineImportService engineImportService)
        {
            var typing = new LinkedHashMap<string, Object>();
            var columnNames = new HashSet<string>();
            foreach (ColumnDesc column in columns)
            {
                bool added = columnNames.Add(column.Name);
                if (!added)
                {
                    throw new ExprValidationException("Duplicate column name '" + column.Name + "'");
                }
                object columnType = BuildType(column, engineImportService);
                typing.Put(column.Name, columnType);
            }

            if (copyFrom != null && !copyFrom.IsEmpty())
            {
                foreach (string copyFromName in copyFrom)
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
                Type primitive = TypeHelper.GetPrimitiveTypeForName(column.Type);
                if (primitive != null)
                {
                    return Array.CreateInstance(primitive, 0).GetType();
                }
                throw new ExprValidationException("Type '" + column.Type + "' is not a primitive type");
            }

            Type plain = TypeHelper.GetTypeForSimpleName(column.Type);
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
            catch (EngineImportException)
            {
                // expected
            }

            // resolve from classpath when not found
            if (resolved == null)
            {
                try
                {
                    resolved = TypeHelper.GetClassForName(column.Type, engineImportService.GetClassForNameProvider());
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

        private static void MergeType(IDictionary<string, Object> typing, EventType typeToMerge)
        {
            foreach (EventPropertyDescriptor prop in typeToMerge.PropertyDescriptors)
            {
                object existing = typing.Get(prop.PropertyName);

                if (!prop.IsFragment)
                {
                    Type assigned = prop.PropertyType;
                    Type existingType = existing as Type;
                    if (existing != null)
                    {
                        if (existingType.GetBoxedType() != assigned.GetBoxedType())
                        {
                            throw new ExprValidationException(
                                string.Format(
                                    "Type by name '{0}' contributes property '{1}' defined as '{2}' which overides the same property of type '{3}'",
                                    typeToMerge.Name, prop.PropertyName,
                                    assigned.GetCleanName(),
                                    existingType.GetCleanName()));
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

                    FragmentEventType fragment = typeToMerge.GetFragmentType(prop.PropertyName);
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
            string startTimestampProperty,
            string endTimestampProperty)
        {
            if (startTimestampProperty != null)
            {
                if (eventType.GetGetter(startTimestampProperty) == null)
                {
                    throw new ConfigurationException(
                        "Declared start timestamp property name '" + startTimestampProperty + "' was not found");
                }
                Type type = eventType.GetPropertyType(startTimestampProperty);
                if (!TypeHelper.IsDateTime(type))
                {
                    throw new ConfigurationException(
                        "Declared start timestamp property '" + startTimestampProperty +
                        "' is expected to return a DateTime, DateTimeEx or long-typed value but returns '" + Name.Clean(type) + "'");
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
                Type type = eventType.GetPropertyType(endTimestampProperty);
                if (!TypeHelper.IsDateTime(type))
                {
                    throw new ConfigurationException(
                        "Declared end timestamp property '" + endTimestampProperty +
                        "' is expected to return a DateTime, DateTimeEx or long-typed value but returns '" + Name.Clean(type) + "'");
                }
                Type startType = eventType.GetPropertyType(startTimestampProperty);
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
                return candidate.DeepSuperTypes
                    .Any(eventType => Equals(eventType, superType));
            }
            return false;
        }

        /// <summary>
        ///     Determine among the Map-type properties which properties are Bean-type event type names,
        ///     rewrites these as Type-type instead so that they are configured as native property and do not require wrapping,
        ///     but may require unwrapping.
        /// </summary>
        /// <param name="typing">properties of map type</param>
        /// <param name="eventAdapterService">event adapter service</param>
        /// <returns>
        ///     compiled properties, same as original unless Bean-type event type names were specified.
        /// </returns>
        public static IDictionary<string, Object> CompileMapTypeProperties(
            IDictionary<string, Object> typing,
            EventAdapterService eventAdapterService)
        {
            var compiled = new LinkedHashMap<string, Object>(typing);
            foreach (var specifiedEntry in typing)
            {
                var typeSpec = specifiedEntry.Value;
                var nameSpec = specifiedEntry.Key;
                if (!(typeSpec is string))
                {
                    continue;
                }

                var typeNameSpec = (string) typeSpec;
                var isArray = IsPropertyArray(typeNameSpec);
                if (isArray)
                {
                    typeNameSpec = GetPropertyRemoveArray(typeNameSpec);
                }

                EventType eventType = eventAdapterService.GetEventTypeByName(typeNameSpec);
                if (!(eventType is BeanEventType))
                {
                    continue;
                }

                var beanEventType = (BeanEventType) eventType;
                Type underlyingType = beanEventType.UnderlyingType;
                if (isArray)
                {
                    underlyingType = TypeHelper.GetArrayType(underlyingType);
                }
                compiled.Put(nameSpec, underlyingType);
            }
            return compiled;
        }

        /// <summary>
        ///     Returns true if the name indicates that the type is an array type.
        /// </summary>
        /// <param name="name">the property name</param>
        /// <returns>true if array type</returns>
        public static bool IsPropertyArray(string name)
        {
            return name.Trim().EndsWith("[]");
        }

        public static bool IsTypeOrSubTypeOf(string typeName, EventType sameTypeOrSubtype)
        {
            if (sameTypeOrSubtype.Name.Equals(typeName))
            {
                return true;
            }
            if (sameTypeOrSubtype.SuperTypes == null)
            {
                return false;
            }
            
            return sameTypeOrSubtype.SuperTypes.Any(superType => superType.Name == typeName);
        }

        /// <summary>
        ///     Returns the property name without the array type extension, if present.
        /// </summary>
        /// <param name="name">property name</param>
        /// <returns>property name with removed array extension name</returns>
        public static string GetPropertyRemoveArray(string name)
        {
            return name
                .RegexReplaceAll("\\[", "")
                .RegexReplaceAll("\\]", "");
        }

        public static PropertySetDescriptor GetNestableProperties(
            IDictionary<string, Object> propertiesToAdd,
            EventAdapterService eventAdapterService,
            EventTypeNestableGetterFactory factory,
            EventType[] optionalSuperTypes)
        {
            var propertyNameList = new List<string>();
            var propertyDescriptors = new List<EventPropertyDescriptor>();
            var nestableTypes = new LinkedHashMap<string, Object>();
            var propertyItems = new Dictionary<string, PropertySetDescriptorItem>();

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
                    throw new EPException("Invalid type configuration: property name is not a string-type value");
                }

                // handle types that are string values
                var entryValue = entry.Value;
                if (entryValue is string)
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
                    var classType = (Type) entryValue;

                    var componentType = classType.GetIndexType();
                    var isIndexed = componentType != null;

                    var isMapped = !isIndexed && classType.IsGenericStringDictionary();
                    if (isMapped)
                    {
                        componentType = classType.GetGenericArguments()[1];
                    }
                    
                    var isFragment = classType.IsFragmentableType();
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
                    var getter = factory.GetGetterProperty(name, nativeFragmentType, eventAdapterService);
                    var descriptor = new EventPropertyDescriptor(name, classType, componentType, false, false, isIndexed, isMapped, isFragment);
                    var item = new PropertySetDescriptorItem(descriptor, classType, getter, fragmentType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                // A null-type is also allowed
                if (entryValue == null)
                {
                    var getter = factory.GetGetterProperty(name, null, null);
                    var descriptor = new EventPropertyDescriptor(name, null, null, false, false, false, false, false);
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
                    var descriptor = new EventPropertyDescriptor(name, typeof(IDictionary<string, object>), null, false, false, false, true, false);
                    var item = new PropertySetDescriptorItem(descriptor, typeof(IDictionary<string, object>), getter, null);
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
                        descriptor, eventType.UnderlyingType, getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entryValue is EventType[])
                {
                    // Add EventType array itself as a property, type is expected to be first array element
                    var eventType = ((EventType[]) entryValue)[0];
                    var prototypeArray = Array.CreateInstance(eventType.UnderlyingType, 0);
                    var getter = factory.GetGetterEventBeanArray(name, eventType);
                    var descriptor = new EventPropertyDescriptor(
                        name, prototypeArray.GetType(), eventType.UnderlyingType, false, false, true, false, true);
                    var fragmentEventType = new FragmentEventType(eventType, true, false);
                    var item = new PropertySetDescriptorItem(
                        descriptor, prototypeArray.GetType(), getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                if (entryValue is string)
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

                    Type underlyingType = eventType.UnderlyingType;
                    Type propertyComponentType = null;
                    if (isArray)
                    {
                        propertyComponentType = underlyingType;
                        if (underlyingType != typeof (Object[]))
                        {
                            //underlyingType = underlyingType.GetElementType();
                            underlyingType = Array.CreateInstance(underlyingType, 0).GetType();
                        }
                    }

                    EventPropertyGetterSPI getter;
                    if (!isArray)
                    {
                        getter = factory.GetGetterBeanNested(name, eventType, eventAdapterService);
                    }
                    else
                    {
                        getter = factory.GetGetterBeanNestedArray(name, eventType, eventAdapterService);
                    }

                    var descriptor = new EventPropertyDescriptor(
                        name, underlyingType, propertyComponentType, false, false, isArray, false, true);
                    var fragmentEventType = new FragmentEventType(eventType, isArray, false);
                    var item = new PropertySetDescriptorItem(descriptor, underlyingType, getter, fragmentEventType);
                    propertyNameList.Add(name);
                    propertyDescriptors.Add(descriptor);
                    propertyItems.Put(name, item);
                    continue;
                }

                GenerateExceptionNestedProp(name, entryValue);
            }

            return new PropertySetDescriptor(propertyNameList, propertyDescriptors, propertyItems, nestableTypes);
        }

        private static void GenerateExceptionNestedProp(string name, Object value)
        {
            string clazzName = (value == null) ? "null" : value.GetType().Name;
            throw new EPException(
                "Nestable type configuration encountered an unexpected property type of '"
                + clazzName + "' for property '" + name +
                "', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
        }

        public static Type GetNestablePropertyType(
            string propertyName,
            IDictionary<string, PropertySetDescriptorItem> simplePropertyTypes,
            IDictionary<string, Object> nestableTypes,
            EventAdapterService eventAdapterService)
        {
            PropertySetDescriptorItem item = simplePropertyTypes.Get(ASTUtil.UnescapeDot(propertyName));
            if (item != null)
            {
                return item.SimplePropertyType;
            }

            // see if this is a nested property
            int index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                // dynamic simple property
                if (propertyName.EndsWith("?"))
                {
                    return typeof (Object);
                }

                // parse, can be an indexed property
                Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);

                if (property is SimpleProperty)
                {
                    PropertySetDescriptorItem propitem = simplePropertyTypes.Get(propertyName);
                    if (propitem != null)
                    {
                        return propitem.SimplePropertyType;
                    }
                    return null;
                }

                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    object type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    else if (type is EventType[])
                    {
                        return ((EventType[]) type)[0].UnderlyingType;
                    }
                    else if (type is string)
                    {
                        var propTypeName = type.ToString();
                        var propTypeNameIsArray = IsPropertyArray(propTypeName);
                        if (propTypeNameIsArray)
                        {
                            propTypeName = GetPropertyRemoveArray(propTypeName);
                            var innerTypeX = eventAdapterService.GetEventTypeByName(propTypeName);
                            return innerTypeX == null ? null : innerTypeX.UnderlyingType;
                        }

                        var innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                        if (innerType == null)
                        {
                            var propType = eventAdapterService.EngineImportService.ResolveType(propTypeName, false);
                            return propType != null ? propType.GetIndexType() : null;
                        }
                        return innerType == null ? null : innerType.UnderlyingType;
                    }
                    if (!(type is Type))
                    {
                        return null;
                    }

                    var asType = (Type) type;
                    return asType.GetIndexType();
                }
                else if (property is MappedProperty)
                {
                    var mappedProp = (MappedProperty) property;
                    var type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    if (type is Type)
                    {
                        var trueType = (Type)type;
                        if (trueType.IsGenericStringDictionary())
                        {
                            return trueType.GetGenericArguments()[1];
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
            string propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            string propertyNested = propertyName.Between(index + 1, propertyName.Length);
            bool isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?"))
            {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            object nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null)
            {
                // parse, can be an indexed property
                Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    object type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    // handle map-in-map case
                    if (type is string)
                    {
                        string propTypeName = type.ToString();
                        bool isArray = IsPropertyArray(propTypeName);
                        if (isArray)
                        {
                            propTypeName = GetPropertyRemoveArray(propTypeName);
                        }
                        EventType innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        return innerType.GetPropertyType(propertyNested);
                    }
                    else if (type is EventType[])
                    {
                        // handle eventtype[] in map
                        EventType innerType = ((EventType[]) type)[0];
                        return innerType.GetPropertyType(propertyNested);
                    }
                    else
                    {
                        // handle array class in map case
                        if (!(type is Type))
                        {
                            return null;
                        }
                        if (!((Type) type).IsArray)
                        {
                            return null;
                        }
                        Type componentType = ((Type) type).GetElementType();
                        EventType nestedEventType = eventAdapterService.AddBeanType(
                            componentType.FullName, componentType, false, false, false);
                        return nestedEventType.GetPropertyType(propertyNested);
                    }
                }
                else if (property is MappedProperty)
                {
                    return null; // Since no type information is available for the property
                }
                else
                {
                    return isRootedDynamic ? typeof (Object) : null;
                }
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if (Equals(nestedType, typeof (IDictionary<string, object>)))
            {
                Property prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                return isRootedDynamic ? typeof (Object) : prop.GetPropertyTypeMap(null, eventAdapterService);
                    // we don't have a definition of the nested props
            }
            else if (nestedType is IDictionary<string, object>)
            {
                Property prop = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                var nestedTypes = (IDictionary<string, object>)nestedType;
                return isRootedDynamic ? typeof (Object) : prop.GetPropertyTypeMap(nestedTypes, eventAdapterService);
            }
            else if (nestedType is Type)
            {
                var simpleClass = (Type) nestedType;
                if (simpleClass.IsBuiltinDataType())
                {
                    return null;
                }
                if (simpleClass.IsArray &&
                    (simpleClass.GetElementType().IsBuiltinDataType() ||
                     simpleClass.GetElementType() == typeof (Object)))
                {
                    return null;
                }
                EventType nestedEventType = eventAdapterService.AddBeanType(
                    simpleClass.FullName, simpleClass, false, false, false);
                return isRootedDynamic ? typeof (Object) : nestedEventType.GetPropertyType(propertyNested);
            }
            else if (nestedType is EventType)
            {
                var innerType = (EventType) nestedType;
                return isRootedDynamic ? typeof (Object) : innerType.GetPropertyType(propertyNested);
            }
            else if (nestedType is EventType[])
            {
                return null; // requires indexed property
            }
            else if (nestedType is string)
            {
                string nestedName = nestedType.ToString();
                bool isArray = IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = GetPropertyRemoveArray(nestedName);
                }
                EventType innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is BaseNestableEventType))
                {
                    return null;
                }
                return isRootedDynamic ? typeof (Object) : innerType.GetPropertyType(propertyNested);
            }
            else
            {
                string message = "Nestable map type configuration encountered an unexpected value type of '"
                                 + nestedType.GetType().FullName + " for property '" + propertyName +
                                 "', expected Type, typeof(Map) or IDictionary<string, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        public static EventPropertyGetterSPI GetNestableGetter(
            string propertyName,
            IDictionary<string, PropertySetDescriptorItem> propertyGetters,
            IDictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            IDictionary<string, Object> nestableTypes,
            EventAdapterService eventAdapterService,
            EventTypeNestableGetterFactory factory,
            bool isObjectArray)
        {
            var cachedGetter = propertyGetterCache.Get(propertyName);
            if (cachedGetter != null)
            {
                return cachedGetter;
            }

            string unescapePropName = ASTUtil.UnescapeDot(propertyName);
            PropertySetDescriptorItem item = propertyGetters.Get(unescapePropName);
            if (item != null)
            {
                EventPropertyGetterSPI getter = item.PropertyGetter;
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }

            // see if this is a nested property
            int index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (prop is DynamicProperty)
                {
                    EventPropertyGetterSPI getterDyn = factory.GetPropertyProvidedGetter(
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
                        EventPropertyGetterSPI getterArr = factory.GetGetterIndexedEventBean(
                            indexedProp.PropertyNameAtomic, indexedProp.Index);
                        propertyGetterCache.Put(propertyName, getterArr);
                        return getterArr;
                    }
                    else if (type is string)
                    {
                        EventType innerType;

                        var propTypeName = type.ToString();
                        var propTypeNameIsArray = IsPropertyArray(propTypeName);
                        if (propTypeNameIsArray)
                        {
                            propTypeName = GetPropertyRemoveArray(propTypeName);
                            innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                        }
                        else
                        {
                            innerType = eventAdapterService.GetEventTypeByName(propTypeName);
                            if (innerType == null)
                            {
                                var propType = eventAdapterService.EngineImportService.ResolveType(propTypeName, false);
                                if (propType != null)
                                {
                                    var indexType = propType.GetIndexType();
                                    var indexGetter = factory.GetGetterIndexedPono(indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, indexType);
                                    propertyGetterCache.Put(propertyName, indexGetter);
                                    return indexGetter;
                                }
                            }
                        }

                        if (innerType is BaseNestableEventType)
                        {
                            EventPropertyGetterSPI typeGetter;
                            if (!propTypeNameIsArray)
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

                        return null;
                    }

                    // handle map type name in map
                    var asType = (Type) type;
                    if (asType == null)
                    {
                        return null;
                    }

                    // its a collection
                    var componentType = asType.GetIndexType();
                    if (componentType == null)
                    {
                        return null;
                    }

                    var indexedGetter = factory.GetGetterIndexedPono(
                        indexedProp.PropertyNameAtomic, indexedProp.Index, eventAdapterService, componentType);
                    propertyGetterCache.Put(propertyName, indexedGetter);
                    return indexedGetter;
                }
                else if (prop is MappedProperty)
                {
                    var mappedProp = (MappedProperty) prop;
                    object type = nestableTypes.Get(mappedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    if (type is Type)
                    {
                        if (((Type) type).IsGenericStringDictionary())
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
            string propertyMap = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            string propertyNested = propertyName.Between(index + 1, propertyName.Length);
            bool isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyMap.EndsWith("?"))
            {
                propertyMap = propertyMap.Substring(0, propertyMap.Length - 1);
                isRootedDynamic = true;
            }

            object nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null)
            {
                // parse, can be an indexed property
                Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) property;
                    object type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    if (type is string)
                    {
                        string nestedTypeName = type.ToString();
                        bool isArray = IsPropertyArray(nestedTypeName);
                        if (isArray)
                        {
                            nestedTypeName = GetPropertyRemoveArray(nestedTypeName);
                        }
                        EventTypeSPI innerType = (EventTypeSPI) eventAdapterService.GetEventTypeByName(nestedTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        EventPropertyGetterSPI typeGetter;
                        if (!isArray)
                        {
                            typeGetter = factory.GetGetterNestedEntryBean(
                                propertyMap, innerType.GetGetter(propertyNested), innerType, eventAdapterService);
                        }
                        else
                        {
                            EventPropertyGetterSPI innerGetter = innerType.GetGetterSPI(propertyNested);
                            if (innerGetter == null)
                            {
                                return null;
                            }
                            typeGetter = factory.GetGetterNestedEntryBeanArray(
                                indexedProp.PropertyNameAtomic, indexedProp.Index, innerGetter, innerType,
                                eventAdapterService);
                        }
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else if (type is EventType[])
                    {
                        EventTypeSPI componentType = (EventTypeSPI) ((EventType[]) type)[0];
                        EventPropertyGetterSPI nestedGetter = componentType.GetGetterSPI(propertyNested);
                        if (nestedGetter == null)
                        {
                            return null;
                        }
                        EventPropertyGetterSPI typeGetter =
                            factory.GetGetterIndexedEntryEventBeanArrayElement(
                                indexedProp.PropertyNameAtomic, indexedProp.Index, nestedGetter);
                        propertyGetterCache.Put(propertyName, typeGetter);
                        return typeGetter;
                    }
                    else
                    {
                        if (!(type is Type))
                        {
                            return null;
                        }
                        if (!((Type) type).IsArray)
                        {
                            return null;
                        }
                        Type componentType = ((Type) type).GetElementType();
                        EventTypeSPI nestedEventType = (EventTypeSPI) eventAdapterService.AddBeanType(
                            componentType.FullName, componentType, false, false, false);

                        var nestedGetter = (BeanEventPropertyGetter) nestedEventType.GetGetterSPI(propertyNested);
                        if (nestedGetter == null)
                        {
                            return null;
                        }
                        Type propertyTypeGetter = nestedEventType.GetPropertyType(propertyNested);
                        // construct getter for nested property
                        EventPropertyGetterSPI indexGetter =
                            factory.GetGetterIndexedEntryPono(
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
                    if (isRootedDynamic)
                    {
                        Property prop = PropertyParser.ParseAndWalk(propertyNested, true);
                        if (!isObjectArray)
                        {
                            EventPropertyGetterSPI getterNested = prop.GetGetterMap(null, eventAdapterService);
                            EventPropertyGetterSPI dynamicGetter =
                                factory.GetGetterNestedPropertyProvidedGetterDynamic(
                                    nestableTypes, propertyMap, getterNested, eventAdapterService);
                            propertyGetterCache.Put(propertyName, dynamicGetter);
                            return dynamicGetter;
                        }
                        return null;
                    }
                    return null;
                }
            }

            // The map contains another map, we resolve the property dynamically
            if (Equals(nestedType, typeof (IDictionary<string, object>)))
            {
                Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyNested);
                MapEventPropertyGetter getterNestedMap = prop.GetGetterMap(null, eventAdapterService);
                if (getterNestedMap == null)
                {
                    return null;
                }
                EventPropertyGetterSPI mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
                propertyGetterCache.Put(propertyName, mapGetter);
                return mapGetter;
            }
            else if (nestedType is IDictionary<string, object>)
            {
                Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyNested);
                var nestedTypes = (IDictionary<string, object>) nestedType;
                MapEventPropertyGetter getterNestedMap = prop.GetGetterMap(nestedTypes, eventAdapterService);
                if (getterNestedMap == null)
                {
                    return null;
                }
                EventPropertyGetterSPI mapGetter = factory.GetGetterNestedMapProp(propertyMap, getterNestedMap);
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
                EventTypeSPI nestedEventType = (EventTypeSPI) eventAdapterService.AddBeanType(
                    simpleClass.FullName, simpleClass, false, false, false);
                var nestedGetter = (BeanEventPropertyGetter) nestedEventType.GetGetter(propertyNested);
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
                    propertyComponentType = propertyType.IsArray
                        ? propertyType.GetElementType()
                        : propertyType.GetGenericType(0);
                }
                else
                {
                    propertyType = desc.PropertyType;
                    propertyComponentType = desc.PropertyComponentType;
                }

                // construct getter for nested property
                EventPropertyGetterSPI getter = factory.GetGetterNestedPonoProp(
                    propertyMap, nestedGetter, eventAdapterService, propertyType, propertyComponentType);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType)
            {
                // ask the nested class to resolve the property
                var innerType = (EventTypeSPI) nestedType;
                EventPropertyGetterSPI nestedGetter = innerType.GetGetterSPI(propertyNested);
                if (nestedGetter == null)
                {
                    return null;
                }

                // construct getter for nested property
                EventPropertyGetterSPI getter = factory.GetGetterNestedEventBean(propertyMap, nestedGetter);
                propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            else if (nestedType is EventType[])
            {
                var typeArray = (EventType[]) nestedType;
                EventPropertyGetterSPI beanArrGetter = factory.GetGetterEventBeanArray(propertyMap, typeArray[0]);
                propertyGetterCache.Put(propertyName, beanArrGetter);
                return beanArrGetter;
            }
            else if (nestedType is string)
            {
                string nestedName = nestedType.ToString();
                bool isArray = IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = GetPropertyRemoveArray(nestedName);
                }
                EventType innerType = eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is BaseNestableEventType))
                {
                    return null;
                }

                EventPropertyGetterSPI innerGetter = ((EventTypeSPI)innerType).GetGetterSPI(propertyNested);
                if (innerGetter == null)
                {
                    return null;
                }
                EventPropertyGetterSPI outerGetter;
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
                string message = "Nestable type configuration encountered an unexpected value type of '"
                                 + nestedType.GetType() + " for property '" + propertyName +
                                 "', expected Type, typeof(Map) or IDictionary<string, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        public static IDictionary<string, Object> ValidateObjectArrayDef(
            string[] propertyNames,
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
            var propertyNamesSet = new HashSet<string>();
            var propertyTypesMap = new Dictionary<string, object>();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                string propertyName = propertyNames[i];
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
                var representation = EventRepresentationUtil.GetRepresentation(
                    annotations, configSnapshot, spec.AssignedType);
                IDictionary<string, Object> typing = BuildType(
                    spec.Columns, eventAdapterService, spec.CopyFrom, engineImportService);
                IDictionary<string, object> compiledTyping = CompileMapTypeProperties(typing, eventAdapterService);

                ConfigurationEventTypeWithSupertype config;
                if (representation == EventUnderlyingType.MAP)
                {
                    config = new ConfigurationEventTypeMap();
                }
                else if (representation == EventUnderlyingType.OBJECTARRAY)
                {
                    config = new ConfigurationEventTypeObjectArray();
                }
                else if (representation == EventUnderlyingType.AVRO)
                {
                    config = new ConfigurationEventTypeAvro();
                }
                else
                {
                    throw new IllegalStateException("Unrecognized representation '" + representation + "'");
                }

                if (spec.Inherits != null)
                {
                    config.SuperTypes.AddAll(spec.Inherits);
                }
                config.StartTimestampPropertyName = spec.StartTimestampProperty;
                config.EndTimestampPropertyName = spec.EndTimestampProperty;

                if (representation == EventUnderlyingType.MAP)
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
                else if (representation == EventUnderlyingType.OBJECTARRAY)
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
                else if (representation == EventUnderlyingType.AVRO)
                {
                    if (isAnonymous)
                    {
                        eventType = eventAdapterService.CreateAnonymousAvroType(
                            spec.SchemaName, compiledTyping, annotations, null, null);
                    }
                    else
                    {
                        eventType = eventAdapterService.AddAvroType(
                            spec.SchemaName, compiledTyping, false, false, true, false, false, annotations,
                            (ConfigurationEventTypeAvro) config, null, null);
                    }
                }
                else
                {
                    throw new IllegalStateException("Unrecognized representation " + representation);
                }
            }
            else
            {
                // object type definition
                if (spec.CopyFrom != null && !spec.CopyFrom.IsEmpty())
                {
                    throw new ExprValidationException("Copy-from types are not allowed with class-provided types");
                }
                if (spec.Types.Count != 1)
                {
                    throw new IllegalStateException("Multiple types provided");
                }
                string typeName = TypeHelper.TryResolveAbsoluteTypeName(spec.Types.First());
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
                        string className = spec.Types.First();
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
                    catch (EngineImportException)
                    {
                        Log.Debug("Engine import failed to resolve event type '" + typeName + "'");
                        throw ex;
                    }
                }
            }
            return eventType;
        }

        public static WriteablePropertyDescriptor FindWritable(
            string propertyName,
            IEnumerable<WriteablePropertyDescriptor> writables)
        {
            foreach (WriteablePropertyDescriptor writable in writables)
            {
                if (writable.PropertyName == propertyName)
                {
                    return writable;
                }
            }
            return null;
        }

        public static TimestampPropertyDesc ValidatedDetermineTimestampProps(
            EventType type,
            string startProposed,
            string endProposed,
            EventType[] superTypes)
        {
            // determine start&end timestamp as inherited
            string startTimestampPropertyName = startProposed;
            string endTimestampPropertyName = endProposed;

            if (superTypes != null && superTypes.Length > 0)
            {
                foreach (EventType superType in superTypes)
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
            string tstype,
            string firstName,
            string secondName,
            EventType superType)
        {
            string message = "Event type declares " + tstype + " timestamp as property '" + firstName +
                             "' however inherited event type '" + superType.Name +
                             "' declares " + tstype + " timestamp as property '" + secondName + "'";
            return new EPException(message);
        }

        private static void AddRecursiveSupertypes(ISet<EventType> superTypes, EventType child)
        {
            if (child.SuperTypes != null)
            {
                for (int i = 0; i < child.SuperTypes.Length; i++)
                {
                    superTypes.Add(child.SuperTypes[i]);
                    AddRecursiveSupertypes(superTypes, child.SuperTypes[i]);
                }
            }
        }

        public static string DisallowedAtTypeMessage()
        {
            return "The @type annotation is only allowed when the invocation target returns EventBean instances";
        }

        public class TimestampPropertyDesc
        {
            public TimestampPropertyDesc(string start, string end)
            {
                Start = start;
                End = end;
            }

            public string Start { get; private set; }

            public string End { get; private set; }
        }
    }
} // end of namespace