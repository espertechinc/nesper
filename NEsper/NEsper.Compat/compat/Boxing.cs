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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compat
{
    public static class Boxing
    {
        private static readonly IDictionary<Type, Type> BoxedTable;

        /// <summary>
        /// Initializes the <see cref="Boxing"/> class.
        /// </summary>
        static Boxing()
        {
            BoxedTable = new Dictionary<Type, Type>();
            BoxedTable[typeof(int)] = typeof(int?);
            BoxedTable[typeof(long)] = typeof(long?);
            BoxedTable[typeof(bool)] = typeof(bool?);
            BoxedTable[typeof(char)] = typeof(char?);
            BoxedTable[typeof(decimal)] = typeof(decimal?);
            BoxedTable[typeof(double)] = typeof(double?);
            BoxedTable[typeof(float)] = typeof(float?);
            BoxedTable[typeof(sbyte)] = typeof(sbyte?);
            BoxedTable[typeof(byte)] = typeof(byte?);
            BoxedTable[typeof(short)] = typeof(short?);
            BoxedTable[typeof(ushort)] = typeof(ushort?);
            BoxedTable[typeof(uint)] = typeof(uint?);
            BoxedTable[typeof(ulong)] = typeof(ulong?);
            BoxedTable[typeof(BigInteger)] = typeof(BigInteger?);
        }

        /// <summary>
        /// Gets the unboxed type for the value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetUnboxedType(this Type type)
        {
            if (type == null) {
                return null;
            }

            if (type.IsNullable()) {
                var unboxed = Nullable.GetUnderlyingType(type);
                if (unboxed == null)
                    return type;
                return unboxed;
            }

            return type;
        }

        /// <summary>
        /// Gets the boxed type for the value.
        /// </summary>
        /// <param name="any">Any.</param>
        /// <returns></returns>
        public static Type GetBoxedType(this object any)
        {
            if (any is Type)
                return GetBoxedType((Type)any);
            else if (any == null)
                return null;

            return any.GetType().GetBoxedType();
        }

        /// <summary>
        /// Returns the boxed class for the given class, or the class itself if already boxed or not a primitive type.
        /// For primitive unboxed types returns the boxed types, e.g. returns typeof(int?) for passing typeof(int).
        /// For type other class, returns the class passed.
        /// </summary>
        /// <param name="type">is the type to return the boxed type for</param>

        public static Type GetBoxedType(this Type type)
        {
            if (type == null) return null;
            if (type == typeof(void)) return typeof(void);
            if (type == typeof(int) || type == typeof(int?)) return typeof(int?);
            if (type == typeof(long) || type == typeof(long?)) return typeof(long?);
            if (type == typeof(bool) || type == typeof(bool?)) return typeof(bool?);
            if (type == typeof(double) || type == typeof(double?)) return typeof(double?);
            if (type == typeof(decimal) || type == typeof(decimal?)) return typeof(decimal?);
            if (type == typeof(float) || type == typeof(float?)) return typeof(float?);
            if (type == typeof(BigInteger) || type == typeof(BigInteger?)) return typeof(BigInteger?);

            Type boxed;
            if (BoxedTable.TryGetValue(type, out boxed))
            {
                return boxed;
            }

            if (type.IsNullable())
            {
                return type;
            }

            if (type.IsValueType)
            {
                boxed = typeof(Nullable<>).MakeGenericType(type);
                BoxedTable[type] = boxed;
                return boxed;
            }

            return type;
        }

        public static bool IsBoxedType(this Type type)
        {
            return (type.GetBoxedType() == type);
        }
        
        public static bool IsUnboxedType(this Type type)
        {
            return (type.GetBoxedType() != type);
        }
    }
}
