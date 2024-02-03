///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.core.EventTypeUtility;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Implementation of the <seealso cref="com.espertech.esper.common.client.EventType" /> interface for handling name
    ///     value pairs.
    /// </summary>
    public abstract class BaseNestableEventType : EventTypeSPI
    {
        private readonly bool _publicFields;
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly EventTypeNestableGetterFactory _getterFactory;
        private readonly ISet<EventType> _optionalDeepSupertypes;
        private readonly EventType[] _optionalSuperTypes;

        private readonly IDictionary<string, PropertySetDescriptorItem> _propertyItems;
        private readonly string _endTimestampPropertyName;
        private EventTypeMetadata _metadata;

        // Nestable definition of Map contents is here
        private readonly IDictionary<string, object>
            _nestableTypes; // Deep definition of the map-type, containing nested maps and objects

        private readonly EventPropertyDescriptor[] _propertyDescriptors;

        private IDictionary<string, EventPropertyGetterSPI>
            _propertyGetterCache; // Mapping of all property names and getters

        // Simple (not-nested) properties are stored here
        private readonly string[] _propertyNames; // Cache an array of property names so not to construct one frequently

        private readonly string _startTimestampPropertyName;

        /// <summary>
        ///     Constructor takes a type name, map of property names and types, for
        ///     use with nestable Map events.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="propertyTypes">is pairs of property name and type</param>
        /// <param name="optionalSuperTypes">the supertypes to this type if any, or null if there are no supertypes</param>
        /// <param name="optionalDeepSupertypes">the deep supertypes to this type if any, or null if there are no deep supertypes</param>
        /// <param name="startTimestampPropertyName">start timestamp</param>
        /// <param name="endTimestampPropertyName">end timestamp</param>
        /// <param name="getterFactory">getter factory</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <param name="publicFields">true if the properties that are classes are public field default access</param>
        public BaseNestableEventType(
            EventTypeMetadata metadata,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSupertypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            EventTypeNestableGetterFactory getterFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            bool publicFields)
        {
            _metadata = metadata;
            _getterFactory = getterFactory;
            _beanEventTypeFactory = beanEventTypeFactory;
            _publicFields = publicFields;
            _startTimestampPropertyName = startTimestampPropertyName;
            _endTimestampPropertyName = endTimestampPropertyName;

            ValidateMapPropertyTypes(propertyTypes);

            _optionalSuperTypes = optionalSuperTypes;
            _optionalDeepSupertypes = optionalDeepSupertypes ?? EmptySet<EventType>.Instance;

            // determine property set and prepare getters
            var propertySet = GetNestableProperties(
                propertyTypes,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                getterFactory,
                optionalSuperTypes,
                beanEventTypeFactory,
                publicFields);

            _nestableTypes = propertySet.NestableTypes;
            _propertyNames = propertySet.PropertyNameArray;
            _propertyItems = propertySet.PropertyItems;
            _propertyDescriptors = propertySet.PropertyDescriptors.ToArray();

            var desc = ValidatedDetermineTimestampProps(
                this,
                startTimestampPropertyName,
                endTimestampPropertyName,
                optionalSuperTypes);
            _startTimestampPropertyName = desc.Start;
            _endTimestampPropertyName = desc.End;
        }

        /// <summary>
        ///     Returns the name-type map of map properties, each value in the map
        ///     can be a Class or a Map&lt;String, Object&gt; (for nested maps).
        /// </summary>
        /// <value>is the property name and types</value>
        public IDictionary<string, object> Types => _nestableTypes;

        public BeanEventTypeFactory BeanEventTypeFactory => _beanEventTypeFactory;

        public IEnumerable<EventType> DeepSuperTypes => _optionalDeepSupertypes;

        public virtual ICollection<EventType> DeepSuperTypesCollection => _optionalDeepSupertypes;
        

        public string Name => _metadata.Name;

        public string StartTimestampPropertyName => _startTimestampPropertyName;

        public string EndTimestampPropertyName => _endTimestampPropertyName;

        public EventTypeNestableGetterFactory GetterFactory => _getterFactory;

        public IDictionary<string, object> NestableTypes => _nestableTypes;

        public IDictionary<string, PropertySetDescriptorItem> PropertyItems => _propertyItems;

        public Type GetPropertyType(string propertyName)
        {
            return GetNestablePropertyType(
                propertyName,
                _propertyItems,
                _nestableTypes,
                _beanEventTypeFactory,
                _publicFields);
        }

        public EventPropertyGetterSPI GetGetterSPI(string propertyName)
        {
            if (_propertyGetterCache == null) {
                _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }

            return GetNestableGetter(
                propertyName,
                _propertyItems,
                _propertyGetterCache,
                _nestableTypes,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _getterFactory,
                _metadata.ApplicationType == EventTypeApplicationType.OBJECTARR,
                _beanEventTypeFactory,
                _publicFields);
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            return GetGetterSPI(propertyName);
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedPropertyName)
        {
            return GetGetterMappedSPI(mappedPropertyName);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            return GetGetterIndexedSPI(indexedPropertyName);
        }

        public string[] PropertyNames => _propertyNames;

        public bool IsProperty(string propertyName)
        {
            var propertyType = GetPropertyType(propertyName);
            if (propertyType == null) {
                // Could be a native null type, such as "insert into A select null as field..."
                if (_propertyItems.ContainsKey(StringValue.UnescapeDot(propertyName))) {
                    return true;
                }
            }

            return propertyType != null;
        }

        public IList<EventType> SuperTypes => _optionalSuperTypes;

        public IList<EventPropertyDescriptor> PropertyDescriptors => _propertyDescriptors;


        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedPropertyName)
        {
            var item = _propertyItems.Get(mappedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsMapped) {
                return null;
            }

            var mappedProperty = new MappedProperty(mappedPropertyName);
            return _getterFactory.GetPropertyProvidedGetterMap(
                _nestableTypes,
                mappedPropertyName,
                mappedProperty,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }


        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string indexedPropertyName)
        {
            var item = _propertyItems.Get(indexedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsIndexed) {
                return null;
            }

            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return _getterFactory.GetPropertyProvidedGetterIndexed(
                _nestableTypes,
                indexedPropertyName,
                indexedProperty,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            var item = _propertyItems.Get(propertyName);
            if (item == null) {
                return null;
            }

            return item.PropertyDescriptor;
        }

        public EventTypeMetadata Metadata => _metadata;

        public FragmentEventType GetFragmentType(string propertyName)
        {
            var item = _propertyItems.Get(propertyName);
            if (item != null) {
                // may contain null values
                return item.FragmentEventType;
            }

            // see if this is a nested property
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                // dynamic simple property
                if (propertyName.EndsWith("?")) {
                    return null;
                }

                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (property is SimpleProperty) {
                    item = _propertyItems.Get(property.PropertyNameAtomic);
                    if (item != null) {
                        // may contain null values
                        return item.FragmentEventType;
                    }

                    return null;
                }

                if (property is IndexedProperty indexedProp) {
                    var type = _nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is EventType[] eventTypesArray) {
                        var eventType = eventTypesArray[0];
                        return new FragmentEventType(eventType, false, false, true);
                    }

                    if (type is TypeBeanOrUnderlying[] beanOrUnderlyings) {
                        var innerType = beanOrUnderlyings[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        return new FragmentEventType(innerType, false, false, true); // false since an index is present
                    }

                    if (type is Type typeClass) {
                        if (!typeClass.IsArray) {
                            return null;
                        }

                        // its an array
                        var component = typeClass.GetElementType();
                        return EventBeanUtility.CreateNativeFragmentType(
                            component,
                            _beanEventTypeFactory,
                            false);
                    }

                    return null;
                }

                if (property is MappedProperty) {
                    // No type information available for the inner event
                    return null;
                }

                return null;
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is an object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
            // The property getters therefore act on

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap =
                PropertyParser.UnescapeBacktickForProperty(StringValue.UnescapeDot(propertyName.Substring(0, index)));
            var propertyNested = propertyName.Substring(index + 1);

            // If the property is dynamic, it cannot be a fragment
            if (propertyMap.EndsWith("?")) {
                return null;
            }

            var nestedType = _nestableTypes.Get(propertyMap);
            if (nestedType == null) {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty indexedProp) {
                    var type = _nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    // handle map-in-map case
                    if (type is TypeBeanOrUnderlying[] beanOrUnderlyings) {
                        var innerType = beanOrUnderlyings[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        return innerType.GetFragmentType(propertyNested);
                    }

                    if (type is EventType[] innerEventTypeArray) {
                        // handle eventtype[] in map
                        var innerType = innerEventTypeArray[0];
                        return innerType.GetFragmentType(propertyNested);
                    }

                    if (type is Type typeClass) {
                        if (!typeClass.IsArray) {
                            return null;
                        }

                        var fragmentParent =
                            EventBeanUtility.CreateNativeFragmentType(typeClass, _beanEventTypeFactory, false);
                        if (fragmentParent == null) {
                            return null;
                        }

                        return fragmentParent.FragmentType.GetFragmentType(propertyNested);
                    }

                    return null;
                }

                if (property is MappedProperty) {
                    // No type information available for the property's map value
                    return null;
                }

                return null;
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if (nestedType is Type type1 && type1 == typeof(IDictionary<string, object>)) {
                return null;
            }

            if (nestedType is IDictionary<string, object>) {
                return null;
            }

            if (nestedType is Type simpleClass) {
                if (!simpleClass.IsFragmentableType()) {
                    return null;
                }

                EventType nestedEventType = _beanEventTypeFactory.GetCreateBeanType(simpleClass, false);
                return nestedEventType.GetFragmentType(propertyNested);
            }

            if (nestedType is EventType innerEventType) {
                return innerEventType.GetFragmentType(propertyNested);
            }

            if (nestedType is EventType[] eventTypeArray) {
                return eventTypeArray[0].GetFragmentType(propertyNested);
            }

            if (nestedType is TypeBeanOrUnderlying typeBeanOrUnderlying) {
                var innerType = typeBeanOrUnderlying.EventType;
                if (!(innerType is BaseNestableEventType)) {
                    return null;
                }

                return innerType.GetFragmentType(propertyNested);
            }

            if (nestedType is TypeBeanOrUnderlying[] typeBeanOrUnderlyings) {
                var innerType = typeBeanOrUnderlyings[0].EventType;
                if (!(innerType is BaseNestableEventType)) {
                    return null;
                }

                return innerType.GetFragmentType(propertyNested);
            }

            var message = "Nestable map type configuration encountered an unexpected value type of '" +
                          nestedType.GetType() +
                          " for property '" +
                          propertyName +
                          "', expected Class, Map.class or Map<String, Object> as value type";
            throw new PropertyAccessException(message);
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            _metadata = _metadata.WithIds(publicId, protectedId);
        }

        public ExprValidationException EqualsCompareType(EventType otherEventType)
        {
            if (ReferenceEquals(this, otherEventType)) {
                return null;
            }

            return CompareEquals(otherEventType);
        }

        private void ValidateMapPropertyTypes(IDictionary<string, object> propertyTypes)
        {
            foreach (var entry in propertyTypes) {
                var entryValue = entry.Value;
                if (entryValue == null) {
                    // null type is allowable
                }
                else if (
                    !(entryValue is TypeBeanOrUnderlying) &&
                    !(entryValue is TypeBeanOrUnderlying[]) &&
                    !(entryValue is EventType) &&
                    !(entryValue is EventType[]) &&
                    !(entryValue is Type) &&
                    !(entryValue is IDictionary<string, object>)) {
                    throw new IllegalStateException("Unrecognized nestable property type '" + entryValue + "'");
                }
                else if (entryValue is IDictionary<string, object> value) {
                    ValidateMapPropertyTypes(value);
                }
            }
        }

        /// <summary>
        ///     Compares two sets of properties and determines if they are the same, allowing for
        ///     boxed/unboxed types, and nested map types.
        ///     <para>
        ///     Set-one is the predefined inserted-into existing type, set-two is the proposed insert-into select-output type
        ///     </para>
        /// </summary>
        /// <param name="setOne">is the first set of properties</param>
        /// <param name="setTwo">is the second set of properties</param>
        /// <param name="otherName">name of the type compared to</param>
        /// <param name="lenientPropertyCount">true to disregard property count, false to check all properties are provided</param>
        /// <returns>null if the property set is equivalent or message if not</returns>
        public static ExprValidationException IsDeepEqualsProperties(
            string otherName,
            IDictionary<string, object> setOne,
            IDictionary<string, object> setTwo,
            bool lenientPropertyCount)
        {
            // Should have the same number of properties
            if (!lenientPropertyCount && setOne.Count != setTwo.Count) {
                return new ExprValidationException(
                    $"Type by name '{otherName}' expects {setOne.Count} properties but receives {setTwo.Count} properties");
            }

            // Compare property by property
            foreach (var entry in setTwo) {
                var propName = entry.Key;
                var setTwoType = entry.Value;
                var setOneType = setOne.Get(propName);
                var setOneTypeFound = setOne.ContainsKey(propName);

                var message = BaseNestableEventUtil.ComparePropType(
                    propName,
                    setOneType,
                    setOneTypeFound,
                    setTwoType,
                    otherName);
                if (message != null) {
                    return message;
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns a message if the type, compared to this type, is not compatible in regards to the property numbers
        ///     and types.
        /// </summary>
        /// <param name="otherType">to compare to</param>
        /// <returns>message</returns>
        public ExprValidationException CompareEquals(EventType otherType)
        {
            if (!(otherType is BaseNestableEventType other)) {
                return new ExprValidationException(
                    $"Type by name '{otherType.Name}' is not a compatible type (target type underlying is '{otherType.UnderlyingType.CleanName()}')");
            }

            return IsDeepEqualsProperties(otherType.Name, other._nestableTypes, _nestableTypes, true);
        }

        public abstract Type UnderlyingType { get; }
        public abstract EventPropertyDescriptor[] WriteableProperties { get; }
        public abstract EventPropertyWriterSPI GetWriter(string propertyName);
        public abstract EventPropertyDescriptor GetWritableProperty(string propertyName);
        public abstract EventBeanCopyMethodForge GetCopyMethodForge(string[] properties);
        public abstract EventBeanWriter GetWriter(string[] properties);
    }
} // end of namespace