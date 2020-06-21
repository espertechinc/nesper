///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;

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
			JsonWriter writer,
			string value)
		{
			if (value == null) {
				writer.WriteLiteral("null");
			}
			else {
				writer.WriteString(value);
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableStringToString(
			JsonWriter writer,
			object value)
		{
			if (value == null) {
				writer.WriteLiteral("null");
			}
			else {
				writer.WriteString(value.ToString());
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableBoolean(
			JsonWriter writer,
			bool? value)
		{
			if (value == null) {
				writer.WriteLiteral("null");
			}
			else {
				writer.WriteLiteral(value ? "true" : "false");
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="value">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNullableNumber(
			JsonWriter writer,
			object value)
		{
			if (value == null) {
				writer.WriteLiteral("null");
			}
			else {
				writer.WriteNumber(value.ToString());
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimString(
			JsonWriter writer,
			string[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (string[] strings in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayString(writer, strings);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimCharacter(
			JsonWriter writer,
			char?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (var characters in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayCharacter(writer, characters);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimLong(
			JsonWriter writer,
			long?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (long?[] longs in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayLong(writer, longs);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimInteger(
			JsonWriter writer,
			int?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (int?[] ints in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayInteger(writer, ints);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <param name="factory">write class</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimAppClass(
			JsonWriter writer,
			object[][] array,
			JsonDelegateFactory factory)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object[] values in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayAppClass(writer, values, factory);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimShort(
			JsonWriter writer,
			short?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (var shorts in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayShort(writer, shorts);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimDouble(
			JsonWriter writer,
			double?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Double[] doubles in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayDouble(writer, doubles);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimFloat(
			JsonWriter writer,
			float?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Float[] floats in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayFloat(writer, floats);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimByte(
			JsonWriter writer,
			byte?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Byte[] b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayByte(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBoolean(
			JsonWriter writer,
			bool?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (bool?[] bools in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayBoolean(writer, bools);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBigInteger(
			JsonWriter writer,
			BigInteger?[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (BigInteger?[] b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayBigInteger(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimObjectToString(
			JsonWriter writer,
			object[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object[] b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayObjectToString(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBooleanPrimitive(
			JsonWriter writer,
			bool[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (bool[] b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayBooleanPrimitive(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimBytePrimitive(
			JsonWriter writer,
			byte[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (byte[] bytes in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayBytePrimitive(writer, bytes);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimShortPrimitive(
			JsonWriter writer,
			short[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (short[] shorts in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayShortPrimitive(writer, shorts);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimIntPrimitive(
			JsonWriter writer,
			int[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (int[] ints in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayIntPrimitive(writer, ints);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimLongPrimitive(
			JsonWriter writer,
			long[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (long[] longs in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayLongPrimitive(writer, longs);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimFloatPrimitive(
			JsonWriter writer,
			float[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (float[] floats in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayFloatPrimitive(writer, floats);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimDoublePrimitive(
			JsonWriter writer,
			double[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (double[] doubles in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayDoublePrimitive(writer, doubles);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArray2DimCharPrimitive(
			JsonWriter writer,
			char[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (char[] chars in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteArrayCharPrimitive(writer, chars);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayString(
			JsonWriter writer,
			string[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (string string in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableString(writer, string);
			}
			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionString(
			JsonWriter writer,
			ICollection<string> values)
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (string string in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableString(writer, string);
			}
			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <param name="factory">delegate factory</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionAppClass(
			JsonWriter writer,
			ICollection<object> values,
			JsonDelegateFactory factory)
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object value in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				if (value == null) {
					writer.WriteLiteral("null");
				}
				else {
					factory.Write(writer, value);
				}
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <param name="factory">delegate factory</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayAppClass(
			JsonWriter writer,
			object[] values,
			JsonDelegateFactory factory)
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object value in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				if (value == null) {
					writer.WriteLiteral("null");
				}
				else {
					factory.Write(writer, value);
				}
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayCharacter(
			JsonWriter writer,
			char?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Character character in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableStringToString(writer, character);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayLong(
			JsonWriter writer,
			long?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (long? l in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, l);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayInteger(
			JsonWriter writer,
			int?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (int? i in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, i);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionNumber(
			JsonWriter writer,
			ICollection<Number> values)
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Number i in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, i);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayShort(
			JsonWriter writer,
			short?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (short? s in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, s);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayDouble(
			JsonWriter writer,
			double?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Double d in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, d);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayFloat(
			JsonWriter writer,
			float?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Float f in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, f);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayByte(
			JsonWriter writer,
			byte?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (Byte b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBoolean(
			JsonWriter writer,
			bool?[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (bool? b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableBoolean(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionBoolean(
			JsonWriter writer,
			ICollection<bool?> values)
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (bool? b in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableBoolean(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBigInteger(
			JsonWriter writer,
			BigInteger[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (BigInteger b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBigDecimal(
			JsonWriter writer,
			BigDecimal[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (BigDecimal b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteNullableNumber(writer, b);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBooleanPrimitive(
			JsonWriter writer,
			bool[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (bool b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteLiteral(b ? "true" : "false");
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayBytePrimitive(
			JsonWriter writer,
			byte[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (byte b in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(b.ToString());
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayShortPrimitive(
			JsonWriter writer,
			short[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (short s in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(s.ToString());
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayIntPrimitive(
			JsonWriter writer,
			int[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (int i in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(i.ToString());
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayLongPrimitive(
			JsonWriter writer,
			long[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (long l in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(l.ToString());
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayFloatPrimitive(
			JsonWriter writer,
			float[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (float f in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(f.ToString(CultureInfo.InvariantCulture));
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayDoublePrimitive(
			JsonWriter writer,
			double[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (double d in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteNumber(d.ToString(CultureInfo.InvariantCulture));
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayCharPrimitive(
			JsonWriter writer,
			char[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (char c in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteString(c.ToString());
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteArrayObjectToString(
			JsonWriter writer,
			object[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object value in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				if (value == null) {
					writer.WriteLiteral("null");
				}
				else {
					writer.WriteString(value.ToString());
				}
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteEnumArray(
			JsonWriter writer,
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
		public static void WriteEnumCollection(
			JsonWriter writer,
			ICollection values)
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
			JsonWriter writer,
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
			JsonWriter writer,
			string name,
			object jsonValue)
		{
			if (jsonValue == null) {
				writer.WriteLiteral("null");
			}
			else if (jsonValue is bool) {
				writer.WriteLiteral((bool) jsonValue ? "true" : "false");
			}
			else if (jsonValue is string) {
				writer.WriteString((string) jsonValue);
			}
			else if (jsonValue.IsNumber()) {
				writer.WriteNumber(jsonValue.ToString());
			}
			else if (jsonValue is object[]) {
				WriteJsonArray(writer, name, (object[]) jsonValue);
			}
			else if (jsonValue is IDictionary<string, object> mapValue) {
				WriteJsonMap(writer, mapValue);
			}
			else if (jsonValue is JsonEventObjectBase) {
				writer.WriteObjectOpen();
				JsonEventObjectBase und = (JsonEventObjectBase) jsonValue;
				und.Write(writer);
				writer.WriteObjectClose();
			}
			else {
				log.Warn("Unknown json value of type " + jsonValue.GetType() + " encountered, skipping member '" + name + "'");
			}
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="map">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonMap(
			JsonWriter writer,
			IDictionary<string, object> map)
		{
			if (map == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteObjectOpen();
			bool first = true;
			foreach (KeyValuePair<string, object> entry in map) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				writer.WriteMemberName(entry.Key);
				writer.WriteMemberSeparator();
				WriteJsonValue(writer, entry.Key, entry.Value);
			}

			writer.WriteObjectClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="writer">writer</param>
		/// <param name="array">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteJsonArray(
			JsonWriter writer,
			string name,
			object[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object item in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteJsonValue(writer, name, item);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nested">value</param>
		/// <param name="nestedFactory">writer for nested object</param>
		/// <throws>IOException io error</throws>
		public static void WriteNested(
			JsonWriter writer,
			object nested,
			JsonDelegateFactory nestedFactory)
		{
			if (nested == null) {
				writer.WriteLiteral("null");
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
			JsonWriter writer,
			JsonEventObjectBase nested)
		{
			if (nested == null) {
				writer.WriteLiteral("null");
				return;
			}

			nested.Write(writer);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nesteds">value</param>
		/// <param name="nestedFactory">writer for nested object</param>
		/// <throws>IOException io error</throws>
		public static void WriteNestedArray(
			JsonWriter writer,
			object[] nesteds,
			JsonDelegateFactory nestedFactory)
		{
			if (nesteds == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object nested in nesteds) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				nestedFactory.Write(writer, nested);
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="nesteds">value</param>
		/// <throws>IOException io error</throws>
		public static void WriteNestedArray(
			JsonWriter writer,
			JsonEventObjectBase[] nesteds)
		{
			if (nesteds == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (JsonEventObjectBase nested in nesteds) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				nested.Write(writer);
			}

			writer.WriteArrayClose();
		}

		private static void WriteObjectArrayWToString(
			JsonWriter writer,
			object[] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object @object in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				if (@object == null) {
					writer.WriteLiteral("null");
				}
				else {
					writer.WriteString(@object.ToString());
				}
			}

			writer.WriteArrayClose();
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="writer">writer</param>
		/// <param name="values">collection</param>
		/// <throws>IOException io error</throws>
		public static void WriteCollectionWToString<T>(
			JsonWriter writer,
			ICollection<T> values)
			where T : class
		{
			if (values == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (T @object in values) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				if (@object == null) {
					writer.WriteLiteral("null");
				}
				else {
					writer.WriteString(@object.ToString());
				}
			}

			writer.WriteArrayClose();
		}

		private static void WriteObjectArray2DimWToString(
			JsonWriter writer,
			object[][] array)
		{
			if (array == null) {
				writer.WriteLiteral("null");
				return;
			}

			writer.WriteArrayOpen();
			bool first = true;
			foreach (object[] objects in array) {
				if (!first) {
					writer.WriteObjectSeparator();
				}

				first = false;
				WriteObjectArrayWToString(writer, objects);
			}

			writer.WriteArrayClose();
		}
	}
} // end of namespace
