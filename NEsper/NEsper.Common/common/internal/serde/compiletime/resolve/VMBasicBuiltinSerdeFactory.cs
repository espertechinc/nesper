///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using java.math;
namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	/// <summary>
	/// Factory for serde implementations that provides a serde for a given built-in type.
	/// </summary>
	public class VMBasicBuiltinSerdeFactory
	{
		private static readonly IDictionary<Type, DataInputOutputSerde> PRIMITIVES =
			new Dictionary<Type, DataInputOutputSerde>();
		private static readonly IDictionary<Type, DataInputOutputSerde> BOXED =
			new Dictionary<Type, DataInputOutputSerde>();

		static VMBasicBuiltinSerdeFactory() 
		{
			AddPrimitive(typeof(char), DIOCharacterSerde.INSTANCE);
			AddPrimitive(typeof(bool), DIOBooleanSerde.INSTANCE);
			AddPrimitive(typeof(byte), DIOByteSerde.INSTANCE);
			AddPrimitive(typeof(short), DIOShortSerde.INSTANCE);
			AddPrimitive(typeof(int), DIOIntegerSerde.INSTANCE);
			AddPrimitive(typeof(long), DIOLongSerde.INSTANCE);
			AddPrimitive(typeof(float), DIOFloatSerde.INSTANCE);
			AddPrimitive(typeof(double), DIODoubleSerde.INSTANCE);
			AddPrimitive(typeof(void), DIOSkipSerde.INSTANCE);

			AddBoxed(typeof(string), DIOStringSerde.INSTANCE);
			AddBoxed(typeof(CharSequence), DIOCharSequenceSerde.INSTANCE);
			AddBoxed(typeof(Character), DIONullableCharacterSerde.INSTANCE);
			AddBoxed(typeof(bool?), DIONullableBooleanSerde.INSTANCE);
			AddBoxed(typeof(byte?), DIONullableByteSerde.INSTANCE);
			AddBoxed(typeof(short?), DIONullableShortSerde.INSTANCE);
			AddBoxed(typeof(int?), DIONullableIntegerSerde.INSTANCE);
			AddBoxed(typeof(long?), DIONullableLongSerde.INSTANCE);
			AddBoxed(typeof(float?), DIONullableFloatSerde.INSTANCE);
			AddBoxed(typeof(double?), DIONullableDoubleSerde.INSTANCE);

			AddBoxed(typeof(string[]), DIOStringArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(CharSequence[]), DIOStringArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(Character[]), DIOBoxedCharacterArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(bool?[]), DIOBoxedBooleanArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(byte?[]), DIOBoxedByteArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(short?[]), DIOBoxedShortArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(int?[]), DIOBoxedIntegerArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(long?[]), DIOBoxedLongArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(float?[]), DIOBoxedFloatArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(double?[]), DIOBoxedDoubleArrayNullableSerde.INSTANCE);

			AddBoxed(typeof(char[]), DIOPrimitiveCharArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(bool[]), DIOPrimitiveBooleanArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(byte[]), DIOPrimitiveByteArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(short[]), DIOPrimitiveShortArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(int[]), DIOPrimitiveIntArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(long[]), DIOPrimitiveLongArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(float[]), DIOPrimitiveFloatArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(double[]), DIOPrimitiveDoubleArrayNullableSerde.INSTANCE);

			AddBoxed(typeof(char[][]), DIOPrimitiveCharArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(bool[][]), DIOPrimitiveBooleanArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(byte[][]), DIOPrimitiveByteArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(short[][]), DIOPrimitiveShortArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(int[][]), DIOPrimitiveIntArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(long[][]), DIOPrimitiveLongArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(float[][]), DIOPrimitiveFloatArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(double[][]), DIOPrimitiveDoubleArray2DimNullableSerde.INSTANCE);

			AddBoxed(typeof(string[][]), DIOStringArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(CharSequence[][]), DIOStringArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(char?[][]), DIOBoxedCharacterArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(bool?[][]), DIOBoxedBooleanArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(byte?[][]), DIOBoxedByteArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(short?[][]), DIOBoxedShortArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(int?[][]), DIOBoxedIntegerArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(long?[][]), DIOBoxedLongArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(float?[][]), DIOBoxedFloatArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(double?[][]), DIOBoxedDoubleArray2DimNullableSerde.INSTANCE);

			AddBoxed(typeof(BigInteger[]), DIOBigIntegerArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(BigDecimal[]), DIOBigDecimalArrayNullableSerde.INSTANCE);
			AddBoxed(typeof(BigInteger[][]), DIOBigIntegerArray2DimNullableSerde.INSTANCE);
			AddBoxed(typeof(BigDecimal[][]), DIOBigDecimalArray2DimNullableSerde.INSTANCE);
		}

		private static void AddPrimitive(
			Type cls,
			DataInputOutputSerde serde)
		{
			PRIMITIVES.Put(cls, serde);
		}

		private static void AddBoxed(
			Type cls,
			DataInputOutputSerde serde)
		{
			BOXED.Put(cls, serde);
		}

		/// <summary>
		/// Returns the serde for the given Java built-in type.
		/// </summary>
		/// <param name="cls">is the Java type</param>
		/// <returns>serde for marshalling and unmarshalling that type</returns>
		protected static DataInputOutputSerde GetSerde(Type cls)
		{
			if (cls.IsPrimitive) {
				return PRIMITIVES.Get(cls);
			}

			return BOXED.Get(cls);
		}
	}
} // end of namespace
