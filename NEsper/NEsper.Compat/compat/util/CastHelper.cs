///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private static TypeParser _parseSingle = v => SimpleTypeParserFunctions.ParseFloat(v);
        private static TypeParser _parseDouble = v => SimpleTypeParserFunctions.ParseDouble(v);
        private static TypeParser _parseDecimal = v => SimpleTypeParserFunctions.ParseDecimal(v);
        private static TypeParser _parseByte = v => SimpleTypeParserFunctions.ParseByte(v);
        private static TypeParser _parseSByte = v => SimpleTypeParserFunctions.ParseSByte(v);
        private static TypeParser _parseInt16 = v => SimpleTypeParserFunctions.ParseInt16(v);
        private static TypeParser _parseInt32 = v => SimpleTypeParserFunctions.ParseInt32(v);
        private static TypeParser _parseInt64 = v => SimpleTypeParserFunctions.ParseInt64(v);
        private static TypeParser _parseUInt16 = v => SimpleTypeParserFunctions.ParseUInt16(v);
        private static TypeParser _parseUInt32 = v => SimpleTypeParserFunctions.ParseUInt32(v);
        private static TypeParser _parseUInt64 = v => SimpleTypeParserFunctions.ParseUInt64(v);
        private static TypeParser _parseBigInteger = v => SimpleTypeParserFunctions.ParseBigInteger(v);

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

        /// <summary>
        /// Gets the cast converter for the specified type.  If none is
        /// found, this method returns null.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static TypeCaster GetCastConverter(Type t)
        {
            var baseT = Nullable.GetUnderlyingType(t);
            if (baseT != null) {
                t = baseT;
            }

            if (t == typeof(int)) {
                return v => PrimitiveCastInt32(v);
            }

            if (t == typeof(long)) {
                return v => PrimitiveCastInt64(v);
            }

            if (t == typeof(short)) {
                return v => PrimitiveCastInt16(v);
            }

            if (t == typeof(sbyte)) {
                return v => PrimitiveCastSByte(v);
            }

            if (t == typeof(float)) {
                return v => PrimitiveCastSingle(v);
            }

            if (t == typeof(double)) {
                return v => PrimitiveCastDouble(v);
            }

            if (t == typeof(decimal)) {
                return v => PrimitiveCastDecimal(v);
            }

            if (t == typeof(BigInteger)) {
                return v => PrimitiveCastBigInteger(v);
            }

            if (t == typeof(uint)) {
                return v => PrimitiveCastUInt32(v);
            }

            if (t == typeof(ulong)) {
                return v => PrimitiveCastUInt64(v);
            }

            if (t == typeof(ushort)) {
                return v => PrimitiveCastUInt16(v);
            }

            if (t == typeof(char)) {
                return v => PrimitiveCastChar(v);
            }

            if (t == typeof(byte)) {
                return v => PrimitiveCastByte(v);
            }

            if (t.IsEnum) {
                return sourceObj => PrimitiveCastEnum(t, sourceObj);
            }

            return delegate(object sourceObj) {
                var sourceObjType = sourceObj.GetType();
                if (t.IsAssignableFrom(sourceObjType)) {
                    return sourceObj;
                }

                return null;
            };
        }

        public static T WithParser<T>(
            TypeParser parser,
            object sourceObj)
        {
            try {
                return (T) parser.Invoke((string) sourceObj);
            }
            catch (FormatException) {
                return default(T);
            }
        }

        /// <summary>
        /// Casts the object to a enumerated type
        /// </summary>
        /// <param name="enumType">The type.</param>
        /// <param name="sourceObj">The source object</param>
        /// <returns></returns>
        public static object PrimitiveCastEnum(
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

            if (sourceObj is sbyte sbyteValue) {
                return Enum.ToObject(enumType, sbyteValue);
            }

            if (sourceObj is byte byteValue) {
                return Enum.ToObject(enumType, byteValue);
            }

            if (sourceObj is char charValue) {
                return Enum.ToObject(enumType, charValue);
            }

            if (sourceObj is short shortValue) {
                return Enum.ToObject(enumType, shortValue);
            }

            if (sourceObj is int intValue) {
                return Enum.ToObject(enumType, intValue);
            }

            if (sourceObj is long longValue) {
                return Enum.ToObject(enumType, longValue);
            }

            if (sourceObj is ushort ushortValue) {
                return Enum.ToObject(enumType, ushortValue);
            }

            if (sourceObj is uint uintValue) {
                return Enum.ToObject(enumType, uintValue);
            }

            if (sourceObj is ulong ulongValue) {
                return Enum.ToObject(enumType, ulongValue);
            }

            if (sourceObj is float floatValue) {
                return Enum.ToObject(enumType, floatValue);
            }

            if (sourceObj is double doubleValue) {
                return Enum.ToObject(enumType, doubleValue);
            }

            if (sourceObj is decimal decimalValue) {
                return Enum.ToObject(enumType, decimalValue);
            }

            if (sourceObj is string stringValue) {
                return Enum.ToObject(enumType, stringValue[0]);
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.SByte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static sbyte? PrimitiveCastSByte(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<sbyte?>(_parseSByte, stringValue);
            }

            if (sourceObj is sbyte sbyteValue) {
                return sbyteValue;
            }

            if (sourceObj is byte byteValue) {
                return (sbyte) byteValue;
            }

            if (sourceObj is char charValue) {
                return (sbyte) charValue;
            }

            if (sourceObj is short shortValue) {
                return (sbyte) shortValue;
            }

            if (sourceObj is int intValue) {
                return (sbyte) intValue;
            }

            if (sourceObj is long longValue) {
                return (sbyte) longValue;
            }

            if (sourceObj is ushort ushortValue) {
                return (sbyte) ushortValue;
            }

            if (sourceObj is uint uintValue) {
                return (sbyte) uintValue;
            }

            if (sourceObj is ulong ulongValue) {
                return (sbyte) ulongValue;
            }

            if (sourceObj is float floatValue) {
                return (sbyte) floatValue;
            }

            if (sourceObj is double doubleValue) {
                return (sbyte) doubleValue;
            }

            if (sourceObj is decimal decimalValue) {
                return (sbyte) decimalValue;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Byte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static byte? PrimitiveCastByte(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<byte?>(_parseByte, stringValue);
            }

            return sourceObj.AsByte();
        }

        /// <summary>
        /// Casts the object to the System.Char
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static char? PrimitiveCastChar(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is sbyte sbyteValue) {
                return (char) sbyteValue;
            }

            if (sourceObj is byte byteValue) {
                return (char) byteValue;
            }

            if (sourceObj is char charValue) {
                return charValue;
            }

            if (sourceObj is short shortValue) {
                return (char) shortValue;
            }

            if (sourceObj is int intValue) {
                return (char) intValue;
            }

            if (sourceObj is long longValue) {
                return (char) longValue;
            }

            if (sourceObj is ushort ushortValue) {
                return (char) ushortValue;
            }

            if (sourceObj is uint uintValue) {
                return (char) uintValue;
            }

            if (sourceObj is ulong ulongValue) {
                return (char) ulongValue;
            }

            if (sourceObj is float floatValue) {
                return (char) floatValue;
            }

            if (sourceObj is double doubleValue) {
                return (char) doubleValue;
            }

            if (sourceObj is decimal decimalValue) {
                return (char) decimalValue;
            }

            if (sourceObj is string stringValue) {
                return stringValue[0];
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static short? PrimitiveCastInt16(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<short?>(_parseInt16, stringValue);
            }

            return sourceObj.AsShort();
        }

        /// <summary>
        /// Casts the object to the System.Int32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static int? PrimitiveCastInt32(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<int?>(_parseInt32, stringValue);
            }

            return sourceObj.AsInt();
        }

        /// <summary>
        /// Casts the object to the System.Int64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static long? PrimitiveCastInt64(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<long?>(_parseInt64, stringValue);
            }

            return sourceObj.AsLong();
        }

        /// <summary>
        /// Casts the object to the System.UInt16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static ushort? PrimitiveCastUInt16(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<ushort?>(_parseUInt16, stringValue);
            }

            if (sourceObj is sbyte sbyteValue) {
                return (ushort) sbyteValue;
            }

            if (sourceObj is byte byteValue) {
                return byteValue;
            }

            if (sourceObj is char charValue) {
                return charValue;
            }

            if (sourceObj is short shortValue) {
                return (ushort) shortValue;
            }

            if (sourceObj is int intValue) {
                return (ushort) intValue;
            }

            if (sourceObj is long longValue) {
                return (ushort) longValue;
            }

            if (sourceObj is ushort ushortValue) {
                return ushortValue;
            }

            if (sourceObj is uint uintValue) {
                return (ushort) uintValue;
            }

            if (sourceObj is ulong ulongValue) {
                return (ushort) ulongValue;
            }

            if (sourceObj is float floatValue) {
                return (ushort) floatValue;
            }

            if (sourceObj is double doubleValue) {
                return (ushort) doubleValue;
            }

            if (sourceObj is decimal decimalValue) {
                return (ushort) decimalValue;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static uint? PrimitiveCastUInt32(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<uint?>(_parseUInt32, stringValue);
            }

            if (sourceObj is sbyte sbyteValue) {
                return (uint) sbyteValue;
            }

            if (sourceObj is byte byteValue) {
                return byteValue;
            }

            if (sourceObj is char charValue) {
                return charValue;
            }

            if (sourceObj is short shortValue) {
                return (uint) shortValue;
            }

            if (sourceObj is int intValue) {
                return (uint) intValue;
            }

            if (sourceObj is long longValue) {
                return (uint) longValue;
            }

            if (sourceObj is ushort ushortValue) {
                return ushortValue;
            }

            if (sourceObj is uint uintValue) {
                return uintValue;
            }

            if (sourceObj is ulong ulongValue) {
                return (uint) ulongValue;
            }

            if (sourceObj is float floatValue) {
                return (uint) floatValue;
            }

            if (sourceObj is double doubleValue) {
                return (uint) doubleValue;
            }

            if (sourceObj is decimal decimalValue) {
                return (uint) decimalValue;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static ulong? PrimitiveCastUInt64(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<ulong?>(_parseUInt64, stringValue);
            }

            if (sourceObj is sbyte sbyteValue) {
                return (ulong) sbyteValue;
            }

            if (sourceObj is byte byteValue) {
                return byteValue;
            }

            if (sourceObj is char charValue) {
                return charValue;
            }

            if (sourceObj is short shortValue) {
                return (ulong) shortValue;
            }

            if (sourceObj is int intValue) {
                return (ulong) intValue;
            }

            if (sourceObj is long longValue) {
                return (ulong) longValue;
            }

            if (sourceObj is ushort ushortValue) {
                return ushortValue;
            }

            if (sourceObj is uint uintValue) {
                return uintValue;
            }

            if (sourceObj is ulong ulongValue) {
                return ulongValue;
            }

            if (sourceObj is float floatValue) {
                return (ulong) floatValue;
            }

            if (sourceObj is double doubleValue) {
                return (ulong) doubleValue;
            }

            if (sourceObj is decimal decimalValue) {
                return (ulong) decimalValue;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Single
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static float? PrimitiveCastSingle(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                return WithParser<float?>(_parseSingle, stringValue);
            }

            return sourceObj.AsFloat();
        }

        /// <summary>
        /// Casts the object to the System.Double
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static double? PrimitiveCastDouble(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            if (sourceObj is string stringValue) {
                try {
                    return WithParser<double?>(_parseDouble, stringValue);
                }
                catch (FormatException) {
                    return null;
                }
            }

            return sourceObj.AsDouble();
        }

        /// <summary>
        /// Casts the object to the System.Decimal
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static decimal? PrimitiveCastDecimal(object sourceObj)
        {
            if (sourceObj == null)
                return null;

            if (sourceObj is string stringValue) {
                return WithParser<decimal?>(_parseDecimal, stringValue);
            }

            return sourceObj.AsDecimal();
        }

        /// <summary>
        /// Casts the object to the System.Numerics.BigInteger
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static BigInteger? PrimitiveCastBigInteger(object sourceObj)
        {
            if (sourceObj == null)
                return null;

            if (sourceObj is string stringValue) {
                return WithParser<BigInteger?>(_parseBigInteger, stringValue);
            }

            return sourceObj.AsBigInteger();
        }
    }
}