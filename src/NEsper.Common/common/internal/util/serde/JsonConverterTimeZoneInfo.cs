using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterTimeZoneInfo : JsonConverter<TimeZoneInfo>
    {
        internal JsonConverterTimeZoneInfo()
        {
        }

        public override TimeZoneInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var timeZoneId = reader.GetString();
            if (timeZoneId == null) {
                return null;
            }

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeZoneInfo value,
            JsonSerializerOptions options)
        {
            if (value == null) {
                writer.WriteNullValue();
            }
            else {
                writer.WriteStringValue(value.Id.AsSpan());
            }
        }
    }
}