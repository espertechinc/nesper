///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using Avro;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.IO
{
    public class SchemaToJsonEncoder
    {
        private static readonly JValue NULL_VALUE = new JValue((object) null);
        private static readonly JValue TRUE_VALUE = new JValue(true);
        private static readonly JValue FALSE_VALUE = new JValue(false);

        public SchemaToJsonEncoder()
        {
        }

        private static JObject EncodeField(Field field)
        {
            var jobject = new JObject();
            jobject.Add("name", field.Name);
            jobject.Add("type", Encode(field.Schema));

            return jobject;
        }

        private static JObject EncodeRecord(RecordSchema schema)
        {
            var jfields = new JArray();
            schema.Fields.ForEach(field => jfields.Add(EncodeField(field)));

            var jobject = new JObject();
            jobject.Add(new JProperty("type", "record"));
            jobject.Add(new JProperty("name", schema.Name));
            jobject.Add(new JProperty("fields", jfields));

            return jobject;
        }

        private static JObject EncodeFixed(FixedSchema schema)
        {
            var jobject = new JObject();
            jobject.Add(new JProperty("type", "fixed"));
            jobject.Add(new JProperty("name", schema.Name));
            jobject.Add(new JProperty("size", schema.Size));

            return jobject;
        }

        private static JObject EncodeArray(ArraySchema schema)
        {
            var jobject = new JObject();
            jobject.Add(new JProperty("type", "array"));
            jobject.Add(new JProperty("items", Encode(schema.ItemSchema)));

            return jobject;
        }

        private static JObject EncodeMap(MapSchema schema)
        {
            var jobject = new JObject();
            jobject.Add(new JProperty("type", "map"));
            jobject.Add(new JProperty("values", Encode(schema.ValueSchema)));

            return jobject;
        }

        private static JArray EncodeUnion(UnionSchema schema)
        {
            var jarray = new JArray();

            schema.Schemas
                .Select(child => Encode(child))
                .ToList()
                .ForEach(jarray.Add);

            return jarray;
        }

        private static JObject EncodeEnum(EnumSchema schema)
        {
            var jsymbols = new JArray();
            schema.Symbols.ToList().ForEach(symbol => jsymbols.Add(symbol));

            var jobject = new JObject();
            jobject.Add(new JProperty("type", "enum"));
            jobject.Add(new JProperty("name", schema.Name));
            jobject.Add(new JProperty("symbols", jsymbols));

            return jobject;
        }

        private static JToken EncodePrimitive(PrimitiveSchema schema)
        {
            JToken jtoken = null;

            switch (schema.Tag)
            {
                case Schema.Type.Null:
                    jtoken = new JValue("null");
                    break;
                case Schema.Type.Boolean:
                    jtoken = new JValue("boolean");
                    break;
                case Schema.Type.Int:
                    jtoken = new JValue("int");
                    break;
                case Schema.Type.Long:
                    jtoken = new JValue("long");
                    break;
                case Schema.Type.Float:
                    jtoken = new JValue("float");
                    break;
                case Schema.Type.Double:
                    jtoken = new JValue("double");
                    break;
                case Schema.Type.String:
                    jtoken = new JValue("string");
                    break;
                case Schema.Type.Bytes:
                    jtoken = new JValue("bytes");
                    break;
                default:
                    throw new ArgumentException("unknown schema tag: " + schema.Tag, nameof(schema));
            }

            var propertyMap = schema.GetPropertyMap();
            if ((propertyMap == null) || (propertyMap.Count == 0))
            {
                return jtoken;
            }

            var jobject = new JObject();
            jobject.Add(new JProperty("type", jtoken));

            foreach (var property in propertyMap)
            {
                var propertyKey = property.Key;
                var propertyValue = property.Value;
                if ((propertyValue.Length >= 2) &&
                    (propertyValue[0] == '"') &&
                    (propertyValue[propertyValue.Length - 1] == '"'))
                {
                    propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
                }

                jobject.Add(new JProperty(property.Key, propertyValue));
            }

            return jobject;
        }

        public static JToken Encode(Schema schema)
        {
            switch (schema.Tag)
            {
                case Schema.Type.Null:
                case Schema.Type.Boolean:
                case Schema.Type.Int:
                case Schema.Type.Long:
                case Schema.Type.Float:
                case Schema.Type.Double:
                case Schema.Type.String:
                case Schema.Type.Bytes:
                    return EncodePrimitive((PrimitiveSchema)schema);
                case Schema.Type.Error:
                    throw new NotImplementedException();
                case Schema.Type.Record:
                    return EncodeRecord((RecordSchema) schema);
                case Schema.Type.Enumeration:
                    return EncodeEnum((EnumSchema) schema);
                case Schema.Type.Fixed:
                    return EncodeFixed((FixedSchema) schema);
                case Schema.Type.Array:
                    return EncodeArray((ArraySchema) schema);
                case Schema.Type.Map:
                    return EncodeMap((MapSchema)schema);
                case Schema.Type.Union:
                    return EncodeUnion((UnionSchema) schema);
                default:
                    throw new ArgumentException("unknown schema tag: " + schema.Tag, nameof(schema));
            }
        }
    }
}
