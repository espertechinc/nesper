///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.map;
using com.espertech.esper.events.wrap;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// An event type that adds zero or more fields to an existing event type. 
    /// <para>
    /// The additional fields are represented as a Map. Any queries to event properties are 
    /// first held against the additional fields, and secondly are handed through to the 
    /// underlying event. 
    /// </para>
    /// <para>
    /// If this event type is to add information to another wrapper event type (wrapper to 
    /// wrapper), then it is the responsibility of the creating logic to use the existing event 
    /// type and add to it. 
    /// </para>
    /// <para>
    /// Uses a the map event type <seealso cref="com.espertech.esper.events.map.MapEventType" />
    /// to represent the mapped properties. This is because the additional properties can also be 
    /// beans or complex types and the Map event type handles these nicely.
    /// </para>
    /// </summary>
    public class WrapperEventType : EventTypeSPI
    {
        /// <summary>event type metadata </summary>
        private readonly EventTypeMetadata _metadata;

        /// <summary>The underlying wrapped event type. </summary>
        private readonly EventType _underlyingEventType;

        /// <summary>The map event type that provides the additional properties. </summary>
        private readonly MapEventType _underlyingMapType;

        private String[] _propertyNames;
        private EventPropertyDescriptor[] _propertyDesc;
        private IDictionary<String, EventPropertyDescriptor> _propertyDescriptorMap;

        private readonly bool _isNoMapProperties;
        private readonly IDictionary<String, EventPropertyGetterSPI> _propertyGetterCache;
        private readonly EventAdapterService _eventAdapterService;
        private EventPropertyDescriptor[] _writableProperties;
        private IDictionary<String, Pair<EventPropertyDescriptor, EventPropertyWriter>> _writers;
        private IDictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        private int _numPropertiesUnderlyingType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="typeName">is the event type name</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="eventType">is the event type of the wrapped events</param>
        /// <param name="properties">is the additional properties this wrapper adds</param>
        /// <param name="eventAdapterService">is the service for resolving unknown wrapped types</param>
        public WrapperEventType(EventTypeMetadata metadata,
                                String typeName,
                                int eventTypeId,
                                EventType eventType,
                                IDictionary<String, Object> properties,
                                EventAdapterService eventAdapterService)
        {
            CheckForRepeatedPropertyNames(eventType, properties);

            _metadata = metadata;
            _underlyingEventType = eventType;
            var metadataMapType = EventTypeMetadata.CreateAnonymous(typeName, ApplicationType.MAP);
            _underlyingMapType = new MapEventType(metadataMapType, typeName, 0, eventAdapterService, properties, null, null, null);
            _isNoMapProperties = properties.IsEmpty();
            _eventAdapterService = eventAdapterService;
            EventTypeId = eventTypeId;
            _propertyGetterCache = new Dictionary<String, EventPropertyGetterSPI>();

            UpdatePropertySet();

            if (metadata.TypeClass == TypeClass.NAMED_WINDOW)
            {
                StartTimestampPropertyName = eventType.StartTimestampPropertyName;
                EndTimestampPropertyName = eventType.EndTimestampPropertyName;
                EventTypeUtility.ValidateTimestampProperties(this, StartTimestampPropertyName, EndTimestampPropertyName);
            }
        }

        private void CheckInitProperties()
        {
            if (_numPropertiesUnderlyingType != _underlyingEventType.PropertyDescriptors.Count)
            {
                UpdatePropertySet();
            }
        }

        private void UpdatePropertySet()
        {
            var compositeProperties = GetCompositeProperties(_underlyingEventType, _underlyingMapType);
            _propertyNames = compositeProperties.PropertyNames;
            _propertyDescriptorMap = compositeProperties.PropertyDescriptorMap;
            _propertyDesc = compositeProperties.Descriptors;
            _numPropertiesUnderlyingType = _underlyingEventType.PropertyDescriptors.Count;
        }

        private static PropertyDescriptorComposite GetCompositeProperties(EventType underlyingEventType, MapEventType underlyingMapType)
        {
            var propertyNames = new List<String>();
            propertyNames.AddAll(underlyingEventType.PropertyNames);
            propertyNames.AddAll(underlyingMapType.PropertyNames);
            var propertyNamesArr = propertyNames.ToArray();

            var propertyDesc = new List<EventPropertyDescriptor>();
            var propertyDescriptorMap = new Dictionary<String, EventPropertyDescriptor>();
            foreach (var eventProperty in underlyingEventType.PropertyDescriptors)
            {
                propertyDesc.Add(eventProperty);
                propertyDescriptorMap.Put(eventProperty.PropertyName, eventProperty);
            }
            foreach (var mapProperty in underlyingMapType.PropertyDescriptors)
            {
                propertyDesc.Add(mapProperty);
                propertyDescriptorMap.Put(mapProperty.PropertyName, mapProperty);
            }
            var propertyDescArr = propertyDesc.ToArray();
            return new PropertyDescriptorComposite(propertyDescriptorMap, propertyNamesArr, propertyDescArr);
        }

        public string StartTimestampPropertyName { get; private set; }

        public string EndTimestampPropertyName { get; private set; }

        public EventType[] DeepSuperTypes => null;

        public string Name => _metadata.PublicName;

        public int EventTypeId { get; private set; }

        public EventPropertyGetterSPI GetGetterSPI(String property)
        {
            var cachedGetter = _propertyGetterCache.Get(property);
            if (cachedGetter != null)
            {
                return cachedGetter;
            }

            if (_underlyingMapType.IsProperty(property) && (property.IndexOf('?') == -1))
            {
                var mapGetter = _underlyingMapType.GetGetterSPI(property);
                var getter = new WrapperMapPropertyGetter(this, _eventAdapterService, _underlyingMapType, mapGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }
            else if (_underlyingEventType.IsProperty(property))
            {
                var underlyingGetter = ((EventTypeSPI) _underlyingEventType).GetGetterSPI(property);
                var getter = new WrapperUnderlyingPropertyGetter(underlyingGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }
            else
            {
                return null;
            }
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

        public EventPropertyGetterMapped GetGetterMapped(String mappedProperty)
        {
            var undMapped = _underlyingEventType.GetGetterMapped(mappedProperty);
            if (undMapped != null)
            {
                return new ProxyEventPropertyGetterMapped
                {
                    ProcGet = (theEvent, mapKey) =>
                    {
                        if (!(theEvent is DecoratingEventBean))
                        {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }
                        var wrapperEvent = (DecoratingEventBean)theEvent;
                        var wrappedEvent = wrapperEvent.UnderlyingEvent;
                        return wrappedEvent == null ? null : undMapped.Get(wrappedEvent, mapKey);
                    }
                };
            }
            var decoMapped = _underlyingMapType.GetGetterMapped(mappedProperty);
            if (decoMapped != null)
            {
                return new ProxyEventPropertyGetterMapped
                {
                    ProcGet = (theEvent, mapKey) =>
                    {
                        if (!(theEvent is DecoratingEventBean))
                        {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }
                        var wrapperEvent = (DecoratingEventBean)theEvent;
                        var map = wrapperEvent.DecoratingProperties;
                        return decoMapped.Get(_eventAdapterService.AdapterForTypedMap(map, _underlyingMapType), mapKey);
                    }
                };
            }
            return null;
        }

        public EventPropertyGetterIndexed GetGetterIndexed(String indexedProperty)
        {
            var undIndexed = _underlyingEventType.GetGetterIndexed(indexedProperty);
            if (undIndexed != null)
            {
                return new ProxyEventPropertyGetterIndexed
                {
                    ProcGet = (theEvent, index) =>
                    {
                        if (!(theEvent is DecoratingEventBean))
                        {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }
                        var wrapperEvent = (DecoratingEventBean)theEvent;
                        var wrappedEvent = wrapperEvent.UnderlyingEvent;
                        return wrappedEvent == null ? null : undIndexed.Get(wrappedEvent, index);
                    }
                };
            }
            var decoIndexed = _underlyingMapType.GetGetterIndexed(indexedProperty);
            if (decoIndexed != null)
            {
                return new ProxyEventPropertyGetterIndexed
                {
                    ProcGet = (theEvent, index) =>
                    {
                        if (!(theEvent is DecoratingEventBean))
                        {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }
                        var wrapperEvent = (DecoratingEventBean)theEvent;
                        var map = wrapperEvent.DecoratingProperties;
                        return decoIndexed.Get(_eventAdapterService.AdapterForTypedMap(map, _underlyingMapType), index);
                    }
                };
            }
            return null;
        }

        public string[] PropertyNames
        {
            get
            {
                CheckInitProperties();
                return _propertyNames;
            }
        }

        public Type GetPropertyType(String property)
        {
            if (_underlyingEventType.IsProperty(property))
            {
                return _underlyingEventType.GetPropertyType(property);
            }
            if (_underlyingMapType.IsProperty(property))
            {
                return _underlyingMapType.GetPropertyType(property);
            }
            return null;
        }

        public EventBeanReader Reader => null;

        public EventType[] SuperTypes => null;

        public Type UnderlyingType
        {
            get
            {
                // If the additional properties are empty, such as when wrapping a native event by means of wildcard-only select
                // then the underlying type is simply the wrapped type.
                if (_isNoMapProperties)
                {
                    return _underlyingEventType.UnderlyingType;
                }
                else
                {
                    return typeof(Pair<object, DataMap>);
                }
            }
        }

        /// <summary>Returns the wrapped event type. </summary>
        /// <value>wrapped type</value>
        public EventType UnderlyingEventType => _underlyingEventType;

        /// <summary>Returns the map type. </summary>
        /// <value>map type providing additional properties.</value>
        public MapEventType UnderlyingMapType => _underlyingMapType;

        public bool IsProperty(String property)
        {
            return _underlyingEventType.IsProperty(property) ||
                _underlyingMapType.IsProperty(property);
        }

        public override String ToString()
        {
            return "WrapperEventType " +
            "underlyingEventType=" + _underlyingEventType + " " +
            "underlyingMapType=" + _underlyingMapType;
        }

        public bool EqualsCompareType(EventType otherEventType)
        {
            if (this == otherEventType)
            {
                return true;
            }

            if (!(otherEventType is WrapperEventType))
            {
                return false;
            }

            var other = (WrapperEventType)otherEventType;
            if (!other._underlyingMapType.EqualsCompareType(_underlyingMapType))
            {
                return false;
            }

            if (!(other._underlyingEventType is EventTypeSPI) || (!(_underlyingEventType is EventTypeSPI)))
            {
                return other._underlyingEventType.Equals(_underlyingEventType);
            }

            var otherUnderlying = (EventTypeSPI)other._underlyingEventType;
            var thisUnderlying = (EventTypeSPI)_underlyingEventType;
            return otherUnderlying.EqualsCompareType(thisUnderlying);
        }

        public EventTypeMetadata Metadata => _metadata;

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get
            {
                CheckInitProperties();
                return _propertyDesc;
            }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            CheckInitProperties();
            return _propertyDescriptorMap.Get(propertyName);
        }

        public FragmentEventType GetFragmentType(String property)
        {
            var fragment = _underlyingEventType.GetFragmentType(property);
            if (fragment != null)
            {
                return fragment;
            }
            return _underlyingMapType.GetFragmentType(property);
        }

        public EventPropertyWriter GetWriter(String propertyName)
        {
            if (_writableProperties == null)
            {
                InitializeWriters();
            }
            var pair = _writers.Get(propertyName);
            if (pair == null)
            {
                return null;
            }
            return pair.Second;
        }

        public EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            if (_writableProperties == null)
            {
                InitializeWriters();
            }
            var pair = _writers.Get(propertyName);
            if (pair == null)
            {
                return null;
            }
            return pair.First;
        }

        public EventPropertyDescriptor[] WriteableProperties
        {
            get
            {
                if (_writableProperties == null)
                {
                    InitializeWriters();
                }
                return _writableProperties;
            }
        }

        private void InitializeWriters()
        {
            var writables = new List<EventPropertyDescriptor>();
            var writerMap = new Dictionary<String, Pair<EventPropertyDescriptor, EventPropertyWriter>>();
            writables.AddAll(_underlyingMapType.WriteableProperties);

            foreach (var writableMapProp in _underlyingMapType.WriteableProperties)
            {
                var propertyName = writableMapProp.PropertyName;
                writables.Add(writableMapProp);
                var writer = new ProxyEventPropertyWriter
                {
                    ProcWrite = (value, target) =>
                    {
                        var decorated = (DecoratingEventBean)target;
                        decorated.DecoratingProperties.Put(propertyName, value);
                    }
                };
                writerMap.Put(propertyName, new Pair<EventPropertyDescriptor, EventPropertyWriter>(writableMapProp, writer));
            }

            if (_underlyingEventType is EventTypeSPI spi)
            {
                foreach (var writableUndProp in spi.WriteableProperties)
                {
                    var propertyName = writableUndProp.PropertyName;
                    var innerWriter = spi.GetWriter(propertyName);
                    if (innerWriter == null)
                    {
                        continue;
                    }

                    writables.Add(writableUndProp);
                    var writer = new ProxyEventPropertyWriter
                    {
                        ProcWrite = (value, target) =>
                        {
                            var decorated = (DecoratingEventBean)target;
                            innerWriter.Write(value, decorated.UnderlyingEvent);
                        }
                    };
                    writerMap.Put(propertyName, new Pair<EventPropertyDescriptor, EventPropertyWriter>(writableUndProp, writer));
                }
            }

            _writers = writerMap;
            _writableProperties = writables.ToArray();
        }

        public EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            if (_writableProperties == null)
            {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++)
            {
                if (_underlyingMapType.GetWritableProperty(properties[i]) == null)
                {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap)
            {
                if (!(_underlyingEventType is EventTypeSPI))
                {
                    return null;
                }
                var spi = (EventTypeSPI)_underlyingEventType;
                for (var i = 0; i < properties.Length; i++)
                {
                    if (spi.GetWritableProperty(properties[i]) == null)
                    {
                        isOnlyUnderlying = false;
                    }
                }
            }

            if (isOnlyMap)
            {
                return new WrapperEventBeanMapCopyMethod(this, _eventAdapterService);
            }

            var undCopyMethod = ((EventTypeSPI)_underlyingEventType).GetCopyMethod(properties);
            if (undCopyMethod == null)
            {
                return null;
            }
            if (isOnlyUnderlying)
            {
                return new WrapperEventBeanUndCopyMethod(this, _eventAdapterService, undCopyMethod);
            }

            return new WrapperEventBeanCopyMethod(this, _eventAdapterService, undCopyMethod);
        }

        public EventBeanWriter GetWriter(String[] properties)
        {
            if (_writableProperties == null)
            {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++)
            {
                if (!_writers.ContainsKey(properties[i]))
                {
                    return null;
                }
                if (_underlyingMapType.GetWritableProperty(properties[i]) == null)
                {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap)
            {
                var spi = (EventTypeSPI)_underlyingEventType;
                for (var i = 0; i < properties.Length; i++)
                {
                    if (spi.GetWritableProperty(properties[i]) == null)
                    {
                        isOnlyUnderlying = false;
                    }
                }
            }

            if (isOnlyMap)
            {
                return new WrapperEventBeanMapWriter(properties);
            }
            if (isOnlyUnderlying)
            {
                var spi = (EventTypeSPI)_underlyingEventType;
                var undWriter = spi.GetWriter(properties);
                if (undWriter == null)
                {
                    return undWriter;
                }
                return new WrapperEventBeanUndWriter(undWriter);
            }

            var writerArr = new EventPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                writerArr[i] = _writers.Get(properties[i]).Second;
            }
            return new WrapperEventBeanPropertyWriter(writerArr);
        }

        private static void CheckForRepeatedPropertyNames(EventType eventType, IDictionary<String, Object> properties)
        {
            foreach (var property in eventType.PropertyNames)
            {
                if (properties.Keys.Contains(property))
                {
                    throw new EPException("Property " + property + " occurs in both the underlying event and in the additional properties");
                }
            }
        }

        public class PropertyDescriptorComposite
        {
            public PropertyDescriptorComposite(Dictionary<String, EventPropertyDescriptor> propertyDescriptorMap, String[] propertyNames, EventPropertyDescriptor[] descriptors)
            {
                PropertyDescriptorMap = propertyDescriptorMap;
                PropertyNames = propertyNames;
                Descriptors = descriptors;
            }

            public Dictionary<string, EventPropertyDescriptor> PropertyDescriptorMap { get; private set; }

            public string[] PropertyNames { get; private set; }

            public EventPropertyDescriptor[] Descriptors { get; private set; }
        }
    }
}
