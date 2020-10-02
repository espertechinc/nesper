using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            switch (element.ValueKind) {
                case JsonValueKind.Object:
                    var result = new Dictionary<string, object>();
                    var enumerator = element.EnumerateObject();
                    for (int ii = 0; enumerator.MoveNext(); ii++) {
                        var property = enumerator.Current;
                        result.Add(property.Name, ElementToValue(property.Value));
                    }

                    return result;

                case JsonValueKind.Null:
                    return null;
                
                default:
                    throw new ArgumentException("Invalid JsonElement", nameof(element));
            }
        }

        public static IDictionary<string, T> ElementToDictionary<T>(
            this JsonElement element,
            Func<JsonElement, T> deserializer)
        {
            switch (element.ValueKind) {
                case JsonValueKind.Object:
                    var result = new Dictionary<string, T>();
                    var enumerator = element.EnumerateObject();
                    for (int ii = 0; enumerator.MoveNext(); ii++) {
                        var property = enumerator.Current;
                        var propertyName = property.Name;
                        var propertyValue = deserializer.Invoke(property.Value);
                        result.Add(propertyName, propertyValue);
                    }

                    return result;

                case JsonValueKind.Null:
                    return null;
                
                default:
                    throw new ArgumentException("Invalid JsonElement", nameof(element));
            }
        }
        
        public static IList<object> ElementToList(this JsonElement element)
        {
            return ElementToList(element, ElementToValue);
        }

        public static IList<T> ElementToList<T>(
            this JsonElement element,
            Func<JsonElement, T> deserializer)
        {
            switch (element.ValueKind) {
                case JsonValueKind.Array:
                    var result = new List<T>();
                    var enumerator = element.EnumerateArray();
                    for (int ii = 0; enumerator.MoveNext(); ii++) {
                        var itemElement = enumerator.Current;
                        var itemValue = deserializer.Invoke(itemElement);
                        result.Add(itemValue);
                    }

                    return result;

                case JsonValueKind.Null:
                    return null;
                
                default:
                    throw new ArgumentException("Invalid JsonElement", nameof(element));
            }
        }

        public static IList<T> ElementToList<T>(
            this JsonElement element,
            IJsonDeserializer deserializer)
        {
            switch (element.ValueKind) {
                case JsonValueKind.Array:
                    var result = new List<T>();
                    var enumerator = element.EnumerateArray();
                    for (int ii = 0; enumerator.MoveNext(); ii++) {
                        var itemElement = enumerator.Current;
                        var itemValue = (T) deserializer.Deserialize(itemElement);
                        result.Add(itemValue);
                    }

                    return result;

                case JsonValueKind.Null:
                    return null;
                
                default:
                    throw new ArgumentException("Invalid JsonElement", nameof(element));
            }
        }
        
        public static object[] ElementToArray(this JsonElement element)
        {
            return ElementToList(element)?.ToArray();
        }

        public static T[] ElementToArray<T>(
            this JsonElement element,
            Func<JsonElement, T> deserializer)
        {
            return ElementToList<T>(element, deserializer)?.ToArray();
        }

        public static T[] ElementToArray<T>(
            this JsonElement element,
            IJsonDeserializer deserializer)
        {
            return ElementToList<T>(element, deserializer)?.ToArray();
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
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => new BigInteger(element.GetInt64()),
                JsonValueKind.String => BigInteger.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };

            return element.ValueKind == JsonValueKind.Null ? (BigInteger?) null : GetBigInteger(element);
        }

        public static bool GetSmartBoolean(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetBoolean(),
                JsonValueKind.String => Boolean.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static bool? GetBoxedBoolean(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetBoolean(),
                JsonValueKind.String => Boolean.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static byte GetSmartByte(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetByte(),
                JsonValueKind.String => Byte.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static byte? GetBoxedByte(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetByte(),
                JsonValueKind.String => Byte.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static short GetSmartInt16(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetInt16(),
                JsonValueKind.String => Int16.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static short? GetBoxedInt16(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetInt16(),
                JsonValueKind.String => Int16.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static int GetSmartInt32(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetInt32(),
                JsonValueKind.String => Int32.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static int? GetBoxedInt32(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetInt32(),
                JsonValueKind.String => Int32.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static long GetSmartInt64(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetInt64(),
                JsonValueKind.String => Int64.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static long? GetBoxedInt64(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetInt64(),
                JsonValueKind.String => Int64.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }
        
        public static float GetSmartSingle(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetSingle(),
                JsonValueKind.String => Single.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static float? GetBoxedSingle(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetSingle(),
                JsonValueKind.String => Single.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static double GetSmartDouble(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String => Double.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static double? GetBoxedDouble(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String => Double.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static decimal GetSmartDecimal(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.String => Decimal.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static decimal? GetBoxedDecimal(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.String => Decimal.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static char GetCharacter(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.String => element.GetString()[0], // can generate index out of bounds array
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }
        
        public static char? GetBoxedCharacter(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString()[0],
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static DateTime GetDateTime(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) {
                var stringValue = element.GetString();
                try {
                    return DateTimeParsingFunctions.ParseDefault(stringValue).DateTime;
                }
                catch (ArgumentException ex) {
                    throw HandleParseException(typeof(DateTime), stringValue, ex);
                }
            }

            throw new ArgumentException("Invalid JsonElement", nameof(element));
        }
        
        public static DateTime? GetBoxedDateTime(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => GetDateTime(element),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static DateTimeOffset GetDateTimeOffset(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) {
                var stringValue = element.GetString();
                try {
                    return DateTimeParsingFunctions.ParseDefault(stringValue);
                }
                catch (ArgumentException ex) {
                    throw HandleParseException(typeof(DateTime), stringValue, ex);
                }
            }

            throw new ArgumentException("Invalid JsonElement", nameof(element));
        }

        public static DateTimeOffset? GetBoxedDateTimeOffset(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => GetDateTimeOffset(element),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static DateTimeEx GetDateTimeEx(this JsonElement element)
        {
            switch (element.ValueKind) {
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.String: {
                    var stringValue = element.GetString();
                    try {
                        return DateTimeParsingFunctions.ParseDefaultEx(stringValue);
                    }
                    catch (ArgumentException ex) {
                        throw HandleParseException(typeof(DateTime), stringValue, ex);
                    }
                }
                default:
                    throw new ArgumentException("Invalid JsonElement", nameof(element));
            }
        }

        public static Uri GetUri(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => new Uri(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static Guid GetUuid(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.String => Guid.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static Guid? GetBoxedUuid(this JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => Guid.Parse(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static T GetEnum<T>(this JsonElement element) where T : struct
        {
            return element.ValueKind switch {
                JsonValueKind.String => EnumHelper.Parse<T>(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
        }

        public static Nullable<T> GetBoxedEnum<T>(this JsonElement element) where T : struct
        {
            return element.ValueKind switch {
                JsonValueKind.Null => null,
                JsonValueKind.String => EnumHelper.Parse<T>(element.GetString()),
                _ => throw new ArgumentException("Invalid JsonElement", nameof(element))
            };
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