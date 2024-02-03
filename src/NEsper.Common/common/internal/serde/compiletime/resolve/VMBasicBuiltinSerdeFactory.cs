///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        private static readonly IDictionary<string, DataInputOutputSerde> BY_PRETTY_NAME =
            new Dictionary<string, DataInputOutputSerde>();

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
            AddPrimitive(typeof(decimal), DIODecimalSerde.INSTANCE);
            AddPrimitive(typeof(void), DIOSkipSerde.INSTANCE);

            AddBoxed(typeof(string), DIONullableStringSerde.INSTANCE);
            AddBoxed(typeof(char?), DIONullableCharacterSerde.INSTANCE);
            AddBoxed(typeof(bool?), DIONullableBooleanSerde.INSTANCE);
            AddBoxed(typeof(byte?), DIONullableByteSerde.INSTANCE);
            AddBoxed(typeof(short?), DIONullableShortSerde.INSTANCE);
            AddBoxed(typeof(int?), DIONullableIntegerSerde.INSTANCE);
            AddBoxed(typeof(long?), DIONullableLongSerde.INSTANCE);
            AddBoxed(typeof(float?), DIONullableFloatSerde.INSTANCE);
            AddBoxed(typeof(double?), DIONullableDoubleSerde.INSTANCE);
            AddBoxed(typeof(decimal?), DIONullableDecimalSerde.INSTANCE);

            AddBoxed(typeof(string[]), DIOStringArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(char?[]), DIOBoxedCharacterArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(bool?[]), DIOBoxedBooleanArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(byte?[]), DIOBoxedByteArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(short?[]), DIOBoxedShortArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(int?[]), DIOBoxedIntegerArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(long?[]), DIOBoxedLongArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(float?[]), DIOBoxedFloatArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(double?[]), DIOBoxedDoubleArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(decimal?[]), DIOBoxedDecimalArrayNullableSerde.INSTANCE);

            AddBoxed(typeof(char[]), DIOPrimitiveCharArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(bool[]), DIOPrimitiveBooleanArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(byte[]), DIOPrimitiveByteArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(short[]), DIOPrimitiveShortArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(int[]), DIOPrimitiveIntArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(long[]), DIOPrimitiveLongArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(float[]), DIOPrimitiveFloatArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(double[]), DIOPrimitiveDoubleArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(decimal[]), DIOPrimitiveDecimalArrayNullableSerde.INSTANCE);

            AddBoxed(typeof(char[][]), DIOPrimitiveCharArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(bool[][]), DIOPrimitiveBooleanArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(byte[][]), DIOPrimitiveByteArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(short[][]), DIOPrimitiveShortArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(int[][]), DIOPrimitiveIntArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(long[][]), DIOPrimitiveLongArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(float[][]), DIOPrimitiveFloatArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(double[][]), DIOPrimitiveDoubleArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(decimal[][]), DIOPrimitiveDecimalArray2DimNullableSerde.INSTANCE);

            AddBoxed(typeof(string[][]), DIOStringArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(char?[][]), DIOBoxedCharacterArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(bool?[][]), DIOBoxedBooleanArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(byte?[][]), DIOBoxedByteArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(short?[][]), DIOBoxedShortArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(int?[][]), DIOBoxedIntegerArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(long?[][]), DIOBoxedLongArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(float?[][]), DIOBoxedFloatArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(double?[][]), DIOBoxedDoubleArray2DimNullableSerde.INSTANCE);
            AddBoxed(typeof(decimal?[][]), DIOBoxedDecimalArray2DimNullableSerde.INSTANCE);

            AddBoxed(typeof(BigInteger?[]), DIOBigIntegerArrayNullableSerde.INSTANCE);
            AddBoxed(typeof(BigInteger?[][]), DIOBigIntegerArray2DimNullableSerde.INSTANCE);

            if (BY_PRETTY_NAME == null) {
                BY_PRETTY_NAME = new Dictionary<string, DataInputOutputSerde>();
                AddPrettyName(PRIMITIVES, BY_PRETTY_NAME);
                AddPrettyName(BOXED, BY_PRETTY_NAME);
            }
        }

        private static void AddPrimitive(
            Type type,
            DataInputOutputSerde serde)
        {
            PRIMITIVES.Put(type, serde);
        }

        private static void AddBoxed(
            Type type,
            DataInputOutputSerde serde)
        {
            BOXED.Put(type, serde);
        }

        /// <summary>
        /// Returns the serde for the given built-in type.
        /// </summary>
        /// <param name="type">is the type</param>
        /// <returns>serde for marshalling and unmarshalling that type</returns>
        internal static DataInputOutputSerde GetSerde(Type type)
        {
            if (type.IsPrimitive) {
                return PRIMITIVES.Get(type);
            }

            return BOXED.Get(type);
        }

        public static DataInputOutputSerde GetSerde(string typeNamePretty)
        {
            return BY_PRETTY_NAME.Get(typeNamePretty);
        }

        private static void AddPrettyName(
            IDictionary<Type, DataInputOutputSerde> serdes,
            IDictionary<string, DataInputOutputSerde> allbyname)
        {
            foreach (var serdeKeyValue in serdes) {
                var clazz = serdeKeyValue.Key;
                var pretty = clazz.CleanName();
                if (allbyname.ContainsKey(pretty)) {
                    throw new IllegalStateException("Duplicate key '" + pretty + "'");
                }

                allbyname[pretty] = serdeKeyValue.Value;
            }
        }
    }
} // end of namespace