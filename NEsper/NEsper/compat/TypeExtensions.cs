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
using System.Reflection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.compat
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Finds the property.
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo FindProperty(this Type t, string name)
        {
            var propertyInfo = t.GetProperty(name);
            if (propertyInfo != null)
                return propertyInfo;

            foreach( var property in t.GetProperties() ) {
                if ( String.Equals(name, property.Name, StringComparison.CurrentCultureIgnoreCase) ) {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Ases the singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T[] AsSingleton<T>(this T value)
        {
            return new T[] {value};
        }

        public static ISet<T> AsSet<T>(this T value)
        {
            return new Singleton<T>(value);
        }

        public static bool? AsBoxedBoolean(this object value)
        {
            if (value == null) return null;
            if (value is bool) return (bool)value;
            return AsBoolean(value);
        }

        /// <summary>
        /// Returns the value as a boxed short.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static short? AsBoxedShort(this object value)
        {
            if (value == null) return null;
            if (value is short) return (short) value;
            return AsShort(value);
        }

        /// <summary>
        /// Returns the value as a short.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static short AsShort(this object value)
        {
            if (value is short)
                return (short)value;
            return Convert.ToInt16(value);
        }

        /// <summary>
        /// Returns the value as a boxed int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int? AsBoxedInt(this object value)
        {
            if (value == null) return null;
            if (value is int) return (int) value;
            return AsInt(value);
        }

        /// <summary>
        /// Returns the value as an int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int AsInt(this object value)
        {
            if (value is int)
                return (int) value;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Returns the value as a boxed long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static long? AsBoxedLong(this object value)
        {
            if (value == null) return null;
            if (value is long) return (long)value;
            return AsLong(value);
        }

        /// <summary>
        /// Returns the value as a long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static long AsLong(this object value)
        {
            if (value is long)
                return (long)value;
            if (value is int)
                return (int)value;
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Returns the value as a float.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float AsFloat(this object value)
        {
            if (value is decimal)
                return (float) ((decimal) value);
            if (value is double)
                return (float)((double)value);
            if (value is float)
                return (float)value;
            if (value is long)
                return (long)value;
            if (value is int)
                return (int)value;
            if (value is BigInteger)
                return (float) ((BigInteger)value);

            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Returns the value as a double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static double AsDouble(this object value)
        {
            if (value is decimal)
                return (double)((decimal)value);
            if (value is double)
                return (double) value;
            if (value is float)
                return (float) value;
            if (value is long)
                return (long) value;
            if (value is int)
                return (int) value;
            if (value is BigInteger)
                return (double) ((BigInteger) value);

            return Convert.ToDouble(value);
        }


        /// <summary>
        /// Returns the value as a boxed double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static double? AsBoxedDouble(this object value)
        {
            if (value == null) return null;
            return AsDouble(value);
        }

        /// <summary>
        /// Returns the value as a decimal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static decimal AsDecimal(this object value)
        {
            if (value is decimal)
                return (decimal)value;
            if (value is double)
                return (decimal) ((double) value);
            if (value is float)
                return (decimal) ((float) value);
            if (value is long)
                return (long)value;
            if (value is int)
                return (int)value;
            if (value is BigInteger)
                return (decimal) ((BigInteger)value);

            return Convert.ToDecimal(value);
        }

        public static BigInteger AsBigInteger(this object value)
        {
            if (value is BigInteger)
                return (BigInteger) value;
            if (value is decimal)
                return new BigInteger((decimal) value);
            if (value is double)
                return new BigInteger((double) value);
            if (value is float)
                return new BigInteger((float) value);
            if (value is long)
                return new BigInteger((long) value);
            if (value is int)
                return new BigInteger((int) value);
            if (value is uint)
                return new BigInteger((uint) value);
            if (value is ulong)
                return new BigInteger((ulong) value);
            if (value is byte)
                return new BigInteger((byte) value);

            throw new ArgumentException("invalid value for BigInteger", "value");
        }

        public static bool AsBoolean(this object value)
        {
            if (value == null)
                return false;
            if (value is bool)
                return (bool)value;

            throw new ArgumentException("invalid value for bool", "value");
        }

        public static DateTime AsDateTime(this object value)
        {
            if (value is DateTime)
                return (DateTime) value;
            if (value is int)
                return DateTimeHelper.FromMillis((int) value);
            if (value is long)
                return DateTimeHelper.FromMillis((long) value);

            throw new ArgumentException("invalid value for datetime", "value");
        }

        public static DateTimeOffset AsDateTimeOffset(this object value)
        {
            return AsDateTimeOffset(value, TimeZoneInfo.Local);
        }

        public static DateTimeOffset AsDateTimeOffset(this object value, TimeZoneInfo timeZone)
        {
            if (value is DateTimeOffset)
                return ((DateTimeOffset) value).TranslateTo(timeZone);
            if (value is DateTime)
                return ((DateTime) value).TranslateTo(timeZone);
            if (value is long)
                return DateTimeOffsetHelper.TimeFromMillis((long) value, timeZone);
            if (value is int)
                return DateTimeOffsetHelper.TimeFromMillis((int) value, timeZone);

            throw new ArgumentException("invalid value for datetime", "value");
        }

        /// <summary>
        /// Determines whether the specified value is long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is long; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLong(this object value)
        {
            return value.IsIntegralNumber(typeof (long));
        }

        /// <summary>
        /// Determines whether the specified value is int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="withUpcast">if set to <c>true</c> [with upcast].</param>
        /// <returns>
        ///   <c>true</c> if the specified value is int; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInt(this object value, bool withUpcast = true)
        {
            if (withUpcast)
            {
                return value.IsIntegralNumber(typeof (int));
            }

            return (value is int);
        }

        /// <summary>
        /// Determines whether [is date time] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is date time] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDateTime(this object value)
        {
            var asType = value as Type;
            if (asType == null)
                return false;

            asType = asType.GetBoxedType();
            return (asType == typeof(DateTimeOffset?)) ||
                   (asType == typeof(DateTime?)) ||
                   (asType == typeof(long?));
        }

        /// <summary>
        /// Gets the base type tree.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetBaseTypeTree(this Type type)
        {
            for (; type != null; type = type.BaseType)
            {
                yield return type;
            }
        }

        public static Type FindAttributeInTypeTree(this Type type, Type attributeType)
        {
            // look for a data contract in the hierarchy of types
            foreach (var superType in type.GetBaseTypeTree())
            {
                var attributes = superType.GetCustomAttributes(attributeType, false);
                if (attributes.Length != 0)
                {
                    return superType;
                }
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                var attributes = interfaceType.GetCustomAttributes(attributeType, false);
                if (attributes.Length != 0)
                {
                    return interfaceType;
                }
            }

            return null;
        }

        /// <summary>
        /// Transparent cast for the lazy
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static IDictionary<string,object> AsDataMap(this object o)
        {
            return o as IDictionary<string, object>;
        }

        public static Attribute[] UnwrapAttributes(this MemberInfo memberInfo, bool inherit = true)
        {
            return memberInfo.GetCustomAttributes(inherit).UnwrapIntoArray<Attribute>();
        }

        public static T[] UnwrapAttributes<T>(this MemberInfo memberInfo, bool inherit = true)
            where T : Attribute
        {
            return memberInfo.GetCustomAttributes(inherit).UnwrapIntoArray<T>();
        }

        public static bool IsVarArgs(this MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
            {
                return false;
            }

            return parameters[parameters.Length - 1].GetCustomAttributes(typeof (ParamArrayAttribute), false).Length >= 1;
        }

        public static bool IsVarArgs(this ConstructorInfo constructorInfo)
        {
            var parameters = constructorInfo.GetParameters();
            if (parameters.Length == 0)
            {
                return false;
            }

            return parameters[parameters.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 1;
        }
    }
}
