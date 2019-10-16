///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Implementation of the <seealso cref="com.espertech.esper.common.client.EventType" /> interface for handling name
    ///     value pairs.
    /// </summary>
    public abstract class BaseNestableEventType : EventTypeSPI
    {
        internal readonly BeanEventTypeFactory beanEventTypeFactory;
        internal readonly EventTypeNestableGetterFactory getterFactory;
        internal readonly ISet<EventType> OptionalDeepSuperTypes;
        internal readonly EventType[] optionalSuperTypes;

        internal readonly IDictionary<string, PropertySetDescriptorItem> propertyItems;
        internal string endTimestampPropertyName;
        internal EventTypeMetadata metadata;

        // Nestable definition of Map contents is here
        internal IDictionary<string, object>
            nestableTypes; // Deep definition of the map-type, containing nested maps and objects

        internal EventPropertyDescriptor[] propertyDescriptors;

        internal IDictionary<string, EventPropertyGetterSPI>
            propertyGetterCache; // Mapping of all property names and getters

        // Simple (not-nested) properties are stored here
        internal string[] propertyNames; // Cache an array of property names so not to construct one frequently

        internal string startTimestampPropertyName;

        /// <summary>
        ///     Constructor takes a type name, map of property names and types, for
        ///     use with nestable Map events.
        /// </summary>
        /// <param name="propertyTypes">is pairs of property name and type</param>
        /// <param name="optionalSuperTypes">the supertypes to this type if any, or null if there are no supertypes</param>
        /// <param name="optionalDeepSuperTypes">the deep supertypes to this type if any, or null if there are no deep supertypes</param>
        /// <param name="metadata">event type metadata</param>
        /// <param name="getterFactory">getter factory</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <param name="endTimestampPropertyName">end timestamp</param>
        /// <param name="startTimestampPropertyName">start timestamp</param>
        public BaseNestableEventType(
            EventTypeMetadata metadata,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSuperTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            EventTypeNestableGetterFactory getterFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            this.metadata = metadata;
            this.getterFactory = getterFactory;
            this.beanEventTypeFactory = beanEventTypeFactory;

            this.optionalSuperTypes = optionalSuperTypes;
            if (optionalDeepSuperTypes == null) {
                this.OptionalDeepSuperTypes = Collections.GetEmptySet<EventType>();
            }
            else {
                this.OptionalDeepSuperTypes = optionalDeepSuperTypes;
            }

            // determine property set and prepare getters
            var propertySet = EventTypeUtility.GetNestableProperties(
                propertyTypes,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                getterFactory,
                optionalSuperTypes,
                beanEventTypeFactory);

            nestableTypes = propertySet.NestableTypes;
            propertyNames = propertySet.PropertyNameArray;
            propertyItems = propertySet.PropertyItems;
            propertyDescriptors = propertySet.PropertyDescriptors.ToArray();

            var desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this,
                startTimestampPropertyName,
                endTimestampPropertyName,
                optionalSuperTypes);
            this.startTimestampPropertyName = desc.Start;
            this.endTimestampPropertyName = desc.End;
        }

        /// <summary>
        ///     Returns the name-type map of map properties, each value in the map
        ///     can be a Class or a Map&lt;String, Object&gt; (for nested maps).
        /// </summary>
        /// <returns>is the property name and types</returns>
        public IDictionary<string, object> Types => nestableTypes;

        public IEnumerable<EventType> DeepSuperTypes => OptionalDeepSuperTypes;

        public string Name => metadata.Name;

        public string StartTimestampPropertyName => startTimestampPropertyName;

        public string EndTimestampPropertyName => endTimestampPropertyName;

        public Type GetPropertyType(string propertyName)
        {
            return EventTypeUtility.GetNestablePropertyType(
                propertyName,
                propertyItems,
                nestableTypes,
                beanEventTypeFactory);
        }

        public EventPropertyGetterSPI GetGetterSPI(string propertyName)
        {
            if (propertyGetterCache == null) {
                propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }

            return EventTypeUtility.GetNestableGetter(
                propertyName,
                propertyItems,
                propertyGetterCache,
                nestableTypes,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                getterFactory,
                metadata.ApplicationType == EventTypeApplicationType.OBJECTARR,
                beanEventTypeFactory);
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

        public string[] PropertyNames => propertyNames;

        public bool IsProperty(string propertyName)
        {
            var propertyType = GetPropertyType(propertyName);
            if (propertyType == null) {
                // Could be a native null type, such as "insert into A select null as field..."
                if (propertyItems.ContainsKey(StringValue.UnescapeDot(propertyName))) {
                    return true;
                }
            }

            return propertyType != null;
        }

        public EventType[] SuperTypes => optionalSuperTypes;

        public ICollection<EventType> DeepSuperTypesCollection => OptionalDeepSuperTypes;

        public IList<EventPropertyDescriptor> PropertyDescriptors => propertyDescriptors;

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            var item = propertyItems.Get(propertyName);
            return item?.PropertyDescriptor;
        }

        public EventTypeMetadata Metadata => metadata;

        public FragmentEventType GetFragmentType(string propertyName)
        {
            var item = propertyItems.Get(propertyName);
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
                if (property is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null) {
                        return null;
                    }

                    if (type is EventType[] eventTypesArray) {
                        var eventType = eventTypesArray[0];
                        return new FragmentEventType(eventType, false, false);
                    }

                    if (type is TypeBeanOrUnderlying[] beanOrUnderlyings) {
                        var innerType = beanOrUnderlyings[0].EventType;
                        if (!(innerType is BaseNestableEventType)) {
                            return null;
                        }

                        return new FragmentEventType(innerType, false, false); // false since an index is present
                    }

                    if (!(type is Type)) {
                        return null;
                    }

                    if (!((Type) type).IsArray) {
                        return null;
                    }

                    // its an array
                    return EventBeanUtility.CreateNativeFragmentType(
                        ((Type) type).GetElementType(),
                        null,
                        beanEventTypeFactory);
                }

                if (property is MappedProperty) {
                    // No type information available for the inner event
                    return null;
                }

                return null;
            }

            // Map event types allow 2 types of properties inside:
            //   - a property that is a object is interrogated via bean property getters and BeanEventType
            //   - a property that is a Map itself is interrogated via map property getters
            // The property getters therefore act on

            // Take apart the nested property into a map key and a nested value class property name
            var propertyMap = StringValue.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);

            // If the property is dynamic, it cannot be a fragment
            if (propertyMap.EndsWith("?")) {
                return null;
            }

            var nestedType = nestableTypes.Get(propertyMap);
            if (nestedType == null) {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty indexedProp) {
                    var type = nestableTypes.Get(indexedProp.PropertyNameAtomic);
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
                        // handle EventType[] in map
                        var innerType = innerEventTypeArray[0];
                        return innerType.GetFragmentType(propertyNested);
                    }

                    // handle array class in map case
                    if (!(type is Type)) {
                        return null;
                    }

                    if (!((Type) type).IsArray) {
                        return null;
                    }

                    var fragmentParent = EventBeanUtility.CreateNativeFragmentType(
                        (Type) type,
                        null,
                        beanEventTypeFactory);

                    return fragmentParent?.FragmentType.GetFragmentType(propertyNested);
                }

                if (property is MappedProperty) {
                    // No type information available for the property's map value
                    return null;
                }

                return null;
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if (ReferenceEquals(nestedType, typeof(IDictionary<string, object>))) {
                return null;
            }

            if (nestedType is IDictionary<string, object>) {
                return null;
            }

            if (nestedType is Type simpleClass) {
                if (!simpleClass.IsFragmentableType()) {
                    return null;
                }

                EventType nestedEventType = beanEventTypeFactory.GetCreateBeanType(simpleClass);
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
            metadata = metadata.WithIds(publicId, protectedId);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedPropertyName)
        {
            var item = propertyItems.Get(mappedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsMapped) {
                return null;
            }

            var mappedProperty = new MappedProperty(mappedPropertyName);
            return getterFactory.GetPropertyProvidedGetterMap(
                nestableTypes,
                mappedPropertyName,
                mappedProperty,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string indexedPropertyName)
        {
            var item = propertyItems.Get(indexedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsIndexed) {
                return null;
            }

            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return getterFactory.GetPropertyProvidedGetterIndexed(
                nestableTypes,
                indexedPropertyName,
                indexedProperty,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public ExprValidationException EqualsCompareType(EventType otherEventType)
        {
            if (ReferenceEquals(this, otherEventType)) {
                return null;
            }

            return CompareEquals(otherEventType);
        }

        public abstract Type UnderlyingType { get; }
        public abstract EventPropertyDescriptor[] WriteableProperties { get; }
        public abstract EventBeanReader Reader { get; }
        public abstract EventPropertyWriterSPI GetWriter(string propertyName);
        public abstract EventPropertyDescriptor GetWritableProperty(string propertyName);
        public abstract EventBeanCopyMethodForge GetCopyMethodForge(string[] properties);
        public abstract EventBeanWriter GetWriter(string[] properties);

        internal abstract void PostUpdateNestableTypes();

        /// <summary>
        ///     Compares two sets of properties and determines if they are the same, allowing for
        ///     boxed/unboxed types, and nested map types.
        /// </summary>
        /// <param name="setOne">is the first set of properties</param>
        /// <param name="setTwo">is the second set of properties</param>
        /// <param name="otherName">name of the type compared to</param>
        /// <returns>null if the property set is equivalent or message if not</returns>
        public static ExprValidationException IsDeepEqualsProperties(
            string otherName,
            IDictionary<string, object> setOne,
            IDictionary<string, object> setTwo)
        {
            // Should have the same number of properties
            if (setOne.Count != setTwo.Count) {
                return new ExprValidationException(
                    "Type by name '" +
                    otherName +
                    "' expects " +
                    setOne.Count +
                    " properties but receives " +
                    setTwo.Count +
                    " properties");
            }

            // Compare property by property
            foreach (var entry in setOne) {
                var propName = entry.Key;
                var setTwoType = setTwo.Get(entry.Key);
                var setTwoTypeFound = setTwo.ContainsKey(entry.Key);
                var setOneType = entry.Value;

                var message = BaseNestableEventUtil.ComparePropType(
                    propName,
                    setOneType,
                    setTwoType,
                    setTwoTypeFound,
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
            if (!(otherType is BaseNestableEventType)) {
                return new ExprValidationException(
                    "Type by name '" +
                    otherType.Name +
                    "' is not a compatible type (target type underlying is '" +
                    otherType.UnderlyingType.CleanName() +
                    "')");
            }

            var other = (BaseNestableEventType) otherType;
            return IsDeepEqualsProperties(otherType.Name, other.nestableTypes, nestableTypes);
        }
    }
} // end of namespace