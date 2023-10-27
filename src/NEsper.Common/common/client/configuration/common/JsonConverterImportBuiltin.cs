using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.client.configuration.common
{
    public class JsonConverterImportBuiltin : JsonConverter<ImportBuiltinAnnotations>
    {
        public JsonConverterImportBuiltin()
        {
        }

        public override ImportBuiltinAnnotations Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            JsonDocument.ParseValue(ref reader);
            return ImportBuiltinAnnotations.Instance;
        }

        public override void Write(
            Utf8JsonWriter writer,
            ImportBuiltinAnnotations value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}