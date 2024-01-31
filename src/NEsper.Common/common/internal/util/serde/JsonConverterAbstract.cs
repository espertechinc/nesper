using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterAbstract<T> : JsonConverter<T>
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var typeElement = root.GetProperty("__type");
            var valueType = Type.GetType(typeElement.GetString());
            var valueElement = root.GetProperty("__value");
            var value = valueElement.Deserialize(valueType, options);
            return (T) value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("__type", value.GetType().AssemblyQualifiedName);
            writer.WritePropertyName("__value");
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
            writer.WriteEndObject();
        }
    }
}