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

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;
using NEsper.Avro.Getter;
using NEsper.Avro.Writer;

namespace NEsper.Avro.Core
{
    public class AvroEventType : AvroSchemaEventType, EventTypeSPI
    {
        private readonly Schema _avroSchema;
        private readonly IDictionary<string, PropertySetDescriptorItem> _propertyItems;
        private readonly EventType[] _optionalSuperTypes;
        private readonly ISet<EventType> _deepSupertypes;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EventTypeAvroHandler _eventTypeAvroHandler;
        private readonly AvroEventTypeFragmentTypeCache _fragmentTypeCache = new AvroEventTypeFragmentTypeCache();

        private EventPropertyDescriptor[] _propertyDescriptors;
        private string[] _propertyNames;
        private Dictionary<string, EventPropertyGetterSPI> _propertyGetterCache;
        private IDictionary<string, EventPropertyGetter> _propertyGetterCodegeneratedCache;

        public AvroEventType(EventTypeMetadata metadata,
                             Schema avroSchema,
                             string startTimestampPropertyName,
                             string endTimestampPropertyName,
                             EventType[] optionalSuperTypes,
                             ISet<EventType> deepSupertypes,
                             EventBeanTypedEventFactory eventBeanTypedEventFactory,
                             EventTypeAvroHandler eventTypeAvroHandler)
        {
            Metadata = metadata;
            _avroSchema = avroSchema;
            _optionalSuperTypes = optionalSuperTypes;
            _deepSupertypes = deepSupertypes ?? new EmptySet<EventType>();
            _propertyItems = new LinkedHashMap<string, PropertySetDescriptorItem>();
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _eventTypeAvroHandler = eventTypeAvroHandler;

            Init();

            var desc = EventTypeUtility.ValidatedDetermineTimestampProps(this, startTimestampPropertyName, endTimestampPropertyName, optionalSuperTypes);
            StartTimestampPropertyName = desc.Start;
            EndTimestampPropertyName = desc.End;
        }

        public void SetMetadataId(long publicId, long protectedId)
        {
            Metadata = Metadata.WithIds(publicId, protectedId);
        }

        public Type UnderlyingType => typeof(GenericRecord);

        public Type GetPropertyType(string propertyName)
        {
            var item = _propertyItems.Get(StringValue.UnescapeDot(propertyName));
            if (item != null)
            {
                return item.SimplePropertyType;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            return AvroPropertyUtil.PropertyType(_avroSchema, property);
        }

        public bool IsProperty(string propertyExpression)
        {
            var propertyType = GetPropertyType(propertyExpression);
            if (propertyType != null)
            {
                return true;
            }
            if (_propertyGetterCache == null)
            {
                _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }
            return AvroPropertyUtil.GetGetter(_avroSchema, Metadata.ModuleName, _propertyGetterCache, _propertyItems, propertyExpression, false, _eventBeanTypedEventFactory, _eventTypeAvroHandler, _fragmentTypeCache) != null;
        }

        public EventPropertyGetterSPI GetGetterSPI(string propertyExpression)
        {
            if (_propertyGetterCache == null)
            {
                _propertyGetterCache = new Dictionary<string, EventPropertyGetterSPI>();
            }
            return AvroPropertyUtil.GetGetter(_avroSchema, Metadata.ModuleName, _propertyGetterCache, _propertyItems, propertyExpression, true, _eventBeanTypedEventFactory, _eventTypeAvroHandler, _fragmentTypeCache);
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
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

            _propertyGetterCodegeneratedCache.Put(propertyName, getterSPI);
            return getterSPI;
        }

        public FragmentEventType GetFragmentType(string propertyExpression)
        {
            return AvroFragmentTypeUtil.GetFragmentType(_avroSchema, propertyExpression, Metadata.ModuleName, _propertyItems, _eventBeanTypedEventFactory, _eventTypeAvroHandler, _fragmentTypeCache);
        }

        public string[] PropertyNames => _propertyNames;

        public EventPropertyDescriptor[] PropertyDescriptors => _propertyDescriptors;

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            var item = _propertyItems.Get(propertyName);
            return item?.PropertyDescriptor;
        }

        public EventType[] SuperTypes => _optionalSuperTypes;

        public IEnumerable<EventType> DeepSuperTypes => _deepSupertypes;

        public ICollection<EventType> DeepSuperTypesCollection => _deepSupertypes;

        public string Name => Metadata.Name;

        public EventPropertyGetterMapped GetGetterMapped(string mappedPropertyName)
        {
            return GetGetterMappedSPI(mappedPropertyName);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string mappedPropertyName)
        {
            var desc = _propertyItems.Get(mappedPropertyName);
            if (desc == null || !desc.PropertyDescriptor.IsMapped)
            {
                return null;
            }
            var field = _avroSchema.GetField(mappedPropertyName);
            return new AvroEventBeanGetterMappedRuntimeKeyed(field);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            return GetGetterIndexedSPI(indexedPropertyName);
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string indexedPropertyName)
        {
            var desc = _propertyItems.Get(indexedPropertyName);
            if (desc == null || !desc.PropertyDescriptor.IsIndexed)
            {
                return null;
            }
            var field = _avroSchema.GetField(indexedPropertyName);
            return new AvroEventBeanGetterIndexedRuntimeKeyed(field);
        }

