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

using Newtonsoft.Json.Linq;

using NEsper.Avro.Extensions;

using com.espertech.esper.compat;
using com.espertech.esper.compat.magic;

namespace NEsper.Avro.IO
{
    public static class JsonDecoder
    {
        public static object DecodeMap(this MapSchema schema, JToken value)
        {
            var jobject = value as JObject;
            if (jobject != null)
            {
                var valueSchema = schema.ValueSchema;
                var valueType = GetNativeType(valueSchema);
                var valueDict = typeof(IDictionary<,>)
                    .MakeGenericType(typeof(string), valueType)
                    .GetConstructor(new Type[0])
                    .Invoke(null);

                var magicDict = MagicMarker.GetStringDictionaryFactory(valueType).Invoke(valueDict);

                foreach (var property in jobject.Properties())
                {
                    magicDict[property.Name] = DecodeAny(valueSchema, property.Value);
                }

                return valueDict;
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static Array DecodeArray(this ArraySchema schema, JToken value)
        {
            var jarray = value as JArray;
            if (jarray != null)
            {
                var itemType = GetNativeType(schema.ItemSchema);
                var itemArray = Array.CreateInstance(itemType, jarray.Count);
                var itemIndex = 0;

                foreach (var item in jarray.Values())
                {
                    itemArray.SetValue(DecodeAny(schema.ItemSchema, item), itemIndex);
                    itemIndex++;
                }

                return itemArray;
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static object DecodeUnion(this UnionSchema schema, JToken value)
        {
            foreach (var schemaType in schema.Schemas)
            {
                try
                {
                    return DecodeAny(schemaType, value);
                }
                catch(ArgumentException)
                {
                }
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static GenericRecord DecodeRecord(this RecordSchema schema, JToken value)
        {
            var jvalue = value as JObject;
            if (jvalue != null)
            {
                var record = new GenericRecord(schema);

                foreach (var field in schema.Fields)
                {
                    var property = jvalue.Property(field.Name);
                    var rvalue = DecodeAny(field.Schema, property.Value);
                    record.Put(field.Name, rvalue);
                }

                return record;
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static string DecodeString(this PrimitiveSchema schema, JToken value)
        {
            var jvalue = value as JValue;
            if (jvalue != null)
            {
                var underlying = jvalue.Value;
                if (underlying is string)
                {
                    return (string)underlying;
                }
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static T DecodePrimitive<T>(this PrimitiveSchema schema, JToken value)
        {
            var jvalue = value as JValue;
            if (jvalue != null)
            {
                var underlying = jvalue.Value;
                if (underlying is T)
                {
                    return (T)underlying;
                }

                var castConverter = CastHelper.GetCastConverter<T>();
                if (castConverter != null)
                {
                    return castConverter(underlying);
                }
            }

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static object DecodeNull(this PrimitiveSchema schema, JToken value)
        {
            if (value.Type == JTokenType.Null)
                return null;

            throw new ArgumentException("invalid value type: " + value.GetType().FullName);
        }

        public static object DecodeAny(this Schema schema, JToken value)
        {
            switch (schema.Tag)
            {
                case Schema.Type.Int:
                    return DecodePrimitive<int>((PrimitiveSchema) schema, value);
                case Schema.Type.Long:
                    return DecodePrimitive<long>((PrimitiveSchema) schema, value);
                case Schema.Type.Float:
                    return DecodePrimitive<float>((PrimitiveSchema) schema, value);
                case Schema.Type.Double:
                    return DecodePrimitive<double>((PrimitiveSchema) schema, value);
                case Schema.Type.Boolean:
                    return DecodePrimitive<bool>((PrimitiveSchema) schema, value);
                case Schema.Type.Bytes:
                    return DecodePrimitive<byte[]>((PrimitiveSchema) schema, value);
                case Schema.Type.String:
                    return DecodeString((PrimitiveSchema) schema, value);
                case Schema.Type.Null:
                    return DecodeNull((PrimitiveSchema) schema, value);
                case Schema.Type.Fixed:
                case Schema.Type.Error:
                    throw new NotImplementedException();
                case Schema.Type.Map:
                    return DecodeMap((MapSchema) schema, value);
                case Schema.Type.Union:
                    return DecodeUnion((UnionSchema) schema, value);
                case Schema.Type.Record:
                    return DecodeRecord((RecordSchema) schema, value);
                case Schema.Type.Array:
                    return DecodeArray((ArraySchema) schema, value);
                case Schema.Type.Enumeration:
                    throw new NotImplementedException();
            }

            throw new ArgumentException("invalid schema type: " + schema);
        }

        private static Type GetNativeType(Schema schema)
        {
            switch (schema.Tag)
            {
                case Schema.Type.Int:
                    return typeof(int);
                case Schema.Type.Long:
                    return typeof(long);
                case Schema.Type.Float:
                    return typeof(float);
                case Schema.Type.Double:
                    return typeof(double);
                case Schema.Type.Boolean:
                    return typeof(bool);
                case Schema.Type.Bytes:
                    return typeof(byte[]);
                case Schema.Type.String:
                    return typeof(string);
                case Schema.Type.Null:
                    return typeof(object);
                case Schema.Type.Fixed:
                case Schema.Type.Error:
                    throw new NotImplementedException();
                case Schema.Type.Map:
                    return typeof(IDictionary<,>).MakeGenericType(typeof(string), GetNativeType(((MapSchema)schema).ValueSchema));
                case Schema.Type.Union:
                    return typeof(object);
                case Schema.Type.Record:
                    return typeof(GenericRecord);
                case Schema.Type.Array:
                    return GetNativeType((ArraySchema)schema).MakeArrayType();
                case Schema.Type.Enumeration:
                    return typeof(GenericEnum);
            }

            throw new ArgumentException("invalid schema type: " + schema);
        }
    }
}
