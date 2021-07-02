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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.wrap;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.client.util.NameAccessModifier;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     An event type that adds zero or more fields to an existing event type.
    ///     <para>
    ///         The additional fields are represented as a Map. Any queries to event properties are first
    ///         held against the additional fields, and secondly are handed through to the underlying event.
    ///     </para>
    ///     <para>
    ///         If this event type is to add information to another wrapper event type (wrapper to wrapper), then it is the
    ///         responsibility of the creating logic to use the existing event type and add to it.
    ///     </para>
    ///     <para>
    ///         Uses a the map event type <seealso cref="com.espertech.esper.common.@internal.@event.map.MapEventType" /> to
    ///         represent the mapped properties. This is because the additional properties
    ///         can also be beans or complex types and the Map event type handles these nicely.
    ///     </para>
    /// </summary>
    public class WrapperEventType : EventTypeSPI
    {
        internal readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

        private readonly bool _isNoMapProperties;
        private readonly IDictionary<string, EventPropertyGetterSPI> _propertyGetterCache;

        /// <summary>
        ///     The underlying wrapped event type.
        /// </summary>
        internal readonly EventType underlyingEventType;

        /// <summary>
        ///     The map event type that provides the additional properties.
        /// </summary>
        internal readonly MapEventType underlyingMapType;

        /// <summary>
        ///     event type metadata
        /// </summary>
        internal EventTypeMetadata metadata;

        private int _numPropertiesUnderlyingType;
        private EventPropertyDescriptor[] _propertyDesc;
        private IDictionary<string, EventPropertyDescriptor> _propertyDescriptorMap;

        private string[] _propertyNames;

        private EventPropertyDescriptor[] _writableProperties;
        private IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> _writers;

        internal WrapperEventType(
            EventTypeMetadata metadata,
            EventType underlyingEventType,
            IDictionary<string, object> properties,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            CheckForRepeatedPropertyNames(underlyingEventType, properties);

            this.metadata = metadata;
            this.underlyingEventType = underlyingEventType;
            var innerName = EventTypeNameUtil.GetWrapperInnerTypeName(metadata.Name);
            var ids = ComputeIdFromWrapped(metadata.AccessModifier, innerName, metadata);
            var metadataMapType = new EventTypeMetadata(
                innerName,
                this.metadata.ModuleName,
                metadata.TypeClass,
                metadata.ApplicationType,
                metadata.AccessModifier,
                EventTypeBusModifier.NONBUS,
                false,
                ids);
            underlyingMapType = new MapEventType(
                metadataMapType,
                properties,
                null,
                null,
                null,
                null,
                beanEventTypeFactory);
            _isNoMapProperties = properties.IsEmpty();
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();

            UpdatePropertySet();

            if (metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW) {
                StartTimestampPropertyName = underlyingEventType.StartTimestampPropertyName;
                EndTimestampPropertyName = underlyingEventType.EndTimestampPropertyName;
                EventTypeUtility.ValidateTimestampProperties(
                    this,
                    StartTimestampPropertyName,
                    EndTimestampPropertyName);
            }
        }

        /// <summary>
        ///     Returns the wrapped event type.
        /// </summary>
        /// <returns>wrapped type</returns>
        public EventType UnderlyingEventType => underlyingEventType;

        /// <summary>
        ///     Returns the map type.
        /// </summary>
        /// <returns>map type providing additional properties.</returns>
        public MapEventType UnderlyingMapType => underlyingMapType;

        public IEnumerable<EventType> DeepSuperTypes => null;

        public void SetMetadataId(
            long publicId,
            long internalId)
        {
            metadata = metadata.WithIds(publicId, internalId);
        }

        public string StartTimestampPropertyName { get; }

        public string EndTimestampPropertyName { get; }

        public ICollection<EventType> DeepSuperTypesCollection => Collections.GetEmptySet<EventType>();

        public string Name => metadata.Name;

        public EventPropertyGetterSPI GetGetterSPI(string property)
        {
            var cachedGetter = _propertyGetterCache.Get(property);
            if (cachedGetter != null) {
                return cachedGetter;
            }

            if (underlyingMapType.IsProperty(property) && property.IndexOf('?') == -1) {
                var mapGetter = underlyingMapType.GetGetterSPI(property);
                var getter = new WrapperMapPropertyGetter(
                    this,
                    eventBeanTypedEventFactory,
                    underlyingMapType,
                    mapGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }

            if (underlyingEventType.IsProperty(property)) {
                var underlyingGetter = ((EventTypeSPI) underlyingEventType).GetGetterSPI(property);
                var getter = new WrapperUnderlyingPropertyGetter(this, underlyingGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }

            return null;
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            return GetGetterSPI(propertyName);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedProperty)
        {
            var undMapped = ((EventTypeSPI) underlyingEventType).GetGetterMappedSPI(mappedProperty);
            if (undMapped != null) {
                return new WrapperGetterMapped(undMapped);
            }

            var decoMapped = underlyingMapType.GetGetterMappedSPI(mappedProperty);
            if (decoMapped != null) {
                return new ProxyEventPropertyGetterMappedSPI {
                    procGet = (
                        theEvent,
                        mapKey) => {
                        if (!(theEvent is DecoratingEventBean)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var wrapperEvent = (DecoratingEventBean) theEvent;
                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType);
                        return decoMapped.Get(wrapped, mapKey);
                    },

                    procEventBeanGetMappedCodegen = (
                        codegenMethodScope,
                        codegenClassScope,
                        beanExpression,
                        key) => {
                        var factory =
                            codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
                        var eventType = codegenClassScope.AddDefaultFieldUnshared<EventType>(
                            true,
                            EventTypeUtility.ResolveTypeCodegen(
                                underlyingEventType,
                                EPStatementInitServicesConstants.REF));
                        var method = codegenMethodScope
                            .MakeChild(typeof(object), typeof(WrapperEventType), codegenClassScope)
                            .AddParam(typeof(EventBean), "theEvent")
                            .AddParam(typeof(string), "mapKey")
                            .Block
                            .DeclareVar<DecoratingEventBean>(
                                "wrapperEvent",
                                Cast(typeof(DecoratingEventBean), Ref("theEvent")))
                            .DeclareVar<IDictionary<object, object>>(
                                "map",
                                ExprDotName(Ref("wrapperEvent"), "DecoratingProperties"))
                            .DeclareVar<EventBean>(
                                "wrapped",
                                ExprDotMethod(factory, "AdapterForTypedMap", Ref("map"), eventType))
                            .MethodReturn(
                                decoMapped.EventBeanGetMappedCodegen(
                                    codegenMethodScope,
                                    codegenClassScope,
                                    Ref("wrapped"),
                                    Ref("mapKey")));
                        return LocalMethodBuild(method).Pass(beanExpression).Pass(key).Call();
                    }
                };
            }

            return null;
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedProperty)
        {
            return GetGetterMappedSPI(mappedProperty);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            return GetGetterIndexedSPI(indexedPropertyName);
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string indexedProperty)
        {
            var undIndexed = ((EventTypeSPI) underlyingEventType).GetGetterIndexedSPI(indexedProperty);
            if (undIndexed != null) {
                return new WrapperGetterIndexed(undIndexed);
            }

            var decoIndexed = underlyingMapType.GetGetterIndexedSPI(indexedProperty);
            if (decoIndexed != null) {
                return new ProxyEventPropertyGetterIndexedSPI {
                    procGet = (
                        theEvent,
                        index) => {
                        if (!(theEvent is DecoratingEventBean)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var wrapperEvent = (DecoratingEventBean) theEvent;
                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType);
                        return decoIndexed.Get(wrapped, index);
                    },

                    procEventBeanGetIndexedCodegen = (
                        codegenMethodScope,
                        codegenClassScope,
                        beanExpression,
                        key) => {
                        var factory =
                            codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
                        var eventType = codegenClassScope.AddDefaultFieldUnshared<EventType>(
                            true,
                            EventTypeUtility.ResolveTypeCodegen(
                                underlyingEventType,
                                EPStatementInitServicesConstants.REF));
                        var method = codegenMethodScope
                            .MakeChild(typeof(object), typeof(WrapperEventType), codegenClassScope)
                            .AddParam(typeof(EventBean), "theEvent")
                            .AddParam(typeof(int), "index")
                            .Block
                            .DeclareVar<DecoratingEventBean>(
                                "wrapperEvent",
                                Cast(typeof(DecoratingEventBean), Ref("theEvent")))
                            .DeclareVar<IDictionary<object, object>>(
                                "map",
                                ExprDotName(Ref("wrapperEvent"), "DecoratingProperties"))
                            .DeclareVar<EventBean>(
                                "wrapped",
                                ExprDotMethod(factory, "AdapterForTypedMap", Ref("map"), eventType))
                            .MethodReturn(
                                decoIndexed.EventBeanGetIndexedCodegen(
                                    codegenMethodScope,
                                    codegenClassScope,
                                    Ref("wrapped"),
                                    Ref("index")));
                        return LocalMethodBuild(method).Pass(beanExpression).Pass(key).Call();
                    }
                };
            }

            return null;
        }

        public Type GetPropertyType(string property)
        {
            if (underlyingEventType.IsProperty(property)) {
                return underlyingEventType.GetPropertyType(property);
            }

            if (underlyingMapType.IsProperty(property)) {
                return underlyingMapType.GetPropertyType(property);
            }

            return null;
        }

        public IList<EventType> SuperTypes => null;

        public bool IsProperty(string property)
        {
            return underlyingEventType.IsProperty(property) ||
                   underlyingMapType.IsProperty(property);
        }

        public ExprValidationException EqualsCompareType(EventType otherEventType)
        {
            if (this == otherEventType) {
                return null;
            }

            if (!(otherEventType is WrapperEventType)) {
                return new ExprValidationException("Expected a wrapper event type but received " + otherEventType);
            }

            var other = (WrapperEventType) otherEventType;
            var underlyingMapCompare = other.underlyingMapType.EqualsCompareType(underlyingMapType);
            if (underlyingMapCompare != null) {
                return underlyingMapCompare;
            }

            if (!(other.underlyingEventType is EventTypeSPI) || !(underlyingEventType is EventTypeSPI)) {
                if (!other.underlyingEventType.Equals(underlyingEventType)) {
                    return new ExprValidationException("Wrapper underlying type mismatches");
                }

                return null;
            }

            var otherUnderlying = (EventTypeSPI) other.underlyingEventType;
            var thisUnderlying = (EventTypeSPI) underlyingEventType;
            return otherUnderlying.EqualsCompareType(thisUnderlying);
        }

        public EventTypeMetadata Metadata => metadata;

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            CheckInitProperties();
            return _propertyDescriptorMap.Get(propertyName);
        }

        public FragmentEventType GetFragmentType(string property)
        {
            var fragment = underlyingEventType.GetFragmentType(property);
            if (fragment != null) {
                return fragment;
            }

            return underlyingMapType.GetFragmentType(property);
        }

        public EventPropertyWriterSPI GetWriter(string propertyName)
        {
            if (_writableProperties == null) {
                InitializeWriters();
            }

            var pair = _writers.Get(propertyName);

            return pair?.Second;
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (_writableProperties == null) {
                InitializeWriters();
            }

            var pair = _writers.Get(propertyName);

            return pair?.First;
        }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            if (_writableProperties == null) {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++) {
                if (underlyingMapType.GetWritableProperty(properties[i]) == null) {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap) {
                if (!(underlyingEventType is EventTypeSPI)) {
                    return null;
                }

                var spi = (EventTypeSPI) underlyingEventType;
                for (var i = 0; i < properties.Length; i++) {
                    if (spi.GetWritableProperty(properties[i]) == null) {
                        isOnlyUnderlying = false;
                    }
                }
            }

            if (isOnlyMap) {
                return new WrapperEventBeanMapCopyMethodForge(this);
            }

            var undCopyMethod = ((EventTypeSPI) underlyingEventType).GetCopyMethodForge(properties);
            if (undCopyMethod == null) {
                return null;
            }

            if (isOnlyUnderlying) {
                return new WrapperEventBeanUndCopyMethodForge(this, undCopyMethod);
            }

            return new WrapperEventBeanCopyMethodForge(this, undCopyMethod);
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            if (_writableProperties == null) {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++) {
                if (!_writers.ContainsKey(properties[i])) {
                    return null;
                }

                if (underlyingMapType.GetWritableProperty(properties[i]) == null) {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap) {
                var spi = (EventTypeSPI) underlyingEventType;
                for (var i = 0; i < properties.Length; i++) {
                    if (spi.GetWritableProperty(properties[i]) == null) {
                        isOnlyUnderlying = false;
                    }
                }
            }

            if (isOnlyMap) {
                return new WrapperEventBeanMapWriter(properties);
            }

            if (isOnlyUnderlying) {
                var spi = (EventTypeSPI) underlyingEventType;
                var undWriter = spi.GetWriter(properties);
                if (undWriter == null) {
                    return undWriter;
                }

                return new WrapperEventBeanUndWriter(undWriter);
            }

            var writerArr = new EventPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writerArr[i] = _writers.Get(properties[i]).Second;
            }

            return new WrapperEventBeanPropertyWriter(writerArr);
        }

        public string[] PropertyNames {
            get {
                CheckInitProperties();
                return _propertyNames;
            }
        }

        public Type UnderlyingType {
            get {
                // If the additional properties are empty, such as when wrapping a native event by means of wildcard-only select
                // then the underlying type is simply the wrapped type.
                if (_isNoMapProperties) {
                    return underlyingEventType.UnderlyingType;
                }

                return typeof(Pair<object, IDictionary<string, object>>);
            }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors {
            get {
                CheckInitProperties();
                return _propertyDesc;
            }
        }

        public EventPropertyDescriptor[] WriteableProperties {
            get {
                if (_writableProperties == null) {
                    InitializeWriters();
                }

                return _writableProperties;
            }
        }

        private void CheckInitProperties()
        {
            if (_numPropertiesUnderlyingType != underlyingEventType.PropertyDescriptors.Count) {
                UpdatePropertySet();
            }
        }

        private void UpdatePropertySet()
        {
            var compositeProperties = GetCompositeProperties(underlyingEventType, underlyingMapType);
            _propertyNames = compositeProperties.PropertyNames;
            _propertyDescriptorMap = compositeProperties.PropertyDescriptorMap;
            _propertyDesc = compositeProperties.Descriptors;
            _numPropertiesUnderlyingType = underlyingEventType.PropertyDescriptors.Count;
        }

        private static PropertyDescriptorComposite GetCompositeProperties(
            EventType underlyingEventType,
            MapEventType underlyingMapType)
        {
            IList<string> propertyNames = new List<string>();
            propertyNames.AddAll(underlyingEventType.PropertyNames);
            propertyNames.AddAll(underlyingMapType.PropertyNames);
            var propertyNamesArr = propertyNames.ToArray();

            IList<EventPropertyDescriptor> propertyDesc = new List<EventPropertyDescriptor>();
            var propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            foreach (var eventProperty in underlyingEventType.PropertyDescriptors) {
                propertyDesc.Add(eventProperty);
                propertyDescriptorMap.Put(eventProperty.PropertyName, eventProperty);
            }

            foreach (var mapProperty in underlyingMapType.PropertyDescriptors) {
                propertyDesc.Add(mapProperty);
                propertyDescriptorMap.Put(mapProperty.PropertyName, mapProperty);
            }

            var propertyDescArr = propertyDesc.ToArray();
            return new PropertyDescriptorComposite(propertyDescriptorMap, propertyNamesArr, propertyDescArr);
        }

        public override string ToString()
        {
            return "WrapperEventType " +
                   "name=" +
                   Name +
                   " " +
                   "underlyingEventType=(" +
                   underlyingEventType +
                   ") " +
                   "underlyingMapType=(" +
                   underlyingMapType +
                   ")";
        }

        private void InitializeWriters()
        {
            IList<EventPropertyDescriptor> writables = new List<EventPropertyDescriptor>();
            IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> writerMap =
                new Dictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>>();
            writables.AddAll(underlyingMapType.WriteableProperties);

            foreach (var writableMapProp in underlyingMapType.WriteableProperties) {
                var propertyName = writableMapProp.PropertyName;
                writables.Add(writableMapProp);
                EventPropertyWriterSPI writer = new ProxyEventPropertyWriterSPI {
                    procWrite = (
                        value,
                        target) => {
                        var decorated = (DecoratingEventBean) target;
                        decorated.DecoratingProperties.Put(propertyName, value);
                    },

                    procWriteCodegen = (
                        assigned,
                        und,
                        target,
                        parent,
                        classScope) => {
                        var decorated = Cast(typeof(DecoratingEventBean), target);
                        var decoratingProps = ExprDotName(decorated, "DecoratingProperties");
                        return ExprDotMethod(decoratingProps, "Put", Constant(propertyName), assigned);
                    }
                };
                writerMap.Put(
                    propertyName,
                    new Pair<EventPropertyDescriptor, EventPropertyWriterSPI>(
                        writableMapProp,
                        writer));
            }

            if (underlyingEventType is EventTypeSPI) {
                var spi = (EventTypeSPI) underlyingEventType;
                foreach (var writableUndProp in spi.WriteableProperties) {
                    var propertyName = writableUndProp.PropertyName;
                    EventPropertyWriter innerWriter = spi.GetWriter(propertyName);
                    if (innerWriter == null) {
                        continue;
                    }

                    writables.Add(writableUndProp);
                    EventPropertyWriterSPI writer = new ProxyEventPropertyWriterSPI {
                        procWrite = (
                            value,
                            target) => {
                            var decorated = (DecoratingEventBean) target;
                            innerWriter.Write(value, decorated.UnderlyingEvent);
                        },

                        procWriteCodegen = (
                            assigned,
                            und,
                            target,
                            parent,
                            classScope) => {
                            var decorated = Cast(typeof(DecoratingEventBean), target);
                            var underlyingBean = ExprDotName(decorated, "UnderlyingEvent");
                            var underlying = ExprDotName(underlyingBean, "Underlying");
                            var casted = Cast(underlyingEventType.UnderlyingType, underlying);
                            return ((EventPropertyWriterSPI) innerWriter).WriteCodegen(
                                assigned,
                                casted,
                                target,
                                parent,
                                classScope);
                        }
                    };
                    writerMap.Put(
                        propertyName,
                        new Pair<EventPropertyDescriptor, EventPropertyWriterSPI>(
                            writableUndProp,
                            writer));
                }
            }

            _writers = writerMap;
            _writableProperties = writables.ToArray();
        }

        private void CheckForRepeatedPropertyNames(
            EventType eventType,
            IDictionary<string, object> properties)
        {
            foreach (var property in eventType.PropertyNames) {
                if (properties.Keys.Contains(property)) {
                    throw new EPException(
                        $"Property '{property}' occurs in both the underlying event and in the additional properties");
                }
            }
        }

        public EventTypeIdPair ComputeIdFromWrapped(
            NameAccessModifier visibility,
            string innerName,
            EventTypeMetadata metadataWrapper)
        {
            if (visibility == TRANSIENT || visibility == PRIVATE) {
                return new EventTypeIdPair(metadataWrapper.EventTypeIdPair.PublicId, CRC32Util.ComputeCRC32(innerName));
            }

            return new EventTypeIdPair(CRC32Util.ComputeCRC32(innerName), -1);
        }

        public class PropertyDescriptorComposite
        {
            public PropertyDescriptorComposite(
                Dictionary<string, EventPropertyDescriptor> propertyDescriptorMap,
                string[] propertyNames,
                EventPropertyDescriptor[] descriptors)
            {
                PropertyDescriptorMap = propertyDescriptorMap;
                PropertyNames = propertyNames;
                Descriptors = descriptors;
            }

            public string[] PropertyNames { get; }

            public EventPropertyDescriptor[] Descriptors { get; }

            public Dictionary<string, EventPropertyDescriptor> PropertyDescriptorMap { get; }
        }
    }
} // end of namespace