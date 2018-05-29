///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.util
{
    public sealed class CoercerFactory
    {
        private static readonly IDictionary<Type, Coercer> CoercerTable;

        static CoercerFactory()
        {
            CoercerTable = new Dictionary<Type, Coercer>();
            CoercerTable[typeof (decimal?)] =
                (itemToCoerce => itemToCoerce.AsDecimal());
            CoercerTable[typeof (double?)] =
                (itemToCoerce => itemToCoerce.AsDouble());
            CoercerTable[typeof (ulong?)] =
                (itemToCoerce => Convert.ToUInt64(itemToCoerce));
            CoercerTable[typeof (long?)] =
                (itemToCoerce => itemToCoerce.AsLong());
            CoercerTable[typeof (float?)] =
                (itemToCoerce => itemToCoerce.AsFloat());
            CoercerTable[typeof (uint?)] =
                (itemToCoerce => Convert.ToUInt32(itemToCoerce));
            CoercerTable[typeof (int?)] =
                (itemToCoerce => itemToCoerce.AsInt());
            CoercerTable[typeof (ushort?)] =
                (itemToCoerce => Convert.ToUInt16(itemToCoerce));
            CoercerTable[typeof (short?)] =
                (itemToCoerce => itemToCoerce.AsShort());
            CoercerTable[typeof (byte?)] =
                (itemToCoerce => Convert.ToByte(itemToCoerce));
            CoercerTable[typeof (sbyte?)] =
                (itemToCoerce => Convert.ToSByte(itemToCoerce));
            CoercerTable[typeof (BigInteger?)] =
                (itemToCoerce => itemToCoerce.AsBigInteger());
            CoercerTable[typeof (string)] =
                (itemToCoerce => Convert.ToString(itemToCoerce));
        }

        /// <summary>
        /// Gets the type coercer from any object to the target type.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static Coercer GetCoercer(Type targetType)
        {
            return GetCoercer(typeof (Object), targetType);
        }

        /// <summary>
        /// Gets the type coercer between two types.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <param name="targetType">Boxed target type.</param>
        /// <returns></returns>
        public static Coercer GetCoercer(Type fromType, Type targetType)
        {
            targetType = targetType.GetBoxedType();

            if (fromType.GetBoxedType() == targetType)
                return NullCoercion;

            if (targetType == typeof(Object))
                return NullCoercion;

            Coercer coercer = CoercerTable.Get(targetType);
            if (coercer != null)
                return coercer;

            throw new ArgumentException("Cannot coerce to number subtype " + targetType.GetCleanName());
        }

        /// <summary>
        /// CoerceIndex the given number to the given type. Allows coerce to lower resultion number.
        /// Doesn't coerce to primitive types.
        /// <param name="itemToCoerce">numToCoerce is the number to coerce to the given type</param>
        /// <param name="resultBoxedType">the result type to return</param>
        /// <returns>the itemToCoerce as a value in the given result type</returns>
        /// </summary>

        public static Object CoerceBoxed(Object itemToCoerce, Type resultBoxedType)
        {
            Coercer coercer = GetCoercer(itemToCoerce.GetType(), resultBoxedType);
            return coercer.Invoke(itemToCoerce);
        }

        /// <summary>
        /// Performs a null coercion.
        /// </summary>
        /// <param name="itemToCoerce">The item to coerce.</param>
        /// <returns></returns>
        public static Object NullCoercion(Object itemToCoerce)
        {
            return itemToCoerce;
        }
    }

    public delegate Object Coercer(Object itemToCoerce);
}