        public string StartTimestampPropertyName { get; }

        public string EndTimestampPropertyName { get; }

        public EventTypeMetadata Metadata { get; private set; }

        public EventPropertyWriterSPI GetWriter(string propertyName)
        {
            return GetWriterInternal(propertyName);
        }

        public AvroEventBeanPropertyWriter GetWriterInternal(string propertyName)
        {
            var desc = _propertyItems.Get(propertyName);
            if (desc != null)
            {
                var pos = _avroSchema.GetField(propertyName);
                return new AvroEventBeanPropertyWriter(pos);
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty mapProp)
            {
                var pos = _avroSchema.GetField(mapProp.PropertyNameAtomic);
                return new AvroEventBeanPropertyWriterMapProp(pos, mapProp.Key);
            }

            if (property is IndexedProperty indexedProp)
            {
                var pos = _avroSchema.GetField(indexedProp.PropertyNameAtomic);
                return new AvroEventBeanPropertyWriterIndexedProp(pos, indexedProp.Index);
            }

            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties => _propertyDescriptors;

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            foreach (var desc in _propertyDescriptors)
            {
                if (desc.PropertyName.Equals(propertyName))
                {
                    return desc;
                }
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty mapProp)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }

                return new EventPropertyDescriptor(mapProp.PropertyNameAtomic, typeof(object), null, false, true, false, true, false);
            }
            if (property is IndexedProperty indexedProp)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }

                return new EventPropertyDescriptor(indexedProp.PropertyNameAtomic, typeof(object), null, true, false, true, false, false);
            }
            return null;
        }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            return new AvroEventBeanCopyMethodForge(this);
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            var allSimpleProps = true;
            var writers = new AvroEventBeanPropertyWriter[properties.Length];
            IList<Field> indexes = new List<Field>();

            for (var i = 0; i < properties.Length; i++)
            {
                var writer = GetWriterInternal(properties[i]);
                if (_propertyItems.ContainsKey(properties[i]))
                {
                    writers[i] = writer;
                    indexes.Add(_avroSchema.GetField(properties[i]));
                }
                else
                {
                    writers[i] = GetWriterInternal(properties[i]);
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

        public EventBeanReader Reader => null; // use the default reader

        public ExprValidationException EqualsCompareType(EventType other)
        {
            if (!other.Name.Equals(Name))
            {
                return new ExprValidationException("Expected event type '" + Name +
                        "' but received event type '" + other.Metadata.Name + "'");
            }

            if (Metadata.ApplicationType != other.Metadata.ApplicationType)
            {
                return new ExprValidationException("Expected for event type '" + Name +
                        "' of type " + Metadata.ApplicationType +
                        " but received event type '" + other.Metadata.Name + "' of type " + other.Metadata.ApplicationType);
            }

            var otherAvro = (AvroEventType) other;
            if (!otherAvro._avroSchema.Equals(_avroSchema))
            {
                return new ExprValidationException("Avro schema does not match for type '" + other.Name + "'");
            }

            return null;
        }

        public object Schema => _avroSchema;

        public Schema SchemaAvro => _avroSchema;

        private void Init()
        {
            var avroFields = _avroSchema.GetFields();

            _propertyNames = new string[avroFields.Count];
            _propertyDescriptors = new EventPropertyDescriptor[_propertyNames.Length];
            var fieldNum = 0;

            foreach (var field in avroFields)
            {
                _propertyNames[fieldNum] = field.Name;

                var propertyType = AvroTypeUtil.PropertyType(field.Schema);
                Type componentType = null;
                var indexed = false;
                var mapped = false;
                FragmentEventType fragmentEventType = null;

                if (field.Schema.Tag == global::Avro.Schema.Type.Array)
                {
                    componentType = AvroTypeUtil.PropertyType(field.Schema.AsArraySchema().ItemSchema);
                    indexed = true;
                    if (field.Schema.AsArraySchema().ItemSchema.Tag == global::Avro.Schema.Type.Record)
                    {
                        fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                            field.Schema,
                            Metadata.ModuleName,
                            _eventBeanTypedEventFactory,
                            _eventTypeAvroHandler,
                            _fragmentTypeCache);
                    }
                }
                else if (field.Schema.Tag == global::Avro.Schema.Type.Map)
                {
                    mapped = true;
                    componentType = AvroTypeUtil.PropertyType(field.Schema.AsMapSchema().ValueSchema);
                }
                else
                {
                    fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        field.Schema,
                        Metadata.ModuleName,
                        _eventBeanTypedEventFactory,
                        _eventTypeAvroHandler,
                        _fragmentTypeCache);
                }

                var getter = new AvroEventBeanGetterSimple(field, fragmentEventType?.FragmentType, _eventBeanTypedEventFactory, propertyType);

                var descriptor = new EventPropertyDescriptor(field.Name, propertyType, componentType, false, false, indexed, mapped, fragmentEventType != null);
                var item = new PropertySetDescriptorItem(descriptor, propertyType, getter, fragmentEventType);
                _propertyItems.Put(field.Name, item);
                _propertyDescriptors[fieldNum] = descriptor;

                fieldNum++;
            }
        }
    }
} // end of namespace