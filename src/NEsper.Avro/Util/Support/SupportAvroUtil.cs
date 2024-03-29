///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Util.Support
{
    public static class SupportAvroUtil
    {
        public static string AvroToJson(EventBean theEvent)
        {
            //var schema = (Schema) ((AvroSchemaEventType) theEvent.EventType).Schema;
            var record = (GenericRecord) theEvent.Underlying;
            return AvroToJson(record);
        }

        public static string AvroToJson(GenericRecord datum)
        {
            try {
                using (var textWriter = new StringWriter()) {
                    using (var writer = new JsonTextWriter(textWriter)) {
                        var converter = new GenericRecordToJsonConverter();
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, converter.Encode(datum));
                        return textWriter.ToString();
                    }
                }
            }
            catch (IOException ex) {
                throw new EPException(ex);
            }
        }

        public static GenericRecord ParseQuoted(
            Schema schema,
            string json)
        {
            return Parse(schema, json.Replace("'", "\""));
        }

        public static GenericRecord Parse(
            Schema schema,
            string json)
        {
            var jsonEntity = (JToken) JsonConvert.DeserializeObject(json);
            var avroEntity = JsonDecoder.DecodeAny(schema, jsonEntity);
            if (avroEntity is GenericRecord)
            {
                return (GenericRecord)avroEntity;
            }

            throw new ArgumentException("schema was not a record");
        }

        public static string CompareSchemas(
            Schema schemaOne,
            Schema schemaTwo)
        {
            var names = new HashSet<string>();
            AddSchemaFieldNames(names, schemaOne);
            AddSchemaFieldNames(names, schemaTwo);

            foreach (var name in names) {
                Field fieldOne = schemaOne.GetField(name);
                Field fieldTwo = schemaTwo.GetField(name);
                if (fieldOne == null) {
                    return "Failed to find field '" + name + " in schema-one";
                }

                if (fieldTwo == null) {
                    return "Failed to find field '" + name + " in schema-one";
                }

                if (!fieldOne.Schema.Equals(fieldTwo.Schema)) {
                    return "\nSchema-One: " +
                           fieldOne.Schema +
                           "\n" +
                           "Schema-Two: " +
                           fieldTwo.Schema;
                }
            }

            return null;
        }

        public static AvroEventType MakeAvroSupportEventType(Schema schema)
        {
            EventTypeMetadata metadata = new EventTypeMetadata(
                "typename",
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.AVRO,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            return new AvroEventType(metadata, schema, null, null, null, null, null, new EventTypeAvroHandlerImpl());
        }

        private static void AddSchemaFieldNames(
            ISet<string> names,
            Schema schema)
        {
            foreach (Field field in schema.GetFields()) {
                names.Add(field.Name);
            }
        }

        public static Schema GetAvroSchema(EventBean @event)
        {
            return GetAvroSchema(@event.EventType);
        }

        public static Schema GetAvroSchema(EventType eventType)
        {
            return ((AvroEventType) eventType).SchemaAvro;
        }
    }
} // end of namespace