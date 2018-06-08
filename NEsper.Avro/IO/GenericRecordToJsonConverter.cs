using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using Newtonsoft.Json.Linq;
using com.espertech.esper.compat.magic;

namespace NEsper.Avro.IO
{
    public class GenericRecordToJsonConverter
    {
        private static readonly JValue NULL_VALUE = new JValue((object) null);
        private static readonly JValue TRUE_VALUE = new JValue(true);
        private static readonly JValue FALSE_VALUE = new JValue(false);

        public GenericRecordToJsonConverter()
        {
        }


        private bool ValueMatchSchemaType(Schema schema, object value)
        {
            switch (schema.Tag)
            {
                case Schema.Type.Null:
                    return value == null;
                case Schema.Type.Boolean:
                    return value is bool;
                case Schema.Type.Int:
                    return value is int;
                case Schema.Type.Long:
                    return value is long;
                case Schema.Type.Float:
                    return value is float;
                case Schema.Type.Double:
                    return value is double;
                case Schema.Type.String:
                    return value is string;
                case Schema.Type.Bytes:
                    return value is byte[];
                case Schema.Type.Error:
                case Schema.Type.Record:
                    return (value is GenericRecord)
                           && (schema is RecordSchema)
                           && (value as GenericRecord).Schema.SchemaName.Equals(((RecordSchema)schema).SchemaName);
                case Schema.Type.Enumeration:
                    return (value is GenericEnum)
                           && (schema is EnumSchema)
                           && (value as GenericEnum).Schema.SchemaName.Equals(((EnumSchema)schema).SchemaName);
                case Schema.Type.Fixed:
                    return (value is GenericFixed)
                           && (schema is FixedSchema)
                           && (value as GenericFixed).Schema.SchemaName.Equals(((FixedSchema)schema).SchemaName);
                case Schema.Type.Array:
                    return (value is Array) && (!(value is byte[]));
                case Schema.Type.Map:
                    return (value is IDictionary<string, object>);
                case Schema.Type.Union:
                    return false;
                default:
                    throw new ArgumentException("unknown schema tag: " + schema.Tag, nameof(schema));
            }
        }

        private JToken EncodeFixed(FixedSchema schema, GenericFixed value)
        {
            var jvalue = new JObject();

            jvalue.Add("type", new JValue("fixed"));
            jvalue.Add("data", new JValue(value.Value));

            return jvalue;
        }


        private JObject EncodeRecord(RecordSchema schema, GenericRecord record)
        {
            var jvalue = new JObject();

            foreach (var field in schema.Fields)
            {
                object fieldValue;
                if (record.TryGetValue(field.Name, out fieldValue))
                {
                    jvalue.Add(field.Name, Encode(field.Schema, fieldValue));
                }
                else
                {
                    // field does not exist in schema
                }
            }

            return jvalue;
        }

        private JArray EncodeArray(ArraySchema arraySchema, object value)
        {
            var itemsType = arraySchema.ItemSchema;
            var itemsArray = value as Array;
            var jvalue = new JArray();

            for (int ii = 0; ii < itemsArray.Length; ii++)
            {
                jvalue.Add(Encode(itemsType, itemsArray.GetValue(ii)));
            }

            return jvalue;
        }

        private JObject EncodeMap(MapSchema schema, object value)
        {
            var valueType = schema.ValueSchema;
            var valueDict = MagicMarker.GetStringDictionary(value);
            var jvalue = new JObject();

            foreach (var valueKeyPair in valueDict)
            {
                jvalue.Add(new JProperty(valueKeyPair.Key, Encode(valueType, valueKeyPair.Value)));
            }

            return jvalue;
        }
        
        private JToken EncodeUnion(UnionSchema schema, object value)
        {
            var unionTypes = schema.Schemas.ToArray();

            foreach (var unionType in unionTypes)
            {
                if (ValueMatchSchemaType(unionType, value))
                {
                    if (String.IsNullOrEmpty(unionType.Name))
                    {
                        return Encode(unionType, value);
                    }
                    else
                    {
                        return new JObject(
                            new JProperty(unionType.Name, Encode(unionType, value)));
                    }
                }
            }

            throw new ArgumentException("value did not match schema");
        }

        private JToken EncodeEnum(EnumSchema schema, GenericEnum value)
        {
            return new JValue(value.Value);
        }

        private JToken Encode(Schema schema, Object value)
        {
            switch (schema.Tag)
            {
                case Schema.Type.Null:
                    return NULL_VALUE;
                case Schema.Type.Boolean:
                    return true.Equals(value)
                        ? new JValue(true)
                        : new JValue(false);
                case Schema.Type.Int:
                    return new JValue((int) value);
                case Schema.Type.Long:
                    return new JValue((long) value);
                case Schema.Type.Float:
                    return new JValue((float) value);
                case Schema.Type.Double:
                    return new JValue((double) value);
                case Schema.Type.String:
                    return new JValue((string) value);
                case Schema.Type.Bytes:
                    return new JValue((byte[]) value);
                case Schema.Type.Error:
                    throw new NotImplementedException();
                case Schema.Type.Record:
                    return EncodeRecord((RecordSchema) schema, (GenericRecord) value);
                case Schema.Type.Enumeration:
                    return EncodeEnum((EnumSchema) schema, (GenericEnum) value);
                case Schema.Type.Fixed:
                    return EncodeFixed((FixedSchema) schema, (GenericFixed) value);
                case Schema.Type.Array:
                    return EncodeArray((ArraySchema) schema, value);
                case Schema.Type.Map:
                    return EncodeMap((MapSchema)schema, value);
                case Schema.Type.Union:
                    return EncodeUnion((UnionSchema) schema, value);
                default:
                    throw new ArgumentException("unknown schema tag: " + schema.Tag, nameof(schema));
            }
        }

        public JObject Encode(GenericRecord record)
        {
            return EncodeRecord(record.Schema, record);
        }
    }
}
