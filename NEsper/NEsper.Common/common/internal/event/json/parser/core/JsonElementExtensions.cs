using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public static class JsonElementExtensions
    {
        public static object ElementToValue(this JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => ElementToNumeric(jsonElement),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Object => ElementToDictionary(jsonElement),
                JsonValueKind.Array => ElementToArray(jsonElement),
                JsonValueKind.Undefined => throw new EPException("value is undefined"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static IDictionary<string, object> ElementToDictionary(this JsonElement element)
        {
            var result = new Dictionary<string, object>();
            var enumerator = element.EnumerateObject();
            for (int ii = 0 ; enumerator.MoveNext() ; ii++) {
                var property = enumerator.Current;
                result.Add(property.Name, ElementToValue(property.Value));
            }

            return result;
        }
        
        public static IList<object> ElementToArray(this JsonElement element)
        {
            return ElementToArray(element, ElementToValue);
        }

        public static IList<T> ElementToArray<T>(
            this JsonElement element,
            JsonDeserializer<T> deserializer)
        {
            var result = new List<T>();
            var enumerator = element.EnumerateArray();
            for (int ii = 0; enumerator.MoveNext(); ii++) {
                var itemElement = enumerator.Current;
                var itemValue = deserializer.Invoke(itemElement);
                result.Add(itemValue);
            }

            return result;
        }

        public static object ElementToNumeric(this JsonElement element)
        {
            if (element.TryGetDecimal(out var valueDecimal)) {
                return valueDecimal;
            }
            else if (element.TryGetDouble(out var valueDouble)) {
                return valueDouble;
            }
            else if (element.TryGetInt32(out var valueInt32)) {
                return valueInt32;
            }
            else if (element.TryGetInt64(out var valueInt64)) {
                return valueInt64;
            }

            throw new FormatException($"unable to parse the value \"{element}\" to a numeric");
        }
        
        // --------------------------------------------------------------------------------

        public static BigInteger GetBigInteger(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) {
                return BigInteger.Parse(element.GetString());
            }
            
            if (element.ValueKind == JsonValueKind.Number) {
                if (element.TryGetInt64(out var valueInt64)) {
                    return new BigInteger(valueInt64);
                }
            }

            throw new FormatException($"unable to parse the value \"{element}\" to a BigInteger");
        }

        public static BigInteger? GetBoxedBigInteger(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (BigInteger?) null : GetBigInteger(element);
        }
        
        public static bool? GetBoxedBoolean(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (bool?) null : element.GetBoolean();
        }

        public static byte? GetBoxedByte(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (byte?) null : element.GetByte();
        }

        public static short? GetBoxedInt16(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (short?) null : element.GetInt16();
        }

        public static int? GetBoxedInt32(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (int?) null : element.GetInt32();
        }

        public static long? GetBoxedInt64(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (long?) null : element.GetInt64();
        }

        public static float? GetBoxedSingle(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (float?) null : element.GetSingle();
        }

        public static double? GetBoxedDouble(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (double?) null : element.GetDouble();
        }

        public static decimal? GetBoxedDecimal(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (decimal?) null : element.GetDecimal();
        }
        
        public static char GetCharacter(this JsonElement element)
        {
            return element.GetString()[0]; // can generate index out of bounds array
        }
        
        public static char? GetBoxedCharacter(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (char?) null : element.GetString()[0];
        }

        public static DateTime GetDateTime(this JsonElement element)
        {
            var stringValue = element.GetString();
            try {
                return DateTimeParsingFunctions.ParseDefault(stringValue).DateTime;
            }
            catch (ArgumentException ex) {
                throw HandleParseException(typeof(DateTime), stringValue, ex);
            }
        }
        
        public static DateTime? GetBoxedDateTime(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (DateTime?) null : GetDateTime(element);
        }

        public static DateTimeOffset GetDateTimeOffset(this JsonElement element)
        {
            var stringValue = element.GetString();
            try {
                return DateTimeParsingFunctions.ParseDefault(stringValue);
            }
            catch (ArgumentException ex) {
                throw HandleParseException(typeof(DateTime), stringValue, ex);
            }
        }

        public static DateTimeOffset? GetBoxedDateTimeOffset(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Null ? (DateTimeOffset?) null : GetDateTimeOffset(element);
        }

        public static DateTimeEx GetDateTimeEx(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }

            var stringValue = element.GetString();
            try {
                return DateTimeParsingFunctions.ParseDefaultEx(stringValue);
            }
            catch (ArgumentException ex) {
                throw HandleParseException(typeof(DateTime), stringValue, ex);
            }
        }
        
        public static Uri GetUri(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }

            return new Uri(element.GetString());
        }

        public static Guid GetUuid(this JsonElement element)
        {
            return Guid.Parse(element.GetString());
        }

        public static Guid? GetBoxedUuid(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }

            return GetUuid(element);
        }

        public static T GetEnum<T>(this JsonElement element) where T : struct
        {
            return EnumHelper.Parse<T>(element.GetString());
        }

        public static Nullable<T> GetBoxedEnum<T>(this JsonElement element) where T : struct
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }

            return GetEnum<T>(element);
        }

        // --------------------------------------------------------------------------------

        public static EPException HandleParseException(
            Type type,
            string value,
            Exception ex)
        {
            var innerMsg = ex.Message ?? "";
            return new EPException(
                "Failed to parse json value as a " + type.Name + "-type from value '" + value + "': " + innerMsg,
                ex);
        }       
    }
}