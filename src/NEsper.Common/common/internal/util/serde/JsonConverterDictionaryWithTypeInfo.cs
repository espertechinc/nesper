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

#if false
            // Maybe this is an implementation
            return typeToConvert
                .GetInterfaces()
                .Any(iface => iface == typeof(IDictionary<string, object>));
#endif
        }

        public override IDictionary<string, object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var dictionary = new Dictionary<string, object>();

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);

            foreach (JsonProperty kvp in doc.RootElement.EnumerateObject()) {
                if (kvp.Value.ValueKind == JsonValueKind.Null) {
                    dictionary[kvp.Name] = null;
                }
                else {
                    var kvpValue = kvp.Value;
                    var typeElement = kvpValue.GetProperty("__type");
                    var valueElement = kvpValue.GetProperty("__value");
                    var valueType = Type.GetType(typeElement.GetString());
                    var value = JsonSerializer.Deserialize(valueElement.GetRawText(), valueType, options);
                    dictionary[kvp.Name] = value;
                }
            }

            return dictionary;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<string, object> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value) {
                var kvpValue = kvp.Value;
                if (kvpValue == null) {
                    writer.WriteNull(kvp.Key);
                }
                else {
                    writer.WritePropertyName(kvp.Key);
                    writer.WriteStartObject();
                    writer.WriteString("__type", kvpValue.GetType().AssemblyQualifiedName);
                    writer.WritePropertyName("__value");
                    JsonSerializer.Serialize(writer, kvpValue, kvpValue.GetType(), options);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject();
        }
    }
}