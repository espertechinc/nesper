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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Implementation of the <seealso cref="com.espertech.esper.client.EventType" /> interface for handling name value pairs.
    /// </summary>
    public abstract class BaseNestableEventType : EventTypeSPI
    {
        private readonly EventTypeMetadata _metadata;
        private readonly String _typeName;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType[] _optionalSuperTypes;
        private readonly ICollection<EventType> _optionalDeepSupertypes;
        private readonly int _eventTypeId;

        internal readonly EventTypeNestableGetterFactory GetterFactory;

        // Simple (not-nested) properties are stored here
        private String[] _propertyNames; // Cache an array of property names so not to construct one frequently
        private EventPropertyDescriptor[] _propertyDescriptors;

        private readonly IDictionary<String, PropertySetDescriptorItem> _propertyItems;

        private IDictionary<String, EventPropertyGetterSPI> _propertyGetterCache;
        // Mapping of all property names and getters

        // Nestable definition of Map contents is here
        internal IDictionary<String, Object> NestableTypes;

        private IDictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        private readonly String _startTimestampPropertyName;
        private readonly String _endTimestampPropertyName;

        protected abstract void PostUpdateNestableTypes();

        public abstract Type UnderlyingType { get; }
        public abstract EventPropertyDescriptor[] WriteableProperties { get; }
        public abstract EventPropertyWriter GetWriter(string propertyName);
        public abstract EventBeanWriter GetWriter(string[] properties);
        public abstract EventPropertyDescriptor GetWritableProperty(string propertyName);
        public abstract EventBeanCopyMethod GetCopyMethod(string[] properties);
        public abstract EventBeanReader Reader { get; }

        public EventAdapterService EventAdapterService => _eventAdapterService;

        public IDictionary<string, PropertySetDescriptorItem> PropertyItems => _propertyItems;

        /// <summary>
        /// Constructor takes a type name, map of property names and types, for use with nestable Map events.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="typeName">is the event type name used to distinquish map types that have the same property types,empty string for anonymous maps, or for insert-into statements generating map events the stream name</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="eventAdapterService">is required for access to objects properties within map values</param>
        /// <param name="propertyTypes">is pairs of property name and type</param>
        /// <param name="optionalSuperTypes">the supertypes to this type if any, or null if there are no supertypes</param>
        /// <param name="optionalDeepSupertypes">the deep supertypes to this type if any, or null if there are no deep supertypes</param>
        /// <param name="typeConfig">The type config.</param>
        /// <param name="getterFactory">The getter factory.</param>
        protected BaseNestableEventType(
            EventTypeMetadata metadata,
            String typeName,
            int eventTypeId,
            EventAdapterService eventAdapterService,
            IDictionary<String, Object> propertyTypes,
            EventType[] optionalSuperTypes,
            ICollection<EventType> optionalDeepSupertypes,
            ConfigurationEventTypeWithSupertype typeConfig,
            EventTypeNestableGetterFactory getterFactory)
        {
            _metadata = metadata;
            _eventTypeId = eventTypeId;
            _typeName = typeName;
            _eventAdapterService = eventAdapterService;
            GetterFactory = getterFactory;

            _optionalSuperTypes = optionalSuperTypes;
            _optionalDeepSupertypes = optionalDeepSupertypes ?? Collections.GetEmptySet<EventType>();

            // determine property set and prepare getters
            var propertySet = EventTypeUtility.GetNestableProperties(
                propertyTypes, eventAdapterService, getterFactory, optionalSuperTypes);

            NestableTypes = propertySet.NestableTypes;
            _propertyNames = propertySet.PropertyNameArray;
            _propertyItems = propertySet.PropertyItems;
            _propertyDescriptors = propertySet.PropertyDescriptors.ToArray();

            var desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this, typeConfig == null ? null : typeConfig.StartTimestampPropertyName,
                typeConfig == null ? null : typeConfig.EndTimestampPropertyName, optionalSuperTypes);
            _startTimestampPropertyName = desc.Start;
            _endTimestampPropertyName = desc.End;
        }

        public string Name => _typeName;

        public int EventTypeId => _eventTypeId;

        public string StartTimestampPropertyName => _startTimestampPropertyName;

        public string EndTimestampPropertyName => _endTimestampPropertyName;

        public Type GetPropertyType(String propertyName)
        {
            return EventTypeUtility.GetNestablePropertyType(
                propertyName, _propertyItems, NestableTypes, _eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterSPI(String propertyName)
        {
            if (_propertyGetterCache == null)
            {
                _propertyGetterCache = new Dictionary<String, EventPropertyGetterSPI>();
            }
            return EventTypeUtility.GetNestableGetter(
                propertyName, _propertyItems, _propertyGetterCache, NestableTypes, _eventAdapterService, GetterFactory,
                _metadata.OptionalApplicationType == ApplicationType.OBJECTARR);
        }

        public EventPropertyGetter GetGetter(String propertyName)
        {
            if (!_eventAdapterService.EngineImportService.IsCodegenEventPropertyGetters)
            {
                return GetGetterSPI(propertyName);
            }
            if (_propertyGetterCodegeneratedCache == null)
            {
                _propertyGetterCodegeneratedCache = new Dictionary<string, EventPropertyGetter>();
            }

            var getter = _propertyGetterCodegeneratedCache.Get(propertyName);
            if (getter != null)
            {
                return getter;
            }

            var getterSPI = GetGetterSPI(propertyName);
            if (getterSPI == null)
            {
                return null;
            }

            var getterCode = _eventAdapterService.EngineImportService.CodegenGetter(getterSPI, propertyName);
            _propertyGetterCodegeneratedCache.Put(propertyName, getterCode);
            return getterCode;
        }

        public EventPropertyGetterMapped GetGetterMapped(String mappedPropertyName)
        {
            var item = _propertyItems.Get(mappedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsMapped)
            {
                return null;
            }
            var mappedProperty = new MappedProperty(mappedPropertyName);
            return GetterFactory.GetPropertyProvidedGetterMap(
                NestableTypes, mappedPropertyName, mappedProperty, _eventAdapterService);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(String indexedPropertyName)
        {
            var item = _propertyItems.Get(indexedPropertyName);
            if (item == null || !item.PropertyDescriptor.IsIndexed)
            {
                return null;
            }
            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return GetterFactory.GetPropertyProvidedGetterIndexed(
                NestableTypes, indexedPropertyName, indexedProperty, _eventAdapterService);
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public bool IsProperty(String propertyName)
        {
            var propertyType = GetPropertyType(propertyName);
            if (propertyType == null)
            {
                // Could be a native null type, such as "insert into A select null as field..."
                if (_propertyItems.ContainsKey(ASTUtil.UnescapeDot(propertyName)))
                {
                    return true;
                }
            }
            return propertyType != null;
        }

        public EventType[] SuperTypes
        {
            get { return _optionalSuperTypes; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return _optionalDeepSupertypes.ToArray(); }
        }

        /// <summary>Returns the name-type map of map properties, each value in the map </summary>
        /// <value>is the property name and types</value>
        public IDictionary<string, object> Types
        {
            get { return NestableTypes; }
        }

        /// <summary>
        /// Adds additional properties that do not yet exist on the given type. Ignores properties already present. Allows nesting.
        /// </summary>
        /// <param name="typeMap">properties to add</param>
        /// <param name="eventAdapterService">for resolving further map event types that are property types</param>
        public void AddAdditionalProperties(IDictionary<String, Object> typeMap, EventAdapterService eventAdapterService)
        {
            // merge type graphs
            NestableTypes = GraphUtil.MergeNestableMap(NestableTypes, typeMap);

            PostUpdateNestableTypes();

            // construct getters and types for each property (new or old)
            var propertySet = EventTypeUtility.GetNestableProperties(
                typeMap, eventAdapterService, GetterFactory, _optionalSuperTypes);

            // add each new descriptor
            var newPropertyDescriptors = new List<EventPropertyDescriptor>();
            foreach (var propertyDesc in propertySet.PropertyDescriptors)
            {
                if (_propertyItems.ContainsKey(propertyDesc.PropertyName)) // not a new property
                {
                    continue;
                }
                newPropertyDescriptors.Add(propertyDesc);
            }

            // add each that is not already present
            var newPropertyNames = new List<String>();
            foreach (var propertyName in propertySet.PropertyNameList)
            {
                if (_propertyItems.ContainsKey(propertyName)) // not a new property
                {
                    continue;
                }
                newPropertyNames.Add(propertyName);
                _propertyItems.Put(propertyName, propertySet.PropertyItems.Get(propertyName));
            }

            // expand property name array
            var allPropertyNames = new String[_propertyNames.Length + newPropertyNames.Count];
            Array.Copy(_propertyNames, 0, allPropertyNames, 0, _propertyNames.Length);
            var count = _propertyNames.Length;
            foreach (var newProperty in newPropertyNames)
            {
                allPropertyNames[count++] = newProperty;
            }
            _propertyNames = allPropertyNames;

            // expand descriptor array
            var allPropertyDescriptors =
                new EventPropertyDescriptor[_propertyDescriptors.Length + newPropertyNames.Count];
            Array.Copy(_propertyDescriptors, 0, allPropertyDescriptors, 0, _propertyDescriptors.Length);
            count = _propertyDescriptors.Length;
            foreach (var desc in newPropertyDescriptors)
            {
                allPropertyDescriptors[count++] = desc;
            }
            _propertyDescriptors = allPropertyDescriptors;
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        /// <summary>Compares two sets of properties and determines if they are the same, allowing for boxed/unboxed types, and nested map types. </summary>
        /// <param name="setOne">is the first set of properties</param>
        /// <param name="setTwo">is the second set of properties</param>
        /// <param name="otherName">name of the type compared to</param>
        /// <returns>null if the property set is equivalent or message if not</returns>
        public static String IsDeepEqualsProperties(
            String otherName,
            IDictionary<String, Object> setOne,
            IDictionary<String, Object> setTwo)
        {
            // Should have the same number of properties
            if (setOne.Count != setTwo.Count)
            {
                return "Type by name '" + otherName + "' expects " + setOne.Count + " properties but receives " +
                       setTwo.Count + " properties";
            }

            // Compare property by property
            foreach (var entry in setOne)
            {
                var propName = entry.Key;
                var setTwoType = setTwo.Get(entry.Key);
                var setTwoTypeFound = setTwo.ContainsKey(entry.Key);
                var setOneType = entry.Value;

                var message = BaseNestableEventUtil.ComparePropType(
                    propName, setOneType, setTwoType, setTwoTypeFound, otherName);
                if (message != null)
                {
                    return message;
                }
            }

            return null;
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            var item = _propertyItems.Get(propertyName);
            if (item == null)
            {
                return null;
            }
            return item.PropertyDescriptor;
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public FragmentEventType GetFragmentType(String propertyName)
        {
            var item = _propertyItems.Get(propertyName);
            if (item != null) // may contain null values
            {
                return item.FragmentEventType;
            }

            // see if this is a nested property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                // dynamic simple property
                if (propertyName.EndsWith("?"))
                {
                    return null;
                }

                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty)property;
                    var type = NestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    else if (type is EventType[])
                    {
                        var eventType = ((EventType[])type)[0];
                        return new FragmentEventType(eventType, false, false);
                    }
                    else if (type is String)
                    {
                        var propTypeName = type.ToString();
                        var isArray = EventTypeUtility.IsPropertyArray(propTypeName);
                        if (!isArray)
                        {
                            return null;
                        }
                        propTypeName = EventTypeUtility.GetPropertyRemoveArray(propTypeName);
                        EventType innerType = _eventAdapterService.GetEventTypeByName(propTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        return new FragmentEventType(innerType, false, false); // false since an index is present
                    }
                    if (!(type is Type))
                    {
                        return null;
                    }
                    if (!((Type)type).IsArray)
                    {
                        return null;
                    }
                    // its an array
                    return EventBeanUtility.CreateNativeFragmentType(((Type)type).GetElementType(), null, _eventAdapterService);
                }
                else if (property is MappedProperty)
                {
                    // No type information available for the inner event
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

            // If the property is dynamic, it cannot be a fragment
            if (propertyMap.EndsWith("?"))
            {
                return null;
            }

            var nestedType = NestableTypes.Get(propertyMap);
            if (nestedType == null)
            {
                // parse, can be an indexed property
                var property = PropertyParser.ParseAndWalkLaxToSimple(propertyMap);
                if (property is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty)property;
                    var type = NestableTypes.Get(indexedProp.PropertyNameAtomic);
                    if (type == null)
                    {
                        return null;
                    }
                    // handle map-in-map case
                    if (type is String)
                    {
                        var propTypeName = type.ToString();
                        var isArray = EventTypeUtility.IsPropertyArray(propTypeName);
                        if (isArray)
                        {
                            propTypeName = EventTypeUtility.GetPropertyRemoveArray(propTypeName);
                        }
                        EventType innerType = _eventAdapterService.GetEventTypeByName(propTypeName);
                        if (!(innerType is BaseNestableEventType))
                        {
                            return null;
                        }
                        return innerType.GetFragmentType(propertyNested);
                    }
                    // handle eventtype[] in map
                    else if (type is EventType[])
                    {
                        var innerType = ((EventType[])type)[0];
                        return innerType.GetFragmentType(propertyNested);
                    }
                    // handle array class in map case
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
                        var fragmentParent = EventBeanUtility.CreateNativeFragmentType(
                            (Type)type, null, _eventAdapterService);
                        if (fragmentParent == null)
                        {
                            return null;
                        }
                        return fragmentParent.FragmentType.GetFragmentType(propertyNested);
                    }
                }
                else if (property is MappedProperty)
                {
                    // No type information available for the property's map value
                    return null;
                }
                else
                {
                    return null;
                }
            }

            // If there is a map value in the map, return the Object value if this is a dynamic property
            if (ReferenceEquals(nestedType, typeof(IDictionary<string, object>)))
            {
                return null;
            }
            else if (nestedType is IDictionary<string, object>)
            {
                return null;
            }
            else if (nestedType is Type)
            {
                var simpleClass = (Type)nestedType;
                if (!TypeHelper.IsFragmentableType(simpleClass))
                {
                    return null;
                }
                EventType nestedEventType =
                    _eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(simpleClass);
                return nestedEventType.GetFragmentType(propertyNested);
            }
            else if (nestedType is EventType)
            {
                var innerType = (EventType)nestedType;
                return innerType.GetFragmentType(propertyNested);
            }
            else if (nestedType is EventType[])
            {
                var innerType = (EventType[])nestedType;
                return innerType[0].GetFragmentType(propertyNested);
            }
            else if (nestedType is String)
            {
                var nestedName = nestedType.ToString();
                var isArray = EventTypeUtility.IsPropertyArray(nestedName);
                if (isArray)
                {
                    nestedName = EventTypeUtility.GetPropertyRemoveArray(nestedName);
                }
                var innerType = _eventAdapterService.GetEventTypeByName(nestedName);
                if (!(innerType is BaseNestableEventType))
                {
                    return null;
                }
                return innerType.GetFragmentType(propertyNested);
            }
            else
            {
                var message = "Nestable map type configuration encountered an unexpected value type of '"
                                 + nestedType.GetType() + " for property '" + propertyName +
                                 "', expected Class, typeof(Map) or IDictionary<String, Object> as value type";
                throw new PropertyAccessException(message);
            }
        }

        /// <summary>Returns a message if the type, compared to this type, is not compatible in regards to the property numbers and types. </summary>
        /// <param name="otherType">to compare to</param>
        /// <returns>message</returns>
        public String GetEqualsMessage(EventType otherType)
        {
            if (!(otherType is BaseNestableEventType))
            {
                return string.Format("Type by name '{0}' is not a compatible type (target type underlying is '{1}')", otherType.Name, compat.Name.Clean(otherType.UnderlyingType));
            }

            var other = (BaseNestableEventType)otherType;

            if ((_metadata.TypeClass != TypeClass.ANONYMOUS) && (!other._typeName.Equals(_typeName)))
            {
                return "Type by name '" + otherType.Name + "' is not the same name";
            }

            return IsDeepEqualsProperties(otherType.Name, other.NestableTypes, NestableTypes);
        }

        public bool EqualsCompareType(EventType otherEventType)
        {
            if (Equals(this, otherEventType))
            {
                return true;
            }

            var message = GetEqualsMessage(otherEventType);
            return message == null;
        }
    }
}
