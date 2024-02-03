///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using Avro;
using Avro.Generic;
using Avro.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.compat;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Support
{
	public class SupportAvroUtil
	{
		public static string AvroToJson(EventBean theEvent)
		{
			Schema schema = (Schema)((AvroSchemaEventType)theEvent.EventType).Schema;
			GenericRecord record = (GenericRecord)theEvent.Underlying;
			return AvroToJsonX(schema, record);
		}

		public static string AvroToJson(
			Schema schema,
			GenericRecord datum)
		{
			var writer = new GenericDatumWriter<object>(schema);
			var textWriter = new StringWriter();
			var encoder = new Avro.IO.JsonEncoder(textWriter);
			writer.Write(datum, encoder);
			return writer.ToString();
		}

		public static string AvroToJsonX(
			Schema schema,
			GenericRecord datum)
		{
			var converter = new GenericRecordToJsonConverter();
			var encodedResult = converter.Encode(datum);
			return JsonConvert.SerializeObject(encodedResult, Formatting.Indented);
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
			var input = new MemoryStream(json.GetUTF8Bytes());
			try {
				Decoder decoder = new BinaryDecoder(input);
				DatumReader<object> reader = new GenericDatumReader<object>(schema, schema);
				return (GenericRecord)reader.Read(null, decoder);
			}
			catch (IOException ex) {
				throw new EPRuntimeException("Failed to parse json: " + ex.Message, ex);
			}
		}

		public static string CompareSchemas(
			Schema schemaOne,
			Schema schemaTwo)
		{
			ISet<string> names = new HashSet<string>();
			AddSchemaFieldNames(names, schemaOne);
			AddSchemaFieldNames(names, schemaTwo);

			foreach (string name in names) {
				var fieldOne = schemaOne.GetField(name);
				var fieldTwo = schemaTwo.GetField(name);
				if (fieldOne == null) {
					return "Failed to find field '" + name + " in schema-one";
				}

				if (fieldTwo == null) {
					return "Failed to find field '" + name + " in schema-one";
				}

				var fieldOneSchema = fieldOne.Schema;
				var fieldTwoSchema = fieldTwo.Schema;
				if (!Equals(fieldOneSchema, fieldTwoSchema)) {
					return "\nSchema-One: " +
					       fieldOneSchema +
					       "\n" +
					       "Schema-Two: " +
					       fieldTwoSchema;
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
			foreach (var field in schema.AsRecordSchema().Fields) {
				names.Add(field.Name);
			}
		}

		public static Schema GetAvroSchema(EventBean @event)
		{
			return GetAvroSchema(@event.EventType);
		}

		public static Schema GetAvroSchema(EventType eventType)
		{
			return ((AvroEventType)eventType).SchemaAvro;
		}
	}
} // end of namespace
