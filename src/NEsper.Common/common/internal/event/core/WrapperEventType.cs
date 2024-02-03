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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.wrap;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.core.EventTypeUtility;
using static com.espertech.esper.common.client.util.NameAccessModifier;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    /// An event type that adds zero or more fields to an existing event type.
    /// <para />The additional fields are represented as a Map. Any queries to event properties are first
    /// held against the additional fields, and secondly are handed through to the underlying event.
    /// <para />If this event type is to add information to another wrapper event type (wrapper to wrapper), then it is the
    /// responsibility of the creating logic to use the existing event type and add to it.
    /// <para />Uses a the map event type <seealso cref="com.espertech.esper.common.@internal.@event.map.MapEventType" /> to represent the mapped properties. This is because the additional properties
    /// can also be beans or complex types and the Map event type handles these nicely.
    /// </summary>
    public partial class WrapperEventType : EventTypeSPI
    {
        /// <summary>
        /// event type metadata
        /// </summary>
        private EventTypeMetadata _metadata;

        /// <summary>
        /// The underlying wrapped event type.
        /// </summary>
        private readonly EventType _underlyingEventType;

        /// <summary>
        /// The map event type that provides the additional properties.
        /// </summary>
        private readonly MapEventType _underlyingMapType;

        private string[] _propertyNames;
        private EventPropertyDescriptor[] _propertyDesc;
        private IDictionary<string, EventPropertyDescriptor> _propertyDescriptorMap;

        private readonly bool _isNoMapProperties;
        private readonly IDictionary<string, EventPropertyGetterSPI> _propertyGetterCache;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private EventPropertyDescriptor[] _writableProperties;
        private IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> _writers;

        private readonly string _startTimestampPropertyName;
        private readonly string _endTimestampPropertyName;
        private int _numPropertiesUnderlyingType;
        private IEnumerable<EventType> _deepSuperTypes;

        public WrapperEventType(
            EventTypeMetadata metadata,
            EventType underlyingEventType,
            IDictionary<string, object> properties,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            CheckForRepeatedPropertyNames(underlyingEventType, properties);

            _metadata = metadata;
            _underlyingEventType = underlyingEventType;
            var innerName = EventTypeNameUtil.GetWrapperInnerTypeName(metadata.Name);
            var ids = ComputeIdFromWrapped(metadata.AccessModifier, innerName, metadata);
            var metadataMapType = new EventTypeMetadata(
                innerName,
                _metadata.ModuleName,
                metadata.TypeClass,
                metadata.ApplicationType,
                metadata.AccessModifier,
                EventTypeBusModifier.NONBUS,
                false,
                ids);
            _underlyingMapType = new MapEventType(
                metadataMapType,
                properties,
                null,
                null,
                null,
                null,
                beanEventTypeFactory);
            _isNoMapProperties = properties.IsEmpty();
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();

            UpdatePropertySet();

            if (metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW) {
                _startTimestampPropertyName = underlyingEventType.StartTimestampPropertyName;
                _endTimestampPropertyName = underlyingEventType.EndTimestampPropertyName;
                ValidateTimestampProperties(
                    this,
                    _startTimestampPropertyName,
                    _endTimestampPropertyName);
            }
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            _metadata = _metadata.WithIds(publicId, protectedId);
        }

        private void CheckInitProperties()
        {
            if (_numPropertiesUnderlyingType != _underlyingEventType.PropertyDescriptors.Count) {
                UpdatePropertySet();
            }
        }

        private void UpdatePropertySet()
        {
            var compositeProperties = GetCompositeProperties(
                _underlyingEventType,
                _underlyingMapType);
            _propertyNames = compositeProperties.PropertyNames;
            _propertyDescriptorMap = compositeProperties.PropertyDescriptorMap;
            _propertyDesc = compositeProperties.Descriptors;
            _numPropertiesUnderlyingType = _underlyingEventType.PropertyDescriptors.Count;
        }

        private static PropertyDescriptorComposite GetCompositeProperties(
            EventType underlyingEventType,
            MapEventType underlyingMapType)
        {
            IList<string> propertyNames = new List<string>();
            propertyNames.AddAll(Arrays.AsList(underlyingEventType.PropertyNames));
            propertyNames.AddAll(Arrays.AsList(underlyingMapType.PropertyNames));
            var propertyNamesArr = propertyNames.ToArray();

            IList<EventPropertyDescriptor> propertyDesc = new List<EventPropertyDescriptor>();
            var propertyDescriptorMap =
                new Dictionary<string, EventPropertyDescriptor>();
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

        public string StartTimestampPropertyName => _startTimestampPropertyName;

        public string EndTimestampPropertyName => _endTimestampPropertyName;

        public IEnumerable<EventType> DeepSuperTypes => null;

        public ICollection<EventType> DeepSuperTypesCollection => EmptySet<EventType>.Instance;

        public string Name => _metadata.Name;

        public EventPropertyGetterSPI GetGetterSPI(string property)
        {
            var cachedGetter = _propertyGetterCache.Get(property);
            if (cachedGetter != null) {
                return cachedGetter;
            }

            if (_underlyingMapType.IsProperty(property) && property.IndexOf('?') == -1) {
                var mapGetter = _underlyingMapType.GetGetterSPI(property);
                var getter = new WrapperMapPropertyGetter(
                    this,
                    _eventBeanTypedEventFactory,
                    _underlyingMapType,
                    mapGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }
            else if (_underlyingEventType.IsProperty(property)) {
                var underlyingGetter = ((EventTypeSPI)_underlyingEventType).GetGetterSPI(property);
                var getter = new WrapperUnderlyingPropertyGetter(this, underlyingGetter);
                _propertyGetterCache.Put(property, getter);
                return getter;
            }
            else {
                return null;
            }
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            return GetGetterSPI(propertyName);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedProperty)
        {
            var undMapped =
                ((EventTypeSPI)_underlyingEventType).GetGetterMappedSPI(mappedProperty);
            if (undMapped != null) {
                return new WrapperGetterMapped(undMapped);
            }

            var decoMapped = _underlyingMapType.GetGetterMappedSPI(mappedProperty);
            if (decoMapped != null) {
                return new ProxyEventPropertyGetterMappedSPI() {
                    ProcGet = (
                        theEvent,
                        mapKey) => {
                        if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = _eventBeanTypedEventFactory.AdapterForTypedMap(map, _underlyingMapType);
                        return decoMapped.Get(wrapped, mapKey);
                    },

                    ProcEventBeanGetMappedCodegen = (
                        codegenMethodScope,
                        codegenClassScope,
                        beanExpression,
                        key) => {
                        var factory =
                            codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
                        var eventType = codegenClassScope.AddDefaultFieldUnshared<EventType>(
                            true,
                            ResolveTypeCodegen(_underlyingEventType, EPStatementInitServicesConstants.REF));
                        var method = codegenMethodScope
                            .MakeChild(typeof(object), typeof(WrapperEventType), codegenClassScope)
                            .AddParam<EventBean>("theEvent")
                            .AddParam<string>("mapKey")
                            .Block
                            .DeclareVar<DecoratingEventBean>(
                                "wrapperEvent",
                                Cast(typeof(DecoratingEventBean), Ref("theEvent")))
                            .DeclareVar(
                                typeof(IDictionary<object, object>),
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
            var undIndexed =
                ((EventTypeSPI)_underlyingEventType).GetGetterIndexedSPI(indexedProperty);
            if (undIndexed != null) {
                return new WrapperGetterIndexed(undIndexed);
            }

            var decoIndexed = _underlyingMapType.GetGetterIndexedSPI(indexedProperty);
            if (decoIndexed != null) {
                return new ProxyEventPropertyGetterIndexedSPI() {
                    ProcGet = (
                        theEvent,
                        index) => {
                        if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = _eventBeanTypedEventFactory.AdapterForTypedMap(map, _underlyingMapType);
                        return decoIndexed.Get(wrapped, index);
                    },

                    ProcEventBeanGetIndexedCodegen = (
                        codegenMethodScope,
                        codegenClassScope,
                        beanExpression,
                        key) => {
                        var factory =
                            codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
                        var eventType = codegenClassScope.AddDefaultFieldUnshared(
                            true,
                            typeof(EventType),
                            ResolveTypeCodegen(_underlyingEventType, EPStatementInitServicesConstants.REF));
                        var method = codegenMethodScope
                            .MakeChild(typeof(object), typeof(WrapperEventType), codegenClassScope)
                            .AddParam<EventBean>("theEvent")
                            .AddParam<int>("index")
                            .Block
                            .DeclareVar<DecoratingEventBean>(
                                "wrapperEvent",
                                Cast(typeof(DecoratingEventBean), Ref("theEvent")))
                            .DeclareVar(
                                typeof(IDictionary<object, object>),
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

        public string[] PropertyNames {
            get {
                CheckInitProperties();
                return _propertyNames;
            }
        }

        public Type GetPropertyType(string propertyName)
        {
            if (_underlyingEventType.IsProperty(propertyName)) {
                return _underlyingEventType.GetPropertyType(propertyName);
            }
            else if (_underlyingMapType.IsProperty(propertyName)) {
                return _underlyingMapType.GetPropertyType(propertyName);
            }
            else {
                return null;
            }
        }

        public IList<EventType> SuperTypes => null;


        public Type UnderlyingType {
            get {
                // If the additional properties are empty, such as when wrapping a native event by means of wildcard-only select
                // then the underlying type is simply the wrapped type.
                if (_isNoMapProperties) {
                    return _underlyingEventType.UnderlyingType;
                }

                return typeof(Pair<object, IDictionary<string, object>>);
            }
        }

        public bool IsNoMapProperties => _isNoMapProperties;

        /// <summary>
        /// Returns the wrapped event type.
        /// </summary>
        /// <value>wrapped type</value>
        public EventType UnderlyingEventType => _underlyingEventType;

        /// <summary>
        /// Returns the map type.
        /// </summary>
        /// <value>map type providing additional properties.</value>
        public MapEventType UnderlyingMapType => _underlyingMapType;

        public bool IsProperty(string property)
        {
            return _underlyingEventType.IsProperty(property) ||
                   _underlyingMapType.IsProperty(property);
        }

        public override string ToString()
        {
            return "WrapperEventType " +
                   "name=" +
                   Name +
                   " " +
                   "underlyingEventType=(" +
                   _underlyingEventType +
                   ") " +
                   "underlyingMapType=(" +
                   _underlyingMapType +
                   ")";
        }

        public ExprValidationException EqualsCompareType(EventType otherEventType)
        {
            if (this == otherEventType) {
                return null;
            }

            if (!(otherEventType is WrapperEventType other)) {
                return new ExprValidationException("Expected a wrapper event type but received " + otherEventType);
            }

            var underlyingMapCompare =
                other._underlyingMapType.EqualsCompareType(_underlyingMapType);
            if (underlyingMapCompare != null) {
                return underlyingMapCompare;
            }

            if (!(other._underlyingEventType is EventTypeSPI otherUnderlying) ||
                !(_underlyingEventType is EventTypeSPI thisUnderlying)) {
                if (!other._underlyingEventType.Equals(_underlyingEventType)) {
                    return new ExprValidationException("Wrapper underlying type mismatches");
                }

                return null;
            }

            return thisUnderlying.EqualsCompareType(otherUnderlying);
        }

        public EventTypeMetadata Metadata => _metadata;

        public IList<EventPropertyDescriptor> PropertyDescriptors {
            get {
                CheckInitProperties();
                return _propertyDesc;
            }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            CheckInitProperties();
            return _propertyDescriptorMap.Get(propertyName);
        }

        public FragmentEventType GetFragmentType(string property)
        {
            var fragment = _underlyingEventType.GetFragmentType(property);
            if (fragment != null) {
                return fragment;
            }

            return _underlyingMapType.GetFragmentType(property);
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

        public EventPropertyDescriptor[] WriteableProperties {
            get {
                if (_writableProperties == null) {
                    InitializeWriters();
                }

                return _writableProperties;
            }
        }

        private void InitializeWriters()
        {
            IList<EventPropertyDescriptor> writables = new List<EventPropertyDescriptor>();
            IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> writerMap =
                new Dictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>>();
            writables.AddAll(Arrays.AsList(_underlyingMapType.WriteableProperties));

            foreach (var writableMapProp in _underlyingMapType.WriteableProperties) {
                var propertyName = writableMapProp.PropertyName;
                writables.Add(writableMapProp);
                EventPropertyWriterSPI writer = new ProxyEventPropertyWriterSPI() {
                    ProcWrite = (
                        value,
                        target) => {
                        var decorated = (DecoratingEventBean)target;
                        decorated.DecoratingProperties.Put(propertyName, value);
                    },

                    ProcWriteCodegen = (
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
                    new Pair<EventPropertyDescriptor, EventPropertyWriterSPI>(writableMapProp, writer));
            }

            if (_underlyingEventType is EventTypeSPI spi) {
                foreach (var writableUndProp in spi.WriteableProperties) {
                    var propertyName = writableUndProp.PropertyName;
                    EventPropertyWriter innerWriter = spi.GetWriter(propertyName);
                    if (innerWriter == null) {
                        continue;
                    }

                    writables.Add(writableUndProp);
                    EventPropertyWriterSPI writer = new ProxyEventPropertyWriterSPI() {
                        ProcWrite = (
                            value,
                            target) => {
                            var decorated = (DecoratingEventBean)target;
                            innerWriter.Write(value, decorated.UnderlyingEvent);
                        },

                        ProcWriteCodegen = (
                            assigned,
                            und,
                            target,
                            parent,
                            classScope) => {
                            var decorated = Cast(typeof(DecoratingEventBean), target);
                            var underlyingBean = ExprDotName(decorated, "UnderlyingEvent");
                            var underlying = ExprDotName(underlyingBean, "Underlying");
                            var casted = Cast(_underlyingEventType.UnderlyingType, underlying);
                            return ((EventPropertyWriterSPI)innerWriter).WriteCodegen(
                                assigned,
                                casted,
                                target,
                                parent,
                                classScope);
                        }
                    };
                    writerMap.Put(
                        propertyName,
                        new Pair<EventPropertyDescriptor, EventPropertyWriterSPI>(writableUndProp, writer));
                }
            }

            _writers = writerMap;
            _writableProperties = writables.ToArray();
        }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            if (_writableProperties == null) {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++) {
                if (_underlyingMapType.GetWritableProperty(properties[i]) == null) {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap) {
                if (!(_underlyingEventType is EventTypeSPI spi)) {
                    return null;
                }

                for (var i = 0; i < properties.Length; i++) {
                    if (spi.GetWritableProperty(properties[i]) == null) {
                        isOnlyUnderlying = false;
                    }
                }
            }

            if (isOnlyMap) {
                return new WrapperEventBeanMapCopyMethodForge(this);
            }

            var undCopyMethod = ((EventTypeSPI)_underlyingEventType).GetCopyMethodForge(properties);
            if (undCopyMethod == null) {
                return null;
            }

            if (isOnlyUnderlying) {
                return new WrapperEventBeanUndCopyMethodForge(this, undCopyMethod);
            }
            else {
                return new WrapperEventBeanCopyMethodForge(this, undCopyMethod);
            }
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

                if (_underlyingMapType.GetWritableProperty(properties[i]) == null) {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap) {
                var spi = (EventTypeSPI)_underlyingEventType;
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
                var spi = (EventTypeSPI)_underlyingEventType;
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

        private void CheckForRepeatedPropertyNames(
            EventType eventType,
            IDictionary<string, object> properties)
        {
            foreach (var property in eventType.PropertyNames) {
                if (properties.Keys.Contains(property)) {
                    throw new EPException(
                        "Property '" +
                        property +
                        "' occurs in both the underlying event and in the additional properties");
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
    }
} // end of namespace