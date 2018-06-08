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

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Getter;
using NEsper.Avro.Writer;

namespace NEsper.Avro.Core
{
    public class AvroEventType
        : AvroSchemaEventType
        , EventTypeSPI
    {
        private readonly RecordSchema _avroSchema;
        private readonly ICollection<EventType> _deepSupertypes;
        private readonly string _endTimestampPropertyName;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventTypeMetadata _metadata;
        private readonly EventType[] _optionalSuperTypes;
        private readonly IDictionary<string, PropertySetDescriptorItem> _propertyItems;
        private readonly string _startTimestampPropertyName;
        private readonly int _typeId;

        private EventPropertyDescriptor[] _propertyDescriptors;
        private Dictionary<string, EventPropertyGetterSPI> _propertyGetterCache;
        private Dictionary<String, EventPropertyGetter> _propertyGetterCodegeneratedCache;
        private string[] _propertyNames;

        public AvroEventType(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            RecordSchema avroSchema,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            EventType[] optionalSuperTypes,
            ICollection<EventType> deepSupertypes)
        {
            _metadata = metadata;
            _typeId = typeId;
            _eventAdapterService = eventAdapterService;
            _avroSchema = avroSchema;
            _optionalSuperTypes = optionalSuperTypes;
            _deepSupertypes = deepSupertypes ?? Collections.GetEmptySet<EventType>();
            _propertyItems = new LinkedHashMap<string, PropertySetDescriptorItem>();

            Init();

            EventTypeUtility.TimestampPropertyDesc desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this, startTimestampPropertyName, endTimestampPropertyName, optionalSuperTypes);
            _startTimestampPropertyName = desc.Start;
            _endTimestampPropertyName = desc.End;
        }

        public RecordSchema SchemaAvro
        {
            get { return _avroSchema; }
        }

        public Type UnderlyingType
        {
            get { return typeof (GenericRecord); }
        }

