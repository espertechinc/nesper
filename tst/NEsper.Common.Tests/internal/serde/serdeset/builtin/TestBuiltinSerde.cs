///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.serde.serdeset.builtin
{
	[TestFixture]
	public class TestBuiltinSerde
	{
		[Test]
		public void TestSerde()
		{
			AssertSerde(DIOBooleanSerde.INSTANCE, true);
			AssertSerde(DIOByteSerde.INSTANCE, (byte)0x0f);
			AssertSerde(DIOCharacterSerde.INSTANCE, 'x');
			//AssertSerde(DIOCharSequenceSerde.INSTANCE, "abc");
			AssertSerde(DIODoubleSerde.INSTANCE, 10d);
			AssertSerde(DIOFloatSerde.INSTANCE, 11f);
			AssertSerde(DIOIntegerSerde.INSTANCE, 12);
			AssertSerde(DIOShortSerde.INSTANCE, (short)13);
			AssertSerde(DIOLongSerde.INSTANCE, 14L);
			AssertSerde(DIONullableStringSerde.INSTANCE, "def");

			AssertSerde(DIOPrimitiveByteArraySerde.INSTANCE, new byte[] { 1, 2 });
			AssertSerdeWNull(DIOPrimitiveBooleanArrayNullableSerde.INSTANCE, new bool[] { true, false });
			AssertSerdeWNull(DIOPrimitiveByteArrayNullableSerde.INSTANCE, new byte[] { 1, 2 });
			AssertSerdeWNull(DIOPrimitiveCharArrayNullableSerde.INSTANCE, new char[] { 'a', 'b' });
			AssertSerdeWNull(DIOPrimitiveDoubleArrayNullableSerde.INSTANCE, new double[] { 1d, 2d });
			AssertSerdeWNull(DIOPrimitiveFloatArrayNullableSerde.INSTANCE, new float[] { 1f, 2f });
			AssertSerdeWNull(DIOPrimitiveIntArrayNullableSerde.INSTANCE, new int[] { 1, 2 });
			AssertSerdeWNull(DIOPrimitiveLongArrayNullableSerde.INSTANCE, new long[] { 1, 2 });
			AssertSerdeWNull(DIOPrimitiveShortArrayNullableSerde.INSTANCE, new short[] { 1, 2 });

			AssertSerdeWNull(DIOPrimitiveCharArray2DimNullableSerde.INSTANCE, new char[][] { new char[] { 'a', 'b' }, new char[] { 'c' } });
			AssertSerdeWNull(DIOPrimitiveDoubleArray2DimNullableSerde.INSTANCE, new double[][] { new double[] { 1, 2d }, new double[] { 3d } });
			AssertSerdeWNull(DIOPrimitiveFloatArray2DimNullableSerde.INSTANCE, new float[][] { new float[] { 1f, 2f }, new float[] { 3f } });
			AssertSerdeWNull(DIOPrimitiveIntArray2DimNullableSerde.INSTANCE, new int[][] { new int[] { 1, 2 }, new int[] { 3 } });
			AssertSerdeWNull(DIOPrimitiveLongArray2DimNullableSerde.INSTANCE, new long[][] { new long[] { 1, 2 }, new long[] { 3 } });
			AssertSerdeWNull(DIOPrimitiveShortArray2DimNullableSerde.INSTANCE, new short[][] { new short[] { 1, 2 }, new short[] { 3 } });
			AssertSerdeWNull(
				DIOPrimitiveBooleanArray2DimNullableSerde.INSTANCE,
				new bool[][] { new bool[] { true, false }, new bool[] { true } });
			AssertSerdeWNull(DIOPrimitiveByteArray2DimNullableSerde.INSTANCE, new byte[][] { new byte[] { 1, 2 }, new byte[] { 3 } });

			AssertSerdeWNullValue(DIONullableBooleanSerde.INSTANCE, true);
			AssertSerdeWNullValue(DIONullableByteSerde.INSTANCE, (byte) 0xf);
			AssertSerdeWNullValue(DIONullableCharacterSerde.INSTANCE, 'x');
			AssertSerdeWNullValue(DIONullableDoubleSerde.INSTANCE, 10d);
			AssertSerdeWNullValue(DIONullableFloatSerde.INSTANCE, 11f);
			AssertSerdeWNullValue(DIONullableIntegerSerde.INSTANCE, 12);
			AssertSerdeWNullValue(DIONullableLongSerde.INSTANCE, 13L);
			AssertSerdeWNullValue(DIONullableShortSerde.INSTANCE, (short)14);
			AssertSerdeWNull(DIONullableStringSerde.INSTANCE, "abc");
			AssertSerdeWNullValue(DIONullableBigIntegerSerde.INSTANCE, new BigInteger(10));

			// AssertSerdeWNull(DIODateSerde.INSTANCE, new Date());
			// AssertSerdeWNull(DIOCalendarSerde.INSTANCE, Calendar.Instance);
			// AssertSerdeWNull(DIOSqlDateSerde.INSTANCE, new java.sql.Date(1));
			// AssertSerdeWNull(DIOSqlDateArrayNullableSerde.INSTANCE, new java.sql.Date[] { new java.sql.Date(1), null });

			AssertSerdeWNull(
				DIOBigIntegerArrayNullableSerde.INSTANCE,
				new BigInteger?[] { BigInteger.One, new BigInteger(10) });
			AssertSerdeWNull(
				DIOBigIntegerArray2DimNullableSerde.INSTANCE,
				new BigInteger?[][] { new BigInteger?[] { BigInteger.One, new BigInteger(10) }, null, new BigInteger?[] { null } });

			AssertSerdeWNull(DIOBoxedBooleanArrayNullableSerde.INSTANCE, new bool?[] { true, null, false });
			AssertSerdeWNull(DIOBoxedByteArrayNullableSerde.INSTANCE, new byte?[] { 1, null, 0x2 });
			AssertSerdeWNull(DIOBoxedCharacterArrayNullableSerde.INSTANCE, new char?[] { (char) 1, null, (char) 2 });
			AssertSerdeWNull(DIOBoxedDecimalArrayNullableSerde.INSTANCE, new decimal?[] { 1m, null, 2m });
			AssertSerdeWNull(DIOBoxedDoubleArrayNullableSerde.INSTANCE, new double?[] { 1d, null, 2d });
			AssertSerdeWNull(DIOBoxedFloatArrayNullableSerde.INSTANCE, new float?[] { 1f, null, 2f });
			AssertSerdeWNull(DIOBoxedIntegerArrayNullableSerde.INSTANCE, new int?[] { 1, null, 2 });
			AssertSerdeWNull(DIOBoxedLongArrayNullableSerde.INSTANCE, new long?[] { 1L, null, 2L });
			AssertSerdeWNull(DIOBoxedShortArrayNullableSerde.INSTANCE, new short?[] { 1, null, 2 });
			AssertSerdeWNull(DIOStringArrayNullableSerde.INSTANCE, new string[] { "A", null, "B" });

			AssertSerdeWNull(
				DIOBoxedBooleanArray2DimNullableSerde.INSTANCE,
				new bool?[][] { new bool?[] { true, null, false }, null, new bool?[] { true } });
			AssertSerdeWNull(
				DIOBoxedByteArray2DimNullableSerde.INSTANCE,
				new byte?[][] { new byte?[] { 1, null, 0x2 }, null, new byte?[] { 0x3 } });
			AssertSerdeWNull(
				DIOBoxedCharacterArray2DimNullableSerde.INSTANCE,
				new char?[][] { new char?[] { (char) 1, null, (char) 2 }, null, new char?[] { (char) 1 } });
			AssertSerdeWNull(
				DIOBoxedDecimalArray2DimNullableSerde.INSTANCE,
				new decimal?[][] { new decimal?[] { 1m, null, 2m }, null, new decimal?[] { 3m } });
			AssertSerdeWNull(
				DIOBoxedDoubleArray2DimNullableSerde.INSTANCE,
				new double?[][] { new double?[] { 1d, null, 2d }, null, new double?[] { 3d } });
			AssertSerdeWNull(
				DIOBoxedFloatArray2DimNullableSerde.INSTANCE,
				new float?[][] { new float?[]{ 1f, null, 2f }, null, new float?[]{ 3f } });
			AssertSerdeWNull(
				DIOBoxedIntegerArray2DimNullableSerde.INSTANCE,
				new int?[][] { new int?[] { 1, null, 2 }, null, new int?[] { 3 } });
			AssertSerdeWNull(
				DIOBoxedLongArray2DimNullableSerde.INSTANCE,
				new long?[][] { new long?[] { 1L, null, 2L }, null, new long?[] { 3L } });
			AssertSerdeWNull(
				DIOBoxedShortArray2DimNullableSerde.INSTANCE,
				new short?[][] { new short?[] { 1, null, 2 }, null, new short?[] { 0x3 } });
			AssertSerdeWNull(
				DIOStringArray2DimNullableSerde.INSTANCE,
				new string[][] { new string[] { "A", null, "B" }, null, new string[] { }, new string[] { "a" } });

			// AssertSerdeWNull(DIOCalendarArrayNullableSerde.INSTANCE, new Calendar[] { Calendar.Instance, null });
			// AssertSerdeWNull(DIODateArrayNullableSerde.INSTANCE, new Date[] { new Date(), null });

			AssertSerdeAny(DIOSkipSerde.INSTANCE, null);
			AssertSerdeAny(DIOSerializableObjectSerde.INSTANCE, new SupportBean());

			AssertSerdeWNull(
				new DIONullableObjectArraySerde(typeof(SupportBean), DIOSerializableObjectSerde.INSTANCE),
				new SupportBean[] { new SupportBean(), null });
			AssertSerdeAny(new DIOSetSerde(DIOIntegerSerde.INSTANCE), Collections.Set<object>(1, 2));

			// AssertSerdeWNull(DIOArrayListDateNullableSerde.INSTANCE, Arrays.AsList(new Date(), new Date()));
			// AssertSerdeWNull(
			// 	DIOArrayListSqlDateNullableSerde.INSTANCE,
			// 	Arrays.AsList(new java.sql.Date(1), new java.sql.Date(1)));
			// AssertSerdeWNull(
			// 	DIOArrayListCalendarNullableSerde.INSTANCE,
			// 	Arrays.AsList(GregorianCalendar.Instance, GregorianCalendar.Instance));
		}

		public static void AssertSerdeWNull<T>(
			DataInputOutputSerde<T> serde,
			T serialized)
			where T : class 
		{
			AssertSerde(serde, serialized);
			AssertSerde(serde, null);
		}

		public static void AssertSerdeWNullValue<T>(
			DataInputOutputSerde<T?> serde,
			T serialized)
			where T : struct
		{
			AssertSerde(serde, serialized);
			AssertSerde(serde, null);
		}
		
		public static void AssertSerde<T>(
			DataInputOutputSerde<T> serde,
			T serialized)
		{
			var output = new MemoryStream();
			var dos = new BinaryDataOutput(output);
			serde.Write(serialized, dos, null, null);
			output.Flush();
			output.Close();
			byte[] bytes = output.ToArray();

			var input = new MemoryStream(bytes);
			var dis = new BinaryDataInput(input);
			var deserialized = serde.Read(dis, null);
			input.Close();

			if (serialized == null) {
				ClassicAssert.IsNull(deserialized);
				return;
			}

			if (serialized.GetType().IsArray) {
				var wrapExpected = GetWrapped(serialized);
				var wrapValue = GetWrapped(deserialized);
				ClassicAssert.AreEqual(wrapValue, wrapExpected);
				return;
			}

			ClassicAssert.AreEqual(deserialized, serialized);
		}

		public static void AssertSerdeAny(
			DataInputOutputSerde serde,
			object serialized)
		{
			var output = new MemoryStream();
			var dos = new BinaryDataOutput(output);
			serde.Write(serialized, dos, null, null);
			output.Flush();
			output.Close();
			byte[] bytes = output.ToArray();

			var input = new MemoryStream(bytes);
			var dis = new BinaryDataInput(input);
			var deserialized = serde.Read(dis, null);
			input.Close();

			if (serialized == null) {
				ClassicAssert.IsNull(deserialized);
				return;
			}

			if (serialized.GetType().IsArray) {
				var wrapExpected = GetWrapped(serialized);
				var wrapValue = GetWrapped(deserialized);
				ClassicAssert.AreEqual(wrapValue, wrapExpected);
				return;
			}

			ClassicAssert.AreEqual(deserialized, serialized);
		}

		
		private static MultiKeyArrayWrap GetWrapped(object array)
		{
			var arrayType = array.GetType();
			var mkclzz = MultiKeyPlanner.GetMKClassForComponentType(arrayType.GetElementType());
			var ctor = mkclzz.GetConstructor(new Type[] { arrayType });
			try {
				return (MultiKeyArrayWrap) ctor.Invoke(new object[] { array });
			}
			catch (Exception e) {
				throw new EPRuntimeException(e);
			}
		}
	}
} // end of namespace
