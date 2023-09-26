///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
    public class JsonSerializerUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNullableString(
            JsonSerializationContext context,
            string value)
        {
            var writer = context.Writer;
            if (value == null) {
                writer.WriteNullValue();
            }
            else {
                writer.WriteStringValue(value);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNullableStringToString(
            JsonSerializationContext context,
            object value)
        {
            var writer = context.Writer;
            if (value == null) {
                writer.WriteNullValue();
            }
            else {
                writer.WriteStringValue(value.ToString());
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNullableBoolean(
            JsonSerializationContext context,
            bool? value)
        {
            var writer = context.Writer;
            if (value == null) {
                writer.WriteNullValue();
            }
            else {
                writer.WriteBooleanValue(value.Value);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteBigInteger(
            JsonSerializationContext context,
            BigInteger value)
        {
            var writer = context.Writer;
            var asByteArray = value.ToByteArray();
            writer.WriteStartObject();
            writer.WriteString("$type", typeof(BigInteger).FullName);
            writer.WriteBase64String("value", asByteArray);
            writer.WriteEndObject();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNullableNumber(
            JsonSerializationContext context,
            object value)
        {
            var writer = context.Writer;
            if (value == null) {
                writer.WriteNullValue();
            }
            else if (value is long longValue) {
                writer.WriteNumberValue(longValue);
            }
            else if (value is int intValue) {
                writer.WriteNumberValue(intValue);
            }
            else if (value is short shortValue) {
                writer.WriteNumberValue(shortValue);
            }
            else if (value is decimal decimalValue) {
                writer.WriteNumberValue(decimalValue);
            }
            else if (value is double doubleValue) {
                writer.WriteNumberValue(doubleValue);
            }
            else if (value is float floatValue) {
                writer.WriteNumberValue(floatValue);
            }
            else if (value is BigInteger bigIntegerValue) {
                WriteBigInteger(context, bigIntegerValue);
            }
            else {
                throw new InvalidDataException("unable to determine number type");
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="value">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNumber(
            JsonSerializationContext context,
            object value)
        {
            var writer = context.Writer;
            if (value is long longValue) {
                writer.WriteNumberValue(longValue);
            }
            else if (value is int intValue) {
                writer.WriteNumberValue(intValue);
            }
            else if (value is short shortValue) {
                writer.WriteNumberValue(shortValue);
            }
            else if (value is decimal decimalValue) {
                writer.WriteNumberValue(decimalValue);
            }
            else if (value is double doubleValue) {
                writer.WriteNumberValue(doubleValue);
            }
            else if (value is float floatValue) {
                writer.WriteNumberValue(floatValue);
            }
            else {
                throw new InvalidDataException("unable to determine number type");
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimString(
            JsonSerializationContext context,
            string[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            foreach (var strings in array) {
                WriteArrayString(context, strings);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimCharacter(
            JsonSerializationContext context,
            char?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var characters in array) {
                WriteArrayCharacter(context, characters);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimLong(
            JsonSerializationContext context,
            long?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var longs in array) {
                WriteArrayLong(context, longs);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimInteger(
            JsonSerializationContext context,
            int?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var ints in array) {
                WriteArrayInteger(context, ints);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimAppClass(
            JsonSerializationContext context,
            object[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var values in array) {
                WriteArrayAppClass(context, values);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimShort(
            JsonSerializationContext context,
            short?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var shorts in array) {
                WriteArrayShort(context, shorts);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimDouble(
            JsonSerializationContext context,
            double?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var doubles in array) {
                WriteArrayDouble(context, doubles);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimFloat(
            JsonSerializationContext context,
            float?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var floats in array) {
                WriteArrayFloat(context, floats);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimByte(
            JsonSerializationContext context,
            byte?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteArrayByte(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimBoolean(
            JsonSerializationContext context,
            bool?[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var bools in array) {
                WriteArrayBoolean(context, bools);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimBigInteger(
            JsonSerializationContext context,
            BigInteger[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteArrayBigInteger(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimObjectToString(
            JsonSerializationContext context,
            object[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteArrayObjectToString(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimBooleanPrimitive(
            JsonSerializationContext context,
            bool[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteArrayBooleanPrimitive(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimBytePrimitive(
            JsonSerializationContext context,
            byte[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var bytes in array) {
                WriteArrayBytePrimitive(context, bytes);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimShortPrimitive(
            JsonSerializationContext context,
            short[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var shorts in array) {
                WriteArrayShortPrimitive(context, shorts);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimIntPrimitive(
            JsonSerializationContext context,
            int[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var ints in array) {
                WriteArrayIntPrimitive(context, ints);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimLongPrimitive(
            JsonSerializationContext context,
            long[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var longs in array) {
                WriteArrayLongPrimitive(context, longs);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimFloatPrimitive(
            JsonSerializationContext context,
            float[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var floats in array) {
                WriteArrayFloatPrimitive(context, floats);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimDoublePrimitive(
            JsonSerializationContext context,
            double[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var doubles in array) {
                WriteArrayDoublePrimitive(context, doubles);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArray2DimCharPrimitive(
            JsonSerializationContext context,
            char[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var chars in array) {
                WriteArrayCharPrimitive(context, chars);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayString(
            JsonSerializationContext context,
            string[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var value in array) {
                WriteNullableString(context, value);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteCollectionString(
            JsonSerializationContext context,
            ICollection<string> values)
        {
            if (values == null) {
                var writer = context.Writer;
                writer.WriteNullValue();
                return;
            }

            context.Writer.WriteStartArray();
            foreach (var value in values) {
                WriteNullableString(context, value);
            }

            context.Writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteCollectionAppClass(
            JsonSerializationContext context,
            ICollection<object> values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            foreach (var value in values) {
                if (value == null) {
                    writer.WriteNullValue();
                }
                else {
                    context
                        .SerializerFor(value.GetType())
                        .Serialize(context, value);
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayAppClass(
            JsonSerializationContext context,
            object[] values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            foreach (var value in values) {
                if (value == null) {
                    writer.WriteNullValue();
                }
                else {
                    context
                        .SerializerFor(value.GetType())
                        .Serialize(context, value);
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayCharacter(
            JsonSerializationContext context,
            char?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var character in array) {
                WriteNullableStringToString(context, character);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayLong(
            JsonSerializationContext context,
            long?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var l in array) {
                WriteNullableNumber(context, l);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayInteger(
            JsonSerializationContext context,
            int?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var i in array) {
                WriteNullableNumber(context, i);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteCollectionNumber(
            JsonSerializationContext context,
            ICollection<object> values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var i in values) {
                WriteNullableNumber(context, i);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayShort(
            JsonSerializationContext context,
            short?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var s in array) {
                WriteNullableNumber(context, s);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayDouble(
            JsonSerializationContext context,
            double?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var d in array) {
                WriteNullableNumber(context, d);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayFloat(
            JsonSerializationContext context,
            float?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var f in array) {
                WriteNullableNumber(context, f);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayByte(
            JsonSerializationContext context,
            byte?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteNullableNumber(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayBoolean(
            JsonSerializationContext context,
            bool?[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteNullableBoolean(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteCollectionBoolean(
            JsonSerializationContext context,
            ICollection<bool?> values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in values) {
                WriteNullableBoolean(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayBigInteger(
            JsonSerializationContext context,
            BigInteger[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                WriteNullableNumber(context, b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayBooleanPrimitive(
            JsonSerializationContext context,
            bool[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                writer.WriteBooleanValue(b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayBytePrimitive(
            JsonSerializationContext context,
            byte[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var b in array) {
                // Bytes are not natively supported by JSON, but we can
                // just increase their size to fit into the standard set
                // of numbers.  Just be sure to decode as bytes.
                writer.WriteNumberValue(b);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayShortPrimitive(
            JsonSerializationContext context,
            short[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var s in array) {
                writer.WriteNumberValue(s);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayIntPrimitive(
            JsonSerializationContext context,
            int[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var i in array) {
                writer.WriteNumberValue(i);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayLongPrimitive(
            JsonSerializationContext context,
            long[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var l in array) {
                writer.WriteNumberValue(l);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayFloatPrimitive(
            JsonSerializationContext context,
            float[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var f in array) {
                writer.WriteNumberValue(f);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayDoublePrimitive(
            JsonSerializationContext context,
            double[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var d in array) {
                writer.WriteNumberValue(d);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayCharPrimitive(
            JsonSerializationContext context,
            char[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var c in array) {
                writer.WriteStringValue(c.ToString());
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteArrayObjectToString(
            JsonSerializationContext context,
            object[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var value in array) {
                if (value == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStringValue(value.ToString());
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteEnumArray(
            JsonSerializationContext context,
            object[] array)
        {
            WriteObjectArrayWToString(context, array);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteEnumCollection<T>(
            JsonSerializationContext context,
            ICollection<T> values)
            where T : Enum
        {
            WriteCollectionWToString(context, values);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="array">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteEnumArray2Dim(
            JsonSerializationContext context,
            object[][] array)
        {
            WriteObjectArray2DimWToString(context, array);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="context">the serialization context</param>
        /// <param name="jsonValue">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteJsonValue(
            JsonSerializationContext context,
            string name,
            object jsonValue)
        {
            var writer = context.Writer;
            if (jsonValue == null) {
                writer.WriteNullValue();
            }
            else if (jsonValue is bool boolValue) {
                writer.WriteBooleanValue(boolValue);
            }
            else if (jsonValue is string stringValue) {
                writer.WriteStringValue(stringValue);
            }
            else if (jsonValue is long longValue) {
                writer.WriteNumberValue(longValue);
            }
            else if (jsonValue is int intValue) {
                writer.WriteNumberValue(intValue);
            }
            else if (jsonValue is short shortValue) {
                writer.WriteNumberValue(shortValue);
            }
            else if (jsonValue is decimal decimalValue) {
                writer.WriteNumberValue(decimalValue);
            }
            else if (jsonValue is double doubleValue) {
                writer.WriteNumberValue(doubleValue);
            }
            else if (jsonValue is float floatValue) {
                writer.WriteNumberValue(floatValue);
            }
            else if (jsonValue is JsonEventObjectBase jsonEventObjectBase) {
                writer.WriteStartObject();
                jsonEventObjectBase.WriteTo(context);
                writer.WriteEndObject();
            }
            else if (jsonValue is IDictionary<string, object> mapValue) {
                WriteJsonMap(context, mapValue);
            }
            else {
                Log.Warn(
                    "Unknown json value of type " +
                    jsonValue.GetType() +
                    " encountered, skipping member '" +
                    name +
                    "'");
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="map">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteJsonMap(
            JsonSerializationContext context,
            IDictionary<string, object> map)
        {
            var writer = context.Writer;
            if (map == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var entry in map) {
                writer.WritePropertyName(entry.Key);
                WriteJsonValue(context, entry.Key, entry.Value);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="map">value</param>
        /// <param name="serializer"></param>
        /// <throws>IOException io error</throws>
        public static void WriteJsonMap<T>(
            JsonSerializationContext context,
            IDictionary<string, T> map,
            Action<JsonSerializationContext, T> serializer)
        {
            var writer = context.Writer;
            if (map == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var entry in map) {
                writer.WritePropertyName(entry.Key);
                serializer.Invoke(context, entry.Value);
            }

            writer.WriteEndObject();
        }


// Deprecating this behavior.  It assumes that every item should be encoded into an
// array of objects.  It transfers the responsibility of figuring out the wrapping
// from the serializer to this method.  Use WriteArray instead.

#if false
		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="itemName">name</param>
		/// <param name="context">the serialization context</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonArray(
			JsonSerializationContext context,
			string itemName,
			object[] array)
		{
			var writer = context.JsonWriter;
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var item in array) {
				WriteJsonValue(context, itemName, item);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="context">the serialization context</param>
		/// <param name="list">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonList<T>(
			JsonSerializationContext context,
			string name,
			IList<T>[] list)
		{
			var writer = context.JsonWriter;
			if (list == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var item in list) {
				WriteJsonValue(context, name, item);
			}

			writer.WriteEndArray();
		}
#endif

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="nested">value</param>
        /// <param name="serializer">the serializer</param>
        /// <throws>IOException io error</throws>
        public static void WriteNested(
            JsonSerializationContext context,
            object nested,
            IJsonSerializer serializer)
        {
            var writer = context.Writer;
            if (nested == null) {
                writer.WriteNullValue();
                return;
            }

            serializer.Serialize(context, nested);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="nested">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNested(
            JsonSerializationContext context,
            JsonEventObjectBase nested)
        {
            var writer = context.Writer;
            if (nested == null) {
                writer.WriteNullValue();
                return;
            }

            nested.WriteTo(context);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <param name="serializer">serializer for the values</param>
        /// <throws>IOException io error</throws>
        public static void WriteNestedArray<T>(
            JsonSerializationContext context,
            T[] values,
            Action<JsonSerializationContext, T> serializer)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var nested in values) {
                serializer.Invoke(context, nested);
            }

            writer.WriteEndArray();
        }


        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <param name="serializer">serializer for the values</param>
        /// <throws>IOException io error</throws>
        public static void WriteNestedArray(
            JsonSerializationContext context,
            object[] values,
            IJsonSerializer serializer)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var nested in values) {
                serializer.Serialize(context, nested);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">value</param>
        /// <throws>IOException io error</throws>
        public static void WriteNestedArray(
            JsonSerializationContext context,
            JsonEventObjectBase[] values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var nested in values) {
                nested.WriteTo(context);
            }

            writer.WriteEndArray();
        }

        private static void WriteObjectArrayWToString(
            JsonSerializationContext context,
            object[] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var @object in array) {
                if (@object == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStringValue(@object.ToString());
                }
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <param name="values">collection</param>
        /// <throws>IOException io error</throws>
        public static void WriteCollectionWToString<T>(
            JsonSerializationContext context,
            ICollection<T> values)
        {
            var writer = context.Writer;
            if (values == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var @object in values) {
                if (@object == null) {
                    writer.WriteNullValue();
                }
                else {
                    writer.WriteStringValue(@object.ToString());
                }
            }

            writer.WriteEndArray();
        }

        private static void WriteObjectArray2DimWToString(
            JsonSerializationContext context,
            object[][] array)
        {
            var writer = context.Writer;
            if (array == null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var objects in array) {
                WriteObjectArrayWToString(context, objects);
            }

            writer.WriteEndArray();
        }
    }
} // end of namespace