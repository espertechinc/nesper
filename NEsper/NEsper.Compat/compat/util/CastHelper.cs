///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

namespace com.espertech.esper.compat.util
{
    using TypeParser = Func<string, object>;

    /// <summary>
    /// Provides efficient cast methods for converting from object to
    /// primitive types.  The cast method provided here-in is consistent
    /// with the cast mechanics of C#.  These cast mechanics are not
    /// the same as those provided by the IConvertible interface.
    /// </summary>
    public class CastHelper
    {
        public static GenericTypeCaster<T> GetCastConverter<T>()
        {
            var typeCaster = GetCastConverter(typeof(T));
            return o => (T) typeCaster.Invoke(o);
        }

        /// <summary>
        /// Gets the cast converter.
        /// </summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static TypeCaster GetTypeCaster(
            Type sourceType,
            Type targetType)
        {
            return GetCastConverter(targetType);
            //return typeCasterFactory.GetTypeCaster(sourceType, targetType);
        }

        private static readonly IDictionary<Type, TypeCaster> TypeCasterDictionary = new Dictionary<Type, TypeCaster> {
            [typeof(sbyte?)] = v => CastNullableSByte(v),
            [typeof(short?)] = v => CastNullableInt16(v),
            [typeof(int?)] = v => CastNullableInt32(v),
            [typeof(long?)] = v => CastNullableInt64(v),
            [typeof(byte?)] = v => CastNullableByte(v),
            [typeof(ushort?)] = v => CastNullableUInt16(v),
            [typeof(uint?)] = v => CastNullableUInt32(v),
            [typeof(ulong?)] = v => CastNullableUInt64(v),
            [typeof(char?)] = v => CastNullableChar(v),
            [typeof(float?)] = v => CastNullableSingle(v),
            [typeof(double?)] = v => CastNullableDouble(v),
            [typeof(decimal?)] = v => CastNullableDecimal(v),
            [typeof(BigInteger?)] = v => CastNullableBigInteger(v),

            [typeof(sbyte)] = v => CastSByte(v),
            [typeof(short)] = v => CastInt16(v),
            [typeof(int)] = v => CastInt32(v),
            [typeof(long)] = v => CastInt64(v),
            [typeof(byte)] = v => CastByte(v),
            [typeof(ushort)] = v => CastUInt16(v),
            [typeof(uint)] = v => CastUInt32(v),
            [typeof(ulong)] = v => CastUInt64(v),
            [typeof(char)] = v => CastChar(v),
            [typeof(float)] = v => CastSingle(v),
            [typeof(double)] = v => CastDouble(v),
            [typeof(decimal)] = v => CastDecimal(v),
            [typeof(BigInteger)] = v => CastBigInteger(v)
        };

        /// <summary>
        /// Gets the cast converter for the specified type.  If none is
        /// found, this method returns null.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static TypeCaster GetCastConverter(Type t)
        {
            if (TypeCasterDictionary.TryGetValue(t, out var typeCaster)) {
                return typeCaster;
            }

            if (t.IsEnum) {
                return sourceObj => CastEnum(t, sourceObj);
            }

            return delegate(object sourceObj) {
                var sourceObjType = sourceObj.GetType();
                if (t.IsAssignableFrom(sourceObjType)) {
                    return sourceObj;
                }

                return null;
            };
        }

        /// <summary>
        /// Casts the object to a enumerated type
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        /// <returns></returns>
        public static T CastEnum<T>(
            object sourceObj)
        {
            return (T) CastEnum(typeof(T), sourceObj);
        }

        public static object CastEnum(
            Type enumType,
            object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == enumType) {
                return sourceObj;
            }

