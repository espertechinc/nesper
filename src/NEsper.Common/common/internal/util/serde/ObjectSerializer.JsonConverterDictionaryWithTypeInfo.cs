using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public partial class ObjectSerializer
    {
        private class JsonConverterDictionaryWithTypeInfo : JsonConverter<IDictionary<string,object>>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                if (typeToConvert == typeof(IDictionary<string, object>)) {
                    return true;
                }

                // Maybe this is an implementation
                return typeToConvert
                    .GetInterfaces()
                    .Any(iface => iface == typeof(IDictionary<string, object>));
            }

            public override IDictionary<string,object> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var dictionary = new Dictionary<string, object>();

                using JsonDocument doc = JsonDocument.ParseValue(ref reader);
                
                foreach (JsonProperty kvp in doc.RootElement.EnumerateObject())
                {
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
                IDictionary<string,object> value,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                
                foreach (var kvp in value)
                {
                    var kvpValue = kvp.Value;
                    if (kvpValue == null) {
                        writer.WriteNull(kvp.Key);
                    } else {
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
}