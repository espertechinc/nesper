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
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.json.write
{
	public class JsonWriteUtil
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableString(
			Utf8JsonWriter writer,
			string value)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableStringToString(
			Utf8JsonWriter writer,
			object value)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableBoolean(
			Utf8JsonWriter writer,
			bool? value)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteBigInteger(
			Utf8JsonWriter writer,
			BigInteger value)
		{
			var asByteArray = value.ToByteArray();
			writer.WriteStartObject();
			writer.WriteString("$type", typeof(BigInteger).FullName);
			writer.WriteBase64String("value", asByteArray);
			writer.WriteEndObject();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableNumber(
			Utf8JsonWriter writer,
			object value)
		{
			if (value == null) {
				writer.WriteNullValue();
			} else if (value is long longValue) {
				writer.WriteNumberValue(longValue);
			} else if (value is int intValue) {
				writer.WriteNumberValue(intValue);
			} else if (value is short shortValue) {
				writer.WriteNumberValue(shortValue);
			} else if (value is decimal decimalValue) {
				writer.WriteNumberValue(decimalValue);
			} else if (value is double doubleValue) {
				writer.WriteNumberValue(doubleValue);
			} else if (value is float floatValue) {
				writer.WriteNumberValue(floatValue);
			} else if (value is BigInteger bigIntegerValue) {
				WriteBigInteger(writer, bigIntegerValue);
			} else {
				throw new InvalidDataException("unable to determine number type");
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNumber(
			Utf8JsonWriter writer,
			object value)
		{
			if (value is long longValue) {
				writer.WriteNumberValue(longValue);
			} else if (value is int intValue) {
				writer.WriteNumberValue(intValue);
			} else if (value is short shortValue) {
				writer.WriteNumberValue(shortValue);
			} else if (value is decimal decimalValue) {
				writer.WriteNumberValue(decimalValue);
			} else if (value is double doubleValue) {
				writer.WriteNumberValue(doubleValue);
			} else if (value is float floatValue) {
				writer.WriteNumberValue(floatValue);
			} else {
				throw new InvalidDataException("unable to determine number type");
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimString(
			Utf8JsonWriter writer,
			string[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			
			foreach (var strings in array) {
				WriteArrayString(writer, strings);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimCharacter(
			Utf8JsonWriter writer,
			char?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var characters in array) {
				WriteArrayCharacter(writer, characters);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimLong(
			Utf8JsonWriter writer,
			long?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var longs in array) {
				WriteArrayLong(writer, longs);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimInteger(
			Utf8JsonWriter writer,
			int?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var ints in array) {
				WriteArrayInteger(writer, ints);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <param name="factory">write class</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimAppClass(
			Utf8JsonWriter writer,
			object[][] array,
			JsonDelegateFactory factory)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var values in array) {
				WriteArrayAppClass(writer, values, factory);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimShort(
			Utf8JsonWriter writer,
			short?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var shorts in array) {
				WriteArrayShort(writer, shorts);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimDouble(
			Utf8JsonWriter writer,
			double?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var doubles in array) {
				WriteArrayDouble(writer, doubles);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimFloat(
			Utf8JsonWriter writer,
			float?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var floats in array) {
				WriteArrayFloat(writer, floats);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimByte(
			Utf8JsonWriter writer,
			byte?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteArrayByte(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBoolean(
			Utf8JsonWriter writer,
			bool?[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var bools in array) {
				WriteArrayBoolean(writer, bools);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBigInteger(
			Utf8JsonWriter writer,
			BigInteger[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteArrayBigInteger(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimObjectToString(
			Utf8JsonWriter writer,
			object[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteArrayObjectToString(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBooleanPrimitive(
			Utf8JsonWriter writer,
			bool[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteArrayBooleanPrimitive(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBytePrimitive(
			Utf8JsonWriter writer,
			byte[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var bytes in array) {
				WriteArrayBytePrimitive(writer, bytes);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimShortPrimitive(
			Utf8JsonWriter writer,
			short[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var shorts in array) {
				WriteArrayShortPrimitive(writer, shorts);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimIntPrimitive(
			Utf8JsonWriter writer,
			int[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var ints in array) {
				WriteArrayIntPrimitive(writer, ints);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimLongPrimitive(
			Utf8JsonWriter writer,
			long[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var longs in array) {
				WriteArrayLongPrimitive(writer, longs);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimFloatPrimitive(
			Utf8JsonWriter writer,
			float[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var floats in array) {
				WriteArrayFloatPrimitive(writer, floats);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimDoublePrimitive(
			Utf8JsonWriter writer,
			double[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var doubles in array) {
				WriteArrayDoublePrimitive(writer, doubles);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimCharPrimitive(
			Utf8JsonWriter writer,
			char[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var chars in array) {
				WriteArrayCharPrimitive(writer, chars);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayString(
			Utf8JsonWriter writer,
			string[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var value in array) {
				WriteNullableString(writer, value);
			}
			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionString(
			Utf8JsonWriter writer,
			ICollection<string> values)
		{
			if (values == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var value in values) {
				WriteNullableString(writer, value);
			}
			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <param name="factory">delegate factory</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionAppClass(
			Utf8JsonWriter writer,
			ICollection<object> values,
			JsonDelegateFactory factory)
		{
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
					factory.Write(writer, value);
				}
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <param name="factory">delegate factory</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayAppClass(
			Utf8JsonWriter writer,
			object[] values,
			JsonDelegateFactory factory)
		{
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
					factory.Write(writer, value);
				}
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayCharacter(
			Utf8JsonWriter writer,
			char?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var character in array) {
				WriteNullableStringToString(writer, character);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayLong(
			Utf8JsonWriter writer,
			long?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var l in array) {
				WriteNullableNumber(writer, l);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayInteger(
			Utf8JsonWriter writer,
			int?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var i in array) {
				WriteNullableNumber(writer, i);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionNumber(
			Utf8JsonWriter writer,
			ICollection<object> values)
		{
			if (values == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var i in values) {
				WriteNullableNumber(writer, i);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayShort(
			Utf8JsonWriter writer,
			short?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var s in array) {
				WriteNullableNumber(writer, s);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayDouble(
			Utf8JsonWriter writer,
			double?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var d in array) {
				WriteNullableNumber(writer, d);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayFloat(
			Utf8JsonWriter writer,
			float?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var f in array) {
				WriteNullableNumber(writer, f);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayByte(
			Utf8JsonWriter writer,
			byte?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteNullableNumber(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBoolean(
			Utf8JsonWriter writer,
			bool?[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteNullableBoolean(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionBoolean(
			Utf8JsonWriter writer,
			ICollection<bool?> values)
		{
			if (values == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in values) {
				WriteNullableBoolean(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBigInteger(
			Utf8JsonWriter writer,
			BigInteger[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var b in array) {
				WriteNullableNumber(writer, b);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBooleanPrimitive(
			Utf8JsonWriter writer,
			bool[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBytePrimitive(
			Utf8JsonWriter writer,
			byte[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayShortPrimitive(
			Utf8JsonWriter writer,
			short[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayIntPrimitive(
			Utf8JsonWriter writer,
			int[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayLongPrimitive(
			Utf8JsonWriter writer,
			long[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayFloatPrimitive(
			Utf8JsonWriter writer,
			float[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayDoublePrimitive(
			Utf8JsonWriter writer,
			double[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayCharPrimitive(
			Utf8JsonWriter writer,
			char[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayObjectToString(
			Utf8JsonWriter writer,
			object[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteEnumArray(
			Utf8JsonWriter writer,
			object[] array)
		{
			WriteObjectArrayWToString(writer, array);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteEnumCollection<T>(
			Utf8JsonWriter writer,
			ICollection<T> values)
			where T : Enum
		{
			WriteCollectionWToString(writer, values);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteEnumArray2Dim(
			Utf8JsonWriter writer,
			object[][] array)
		{
			WriteObjectArray2DimWToString(writer, array);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="writer">writer</param>
		/// <param name="jsonValue">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonValue(
			Utf8JsonWriter writer,
			string name,
			object jsonValue)
		{
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
			} else if (jsonValue is int intValue) {
				writer.WriteNumberValue(intValue);
			} else if (jsonValue is short shortValue) {
				writer.WriteNumberValue(shortValue);
			} else if (jsonValue is decimal decimalValue) {
				writer.WriteNumberValue(decimalValue);
			} else if (jsonValue is double doubleValue) {
				writer.WriteNumberValue(doubleValue);
			} else if (jsonValue is float floatValue) {
				writer.WriteNumberValue(floatValue);
			}
			else if (jsonValue is object[]) {
				WriteJsonArray(writer, name, (object[]) jsonValue);
			}
			else if (jsonValue is JsonEventObjectBase jsonEventObjectBase) {
				writer.WriteStartObject();
				jsonEventObjectBase.WriteTo(writer);
				writer.WriteEndObject();
			}
			else if (jsonValue is IDictionary<string, object> mapValue) {
				WriteJsonMap(writer, mapValue);
			}
			else {
				Log.Warn("Unknown json value of type " + jsonValue.GetType() + " encountered, skipping member '" + name + "'");
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="map">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonMap(
			Utf8JsonWriter writer,
			IDictionary<string, object> map)
		{
			if (map == null) {
				writer.WriteNullValue();
				return;
			}
			
			writer.WriteStartObject();

			foreach (var entry in map) {
				writer.WritePropertyName(entry.Key);
				WriteJsonValue(writer, entry.Key, entry.Value);
			}

			writer.WriteEndObject();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonArray(
			Utf8JsonWriter writer,
			string name,
			object[] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var item in array) {
				WriteJsonValue(writer, name, item);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nested">value</param>
		/// <param name="nestedFactory">writer for nested object</param>
		/// <throws>IOException io error</throws>
		public static void WriteNested(
			Utf8JsonWriter writer,
			object nested,
			JsonDelegateFactory nestedFactory)
		{
			if (nested == null) {
				writer.WriteNullValue();
				return;
			}

			nestedFactory.Write(writer, nested);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nested">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNested(
			Utf8JsonWriter writer,
			JsonEventObjectBase nested)
		{
			if (nested == null) {
				writer.WriteNullValue();
				return;
			}

			nested.WriteTo(writer);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nesteds">value</param>
		/// <param name="nestedFactory">writer for nested object</param>
		/// <throws>IOException io error</throws>
		public static void WriteNestedArray(
			Utf8JsonWriter writer,
			object[] nesteds,
			JsonDelegateFactory nestedFactory)
		{
			if (nesteds == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var nested in nesteds) {
				nestedFactory.Write(writer, nested);
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nesteds">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNestedArray(
			Utf8JsonWriter writer,
			JsonEventObjectBase[] nesteds)
		{
			if (nesteds == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var nested in nesteds) {
				nested.WriteTo(writer);
			}

			writer.WriteEndArray();
		}

		private static void WriteObjectArrayWToString(
			Utf8JsonWriter writer,
			object[] array)
		{
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
		/// <param name="writer">writer</param>
		/// <param name="values">collection</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionWToString<T>(
			Utf8JsonWriter writer,
			ICollection<T> values)
		{
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
			Utf8JsonWriter writer,
			object[][] array)
		{
			if (array == null) {
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var objects in array) {
				WriteObjectArrayWToString(writer, objects);
			}

			writer.WriteEndArray();
		}
	}
} // end of namespace