            switch (sourceObj) {
                case sbyte sbyteValue:
                    return Enum.ToObject(enumType, sbyteValue);

                case byte byteValue:
                    return Enum.ToObject(enumType, byteValue);

                case char charValue:
                    return Enum.ToObject(enumType, charValue);

                case short shortValue:
                    return Enum.ToObject(enumType, shortValue);

                case int intValue:
                    return Enum.ToObject(enumType, intValue);

                case long longValue:
                    return Enum.ToObject(enumType, longValue);

                case ushort ushortValue:
                    return Enum.ToObject(enumType, ushortValue);

                case uint uintValue:
                    return Enum.ToObject(enumType, uintValue);

                case ulong ulongValue:
                    return Enum.ToObject(enumType, ulongValue);

                case float floatValue:
                    return Enum.ToObject(enumType, floatValue);

                case double doubleValue:
                    return Enum.ToObject(enumType, doubleValue);

                case decimal decimalValue:
                    return Enum.ToObject(enumType, decimalValue);

                case string stringValue:
                    return Enum.ToObject(enumType, stringValue[0]);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Casts the object to the System.SByte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static sbyte? CastNullableSByte(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseSByte(stringValue);
                    }
                    catch (FormatException) {
                        return default(sbyte?);
                    }

                case sbyte sbyteValue:
                    return sbyteValue;

                case byte byteValue:
                    return (sbyte) byteValue;

                case char charValue:
                    return (sbyte) charValue;

                case short shortValue:
                    return (sbyte) shortValue;

                case int intValue:
                    return (sbyte) intValue;

                case long longValue:
                    return (sbyte) longValue;

                case ushort ushortValue:
                    return (sbyte) ushortValue;

                case uint uintValue:
                    return (sbyte) uintValue;

                case ulong ulongValue:
                    return (sbyte) ulongValue;

                case float floatValue:
                    return (sbyte) floatValue;

                case double doubleValue:
                    return (sbyte) doubleValue;

                case decimal decimalValue:
                    return (sbyte) decimalValue;

                default:
                    throw new ArgumentException();
            }
        }

        public static sbyte CastSByte(object sourceObj)
        {
            switch (sourceObj) {
                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseSByte(stringValue);
                    }
                    catch (FormatException) {
                        return default(sbyte);
                    }

                case sbyte sbyteValue:
                    return sbyteValue;

                case byte byteValue:
                    return (sbyte) byteValue;

                case char charValue:
                    return (sbyte) charValue;

                case short shortValue:
                    return (sbyte) shortValue;

                case int intValue:
                    return (sbyte) intValue;

                case long longValue:
                    return (sbyte) longValue;

                case ushort ushortValue:
                    return (sbyte) ushortValue;

                case uint uintValue:
                    return (sbyte) uintValue;

                case ulong ulongValue:
                    return (sbyte) ulongValue;

                case float floatValue:
                    return (sbyte) floatValue;

                case double doubleValue:
                    return (sbyte) doubleValue;

                case decimal decimalValue:
                    return (sbyte) decimalValue;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Casts the object to the System.Byte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static byte? CastNullableByte(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseByte(stringValue);
                    }
                    catch (FormatException) {
                        return default(byte?);
                    }

                default:
                    return sourceObj.AsByte();
            }
        }

        public static byte CastByte(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseByte(stringValue);
                    }
                    catch (FormatException) {
                        return default(byte);
                    }

                default:
                    return sourceObj.AsByte();
            }
        }

        /// <summary>
        /// Casts the object to the System.Char
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static char? CastNullableChar(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case sbyte sbyteValue:
                    return (char) sbyteValue;

                case byte byteValue:
                    return (char) byteValue;

                case char charValue:
                    return charValue;

                case short shortValue:
                    return (char) shortValue;

                case int intValue:
                    return (char) intValue;

                case long longValue:
                    return (char) longValue;

                case ushort ushortValue:
                    return (char) ushortValue;

                case uint uintValue:
                    return (char) uintValue;

                case ulong ulongValue:
                    return (char) ulongValue;

                case float floatValue:
                    return (char) floatValue;

                case double doubleValue:
                    return (char) doubleValue;

                case decimal decimalValue:
                    return (char) decimalValue;

                case string stringValue:
                    return stringValue[0];

                default:
                    throw new ArgumentException();
            }
        }

        public static char CastChar(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case sbyte sbyteValue:
                    return (char) sbyteValue;

                case byte byteValue:
                    return (char) byteValue;

                case char charValue:
                    return charValue;

                case short shortValue:
                    return (char) shortValue;

                case int intValue:
                    return (char) intValue;

                case long longValue:
                    return (char) longValue;

                case ushort ushortValue:
                    return (char) ushortValue;

                case uint uintValue:
                    return (char) uintValue;

                case ulong ulongValue:
                    return (char) ulongValue;

                case float floatValue:
                    return (char) floatValue;

                case double doubleValue:
                    return (char) doubleValue;

                case decimal decimalValue:
                    return (char) decimalValue;

                case string stringValue:
                    return stringValue[0];

                default:
                    throw new ArgumentException();
            }
        }

        // --------------------------------------------------------------------------------

        /// <summary>
        /// Casts the object to the System.Int16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static short? CastNullableInt16(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt16(stringValue);
                    }
                    catch (FormatException) {
                        return default(short?);
                    }

                default:
                    return sourceObj.AsInt16();
            }
        }

        public static short CastInt16(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt16(stringValue);
                    }
                    catch (FormatException) {
                        return default(short);
                    }

                default:
                    return sourceObj.AsInt16();
            }
        }

        /// <summary>
        /// Casts the object to the System.Int32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static int? CastNullableInt32(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt32(stringValue);
                    }
                    catch (FormatException) {
                        return default(int?);
                    }

                default:
                    return sourceObj.AsInt32();
            }
        }

        public static int CastInt32(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt32(stringValue);
                    }
                    catch (FormatException) {
                        return default(int);
                    }

                default:
                    return sourceObj.AsInt32();
            }
        }

        /// <summary>
        /// Casts the object to the System.Int64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static long? CastNullableInt64(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt64(stringValue);
                    }
                    catch (FormatException) {
                        return default(long?);
                    }

                default:
                    return sourceObj.AsInt64();
            }
        }

        public static long CastInt64(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseInt64(stringValue);
                    }
                    catch (FormatException) {
                        return default(long);
                    }

                default:
                    return sourceObj.AsInt64();
            }
        }

        // --------------------------------------------------------------------------------

        /// <summary>
        /// Casts the object to the System.UInt16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static ushort? CastNullableUInt16(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt16(stringValue);
                    }
                    catch (FormatException) {
                        return default(ushort?);
                    }

                default:
                    return sourceObj.AsUInt16();
            }
        }

        public static ushort CastUInt16(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt16(stringValue);
                    }
                    catch (FormatException) {
                        return default(ushort);
                    }

                default:
                    return sourceObj.AsUInt16();
            }
        }

        /// <summary>
        /// Casts the object to the System.UInt32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static uint? CastNullableUInt32(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt32(stringValue);
                    }
                    catch (FormatException) {
                        return default(uint?);
                    }

                default:
                    return sourceObj.AsUInt32();
            }
        }

        public static uint CastUInt32(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt32(stringValue);
                    }
                    catch (FormatException) {
                        return default(uint);
                    }

                default:
                    return sourceObj.AsUInt32();
            }
        }

        /// <summary>
        /// Casts the object to the System.UInt64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static ulong? CastNullableUInt64(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt64(stringValue);
                    }
                    catch (FormatException) {
                        return default(ulong?);
                    }

                default:
                    return sourceObj.AsUInt64();
            }
        }

        public static ulong CastUInt64(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseUInt64(stringValue);
                    }
                    catch (FormatException) {
                        return default(ulong);
                    }

                default:
                    return sourceObj.AsUInt64();
            }
        }

        /// <summary>
        /// Casts the object to the System.Single
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static float? CastNullableSingle(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseFloat(stringValue);
                    }
                    catch (FormatException) {
                        return default(float?);
                    }

                default:
                    return sourceObj.AsFloat();
            }
        }

        public static float CastSingle(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseFloat(stringValue);
                    }
                    catch (FormatException) {
                        return default(float);
                    }

                default:
                    return sourceObj.AsFloat();
            }
        }

        /// <summary>
        /// Casts the object to the System.Double
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static double? CastNullableDouble(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseDouble(stringValue);
                    }
                    catch (FormatException) {
                        return default(double?);
                    }

                default:
                    return sourceObj.AsDouble();
            }
        }

        public static double CastDouble(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseDouble(stringValue);
                    }
                    catch (FormatException) {
                        return default(double);
                    }


                default:
                    return sourceObj.AsDouble();
            }
        }

        /// <summary>
        /// Casts the object to the System.Decimal
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static decimal? CastNullableDecimal(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseDecimal(stringValue);
                    }
                    catch (FormatException) {
                        return default(decimal?);
                    }

                default:
                    return sourceObj.AsDecimal();
            }
        }

        public static decimal CastDecimal(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseDecimal(stringValue);
                    }
                    catch (FormatException) {
                        return default(decimal);
                    }

                default:
                    return sourceObj.AsDecimal();
            }
        }

        /// <summary>
        /// Casts the object to the System.Numerics.BigInteger
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static BigInteger? CastNullableBigInteger(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    return null;

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseBigInteger(stringValue);
                    }
                    catch (FormatException) {
                        return default(BigInteger?);
                    }

                default:
                    return sourceObj.AsBigInteger();
            }
        }

        public static BigInteger CastBigInteger(object sourceObj)
        {
            switch (sourceObj) {
                case null:
                    throw new ArgumentException();

                case string stringValue:
                    try {
                        return SimpleTypeParserFunctions.ParseBigInteger(stringValue);
                    }
                    catch (FormatException) {
                        return default(BigInteger);
                    }

                default:
                    return sourceObj.AsBigInteger();
            }
        }
    }
}