///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Collection of utility methods to help with enumerated types.
    /// </summary>
    public static class EnumHelper
    {
        public static TOut Xlate<TOut>(this object value)
            where TOut : struct
        {
            TOut ovalue;
            Enum.TryParse(value.ToString(), true, out ovalue);
            return ovalue;
        }

        public static TOut Translate<TIn,TOut>(this TIn value) 
            where TIn : struct
            where TOut : struct
        {
            TOut ovalue;
            Enum.TryParse(value.ToString(), true, out ovalue);
            return ovalue;
        }

        /// <summary>
        /// Parses the specified text value and converts it into the specified
        /// type of enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textValue">The text value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static T Parse<T>( String textValue, bool ignoreCase = true )
        {
            return (T) Enum.Parse(typeof (T), textValue, ignoreCase);
        }

        /// <summary>
        /// Parses the specified enumeration returning the value in a boxable container
        /// to allow for null values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textValue">The text value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static T? ParseBoxed<T>( String textValue, bool ignoreCase = true ) where T : struct
        {
            T value;
            if (Enum.TryParse(textValue, ignoreCase, out value))
                return value;
            return null;
        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<string> GetNames<T>()
        {
            return Enum.GetNames(typeof (T));
        }

        /// <summary>
        /// Gets the name associated with the value presented in enumValue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static string GetName<T>( this T enumValue )
        {
            if (typeof(T).IsEnum)
                return Enum.GetName(typeof (T), enumValue);
            throw new ArgumentException("type is not an enumeration");
        }

        public static string GetNameInvariant<T>(this T enumValue)
        {
            if (typeof(T).IsEnum)
                return Enum.GetName(typeof(T), enumValue).ToLowerInvariant();
            throw new ArgumentException("type is not an enumeration");
        }

        public static T GetValue<T>(int ordinal)
        {
            foreach(T value in GetValues<T>())
            {
                if (Equals(ordinal, value))
                {
                    return value;
                }
            }

            throw new ArgumentException("ordinal value not found");
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            Array array = Enum.GetValues(typeof (T));
            for(int ii = 0 ; ii < array.Length ; ii++)
            {
                yield return (T) array.GetValue(ii);
            }
        }

        public static int CountValues<T>()
        {
            Array array = Enum.GetValues(typeof(T));
            return array.Length;
        }

        public static void ForEach<T>(Action<T> valueHandler)
        {
            Array array = Enum.GetValues(typeof(T));
            for(int ii = 0; ii < array.Length; ii++)
            {
                valueHandler((T)array.GetValue(ii));
            }
        }
    }
}
