///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

using com.espertech.esper.compat.collections;

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

        public static byte? AsBoxedByte(this object value)
        {
            if (value == null) return null;
            if (value is byte) return (byte) value;
            return Convert.ToByte(value);
        }

        public static byte AsByte(this object value)
        {
            if (value is byte)
                return (byte) value;
            return Convert.ToByte(value);
        }

        /// <summary>
        /// Returns the value as a boxed short.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static short? AsBoxedInt16(this object value)
        {
            if (value == null) return null;
            if (value is short) return (short) value;
            return AsInt16(value);
        }

        /// <summary>
        /// Returns the value as a short.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static short AsInt16(this object value)
        {
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is int intValue)
                return (short) intValue;
            if (value is long longValue)
                return (short) longValue;
            if (value is float floatValue)
                return (short) floatValue;
            if (value is double doubleValue)
                return (short) doubleValue;
            if (value is decimal decimalValue)
                return (short) decimalValue;

            return Convert.ToInt16(value);
        }

        /// <summary>
        /// Returns the value as a boxed int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int? AsBoxedInt32(this object value)
        {
            if (value == null) return null;
            if (value is int intValue) return intValue;
            return AsInt32(value);
        }

        /// <summary>
        /// Returns the value as an int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int AsInt32(this object value)
        {
            if (value is int intValue)
                return intValue;
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;
            
            // -- Down-casts / Cross-casts
            if (value is long longValue)
                return (int) longValue;
            if (value is float floatValue)
                return (int) floatValue;
            if (value is double doubleValue)
                return (int) doubleValue;
            if (value is decimal decimalValue)
                return (int) decimalValue;

            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Returns the value as a boxed long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static long? AsBoxedInt64(this object value)
        {
            if (value == null) return null;
            if (value is long longValue) return longValue;
            return AsInt64(value);
        }

        /// <summary>
        /// Returns the value as a long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static long AsInt64(this object value)
        {
            if (value is long longValue)
                return longValue;
            if (value is int intValue)
                return intValue;
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is float floatValue)
                return (long) floatValue;
            if (value is double doubleValue)
                return (long) doubleValue;
            if (value is decimal decimalValue)
                return (long) decimalValue;

            return Convert.ToInt64(value);
        }
        
        public static ulong AsUInt64(this object value)
        {
            if (value is ulong longValue)
                return longValue;
            if (value is uint intValue)
                return intValue;
            if (value is ushort shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is float floatValue)
                return (ulong) floatValue;
            if (value is double doubleValue)
                return (ulong) doubleValue;
            if (value is decimal decimalValue)
                return (ulong) decimalValue;

            return Convert.ToUInt64(value);
        }

        public static uint AsUInt32(this object value)
        {
            if (value is uint intValue)
                return intValue;
            if (value is ushort shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is float floatValue)
                return (uint) floatValue;
            if (value is double doubleValue)
                return (uint) doubleValue;
            if (value is decimal decimalValue)
                return (uint) decimalValue;

            return Convert.ToUInt32(value);
        }

        public static ushort AsUInt16(this object value)
        {
            if (value is ushort shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is float floatValue)
                return (ushort) floatValue;
            if (value is double doubleValue)
                return (ushort) doubleValue;
            if (value is decimal decimalValue)
                return (ushort) decimalValue;

            return Convert.ToUInt16(value);
        }
        
        /// <summary>
        /// Returns the value as a float.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float AsFloat(this object value)
        {
            if (value is float floatValue)
                return floatValue;
            if (value is double doubleValue)
                return (float) doubleValue;
            if (value is decimal decimalValue)
                return (float) decimalValue;

            if (value is long longValue)
                return longValue;
            if (value is int intValue)
                return intValue;
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;
            
            if (value is BigInteger bigIntegerValue)
                return (float) bigIntegerValue;

            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Returns the value as a boxed float.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float? AsBoxedFloat(this object value)
        {
            if (value == null)
                return null;
            return AsFloat(value);
        }

        /// <summary>
        /// Returns the value as a double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static double AsDouble(this object value)
        {
            if (value is double doubleValue)
                return doubleValue;
            if (value is float floatValue)
                return floatValue;

            if (value is long longValue)
                return longValue;
            if (value is int intValue)
                return intValue;
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;

            // -- Down-casts / Cross-casts
            if (value is decimal decimalValue)
                return (double) decimalValue;
            if (value is BigInteger bigIntegerValue)
                return (double) bigIntegerValue;

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
            if (value is decimal decimalValue)
                return decimalValue;
            if (value is double doubleValue)
                return (decimal) doubleValue;
            if (value is float floatValue)
                return (decimal) floatValue;
            
            if (value is long longValue)
                return longValue;
            if (value is int intValue)
                return intValue;
            if (value is short shortValue)
                return shortValue;
            if (value is byte byteValue)
                return byteValue;
            
            if (value is BigInteger bigIntegerValue)
                return (decimal) bigIntegerValue;

            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// Returns the value as a boxed decimal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static decimal? AsBoxedDecimal(this object value)
        {
            if (value == null)
                return null;
            return AsDecimal(value);
        }

        public static BigInteger AsBigInteger(this object value)
        {
            if (value is BigInteger bigIntegerValue)
                return bigIntegerValue;
            
            if (value is decimal decimalValue)
                return new BigInteger(decimalValue);
            if (value is double doubleValue)
                return new BigInteger(doubleValue);
            if (value is float floatValue)
                return new BigInteger(floatValue);

            if (value is long longValue)
                return new BigInteger(longValue);
            if (value is int intValue)
                return new BigInteger(intValue);
            if (value is short shortValue)
                return new BigInteger(shortValue);
            if (value is byte byteValue)
                return new BigInteger(byteValue);

            if (value is ulong ulongValue)
                return new BigInteger(ulongValue);
            if (value is uint uintValue)
                return new BigInteger(uintValue);
 
            throw new ArgumentException("invalid value for BigInteger", nameof(value));
        }

        public static bool AsBoolean(this object value)
        {
            if (value == null)
                return false;
            if (value is bool boolValue)
                return boolValue;

            throw new ArgumentException("invalid value for bool", nameof(value));
        }

        public static bool? AsBoxedBoolean(this object value)
        {
            if (value == null)
                return null;
            return AsBoolean(value);
        }

        public static DateTime AsDateTime(this object value)
        {
            if (value is DateTime dateTimeValue)
                return dateTimeValue;
            if (value is int intValue)
                return DateTimeHelper.TimeFromMillis(intValue);
            if (value is long longValue)
                return DateTimeHelper.TimeFromMillis(longValue);

            throw new ArgumentException("invalid value for datetime", nameof(value));
        }

        public static DateTime? AsBoxedDateTime(this object value)
        {
            if (value == null)
                return null;
            return AsDateTime(value);
        }

        public static DateTimeOffset AsDateTimeOffset(this object value)
        {
            return AsDateTimeOffset(value, TimeZoneInfo.Utc);
        }

        public static DateTimeOffset AsDateTimeOffset(this object value, TimeZoneInfo timeZone)
        {
            if (value is DateTimeOffset dateTimeOffsetValue)
                return dateTimeOffsetValue.TranslateTo(timeZone);
            if (value is DateTime dateTimeValue)
                return dateTimeValue.TranslateTo(timeZone);
            if (value is long longValue)
                return DateTimeOffsetHelper.TimeFromMillis(longValue, timeZone);
            if (value is int intValue)
                return DateTimeOffsetHelper.TimeFromMillis(intValue, timeZone);

            throw new ArgumentException("invalid value for datetime", nameof(value));
        }

        public static DateTimeOffset? AsBoxedDateTimeOffset(this object value)
        {
            if (value == null)
                return null;
            return AsDateTimeOffset(value);
        }

        public static DateTimeOffset? AsBoxedDateTimeOffset(this object value, TimeZoneInfo timeZone)
        {
            if (value == null)
                return null;
            return AsDateTimeOffset(value, timeZone);
        }

        /// <summary>
        /// Determines whether the specified value is long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is long; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInt64(this object value)
        {
            return (value is short) ||
                   (value is int) ||
                   (value is long);
        }

        /// <summary>
        /// Determines whether the specified value is int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="withUpcast">if set to <c>true</c> [with upcast].</param>
        /// <returns>
        ///   <c>true</c> if the specified value is int; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInt32(this object value, bool withUpcast = true)
        {
            if (withUpcast) {
                return (value is short) ||
                       (value is int);
            }

            return (value is int);
        }

        /// <summary>
        /// Determines whether the specified value is Int16 (short).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is short; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInt16(this object value)
        {
            return (value is short);
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
            return (asType == typeof(DateTimeEx)) ||
                   (asType == typeof(DateTimeOffset?)) ||
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

        public static bool IsImplementsInterface<T>(this Type clazz)
        {
            return IsImplementsInterface(clazz, typeof(T));
        }

        /// <summary>
        /// Method to check if a given class, and its superclasses and interfaces (deep), implement a given interface.
        /// </summary>
        /// <param name="clazz">to check, including all its superclasses and their interfaces and extends</param>
        /// <param name="interfaceClass">is the interface class to look for</param>
        /// <returns>
        /// true if such interface is implemented by type of the clazz or its superclasses orextends by type interface and superclasses (deep check)
        /// </returns>
        public static bool IsImplementsInterface(this Type clazz, Type interfaceClass)
        {
            if (!(interfaceClass.IsInterface))
            {
                throw new ArgumentException("Interface class passed in is not an interface");
            }
            var resultThisClass = RecursiveIsImplementsInterface(clazz, interfaceClass);
            if (resultThisClass)
            {
                return true;
            }
            return RecursiveSuperclassImplementsInterface(clazz, interfaceClass);
        }

        // NOTE: Java vs. CLR interface model
        //  Java needs recursive functions to introspect
        //  classes, but I dont think the CLR behaves this way.  I think that it flattens
        //  the entire structure.

        public static bool RecursiveSuperclassImplementsInterface(this Type clazz, Type interfaceClass)
        {
            Type baseType = clazz.BaseType;
            if ((baseType == null) || (baseType == typeof(Object)))
            {
                return false;
            }
            bool result = RecursiveIsImplementsInterface(baseType, interfaceClass);
            return result || RecursiveSuperclassImplementsInterface(baseType, interfaceClass);
        }

        public static bool RecursiveIsSuperClass(Type clazz, Type superClass)
        {
            if (clazz == null)
            {
                return false;
            }
            if (clazz.IsValueType)
            {
                return false;
            }
            Type mySuperClass = clazz.BaseType;
            if (mySuperClass == superClass)
            {
                return true;
            }
            if (mySuperClass == typeof(Object))
            {
                return false;
            }
            return RecursiveIsSuperClass(mySuperClass, superClass);
        }

        public static bool RecursiveIsImplementsInterface(Type clazz, Type interfaceClass)
        {
            if (clazz == interfaceClass)
            {
                return true;
            }

            var interfaces = clazz.GetInterfaces();
            if (interfaces.Length == 0)
            {
                return false;
            }

            return interfaces
                .Select(interfaceX => RecursiveIsImplementsInterface(interfaceX, interfaceClass))
                .Any(result => result);
        }
        
        public static string CleanName<T>()
        {
            return CleanName(typeof(T), true);
        }

        public static string CleanName(
            this Type type,
            bool useFullName = true)
        {
            if (type == null)
            {
                return "null (any type)";
            }

            if (type.IsArray)
            {
                return CleanName(type.GetElementType()) + "[]";
            }

            if (type.IsGenericType)
            {
                var genericName = useFullName
                    ? type.FullName ?? type.Name
                    : type.Name;
                var index = genericName.IndexOf('`');
                if (index != -1)
                {
                    genericName = genericName.Substring(0, index);
                }

                var separator = "";
                var builder = new StringBuilder();
                builder.Append(genericName);
                builder.Append('<');
                foreach (var genericType in type.GetGenericArguments())
                {
                    builder.Append(separator);
                    builder.Append(CleanName(genericType, useFullName));
                    separator = ", ";
                }

                builder.Append('>');
                return builder.ToString();
            }

            return useFullName ? type.FullName : type.Name;
        }

        public static string CleanName<T>(bool useFullName)
        {
            return CleanName(typeof(T), useFullName);
        }
        
        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null) {
                return Activator.CreateInstance(t);
            }
            else {
                return null;
            }
        }
    }
}
