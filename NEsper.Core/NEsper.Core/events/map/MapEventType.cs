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

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>
    /// Implementation of the <seealso cref="EventType" /> interface for handling plain
    /// Maps containing name value pairs.
    /// </summary>
    public class MapEventType : BaseNestableEventType
    {
        private static readonly EventTypeNestableGetterFactory GETTER_FACTORY = new EventTypeNestableGetterFactoryMap();

        internal IDictionary<String, Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>> PropertyWriters;
        internal EventPropertyDescriptor[] WritablePropertyDescriptors;

        public MapEventType(EventTypeMetadata metadata,
                            String typeName,
                            int eventTypeId,
                            EventAdapterService eventAdapterService,
                            IDictionary<String, Object> propertyTypes,
                            EventType[] optionalSuperTypes,
                            ICollection<EventType> optionalDeepSupertypes,
                            ConfigurationEventTypeMap configMapType)
            : base(metadata, typeName, eventTypeId, eventAdapterService, propertyTypes, optionalSuperTypes, optionalDeepSupertypes, configMapType, GETTER_FACTORY)
        {
        }

        protected override void PostUpdateNestableTypes()
        {
        }

        public override Type UnderlyingType
        {
            get { return typeof(Map); }
        }

        public override EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            var pair = BaseNestableEventUtil.GetIndexedAndMappedProps(properties);

            if (pair.MapProperties.IsEmpty() && pair.ArrayProperties.IsEmpty())
            {
                return new MapEventBeanCopyMethod(this, EventAdapterService);
            }
            else
            {
                return new MapEventBeanCopyMethodWithArrayMap(
                    this, EventAdapterService, pair.MapProperties, pair.ArrayProperties);
            }
        }

        public override EventBeanReader Reader
        {
            get { return new MapEventBeanReader(this); }
        }

        public Object GetValue(String propertyName, Map values)
        {
            var getter = (MapEventPropertyGetter)GetGetter(propertyName);
            return getter.GetMap(values);
        }

        public override EventPropertyWriter GetWriter(String propertyName)
        {
            if (WritablePropertyDescriptors == null)
            {
                InitializeWriters();
            }
            var pair = PropertyWriters.Get(propertyName);
            if (pair != null)
            {
                return pair.Second;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var mapProp = (MappedProperty)property;
                return new MapEventBeanPropertyWriterMapProp(mapProp.PropertyNameAtomic, mapProp.Key);
            }

            if (property is IndexedProperty)
            {
                var indexedProp = (IndexedProperty)property;
                return new MapEventBeanPropertyWriterIndexedProp(indexedProp.PropertyNameAtomic, indexedProp.Index);
            }

            return null;
        }

        public override EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            if (WritablePropertyDescriptors == null)
            {
                InitializeWriters();
            }
            var pair = PropertyWriters.Get(propertyName);
            if (pair != null)
            {
                return pair.First;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var mapProp = (MappedProperty)property;
                return new EventPropertyDescriptor(
                    mapProp.PropertyNameAtomic, typeof(Object), null, false, true, false, true, false);
            }
            if (property is IndexedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var indexedProp = (IndexedProperty)property;
                return new EventPropertyDescriptor(
                    indexedProp.PropertyNameAtomic, typeof(Object), null, true, false, true, false, false);
            }
            return null;
        }

        public override EventPropertyDescriptor[] WriteableProperties
        {
            get
            {
                if (WritablePropertyDescriptors == null)
                {
                    InitializeWriters();
                }
                return WritablePropertyDescriptors;
            }
        }

        public override EventBeanWriter GetWriter(String[] properties)
        {
            if (WritablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var allSimpleProps = true;
            var writers = new MapEventBeanPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                var writerPair = PropertyWriters.Get(properties[i]);
                if (writerPair != null)
                {
                    writers[i] = writerPair.Second;
                }
                else
                {
                    writers[i] = (MapEventBeanPropertyWriter)GetWriter(properties[i]);
                    if (writers[i] == null)
                    {
                        return null;
                    }
                    allSimpleProps = false;
                }
            }

            if (allSimpleProps)
            {
                return new MapEventBeanWriterSimpleProps(properties);
            }
            else
            {
                return new MapEventBeanWriterPerProp(writers);
            }
        }

        private void InitializeWriters()
        {
            var writeableProps = new List<EventPropertyDescriptor>();
            var propertWritersMap = new Dictionary<String, Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>>();
            foreach (EventPropertyDescriptor prop in PropertyDescriptors)
            {
                writeableProps.Add(prop);
                var propertyName = prop.PropertyName;
                var eventPropertyWriter = new MapEventBeanPropertyWriter(propertyName);
                propertWritersMap.Put(propertyName, new Pair<EventPropertyDescriptor, MapEventBeanPropertyWriter>(prop, eventPropertyWriter));
            }

            PropertyWriters = propertWritersMap;
            WritablePropertyDescriptors = writeableProps.ToArray();
        }
    }
}
