///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.SelectExprRep;

using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Core
{
    public abstract class EventTypeAvroHandlerBase : EventTypeAvroHandler
    {
        private static readonly SelectExprProcessorRepresentationFactoryAvro FACTORY_SELECT =
            new SelectExprProcessorRepresentationFactoryAvro();

        private ConfigurationCommonEventTypeMeta.AvroSettingsConfig _avroSettings;
        private TypeRepresentationMapper _optionalTypeMapper;
        private ObjectValueTypeWidenerFactory _optionalWidenerFactory;

        public AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventAdapterService,
            ConfigurationCommonEventTypeAvro requiredConfig,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes)
        {
            var avroSchemaObj = requiredConfig.AvroSchema;
            var avroSchemaText = requiredConfig.AvroSchemaText;

            if (avroSchemaObj == null && avroSchemaText == null) {
                throw new ArgumentException("Null value for schema and schema text");
            }

            if (avroSchemaObj != null && avroSchemaText != null) {
                throw new ArgumentException(
                    "Both avro schema and avro schema text are supplied and one can be provided");
            }

            if (avroSchemaObj != null && !(avroSchemaObj is Schema)) {
                throw new ArgumentException(
                    "Schema expected of type " + typeof(Schema).Name + " but received " + avroSchemaObj.GetType().Name);
            }

            Schema schema;
            if (avroSchemaObj != null) {
                schema = (Schema) avroSchemaObj;
            }
            else {
                try {
                    schema = Schema.Parse(avroSchemaText);
                }
                catch (Exception ex) {
                    throw new EPException("Failed for parse avro schema: " + ex.Message, ex);
                }
            }

            return MakeType(metadata, eventAdapterService, schema, requiredConfig, superTypes, deepSuperTypes);
        }

        public AvroSchemaEventType NewEventTypeFromNormalized(
            EventTypeMetadata metadata,
            EventTypeNameResolver eventTypeNameResolver,
            EventBeanTypedEventFactory eventAdapterService,
            IDictionary<string, object> properties,
            Attribute[] annotations,
            ConfigurationCommonEventTypeAvro optionalConfig,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes,
            string statementName)
        {
            var assembler = new JArray();
            var eventTypeName = metadata.Name;

            // add supertypes first so the positions are comparable
            var added = new HashSet<string>();
            if (superTypes != null) {
                for (var i = 0; i < superTypes.Length; i++) {
                    var superType = (AvroEventType) superTypes[i];
                    foreach (var field in superType.SchemaAvro.AsRecordSchema().Fields) {
                        if (properties.ContainsKey(field.Name) || added.Contains(field.Name)) {
                            continue;
                        }

                        added.Add(field.Name);
                        assembler.Add(TypeBuilder.Field(field.Name, field.Schema));
                        //assembler.Name(field.Name).Type(field.Schema).NoDefault();
                    }
                }
            }

            foreach (var prop in properties) {
                if (!added.Contains(prop.Key)) {
                    AvroSchemaUtil.AssembleField(
                        prop.Key,
                        prop.Value,
                        assembler,
                        annotations,
                        _avroSettings,
                        eventTypeNameResolver,
                        statementName,
                        _optionalTypeMapper);
                    added.Add(prop.Key);
                }
            }

            var schema = SchemaBuilder.Record(eventTypeName, assembler);
            //Schema schema = assembler.EndRecord();
            return MakeType(metadata, eventAdapterService, schema, optionalConfig, superTypes, deepSuperTypes);
        }

        public AvroSchemaEventType NewEventTypeFromJson(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            string schemaJson,
            IList<EventType> superTypes,
            ISet<EventType> deepSuperTypes)
        {
            Console.WriteLine("Schema: {0}", schemaJson);
            var schema = Schema.Parse(schemaJson);
            return MakeType(metadata, eventBeanTypedEventFactory, schema, null, null, null);
        }

        public SelectExprProcessorRepresentationFactory OutputFactory => FACTORY_SELECT;

        public EventBeanManufacturerForge GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            WriteablePropertyDescriptor[] properties)
        {
            return new EventBeanManufacturerAvroForge(avroSchemaEventType, properties);
        }

        public EventBeanFactory GetEventBeanFactory(
            EventType type,
            EventBeanTypedEventFactory eventAdapterService)
        {
            return new EventBeanFactoryAvro(type, eventAdapterService);
        }

        public void ValidateExistingType(
            EventType existingType,
            AvroSchemaEventType proposedType)
        {
            if (!(existingType is AvroSchemaEventType)) {
                throw new EventAdapterException(
                    "Type by name '" +
                    proposedType.Name +
                    "' is not a compatible type " +
                    "(target type underlying is '" +
                    existingType.UnderlyingType.Name +
                    "', " +
                    "source type underlying is '" +
                    proposedType.UnderlyingType.Name +
                    "')");
            }

            var proposed = (Schema) proposedType.Schema;
            var existing = (Schema) ((AvroSchemaEventType) existingType).Schema;
            if (!proposed.Equals(existing)) {
                throw new EventAdapterException(
                    "Event type named '" +
                    existingType.Name +
                    "' has already been declared with differing column name or type information\n" +
                    "schemaExisting: " +
                    AvroSchemaUtil.ToSchemaStringSafe(existing) +
                    "\n" +
                    "schemaProposed: " +
                    AvroSchemaUtil.ToSchemaStringSafe(proposed));
            }
        }

        public void AvroCompat(
            EventType existingType,
            IDictionary<string, object> selPropertyTypes)
        {
            var schema = ((AvroEventType) existingType).SchemaAvro;

            foreach (var selected in selPropertyTypes) {
                var propertyName = selected.Key;
                var targetField = schema.GetField(selected.Key);

                if (targetField == null) {
                    throw new ExprValidationException(
                        "Property '" +
                        propertyName +
                        "' is not found among the fields for event type '" +
                        existingType.Name +
                        "'");
                }

                if (selected.Value is EventType targetEventTypeX) {
                    var targetAvro = CheckAvroEventTpe(selected.Key, targetEventTypeX);
                    if (targetField.Schema.Tag != Schema.Type.Record ||
                        !targetField.Schema.Equals(targetAvro.SchemaAvro)) {
                        throw new ExprValidationException(
                            "Property '" +
                            propertyName +
                            "' is incompatible, expecting a compatible schema '" +
                            targetField.Schema.Name +
                            "' but received schema '" +
                            targetAvro.SchemaAvro.Name +
                            "'");
                    }
                }
                else if (selected.Value is EventType[] targetEventTypes) {
                    var targetEventType = targetEventTypes[0];
                    var targetAvro = CheckAvroEventTpe(selected.Key, targetEventType);
                    if (targetField.Schema.Tag != Schema.Type.Array ||
                        targetField.Schema.AsArraySchema().ItemSchema.Tag != Schema.Type.Record ||
                        !targetField.Schema.AsArraySchema().ItemSchema.Equals(targetAvro.SchemaAvro)) {
                        throw new ExprValidationException(
                            "Property '" +
                            propertyName +
                            "' is incompatible, expecting an array of compatible schema '" +
                            targetField.Schema.Name +
                            "' but received schema '" +
                            targetAvro.SchemaAvro.Name +
                            "'");
                    }
                }
            }
        }

        public object ConvertEvent(
            EventBean theEvent,
            AvroSchemaEventType targetType)
        {
            var original = ((AvroGenericDataBackedEventBean) theEvent).Properties;
            var targetSchema = (Schema) targetType.Schema;
            var target = new GenericRecord(targetSchema.AsRecordSchema());

            foreach (var field in original.Schema.Fields) {
                var targetField = targetSchema.GetField(field.Name);
                if (targetField == null) {
                    continue;
                }

                if (field.Schema.Tag == Schema.Type.Array) {
                    var originalColl = (ICollection<object>) original.Get(field);
                    if (originalColl != null) {
                        target.Put(targetField, new List<object>(originalColl));
                    }
                }
                else if (field.Schema.Tag == Schema.Type.Map) {
                    var originalMap = (IDictionary<string, object>) original.Get(field);
                    if (originalMap != null) {
                        target.Put(targetField, new Dictionary<string, object>(originalMap));
                    }
                }
                else {
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

        protected abstract AvroSchemaEventType MakeType(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventAdapterService,
            Schema schema,
            ConfigurationCommonEventTypeAvro optionalConfig,
            EventType[] supertypes,
            ISet<EventType> deepSupertypes);

        public void Init(
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
            ImportService importService)
        {
            _avroSettings = avroSettings;

            if (avroSettings.TypeRepresentationMapperClass != null) {
                _optionalTypeMapper = TypeHelper.Instantiate<TypeRepresentationMapper>(
                    avroSettings.TypeRepresentationMapperClass,
                    importService.ClassForNameProvider);
            }

            if (avroSettings.ObjectValueTypeWidenerFactoryClass != null) {
                _optionalWidenerFactory = TypeHelper.Instantiate<ObjectValueTypeWidenerFactory>(
                    avroSettings.ObjectValueTypeWidenerFactoryClass,
                    importService.ClassForNameProvider);
            }
        }

        private AvroEventType CheckAvroEventTpe(
            string propertyName,
            EventType eventType)
        {
            if (!(eventType is AvroEventType)) {
                throw new ExprValidationException(
                    "Property '" +
                    propertyName +
                    "' is incompatible with event type '" +
                    eventType.Name +
                    "' underlying type " +
                    eventType.UnderlyingType.Name);
            }

            return (AvroEventType) eventType;
        }

        public abstract EventBean AdapterForTypeAvro(
            object avroGenericDataDotRecord,
            EventType existingType);
    }
} // end of namespace