        public Type GetPropertyType(string propertyName)
        {
            PropertySetDescriptorItem item = _propertyItems.Get(ASTUtil.UnescapeDot(propertyName));
            if (item != null)
            {
                return item.SimplePropertyType;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            return AvroPropertyUtil.PropertyType(_avroSchema, property);
        }

        public bool IsProperty(string propertyExpression)
        {
            Type propertyType = GetPropertyType(propertyExpression);
            if (propertyType != null)
            {
                return true;
            }
            if (_propertyGetterCache == null)
            {
                _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }
            return
                AvroPropertyUtil.GetGetter(
                    _avroSchema, _propertyGetterCache, _propertyItems, propertyExpression, false, _eventAdapterService) !=
                null;
        }

        public EventPropertyGetterSPI GetGetterSPI(string propertyExpression)
        {
            if (_propertyGetterCache == null)
            {
                _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }
            return AvroPropertyUtil.GetGetter(
                _avroSchema, _propertyGetterCache, _propertyItems, propertyExpression, true, _eventAdapterService);
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

        public FragmentEventType GetFragmentType(string propertyExpression)
        {
            return AvroFragmentTypeUtil.GetFragmentType(
                _avroSchema, propertyExpression, _propertyItems, _eventAdapterService);
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            PropertySetDescriptorItem item = _propertyItems.Get(propertyName);
            if (item == null)
            {
                return null;
            }
            return item.PropertyDescriptor;
        }

        public EventType[] SuperTypes
        {
            get { return _optionalSuperTypes; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return _deepSupertypes.ToArray(); }
        }

        public string Name
        {
            get { return _metadata.PublicName; }
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedPropertyName)
        {
            PropertySetDescriptorItem desc = _propertyItems.Get(mappedPropertyName);
            if (desc == null || !desc.PropertyDescriptor.IsMapped)
            {
                return null;
            }
            Field field = _avroSchema.GetField(mappedPropertyName);
            return new AvroEventBeanGetterMappedRuntimeKeyed(field);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            PropertySetDescriptorItem desc = _propertyItems.Get(indexedPropertyName);
            if (desc == null || !desc.PropertyDescriptor.IsIndexed)
            {
                return null;
            }
            Field field = _avroSchema.GetField(indexedPropertyName);
            return new AvroEventBeanGetterIndexedRuntimeKeyed(field);
        }

        public int EventTypeId
        {
            get { return _typeId; }
        }

        public string StartTimestampPropertyName
        {
            get { return _startTimestampPropertyName; }
        }

        public string EndTimestampPropertyName
        {
            get { return _endTimestampPropertyName; }
        }

        public object Schema
        {
            get { return _avroSchema; }
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public EventPropertyWriter GetWriter(string propertyName)
        {
            PropertySetDescriptorItem desc = _propertyItems.Get(propertyName);
            if (desc != null)
            {
                var field = _avroSchema.GetField(propertyName);
                return new AvroEventBeanPropertyWriter(field);
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var mapProp = (MappedProperty) property;
                var field = _avroSchema.GetField(property.PropertyNameAtomic);
                return new AvroEventBeanPropertyWriterMapProp(field, mapProp.Key);
            }

            if (property is IndexedProperty)
            {
                var indexedProp = (IndexedProperty) property;
                var field = _avroSchema.GetField(property.PropertyNameAtomic);
                return new AvroEventBeanPropertyWriterIndexedProp(field, indexedProp.Index);
            }

            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties
        {
            get { return _propertyDescriptors; }
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            foreach (EventPropertyDescriptor desc in _propertyDescriptors)
            {
                if (desc.PropertyName.Equals(propertyName))
                {
                    return desc;
                }
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var mapProp = (MappedProperty) property;
                return new EventPropertyDescriptor(
                    mapProp.PropertyNameAtomic, typeof (Object), null, false, true, false, true, false);
            }
            if (property is IndexedProperty)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var indexedProp = (IndexedProperty) property;
                return new EventPropertyDescriptor(
                    indexedProp.PropertyNameAtomic, typeof (Object), null, true, false, true, false, false);
            }
            return null;
        }

        public EventBeanCopyMethod GetCopyMethod(string[] properties)
        {
            return new AvroEventBeanCopyMethod(this, _eventAdapterService);
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            bool allSimpleProps = true;
            var writers = new AvroEventBeanPropertyWriter[properties.Length];
            var indexes = new List<Field>();

            for (int i = 0; i < properties.Length; i++)
            {
                var writer = (AvroEventBeanPropertyWriter) GetWriter(properties[i]);
                if (_propertyItems.ContainsKey(properties[i]))
                {
                    writers[i] = writer;
                    indexes.Add(_avroSchema.GetField(properties[i]));
                }
                else
                {
                    writers[i] = (AvroEventBeanPropertyWriter) GetWriter(properties[i]);
                    if (writers[i] == null)
                    {
                        return null;
                    }
                    allSimpleProps = false;
                }
            }

            if (allSimpleProps)
            {
                return new AvroEventBeanWriterSimpleProps(indexes.ToArray());
            }
            return new AvroEventBeanWriterPerProp(writers);
        }

        public EventBeanReader Reader
        {
            get
            {
                return null; // use the default reader
            }
        }

        public bool EqualsCompareType(EventType other)
        {
            var otherAvro = other as AvroEventType;
            if (otherAvro != null)
            {
                if (!otherAvro.Name.Equals(_metadata.PrimaryName))
                {
                    return false;
                }
                return otherAvro._avroSchema.Equals(_avroSchema);
            }
            return false;
        }

        private void Init()
        {
            _propertyNames = new string[_avroSchema.GetFields().Count];
            _propertyDescriptors = new EventPropertyDescriptor[_propertyNames.Length];
            int fieldNum = 0;

            foreach (Field field in _avroSchema.GetFields())
            {
                _propertyNames[fieldNum] = field.Name;

                Type propertyType = AvroTypeUtil.PropertyType(field.Schema);
                Type componentType = null;
                bool indexed = false;
                bool mapped = false;
                FragmentEventType fragmentEventType = null;

                if (field.Schema.Tag == global::Avro.Schema.Type.Array)
                {
                    componentType = AvroTypeUtil.PropertyType(field.Schema.GetElementType());
                    indexed = true;
                    if (field.Schema.GetElementType().Tag == global::Avro.Schema.Type.Record)
                    {
                        fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(field.Schema, _eventAdapterService);
                    }
                }
                else if (field.Schema.Tag == global::Avro.Schema.Type.Map)
                {
                    mapped = true;
                    componentType = AvroTypeUtil.PropertyType(field.Schema.GetValueType());
                }
                else if (field.Schema.Tag == global::Avro.Schema.Type.String)
                {
                    indexed = true;
                    componentType = typeof(char);
                    fragmentEventType = null;
                }
                else
                {
                    fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(field.Schema, _eventAdapterService);
                }

                var getter = new AvroEventBeanGetterSimple(
                    field, fragmentEventType == null ? null : fragmentEventType.FragmentType, _eventAdapterService);

                var descriptor = new EventPropertyDescriptor(
                    field.Name, propertyType, componentType, false, false, indexed, mapped, fragmentEventType != null);
                var item = new PropertySetDescriptorItem(descriptor, propertyType, getter, fragmentEventType);
                _propertyItems.Put(field.Name, item);
                _propertyDescriptors[fieldNum] = descriptor;

                fieldNum++;
            }
        }
    }
} // end of namespace