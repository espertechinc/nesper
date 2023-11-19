///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        protected EventTypeMetadata metadata;

        /// <summary>
        /// The underlying wrapped event type.
        /// </summary>
        protected readonly EventType underlyingEventType;

        /// <summary>
        /// The map event type that provides the additional properties.
        /// </summary>
        protected readonly MapEventType underlyingMapType;

        private string[] propertyNames;
        private EventPropertyDescriptor[] propertyDesc;
        private IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap;

        private readonly bool isNoMapProperties;
        private readonly IDictionary<string, EventPropertyGetterSPI> propertyGetterCache;
        protected readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private EventPropertyDescriptor[] writableProperties;
        private IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> writers;

        private string startTimestampPropertyName;
        private string endTimestampPropertyName;
        private int numPropertiesUnderlyingType;
        private IEnumerable<EventType> deepSuperTypes;

        public WrapperEventType(
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
            isNoMapProperties = properties.IsEmpty();
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();

            UpdatePropertySet();

            if (metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW) {
                startTimestampPropertyName = underlyingEventType.StartTimestampPropertyName;
                endTimestampPropertyName = underlyingEventType.EndTimestampPropertyName;
                ValidateTimestampProperties(
                    this,
                    startTimestampPropertyName,
                    endTimestampPropertyName);
            }
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            metadata = metadata.WithIds(publicId, protectedId);
        }

        private void CheckInitProperties()
        {
            if (numPropertiesUnderlyingType != underlyingEventType.PropertyDescriptors.Count) {
                UpdatePropertySet();
            }
        }

        private void UpdatePropertySet()
        {
            var compositeProperties = GetCompositeProperties(
                underlyingEventType,
                underlyingMapType);
            propertyNames = compositeProperties.PropertyNames;
            propertyDescriptorMap = compositeProperties.PropertyDescriptorMap;
            propertyDesc = compositeProperties.Descriptors;
            numPropertiesUnderlyingType = underlyingEventType.PropertyDescriptors.Count;
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

        public string StartTimestampPropertyName => startTimestampPropertyName;

        public string EndTimestampPropertyName => endTimestampPropertyName;

        public IEnumerable<EventType> DeepSuperTypes => null;

        public ICollection<EventType> DeepSuperTypesCollection => EmptySet<EventType>.Instance;

        public string Name => metadata.Name;

        public EventPropertyGetterSPI GetGetterSPI(string property)
        {
            var cachedGetter = propertyGetterCache.Get(property);
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
                propertyGetterCache.Put(property, getter);
                return getter;
            }
            else if (underlyingEventType.IsProperty(property)) {
                var underlyingGetter = ((EventTypeSPI)underlyingEventType).GetGetterSPI(property);
                var getter = new WrapperUnderlyingPropertyGetter(this, underlyingGetter);
                propertyGetterCache.Put(property, getter);
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
                ((EventTypeSPI)underlyingEventType).GetGetterMappedSPI(mappedProperty);
            if (undMapped != null) {
                return new WrapperGetterMapped(undMapped);
            }

            var decoMapped = underlyingMapType.GetGetterMappedSPI(mappedProperty);
            if (decoMapped != null) {
                return new ProxyEventPropertyGetterMappedSPI() {
                    ProcGet = (
                        theEvent,
                        mapKey) => {
                        if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType);
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
                            ResolveTypeCodegen(underlyingEventType, EPStatementInitServicesConstants.REF));
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
                ((EventTypeSPI)underlyingEventType).GetGetterIndexedSPI(indexedProperty);
            if (undIndexed != null) {
                return new WrapperGetterIndexed(undIndexed);
            }

            var decoIndexed = underlyingMapType.GetGetterIndexedSPI(indexedProperty);
            if (decoIndexed != null) {
                return new ProxyEventPropertyGetterIndexedSPI() {
                    ProcGet = (
                        theEvent,
                        index) => {
                        if (!(theEvent is DecoratingEventBean wrapperEvent)) {
                            throw new PropertyAccessException("Mismatched property getter to EventBean type");
                        }

                        var map = wrapperEvent.DecoratingProperties;
                        EventBean wrapped = eventBeanTypedEventFactory.AdapterForTypedMap(map, underlyingMapType);
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
                            ResolveTypeCodegen(underlyingEventType, EPStatementInitServicesConstants.REF));
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
                return propertyNames;
            }
        }

        public Type GetPropertyType(string propertyName)
        {
            if (underlyingEventType.IsProperty(propertyName)) {
                return underlyingEventType.GetPropertyType(propertyName);
            }
            else if (underlyingMapType.IsProperty(propertyName)) {
                return underlyingMapType.GetPropertyType(propertyName);
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
                if (isNoMapProperties) {
                    return underlyingEventType.UnderlyingType;
                }

                return typeof(Pair<object, IDictionary<string, object>>);
            }
        }

        public bool IsNoMapProperties => isNoMapProperties;

        /// <summary>
        /// Returns the wrapped event type.
        /// </summary>
        /// <value>wrapped type</value>
        public EventType UnderlyingEventType => underlyingEventType;

        /// <summary>
        /// Returns the map type.
        /// </summary>
        /// <value>map type providing additional properties.</value>
        public MapEventType UnderlyingMapType => underlyingMapType;

        public bool IsProperty(string property)
        {
            return underlyingEventType.IsProperty(property) ||
                   underlyingMapType.IsProperty(property);
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

        public ExprValidationException EqualsCompareType(EventType otherEventType)
        {
            if (this == otherEventType) {
                return null;
            }

            if (!(otherEventType is WrapperEventType other)) {
                return new ExprValidationException("Expected a wrapper event type but received " + otherEventType);
            }

            var underlyingMapCompare =
                other.underlyingMapType.EqualsCompareType(underlyingMapType);
            if (underlyingMapCompare != null) {
                return underlyingMapCompare;
            }

            if (!(other.underlyingEventType is EventTypeSPI otherUnderlying) ||
                !(underlyingEventType is EventTypeSPI thisUnderlying)) {
                if (!other.underlyingEventType.Equals(underlyingEventType)) {
                    return new ExprValidationException("Wrapper underlying type mismatches");
                }

                return null;
            }

            return thisUnderlying.EqualsCompareType(otherUnderlying);
        }

        public EventTypeMetadata Metadata => metadata;

        public IList<EventPropertyDescriptor> PropertyDescriptors {
            get {
                CheckInitProperties();
                return propertyDesc;
            }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            CheckInitProperties();
            return propertyDescriptorMap.Get(propertyName);
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
            if (writableProperties == null) {
                InitializeWriters();
            }

            var pair = writers.Get(propertyName);
            return pair?.Second;
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (writableProperties == null) {
                InitializeWriters();
            }

            var pair = writers.Get(propertyName);
            return pair?.First;
        }

        public EventPropertyDescriptor[] WriteableProperties {
            get {
                if (writableProperties == null) {
                    InitializeWriters();
                }

                return writableProperties;
            }
        }

        private void InitializeWriters()
        {
            IList<EventPropertyDescriptor> writables = new List<EventPropertyDescriptor>();
            IDictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>> writerMap =
                new Dictionary<string, Pair<EventPropertyDescriptor, EventPropertyWriterSPI>>();
            writables.AddAll(Arrays.AsList(underlyingMapType.WriteableProperties));

            foreach (var writableMapProp in underlyingMapType.WriteableProperties) {
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

            if (underlyingEventType is EventTypeSPI spi) {
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
                            var casted = Cast(underlyingEventType.UnderlyingType, underlying);
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

            writers = writerMap;
            writableProperties = writables.ToArray();
        }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            if (writableProperties == null) {
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
                if (!(underlyingEventType is EventTypeSPI spi)) {
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

            var undCopyMethod = ((EventTypeSPI)underlyingEventType).GetCopyMethodForge(properties);
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
            if (writableProperties == null) {
                InitializeWriters();
            }

            var isOnlyMap = true;
            for (var i = 0; i < properties.Length; i++) {
                if (!writers.ContainsKey(properties[i])) {
                    return null;
                }

                if (underlyingMapType.GetWritableProperty(properties[i]) == null) {
                    isOnlyMap = false;
                }
            }

            var isOnlyUnderlying = true;
            if (!isOnlyMap) {
                var spi = (EventTypeSPI)underlyingEventType;
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
                var spi = (EventTypeSPI)underlyingEventType;
                var undWriter = spi.GetWriter(properties);
                if (undWriter == null) {
                    return undWriter;
                }

                return new WrapperEventBeanUndWriter(undWriter);
            }

            var writerArr = new EventPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writerArr[i] = writers.Get(properties[i]).Second;
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