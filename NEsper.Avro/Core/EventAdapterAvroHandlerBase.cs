///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.SelectExprRep;
using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Core
{
    public abstract class EventAdapterAvroHandlerBase : EventAdapterAvroHandler
    {
        private static readonly SelectExprProcessorRepresentationFactoryAvro FACTORY_SELECT =
            new SelectExprProcessorRepresentationFactoryAvro();

        private ConfigurationEngineDefaults.AvroSettings _avroSettings;
        private TypeRepresentationMapper _optionalTypeMapper;
        private ObjectValueTypeWidenerFactory _optionalWidenerFactory;

        protected abstract AvroSchemaEventType MakeType(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            RecordSchema schema,
            ConfigurationEventTypeAvro optionalConfig,
            EventType[] supertypes,
            ICollection<EventType> deepSupertypes);

        public void Init(ConfigurationEngineDefaults.AvroSettings avroSettings, EngineImportService engineImportService)
        {
            _avroSettings = avroSettings;

            if (avroSettings.TypeRepresentationMapperClass != null)
            {
                _optionalTypeMapper = TypeHelper.Instantiate<TypeRepresentationMapper>(
                    avroSettings.TypeRepresentationMapperClass,
                    engineImportService.GetClassForNameProvider());
            }

            if (avroSettings.ObjectValueTypeWidenerFactoryClass != null)
            {
                _optionalWidenerFactory = TypeHelper.Instantiate<ObjectValueTypeWidenerFactory>(
                    avroSettings.ObjectValueTypeWidenerFactoryClass,
                    engineImportService.GetClassForNameProvider());
            }
        }

        public AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            ConfigurationEventTypeAvro requiredConfig,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes)
        {
            var avroSchemaObj = requiredConfig.AvroSchema;
            var avroSchemaText = requiredConfig.AvroSchemaText;

            if (avroSchemaObj == null && avroSchemaText == null)
            {
                throw new ArgumentException("Null value for schema and schema text");
            }
            if (avroSchemaObj != null && avroSchemaText != null)
            {
                throw new ArgumentException(
                    "Both avro schema and avro schema text are supplied and one can be provided");
            }
            if (avroSchemaObj != null && !(avroSchemaObj is Schema))
            {
                throw new ArgumentException(
                    "Schema expected of type " + typeof (Schema).FullName + " but received " +
                    avroSchemaObj.GetType().FullName);
            }

            Schema schema;
            if (avroSchemaObj != null)
            {
                schema = (Schema) avroSchemaObj;
            }
            else
            {
                try
                {
                    schema = Schema.Parse(avroSchemaText);
                }
                catch (Exception ex)
                {
                    throw new EPException("Failed for parse avro schema: " + ex.Message, ex);
                }
            }

            return MakeType(
                metadata, eventTypeName, typeId, eventAdapterService, schema.AsRecordSchema(), requiredConfig, superTypes, deepSuperTypes);
        }

        public AvroSchemaEventType NewEventTypeFromNormalized(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            IDictionary<string, object> properties,
            Attribute[] annotations,
            ConfigurationEventTypeAvro optionalConfig,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes,
            string statementName,
            string engineURI)
        {
            JArray assembler = new JArray();
            //FieldAssembler<Schema> assembler = Record(eventTypeName).Fields();

            // add supertypes first so the positions are comparable
            var added = new HashSet<string>();
            if (superTypes != null)
            {
                for (var i = 0; i < superTypes.Length; i++)
                {
                    var superType = (AvroEventType) superTypes[i];
                    foreach (Field field in superType.SchemaAvro.GetFields())
                    {
                        if (properties.ContainsKey(field.Name) || added.Contains(field.Name))
                        {
                            continue;
                        }
                        added.Add(field.Name);
                        assembler.Add(TypeBuilder.Field(field.Name, field.Schema));
                        //assembler.Name(field.Name).Type(field.Schema).NoDefault();
                    }
                }
            }

            foreach (var prop in properties)
            {
                if (!added.Contains(prop.Key))
                {
                    AvroSchemaUtil.AssembleField(
                        prop.Key, prop.Value, assembler, annotations, _avroSettings, eventAdapterService, statementName,
                        engineURI, _optionalTypeMapper);
                    added.Add(prop.Key);
                }
            }

            var schema = SchemaBuilder.Record(
                eventTypeName, assembler);

            //Schema schema = assembler.EndRecord();
            return MakeType(
                metadata, eventTypeName, typeId, eventAdapterService, schema, optionalConfig, superTypes, deepSuperTypes);
        }

        public EventBean AdapterForTypeAvro(Object avroGenericDataDotRecord, EventType existingType)
        {
            if (!(avroGenericDataDotRecord is GenericRecord))
            {
                throw new EPException(
                    "Unexpected event object type '" +
                    (avroGenericDataDotRecord == null ? "null" : avroGenericDataDotRecord.GetType().FullName) +
                    "' encountered, please supply a GenericRecord");
            }

            var record = (GenericRecord) avroGenericDataDotRecord;
            return new AvroGenericDataEventBean(record, existingType);
        }

        public SelectExprProcessorRepresentationFactory GetOutputFactory()
        {
            return FACTORY_SELECT;
        }

        public EventBeanManufacturer GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            EventAdapterService eventAdapterService,
            IList<WriteablePropertyDescriptor> properties)
        {
            return new EventBeanManufacturerAvro(avroSchemaEventType, eventAdapterService, properties);
        }

        public EventBeanFactory GetEventBeanFactory(EventType type, EventAdapterService eventAdapterService)
        {
            return new EventBeanFactoryAvro(type, eventAdapterService);
        }

        public void ValidateExistingType(EventType existingType, AvroSchemaEventType proposedType)
        {
            if (!(existingType is AvroSchemaEventType))
            {
                throw new EventAdapterException(
                    "Type by name '" + proposedType.Name + "' is not a compatible type " +
                    "(target type underlying is '" + Name.Clean(existingType.UnderlyingType) + "', " +
                    "source type underlying is '" + proposedType.UnderlyingType.Name + "')");
            }

            var proposed = (Schema) proposedType.Schema;
            var existing = (Schema) ((AvroSchemaEventType) existingType).Schema;
            if (!proposed.Equals(existing))
            {
                throw new EventAdapterException(
                    "Event type named '" + existingType.Name +
                    "' has already been declared with differing column name or type information\n"
                    + "schemaExisting: " + AvroSchemaUtil.ToSchemaStringSafe(existing) + "\n"
                    + "schemaProposed: " + AvroSchemaUtil.ToSchemaStringSafe(proposed));
            }
        }

        public void AvroCompat(EventType existingType, IDictionary<string, Object> selPropertyTypes)
        {
            Schema schema = ((AvroEventType) existingType).SchemaAvro;

            foreach (var selected in selPropertyTypes)
            {
                var propertyName = selected.Key;
                Field targetField = schema.GetField(selected.Key);

                if (targetField == null)
                {
                    throw new ExprValidationException(
                        "Property '" + propertyName + "' is not found among the fields for event type '" +
                        existingType.Name + "'");
                }

                if (selected.Value is EventType)
                {
                    var targetEventType = (EventType) selected.Value;
                    var targetAvro = CheckAvroEventTpe(selected.Key, targetEventType);
                    if (targetField.Schema.Tag != Schema.Type.Record ||
                        !targetField.Schema.Equals(targetAvro.SchemaAvro))
                    {
                        throw new ExprValidationException(
                            "Property '" + propertyName + "' is incompatible, expecting a compatible schema '" +
                            targetField.Schema.Name + "' but received schema '" + targetAvro.SchemaAvro.Name + "'");
                    }
                }
                else if (selected.Value is EventType[])
                {
                    var targetEventType = ((EventType[]) selected.Value)[0];
                    var targetAvro = CheckAvroEventTpe(selected.Key, targetEventType);
                    if (targetField.Schema.Tag != Schema.Type.Array ||
                        targetField.Schema.GetElementType().Tag != Schema.Type.Record ||
                        !targetField.Schema.GetElementType().Equals(targetAvro.SchemaAvro))
                    {
                        throw new ExprValidationException(
                            "Property '" + propertyName + "' is incompatible, expecting an array of compatible schema '" +
                            targetField.Schema.Name + "' but received schema '" + targetAvro.SchemaAvro.Name + "'");
                    }
                }
            }
        }

        public Object ConvertEvent(EventBean theEvent, AvroSchemaEventType targetType)
        {
            GenericRecord original = ((AvroGenericDataBackedEventBean) theEvent).Properties;
            var targetSchema = (RecordSchema) targetType.Schema;
            var target = new GenericRecord(targetSchema);

            IList<Field> fields = original.Schema.Fields;
            foreach (var field in fields)
            {
                Field targetField = targetSchema.GetField(field.Name);
                if (targetField == null)
                {
                    continue;
                }

                if (field.Schema.Tag == Schema.Type.Array)
                {
                    var originalColl = original.Get(field).Unwrap<object>();
                    if (originalColl != null)
                    {
                        target.Put(targetField, new List<object>(originalColl));
                    }
                }
                else if (field.Schema.Tag == Schema.Type.Map)
                {
                    var originalMap = (IDictionary<string, object>) original.Get(field);
                    if (originalMap != null)
                    {
                        target.Put(targetField, new Dictionary<string, object>(originalMap));
                    }
                }
                else
                {
                    target.Put(targetField, original.Get(field));
                }
            }
            return target;
        }

        public TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType)
        {
            return _optionalWidenerFactory == null
                ? (TypeWidenerCustomizer) AvroTypeWidenerCustomizerDefault.INSTANCE
                : (TypeWidenerCustomizer) new AvroTypeWidenerCustomizerWHook(_optionalWidenerFactory, eventType);
        }

        private AvroEventType CheckAvroEventTpe(string propertyName, EventType eventType)
        {
            if (!(eventType is AvroEventType))
            {
                throw new ExprValidationException(
                    "Property '" + propertyName + "' is incompatible with event type '" + eventType.Name +
                    "' underlying type " + eventType.UnderlyingType.Name);
            }
            return (AvroEventType) eventType;
        }
    }
} // end of namespace
