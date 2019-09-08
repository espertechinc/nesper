///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
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
                return PrimitiveCastInt32;
            }

            if (t == typeof(long)) {
                return PrimitiveCastInt64;
            }

            if (t == typeof(short)) {
                return PrimitiveCastInt16;
            }

            if (t == typeof(sbyte)) {
                return PrimitiveCastSByte;
            }

            if (t == typeof(float)) {
                return PrimitiveCastSingle;
            }

            if (t == typeof(double)) {
                return PrimitiveCastDouble;
            }

            if (t == typeof(decimal)) {
                return PrimitiveCastDecimal;
            }

            if (t.IsBigInteger()) {
                return PrimitiveCastBigInteger;
            }

            if (t == typeof(uint)) {
                return PrimitiveCastUInt32;
            }

            if (t == typeof(ulong)) {
                return PrimitiveCastUInt64;
            }

            if (t == typeof(ushort)) {
                return PrimitiveCastUInt16;
            }

            if (t == typeof(char)) {
                return PrimitiveCastChar;
            }

            if (t == typeof(byte)) {
                return PrimitiveCastByte;
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

        public static object WithParser(
            TypeParser parser,
            object sourceObj)
        {
            try {
                return parser.Invoke((string) sourceObj);
            }
            catch (FormatException) {
                return null;
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

            if (sourceObjType == typeof(sbyte)) {
                return Enum.ToObject(enumType, (sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return Enum.ToObject(enumType, ((sbyte?) sourceObj).Value);
            }

            if (sourceObjType == typeof(byte)) {
                return Enum.ToObject(enumType, (byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return Enum.ToObject(enumType, ((byte?) sourceObj).Value);
            }

            if (sourceObjType == typeof(char)) {
                return Enum.ToObject(enumType, ((char) sourceObj));
            }

            if (sourceObjType == typeof(char?)) {
                return Enum.ToObject(enumType, ((char?) sourceObj).Value);
            }

            if (sourceObjType == typeof(short)) {
                return Enum.ToObject(enumType, ((short) sourceObj));
            }

            if (sourceObjType == typeof(short?)) {
                return Enum.ToObject(enumType, ((short?) sourceObj).Value);
            }

            if (sourceObjType == typeof(int)) {
                return Enum.ToObject(enumType, ((int) sourceObj));
            }

            if (sourceObjType == typeof(int?)) {
                return Enum.ToObject(enumType, ((int?) sourceObj).Value);
            }

            if (sourceObjType == typeof(long)) {
                return Enum.ToObject(enumType, ((long) sourceObj));
            }

            if (sourceObjType == typeof(long?)) {
                return Enum.ToObject(enumType, ((long?) sourceObj).Value);
            }

            if (sourceObjType == typeof(ushort)) {
                return Enum.ToObject(enumType, ((ushort) sourceObj));
            }

            if (sourceObjType == typeof(ushort?)) {
                return Enum.ToObject(enumType, ((ushort?) sourceObj).Value);
            }

            if (sourceObjType == typeof(uint)) {
                return Enum.ToObject(enumType, ((uint) sourceObj));
            }

            if (sourceObjType == typeof(uint?)) {
                return Enum.ToObject(enumType, ((uint?) sourceObj).Value);
            }

            if (sourceObjType == typeof(ulong)) {
                return Enum.ToObject(enumType, ((ulong) sourceObj));
            }

            if (sourceObjType == typeof(ulong?)) {
                return Enum.ToObject(enumType, ((ulong?) sourceObj).Value);
            }

            if (sourceObjType == typeof(float)) {
                return Enum.ToObject(enumType, ((float) sourceObj));
            }

            if (sourceObjType == typeof(float?)) {
                return Enum.ToObject(enumType, ((float?) sourceObj).Value);
            }

            if (sourceObjType == typeof(double)) {
                return Enum.ToObject(enumType, ((double) sourceObj));
            }

            if (sourceObjType == typeof(double?)) {
                return Enum.ToObject(enumType, ((double?) sourceObj).Value);
            }

            if (sourceObjType == typeof(decimal)) {
                return Enum.ToObject(enumType, ((decimal) sourceObj));
            }

            if (sourceObjType == typeof(decimal?)) {
                return Enum.ToObject(enumType, ((decimal?) sourceObj).Value);
            }

            if (sourceObjType == typeof(string)) {
                return Enum.ToObject(enumType, (((string) sourceObj)[0]));
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.SByte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastSByte(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseSByte, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (sbyte) sourceObj;
            }

            if (sourceObjType == typeof(sbyte?)) {
                return ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (sbyte) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (sbyte) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (sbyte) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (sbyte) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (sbyte) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (sbyte) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (sbyte) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (sbyte) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (sbyte) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (sbyte) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (sbyte) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (sbyte) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (sbyte) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (sbyte) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (sbyte) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (sbyte) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (sbyte) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (sbyte) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (sbyte) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (sbyte) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (sbyte) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (sbyte) ((decimal?) sourceObj).Value;
            }

            if (sourceObjType == typeof(string)) {
                return (sbyte) (((string) sourceObj)[0]);
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Byte
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastByte(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseByte, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (byte) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (byte) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (byte) sourceObj;
            }

            if (sourceObjType == typeof(byte?)) {
                return ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (byte) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (byte) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (byte) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (byte) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (byte) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (byte) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (byte) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (byte) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (byte) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (byte) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (byte) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (byte) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (byte) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (byte) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (byte) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (byte) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (byte) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (byte) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (byte) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (byte) ((decimal?) sourceObj).Value;
            }

            if (sourceObjType == typeof(string)) {
                return (byte) (((string) sourceObj)[0]);
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Char
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastChar(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(sbyte)) {
                return (char) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (char) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (char) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (char) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (char) sourceObj;
            }

            if (sourceObjType == typeof(char?)) {
                return ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (char) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (char) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (char) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (char) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (char) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (char) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (char) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (char) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (char) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (char) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (char) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (char) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (char) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (char) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (char) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (char) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (char) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (char) ((decimal?) sourceObj).Value;
            }

            if (sourceObjType == typeof(string)) {
                return ((string) sourceObj)[0];
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastInt16(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseInt16, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (short) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (short) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (short) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (short) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (short) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (short) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (short) sourceObj;
            }

            if (sourceObjType == typeof(short?)) {
                return ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (short) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (short) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (short) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (short) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (short) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (short) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (short) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (short) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (short) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (short) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (short) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (short) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (short) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (short) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (short) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (short) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastInt32(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseInt32, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (int) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (int) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (int) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (int) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (int) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (int) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (int) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (int) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (int) sourceObj;
            }

            if (sourceObjType == typeof(int?)) {
                return ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (int) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (int) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (int) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (int) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (int) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (int) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (int) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (int) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (int) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (int) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (int) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (int) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (int) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (int) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Int64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastInt64(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseInt64, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (long) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (long) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (long) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (long) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (long) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (long) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (long) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (long) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (long) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (long) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (long) sourceObj;
            }

            if (sourceObjType == typeof(long?)) {
                return ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (long) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (long) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (long) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (long) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (long) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (long) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (long) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (long) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (long) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (long) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (long) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (long) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt16
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastUInt16(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseUInt16, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (ushort) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (ushort) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (ushort) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (ushort) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (ushort) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (ushort) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (ushort) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (ushort) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (ushort) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (ushort) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (ushort) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (ushort) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (ushort) sourceObj;
            }

            if (sourceObjType == typeof(ushort?)) {
                return ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (ushort) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (ushort) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (ushort) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (ushort) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (ushort) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (ushort) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (ushort) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (ushort) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (ushort) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (ushort) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt32
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastUInt32(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseUInt32, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (uint) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (uint) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (uint) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (uint) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (uint) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (uint) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (uint) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (uint) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (uint) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (uint) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (uint) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (uint) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (uint) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (uint) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (uint) sourceObj;
            }

            if (sourceObjType == typeof(uint?)) {
                return ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (uint) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (uint) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (uint) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (uint) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (uint) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (uint) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (uint) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (uint) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.UInt64
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastUInt64(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseUInt64, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (ulong) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (ulong) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (ulong) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (ulong) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (ulong) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (ulong) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (ulong) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (ulong) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (ulong) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (ulong) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (ulong) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (ulong) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (ulong) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (ulong) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (ulong) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (ulong) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (ulong) sourceObj;
            }

            if (sourceObjType == typeof(ulong?)) {
                return ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (ulong) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (ulong) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (ulong) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (ulong) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (ulong) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (ulong) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Single
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastSingle(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                return WithParser(_parseSingle, sourceObj);
            }

            if (sourceObjType == typeof(sbyte)) {
                return (float) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (float) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (float) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (float) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (float) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (float) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (float) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (float) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (float) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (float) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (float) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (float) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (float) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (float) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (float) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (float) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (float) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (float) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (float) sourceObj;
            }

            if (sourceObjType == typeof(float?)) {
                return ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (float) ((double) sourceObj);
            }

            if (sourceObjType == typeof(double?)) {
                return (float) ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (float) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (float) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Double
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastDouble(object sourceObj)
        {
            if (sourceObj == null) {
                return null;
            }

            var sourceObjType = sourceObj.GetType();
            if (sourceObjType == typeof(string)) {
                try {
                    return WithParser(_parseDouble, sourceObj);
                }
                catch (FormatException) {
                    return null;
                }
            }

            if (sourceObjType == typeof(sbyte)) {
                return (double) ((sbyte) sourceObj);
            }

            if (sourceObjType == typeof(sbyte?)) {
                return (double) ((sbyte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(byte)) {
                return (double) ((byte) sourceObj);
            }

            if (sourceObjType == typeof(byte?)) {
                return (double) ((byte?) sourceObj).Value;
            }

            if (sourceObjType == typeof(char)) {
                return (double) ((char) sourceObj);
            }

            if (sourceObjType == typeof(char?)) {
                return (double) ((char?) sourceObj).Value;
            }

            if (sourceObjType == typeof(short)) {
                return (double) ((short) sourceObj);
            }

            if (sourceObjType == typeof(short?)) {
                return (double) ((short?) sourceObj).Value;
            }

            if (sourceObjType == typeof(int)) {
                return (double) ((int) sourceObj);
            }

            if (sourceObjType == typeof(int?)) {
                return (double) ((int?) sourceObj).Value;
            }

            if (sourceObjType == typeof(long)) {
                return (double) ((long) sourceObj);
            }

            if (sourceObjType == typeof(long?)) {
                return (double) ((long?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ushort)) {
                return (double) ((ushort) sourceObj);
            }

            if (sourceObjType == typeof(ushort?)) {
                return (double) ((ushort?) sourceObj).Value;
            }

            if (sourceObjType == typeof(uint)) {
                return (double) ((uint) sourceObj);
            }

            if (sourceObjType == typeof(uint?)) {
                return (double) ((uint?) sourceObj).Value;
            }

            if (sourceObjType == typeof(ulong)) {
                return (double) ((ulong) sourceObj);
            }

            if (sourceObjType == typeof(ulong?)) {
                return (double) ((ulong?) sourceObj).Value;
            }

            if (sourceObjType == typeof(float)) {
                return (double) ((float) sourceObj);
            }

            if (sourceObjType == typeof(float?)) {
                return (double) ((float?) sourceObj).Value;
            }

            if (sourceObjType == typeof(double)) {
                return (double) sourceObj;
            }

            if (sourceObjType == typeof(double?)) {
                return ((double?) sourceObj).Value;
            }

            if (sourceObjType == typeof(decimal)) {
                return (double) ((decimal) sourceObj);
            }

            if (sourceObjType == typeof(decimal?)) {
                return (double) ((decimal?) sourceObj).Value;
            }

            return null;
        }

        /// <summary>
        /// Casts the object to the System.Decimal
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastDecimal(object sourceObj)
        {
            if (sourceObj == null)
                return null;

            var stringValue = sourceObj as string;
            if (stringValue != null)
                return WithParser(_parseDecimal, stringValue);

            return sourceObj.AsDecimal();
        }

        /// <summary>
        /// Casts the object to the System.Numerics.BigInteger
        /// </summary>
        /// <param name="sourceObj">The source object</param>
        public static object PrimitiveCastBigInteger(object sourceObj)
        {
            if (sourceObj == null)
                return null;

            var stringValue = sourceObj as string;
            if (stringValue != null)
                return WithParser(_parseBigInteger, stringValue);

            return sourceObj.AsBigInteger();
        }
    }
}