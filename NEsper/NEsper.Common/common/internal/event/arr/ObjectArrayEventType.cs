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
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayEventType : BaseNestableEventType
    {
        private IDictionary<string, Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>> propertyWriters;
        private EventPropertyDescriptor[] writablePropertyDescriptors;

        public ObjectArrayEventType(
            EventTypeMetadata metadata,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSuperTypes,
            string startTimestampName,
            string endTimestampName,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                metadata,
                propertyTypes,
                optionalSuperTypes,
                optionalDeepSuperTypes,
                startTimestampName,
                endTimestampName,
                GetGetterFactory(metadata.Name, propertyTypes, optionalSuperTypes),
                beanEventTypeFactory,
                false)
        {
        }

        public IDictionary<string, int> PropertiesIndexes =>
            ((EventTypeNestableGetterFactoryObjectArray) GetterFactory).PropertiesIndex;

        public override Type UnderlyingType => typeof(object[]);

        public override EventPropertyDescriptor[] WriteableProperties {
            get {
                if (writablePropertyDescriptors == null) {
                    InitializeWriters();
                }

                return writablePropertyDescriptors;
            }
        }

        public override EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            var pair = BaseNestableEventUtil.GetIndexedAndMappedProps(properties);

            if (pair.MapProperties.IsEmpty() && pair.ArrayProperties.IsEmpty()) {
                return new ObjectArrayEventBeanCopyMethodForge(this, BeanEventTypeFactory.EventBeanTypedEventFactory);
            }

            return new ObjectArrayEventBeanCopyMethodWithArrayMapForge(
                this,
                pair.MapProperties,
                pair.ArrayProperties,
                PropertiesIndexes);
        }

        public override EventPropertyWriterSPI GetWriter(string propertyName)
        {
            if (writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var pair = propertyWriters.Get(propertyName);
            if (pair != null) {
                return pair.Second;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty) {
                var mapProp = (MappedProperty) property;
                if (!PropertiesIndexes.TryGetValue(mapProp.PropertyNameAtomic, out var index)) {
                    return null;
                }

                return new ObjectArrayEventBeanPropertyWriterMapProp(index, mapProp.Key);
            }

            if (property is IndexedProperty) {
                var indexedProp = (IndexedProperty) property;
                if (!PropertiesIndexes.TryGetValue(indexedProp.PropertyNameAtomic, out var index)) {
                    return null;
                }

                return new ObjectArrayEventBeanPropertyWriterIndexedProp(index, indexedProp.Index);
            }

            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var pair = propertyWriters.Get(propertyName);
            if (pair != null) {
                return pair.First;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                var mapProp = (MappedProperty) property;
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

            if (property is IndexedProperty) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                var indexedProp = (IndexedProperty) property;
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
            if (writablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var allSimpleProps = true;
            var writers = new ObjectArrayEventBeanPropertyWriter[properties.Length];
            IList<int> indexes = new List<int>();
            var indexesPerProperty = PropertiesIndexes;

            for (var i = 0; i < properties.Length; i++) {
                var writerPair =
                    propertyWriters.Get(properties[i]);
                if (writerPair != null) {
                    writers[i] = writerPair.Second;
                    indexes.Add(indexesPerProperty.Get(writerPair.First.PropertyName));
                }
                else {
                    writers[i] = (ObjectArrayEventBeanPropertyWriter) GetWriter(properties[i]);
                    if (writers[i] == null) {
                        return null;
                    }

                    allSimpleProps = false;
                }
            }

            if (allSimpleProps) {
                var propertyIndexes = CollectionUtil.IntArray(indexes);
                return new ObjectArrayEventBeanWriterSimpleProps(propertyIndexes);
            }

            return new ObjectArrayEventBeanWriterPerProp(writers);
        }

        private void InitializeWriters()
        {
            IList<EventPropertyDescriptor> writeableProps = new List<EventPropertyDescriptor>();
            IDictionary<string, Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>> propertyWritersMap =
                new Dictionary<string, Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>>();
            foreach (var prop in PropertyDescriptors) {
                writeableProps.Add(prop);
                var propertyName = prop.PropertyName;
                if (!PropertiesIndexes.TryGetValue(prop.PropertyName, out var index)) {
                    continue;
                }

                var eventPropertyWriter = new ObjectArrayEventBeanPropertyWriter(index);
                propertyWritersMap.Put(
                    propertyName,
                    new Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>(prop, eventPropertyWriter));
            }

            propertyWriters = propertyWritersMap;
            writablePropertyDescriptors = writeableProps.ToArray();
        }

        private static EventTypeNestableGetterFactory GetGetterFactory(
            string eventTypeName,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSupertypes)
        {
            IDictionary<string, int> indexPerProperty = new Dictionary<string, int>();

            var index = 0;
            if (optionalSupertypes != null) {
                foreach (var superType in optionalSupertypes) {
                    var objectArraySuperType = (ObjectArrayEventType) superType;
                    foreach (var propertyName in objectArraySuperType.PropertyNames) {
                        if (indexPerProperty.ContainsKey(propertyName)) {
                            continue;
                        }

                        indexPerProperty.Put(propertyName, index);
                        index++;
                    }
                }
            }

            foreach (var entry in propertyTypes) {
                indexPerProperty.Put(entry.Key, index);
                index++;
            }

            return new EventTypeNestableGetterFactoryObjectArray(eventTypeName, indexPerProperty);
        }

        public static object[] ConvertEvent(
            EventBean theEvent,
            ObjectArrayEventType targetType)
        {
            var indexesTarget = targetType.PropertiesIndexes;
            var indexesSource = ((ObjectArrayEventType) theEvent.EventType).PropertiesIndexes;
            var dataTarget = new object[indexesTarget.Count];
            var dataSource = (object[]) theEvent.Underlying;
            foreach (var sourceEntry in indexesSource) {
                var propertyName = sourceEntry.Key;
                if (!indexesTarget.TryGetValue(propertyName, out var targetIndex)) {
                    continue;
                }

                var value = dataSource[sourceEntry.Value];
                dataTarget[targetIndex] = value;
            }

            return dataTarget;
        }

        public bool IsDeepEqualsConsiderOrder(ObjectArrayEventType other)
        {
            var factoryOther = (EventTypeNestableGetterFactoryObjectArray) other.GetterFactory;
            var factoryMe = (EventTypeNestableGetterFactoryObjectArray) GetterFactory;

            if (factoryOther.PropertiesIndex.Count != factoryMe.PropertiesIndex.Count) {
                return false;
            }

            foreach (var propMeEntry in factoryMe.PropertiesIndex) {
                var hasOtherIndex = factoryOther.PropertiesIndex.TryGetValue(propMeEntry.Key, out var otherIndex);
                if (!hasOtherIndex || !otherIndex.Equals(propMeEntry.Value)) {
                    return false;
                }

                var propName = propMeEntry.Key;
                var setOneType = NestableTypes.Get(propName);
                var setTwoType = other.NestableTypes.Get(propName);
                var setTwoTypeFound = other.NestableTypes.ContainsKey(propName);

                var comparedMessage = BaseNestableEventUtil.ComparePropType(
                    propName,
                    setOneType,
                    setTwoType,
                    setTwoTypeFound,
                    other.Name);
                if (comparedMessage != null) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace