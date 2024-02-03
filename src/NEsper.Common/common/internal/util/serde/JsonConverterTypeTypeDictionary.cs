using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterTypeTypeDictionary : JsonConverter<IDictionary<Type, Type>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(IDictionary<Type, Type>);
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
        
        public override IDictionary<Type, Type> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var dictionary = new Dictionary<Type, Type>();

            // Check to see if the curren token is the start of an array
            if (reader.TokenType == JsonTokenType.StartArray) {
                // Read each row inside the array; stop if the token that we read is
                // an end of array token.
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    // If it's another start array, we are in the inner array.
                    if (reader.TokenType == JsonTokenType.StartArray) {
                        TryExpect(ref reader, JsonTokenType.String);
                        
                        var keyTypeName = reader.GetString();
                        var keyType = Type.GetType(keyTypeName!);
                        TryExpect(ref reader, JsonTokenType.String);
                        
                        var valTypeName = reader.GetString();
                        var valType = Type.GetType(valTypeName!);
                        TryExpect(ref reader, JsonTokenType.EndArray);

                        dictionary[keyType!] = valType;
                    }
                }
            }
            else {
                throw new JsonException($"invalid content; expecting array, but received {reader.TokenType}");
            }

            return dictionary;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<Type, Type> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var kvp in value) {
                writer.WriteStartArray();
                writer.WriteStringValue(kvp.Key.AssemblyQualifiedName);
                writer.WriteStringValue(kvp.Value.AssemblyQualifiedName);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}