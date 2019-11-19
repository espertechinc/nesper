///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Linq;
using System.Reflection;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Methos for meta enumerations.
    /// </summary>
    public static class MetaEnum
    {
        public static String GetMetaName<T>(this T metaEnumInstance)
        {
            return typeof (T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof (T))
                .Where(field => ReferenceEquals(field.GetValue(null), metaEnumInstance))
                .Select(field => field.Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the named clause.
        /// </summary>
        /// <param name="enumName">Name of the enum.</param>
        /// <returns></returns>
        public static T GetMetaEnum<T>(this String enumName)
        {
            enumName = enumName.ToUpperInvariant();

            FieldInfo fieldInfo = typeof (T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof (T))
                .Where(field => field.Name == enumName)
                .FirstOrDefault();
            if (fieldInfo == null) {
                throw new ArgumentException("enumName");
            }

            return (T) fieldInfo.GetValue(null);
        }

        public static TResult GetMetaEnum<TResult,T2>(this T2 enumValue)
        {
            string enumName = enumValue.ToString().ToUpperInvariant();

            FieldInfo fieldInfo = typeof(TResult)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(TResult))
                .Where(field => field.Name == enumName)
                .FirstOrDefault();
            if (fieldInfo == null)
            {
                throw new ArgumentException("enumName");
            }

            return (TResult)fieldInfo.GetValue(null);
        }

        /// <summary>
        /// Gets the named enumerated value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="enumOrdinal">The enum ordinal.</param>
        /// <returns></returns>
        public static TResult GetNamedEnum<T,TResult>(int enumOrdinal)
        {
            return GetMetaEnum <TResult>(Enum.GetName(typeof(T), enumOrdinal));
        }
    }
}
