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
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.arr
{
    public class ObjectArrayEventType : BaseNestableEventType
    {
        private IDictionary<String, Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>> _propertyWriters;
        private EventPropertyDescriptor[] _writablePropertyDescriptors;

        public ObjectArrayEventType(EventTypeMetadata metadata, String eventTypeName, int eventTypeId, EventAdapterService eventAdapterService, IDictionary<String, Object> propertyTypes, ConfigurationEventTypeObjectArray typeDef, EventType[] optionalSuperTypes, ICollection<EventType> optionalDeepSupertypes)
            : base(metadata, eventTypeName, eventTypeId, eventAdapterService, propertyTypes, optionalSuperTypes, optionalDeepSupertypes, typeDef, GetGetterFactory(eventTypeName, propertyTypes, optionalSuperTypes))
        {
        }

        protected override void PostUpdateNestableTypes()
        {
            var factory = (EventTypeNestableGetterFactoryObjectArray)GetterFactory;
            var indexPerProperty = factory.PropertiesIndex;
            int index = FindMax(indexPerProperty) + 1;
            foreach (KeyValuePair<String, Object> entry in NestableTypes)
            {
                if (indexPerProperty.ContainsKey(entry.Key))
                {
                    continue;
                }
                indexPerProperty.Put(entry.Key, index);
                index++;
            }
        }

        public IDictionary<string, int> PropertiesIndexes
        {
            get { return ((EventTypeNestableGetterFactoryObjectArray)GetterFactory).PropertiesIndex; }
        }

        public override Type UnderlyingType
        {
            get { return typeof(object[]); }
        }

        public override EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            BaseNestableEventUtil.MapIndexedPropPair pair = BaseNestableEventUtil.GetIndexedAndMappedProps(properties);

            if (pair.MapProperties.IsEmpty() && pair.ArrayProperties.IsEmpty())
            {
                return new ObjectArrayEventBeanCopyMethod(this, EventAdapterService);
            }
            else
            {
                return new ObjectArrayEventBeanCopyMethodWithArrayMap(
                    this, EventAdapterService, pair.MapProperties, pair.ArrayProperties, PropertiesIndexes);
            }
        }

        public override EventBeanReader Reader
        {
            get { return null; }
        }

        public override EventPropertyWriter GetWriter(String propertyName)
        {
            if (_writablePropertyDescriptors == null)
            {
                InitializeWriters();
            }
            Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter> pair = _propertyWriters.Get(propertyName);
            if (pair != null)
            {
                return pair.Second;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var mapProp = (MappedProperty)property;

                int index;
                if (!PropertiesIndexes.TryGetValue(mapProp.PropertyNameAtomic, out index))
                {
                    return null;
                }
                return new ObjectArrayEventBeanPropertyWriterMapProp(index, mapProp.Key);
            }

            if (property is IndexedProperty)
            {
                var indexedProp = (IndexedProperty)property;

                int index;
                if (!PropertiesIndexes.TryGetValue(indexedProp.PropertyNameAtomic, out index))
                {
                    return null;
                }

                return new ObjectArrayEventBeanPropertyWriterIndexedProp(index, indexedProp.Index);
            }

            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            if (_writablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var pair = _propertyWriters.Get(propertyName);
            if (pair != null)
            {
                return pair.First;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var mapProp = (MappedProperty)property;
                return new EventPropertyDescriptor(mapProp.PropertyNameAtomic, typeof(Object), null, false, true, false, true, false);
            }
            if (property is IndexedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var indexedProp = (IndexedProperty)property;
                return new EventPropertyDescriptor(indexedProp.PropertyNameAtomic, typeof(Object), null, true, false, true, false, false);
            }
            return null;
        }

        public override EventPropertyDescriptor[] WriteableProperties
        {
            get
            {
                if (_writablePropertyDescriptors == null)
                {
                    InitializeWriters();
                }
                return _writablePropertyDescriptors;
            }
        }

        public override EventBeanWriter GetWriter(String[] properties)
        {
            if (_writablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var allSimpleProps = true;
            var writers = new ObjectArrayEventBeanPropertyWriter[properties.Length];
            var indexes = new List<int>();
            var indexesPerProperty = PropertiesIndexes;

            for (int i = 0; i < properties.Length; i++)
            {
                var writerPair = _propertyWriters.Get(properties[i]);
                if (writerPair != null)
                {
                    writers[i] = writerPair.Second;
                    indexes.Add(indexesPerProperty.Get(writerPair.First.PropertyName));
                }
                else
                {
                    writers[i] = GetWriter(properties[i]) as ObjectArrayEventBeanPropertyWriter;
                    if (writers[i] == null)
                    {
                        return null;
                    }
                    allSimpleProps = false;
                }
            }

            if (allSimpleProps)
            {
                int[] propertyIndexes = CollectionUtil.IntArray(indexes);
                return new ObjectArrayEventBeanWriterSimpleProps(propertyIndexes);
            }
            else
            {
                return new ObjectArrayEventBeanWriterPerProp(writers);
            }
        }

        private void InitializeWriters()
        {
            var writeableProps = new List<EventPropertyDescriptor>();
            var propertWritersMap = new Dictionary<String, Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>>();
            foreach (var prop in PropertyDescriptors)
            {
                writeableProps.Add(prop);
                var propertyName = prop.PropertyName;
                int index;

                if (!PropertiesIndexes.TryGetValue(prop.PropertyName, out index))
                {
                    continue;
                }

                var eventPropertyWriter = new ObjectArrayEventBeanPropertyWriter(index);
                propertWritersMap.Put(propertyName, new Pair<EventPropertyDescriptor, ObjectArrayEventBeanPropertyWriter>(prop, eventPropertyWriter));
            }

            _propertyWriters = propertWritersMap;
            _writablePropertyDescriptors = writeableProps.ToArray();
        }

        private static EventTypeNestableGetterFactory GetGetterFactory(String eventTypeName, IDictionary<String, Object> propertyTypes, EventType[] optionalSupertypes)
        {
            IDictionary<string, int> indexPerProperty = new Dictionary<string, int>();

            int index = 0;
            if (optionalSupertypes != null)
            {
                foreach (EventType superType in optionalSupertypes)
                {
                    var objectArraySuperType = (ObjectArrayEventType)superType;
                    foreach (String propertyName in objectArraySuperType.PropertyNames)
                    {
                        if (indexPerProperty.ContainsKey(propertyName))
                        {
                            continue;
                        }
                        indexPerProperty.Put(propertyName, index);
                        index++;
                    }
                }
            }

            foreach (KeyValuePair<String, Object> entry in propertyTypes)
            {
                indexPerProperty.Put(entry.Key, index);
                index++;
            }
            return new EventTypeNestableGetterFactoryObjectArray(eventTypeName, indexPerProperty);
        }

        private static int FindMax(IDictionary<String, int> indexPerProperty)
        {
            int max = -1;
            foreach (var entry in indexPerProperty)
            {
                if (entry.Value > max)
                {
                    max = entry.Value;
                }
            }
            return max;
        }

        public static Object[] ConvertEvent(EventBean theEvent, ObjectArrayEventType targetType)
        {
            var indexesTarget = targetType.PropertiesIndexes;
            var indexesSource = ((ObjectArrayEventType)theEvent.EventType).PropertiesIndexes;
            var dataTarget = new Object[indexesTarget.Count];
            var dataSource = (Object[])theEvent.Underlying;

            foreach (KeyValuePair<string, int> sourceEntry in indexesSource)
            {
                string propertyName = sourceEntry.Key;
                int targetIndex;
                if (!indexesTarget.TryGetValue(propertyName, out targetIndex))
                {
                    continue;
                }

                object value = dataSource[sourceEntry.Value];
                dataTarget[targetIndex] = value;
            }

            return dataTarget;
        }

        public bool IsDeepEqualsConsiderOrder(ObjectArrayEventType other)
        {
            var factoryOther = (EventTypeNestableGetterFactoryObjectArray)other.GetterFactory;
            var factoryMe = (EventTypeNestableGetterFactoryObjectArray)GetterFactory;

            if (factoryOther.PropertiesIndex.Count != factoryMe.PropertiesIndex.Count)
            {
                return false;
            }

            foreach (var propMeEntry in factoryMe.PropertiesIndex)
            {
                int otherIndex;

                if (!factoryOther.PropertiesIndex.TryGetValue(propMeEntry.Key, out otherIndex) ||
                    (otherIndex != propMeEntry.Value))
                {
                    return false;
                }

                var propName = propMeEntry.Key;
                var setOneType = NestableTypes.Get(propName);
                var setTwoType = other.NestableTypes.Get(propName);
                var setTwoTypeFound = other.NestableTypes.ContainsKey(propName);

                var comparedMessage = BaseNestableEventUtil.ComparePropType(
                    propName, setOneType, setTwoType, setTwoTypeFound, other.Name);
                if (comparedMessage != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
