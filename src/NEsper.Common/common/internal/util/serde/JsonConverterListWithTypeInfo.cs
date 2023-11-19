using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterListWithTypeInfo : JsonConverter<IList<object>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(IList<object>)) {
                return true;
            }

            // Maybe this is an implementation
            return typeToConvert
                .GetInterfaces()
                .Any(iface => iface == typeof(IList<object>));
        }

        public override IList<object> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var list = new List<object>();

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);

            foreach (JsonElement values in doc.RootElement.EnumerateArray()) {
                if (values.ValueKind == JsonValueKind.Null) {
                    list.Add(null);
                }
                else if (values.ValueKind == JsonValueKind.Object) {
                    var typeElement = values.GetProperty("__type");
                    var valueElement = values.GetProperty("__value");
                    var valueType = Type.GetType(typeElement.GetString());
                    var value = JsonSerializer.Deserialize(valueElement.GetRawText(), valueType, options);
                    list.Add(value);
                }
            }

            return list;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IList<object> values,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var value in values) {
                if (value == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStartObject();
                    writer.WriteString("__type", value.GetType().AssemblyQualifiedName);
                    writer.WritePropertyName("__value");
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndArray();
        }
    }
}