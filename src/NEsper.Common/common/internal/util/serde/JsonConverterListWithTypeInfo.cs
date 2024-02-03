using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterListWithTypeInfo : JsonConverter<IList<object>>
    {
        private readonly IDictionary<Type, bool> _typeResultCache;

        public JsonConverterListWithTypeInfo()
        {
            _typeResultCache = new Dictionary<Type, bool>();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(IList<object>)) {
                return true;
            }

            if (typeToConvert.IsPrimitive || typeToConvert.IsEnum) {
                return false;
            }

            if (_typeResultCache.TryGetValue(typeToConvert, out var result)) {
                return result;
            }

            result = typeToConvert.GetInterfaces().Any(_ => _ == typeof(IList<object>));
            _typeResultCache[typeToConvert] = result;
            return result;
        }
        
        private static void TryExpect(ref Utf8JsonReader reader, JsonTokenType tokenType)
        {
            if (reader.Read()) {
                if (reader.TokenType == tokenType) {
                    return;
                }
                throw new JsonException($"invalid content; expecting {tokenType}, but received {reader.TokenType}");
            }

            throw new JsonException($"invalid content; expecting {tokenType}, but received end of stream");
        }

        private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.Read()) {
                if (reader.TokenType == JsonTokenType.String) {
                    var valueTypeName = reader.GetString();
                    var valueType = Type.GetType(valueTypeName);
                    if (reader.Read()) {
                        var value = JsonSerializer.Deserialize(ref reader, valueType, options);
                        return value;
                    }
                    
                    throw new JsonException($"invalid content; expecting token, but received end of stream");
                }

                throw new JsonException($"invalid content; expecting {JsonTokenType.String}, but received {reader.TokenType}");
            }

            throw new JsonException($"invalid content; expecting {JsonTokenType.String}, but received end of stream");
        }

        public override IList<object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;
            } 
            
            if (reader.TokenType == JsonTokenType.StartArray) {
                TryExpect(ref reader, JsonTokenType.String);
                var listTypeName = reader.GetString();
                var listType = Type.GetType(listTypeName);
                var list = listType.IsArray ? new List<object>() : TypeHelper.Instantiate<IList<object>>(listType);

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    if (reader.TokenType == JsonTokenType.Null) {
                        list.Add(null);
                    }
                    else if (reader.TokenType == JsonTokenType.StartArray) {
                        var value = ReadValue(ref reader, options);
                        list.Add(value);
                        TryExpect(ref reader, JsonTokenType.EndArray);
                    }
                }

                if (listType.IsArray) {
                    // convert the "List<>" to an array to match the expected return
                    return list.ToArray();
                }

                return list;
            }

            throw new JsonException($"invalid content; expecting {JsonTokenType.StartArray}, but received end of stream");
        }

        public override void Write(
            Utf8JsonWriter writer,
            IList<object> values,
            JsonSerializerOptions options)
        {
            if (values == null) {
                writer.WriteNullValue();
                return;
            }
            
            writer.WriteStartArray();
            writer.WriteStringValue(values.GetType().AssemblyQualifiedName);

            foreach (var value in values) {
                if (value == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStartArray();
                    writer.WriteStringValue(value.GetType().AssemblyQualifiedName);
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    writer.WriteEndArray();
                }
            }

            writer.WriteEndArray();
        }
    }
}