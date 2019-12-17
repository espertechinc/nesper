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

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Implementation of the <seealso cref="EventType" /> interface for handling plain Maps containing name value pairs.
    /// </summary>
    public class MapEventType : BaseNestableEventType
    {
        private static readonly EventTypeNestableGetterFactory GETTER_FACTORY = new EventTypeNestableGetterFactoryMap();

        private IDictionary<string, Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>> _propertyWriters;
        private EventPropertyDescriptor[] _writablePropertyDescriptors;

        public MapEventType(
            EventTypeMetadata metadata,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSuperTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                metadata,
                propertyTypes,
                optionalSuperTypes,
                optionalDeepSuperTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                GETTER_FACTORY,
                beanEventTypeFactory)
        {
        }

        public override Type UnderlyingType => typeof(IDictionary<string, object>);

        public override EventBeanReader Reader => new MapEventBeanReader(this);

        public override EventPropertyDescriptor[] WriteableProperties {
            get {
                if (_writablePropertyDescriptors == null) {
                    InitializeWriters();
                }

                return _writablePropertyDescriptors;
            }
        }

        internal override void PostUpdateNestableTypes()
        {
        }

        public override EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            var pair = BaseNestableEventUtil.GetIndexedAndMappedProps(properties);

            if (pair.MapProperties.IsEmpty() && pair.ArrayProperties.IsEmpty()) {
                return new MapEventBeanCopyMethodForge(this, BeanEventTypeFactory.EventBeanTypedEventFactory);
            }

            return new MapEventBeanCopyMethodWithArrayMapForge(
                this,
                BeanEventTypeFactory.EventBeanTypedEventFactory,
                pair.MapProperties,
                pair.ArrayProperties);
        }

        public object GetValue(
            string propertyName,
            IDictionary<string, object> values)
        {
            var getter = (MapEventPropertyGetter) GetGetter(propertyName);
            return getter.GetMap(values);
        }

        public override EventPropertyWriterSPI GetWriter(string propertyName)
        {
            if (_writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var pair = _propertyWriters.Get(propertyName);
            if (pair != null) {
                return pair.Second;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty mapProp) {
                return new MapEventBeanPropertyWriterMapProp(mapProp.PropertyNameAtomic, mapProp.Key);
            }

            if (property is IndexedProperty indexedProp) {
                return new MapEventBeanPropertyWriterIndexedProp(indexedProp.PropertyNameAtomic, indexedProp.Index);
            }

            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (_writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var pair = _propertyWriters.Get(propertyName);
            if (pair != null) {
                return pair.First;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty mapProp) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                return new EventPropertyDescriptor(
                    mapProp.PropertyNameAtomic,
                    typeof(object),
                    null,
                    false,
                    true,
                    false,
                    true,
                    false);
            }

            if (property is IndexedProperty indexedProp) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                return new EventPropertyDescriptor(
                    indexedProp.PropertyNameAtomic,
                    typeof(object),
                    null,
                    true,
                    false,
                    true,
                    false,
                    false);
            }

            return null;
        }

        public override EventBeanWriter GetWriter(string[] properties)
        {
            if (_writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var allSimpleProps = true;
            var writers = new MapEventBeanPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                var writerPair = _propertyWriters.Get(properties[i]);
                if (writerPair != null) {
                    writers[i] = writerPair.Second;
                }
                else {
                    writers[i] = (MapEventBeanPropertyWriter) GetWriter(properties[i]);
                    if (writers[i] == null) {
                        return null;
                    }

                    allSimpleProps = false;
                }
            }

            if (allSimpleProps) {
                return new MapEventBeanWriterSimpleProps(properties);
            }

            return new MapEventBeanWriterPerProp(writers);
        }

        private void InitializeWriters()
        {
            IList<EventPropertyDescriptor> writeableProps = new List<EventPropertyDescriptor>();
            IDictionary<string, Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>> propertWritersMap =
                new Dictionary<string, Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>>();
            foreach (var prop in PropertyDescriptors) {
                writeableProps.Add(prop);
                var propertyName = prop.PropertyName;
                var eventPropertyWriter = new MapEventBeanPropertyWriter(propertyName);
                propertWritersMap.Put(
                    propertyName,
                    new Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>(prop, eventPropertyWriter));
            }

            _propertyWriters = propertWritersMap;
            _writablePropertyDescriptors = writeableProps.ToArray();
        }
    }
} // end of namespace