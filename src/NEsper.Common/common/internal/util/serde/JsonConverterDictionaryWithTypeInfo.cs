using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterDictionaryWithTypeInfo : JsonConverter<IDictionary<string, object>>
    {
        private readonly IDictionary<Type, bool> _typeResultCache;

        public JsonConverterDictionaryWithTypeInfo()
        {
            _typeResultCache = new Dictionary<Type, bool>();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(IDictionary<string, object>)) {
                return true;
            }

            if (typeToConvert.IsPrimitive || typeToConvert.IsEnum) {
                return false;
            }

            if (_typeResultCache.TryGetValue(typeToConvert, out var result)) {
                return result;
            }

            result = typeToConvert.GetInterfaces().Any(_ => _ == typeof(IDictionary<string, object>));
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

        private object ReadValueOrNull(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.Read()) {
                if (reader.TokenType == JsonTokenType.Null) {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String) {
                    var valueTypeName = reader.GetString();
                    var valueType = Type.GetType(valueTypeName);
                    if (reader.Read()) {
                        var value = JsonSerializer.Deserialize(ref reader, valueType, options);
                        return value;
                    }
                    
                    throw new JsonException($"invalid content; expecting token, but received end of stream");
                }

                throw new JsonException($"invalid content; expecting {JsonTokenType.Null} or {JsonTokenType.String}, but received {reader.TokenType}");
            }

            throw new JsonException($"invalid content; expecting {JsonTokenType.Null} or {JsonTokenType.String}, but received end of stream");
        }
        
        public override IDictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;
            } 
            
            if (reader.TokenType == JsonTokenType.StartArray) {
                TryExpect(ref reader, JsonTokenType.String);
                var dictionaryTypeName = reader.GetString();
                var dictionaryType = Type.GetType(dictionaryTypeName);
                var dictionary = TypeHelper.Instantiate<IDictionary<string, object>>(dictionaryType);

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    if (reader.TokenType == JsonTokenType.StartArray) {
                        TryExpect(ref reader, JsonTokenType.String);
                        var key = reader.GetString();
                        var val = ReadValueOrNull(ref reader, options);
                        dictionary[key] = val;
                        TryExpect(ref reader, JsonTokenType.EndArray);
                    }
                }

                return dictionary;
            }
            
            throw new JsonException($"invalid content; expecting {JsonTokenType.StartArray}, but received end of stream");
        }

        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<string, object> value,
            JsonSerializerOptions options)
        {
            if (value == null) {
                writer.WriteNullValue();
                return;
            }
                
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetType().AssemblyQualifiedName);

            foreach (var kvp in value) {
                var kvpValue = kvp.Value;
                writer.WriteStartArray();
                writer.WriteStringValue(kvp.Key);
                if (kvpValue == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStringValue(kvpValue.GetType().AssemblyQualifiedName);
                    JsonSerializer.Serialize(writer, kvpValue, kvpValue.GetType(), options);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}