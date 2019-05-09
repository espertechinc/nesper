///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Enumeration of the different built-in types that are used to represent database output column values.
    ///     <para>
    ///         Assigns a name to each type that serves as a short name in mapping, and a type.
    ///     </para>
    ///     <para>
    ///         Provides binding implementations that use the correct ResultSet.get method to pull the correct type
    ///         out of a statement's result set.
    ///     </para>
    /// </summary>
    [Serializable]
    public class DatabaseTypeEnum
    {
        private static readonly IDictionary<DatabaseTypeEnum, DatabaseTypeBinding> bindings;

        /// <summary>Boolean type.</summary>
        public static readonly DatabaseTypeEnum Boolean = new DatabaseTypeEnum(typeof(bool));

        /// <summary>Byte type.</summary>
        public static readonly DatabaseTypeEnum Byte = new DatabaseTypeEnum(typeof(byte));

        /// <summary>Byte array type.</summary>
        public static readonly DatabaseTypeEnum ByteArray = new DatabaseTypeEnum(typeof(byte[]));

        /// <summary>Big decimal.</summary>
        public static readonly DatabaseTypeEnum Decimal = new DatabaseTypeEnum(typeof(decimal));

        /// <summary>Double type.</summary>
        public static readonly DatabaseTypeEnum Double = new DatabaseTypeEnum(typeof(double));

        /// <summary>Float type.</summary>
        public static readonly DatabaseTypeEnum Float = new DatabaseTypeEnum(typeof(float));

        /// <summary>Integer type.</summary>
        public static readonly DatabaseTypeEnum Int = new DatabaseTypeEnum(typeof(int));

        /// <summary>Long type.</summary>
        public static readonly DatabaseTypeEnum Long = new DatabaseTypeEnum(typeof(long));

        /// <summary>Short type.</summary>
        public static readonly DatabaseTypeEnum Short = new DatabaseTypeEnum(typeof(short));

        /// <summary>String type.</summary>
        public static readonly DatabaseTypeEnum String = new DatabaseTypeEnum(typeof(string));

        /// <summary>timestamp type.</summary>
        public static readonly DatabaseTypeEnum Timestamp = new DatabaseTypeEnum(typeof(DateTime));

        public static readonly DatabaseTypeEnum[] Values = {
            String,
            Decimal,
            Boolean,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            ByteArray,
            Timestamp
        };

        static DatabaseTypeEnum()
        {
            bindings = new Dictionary<DatabaseTypeEnum, DatabaseTypeBinding>();

            bindings.Put(
                String,
                new ProxyDatabaseTypeBinding<string>(
                    (
                        rawData,
                        columnName) => Convert.ToString(rawData)));

            bindings.Put(
                Decimal,
                new ProxyDatabaseTypeBinding<decimal>(
                    (
                        rawData,
                        columnName) => Convert.ToDecimal(rawData)));

            bindings.Put(
                Boolean,
                new ProxyDatabaseTypeBinding<bool>(
                    (
                        rawData,
                        columnName) => Convert.ToBoolean(rawData)));

            bindings.Put(
                Byte,
                new ProxyDatabaseTypeBinding<byte>(
                    (
                        rawData,
                        columnName) => Convert.ToByte(rawData)));

            bindings.Put(
                ByteArray,
                new ProxyDatabaseTypeBinding<byte[]>(
                    (
                        rawData,
                        columnName) => Convert.ChangeType(rawData, typeof(byte[]))));

            bindings.Put(
                Double,
                new ProxyDatabaseTypeBinding<double>(
                    (
                        rawData,
                        columnName) => Convert.ToDouble(rawData)));

            bindings.Put(
                Float,
                new ProxyDatabaseTypeBinding<float>(
                    (
                        rawData,
                        columnName) => Convert.ToSingle(rawData)));


            bindings.Put(
                Int,
                new ProxyDatabaseTypeBinding<int>(
                    (
                        rawData,
                        columnName) => Convert.ToInt32(rawData)));

            bindings.Put(
                Long,
                new ProxyDatabaseTypeBinding<long>(
                    (
                        rawData,
                        columnName) => Convert.ToInt64(rawData)));

            bindings.Put(
                Short,
                new ProxyDatabaseTypeBinding<short>(
                    (
                        rawData,
                        columnName) => Convert.ToInt16(rawData)));

            //bindings.Put(
            //    SqlDate,
            //    new ProxyDatabaseTypeBinding<DateTimeUtil>(
            //        delegate(Object rawData, String columnName)
            //            {
            //                return Convert.ToDateTime(rawData);
            //            }));

            //bindings.Put(
            //    SqlTime,
            //    new ProxyDatabaseTypeBinding<DateTimeUtil>(
            //        delegate(Object rawData, String columnName)
            //            {
            //                return Convert.ToDateTime(rawData);
            //            }));

            //bindings.Put(
            //    SqlTimestamp,
            //    new ProxyDatabaseTypeBinding<DateTimeUtil>(
            //        delegate(Object rawData, String columnName)
            //            {
            //                return Convert.ToDateTime(rawData);
            //            }));
        }

        private DatabaseTypeEnum(Type type)
        {
            DataType = type;
            BoxedType = type.GetBoxedType();
        }

        /// <summary>Retuns the type for the name.</summary>
        public Type DataType { get; }

        /// <summary>
        ///     Gets the boxed data type.
        /// </summary>
        /// <value>The type of the boxed.</value>
        public Type BoxedType { get; }

        /// <summary>
        ///     Returns the binding for this enumeration value for
        ///     reading the database result set and returning the right type.
        /// </summary>
        /// <value>The binding.</value>
        /// <returns>mapping of output column type to built-in</returns>
        public DatabaseTypeBinding Binding => bindings.Get(this);

        /// <summary>
        ///     Given a type name, matches for simple and fully-qualified type name (case-insensitive)
        ///     as well as case-insensitive type name.
        /// </summary>
        /// <param name="type">is the named type</param>
        /// <returns>type enumeration value for type</returns>
        public static DatabaseTypeEnum GetEnum(string type)
        {
            var boxedType = TypeHelper.GetBoxedTypeName(type).ToLower();
            var sourceName1 = boxedType.ToLower();

            foreach (var val in Values) {
                var targetName1 = val.BoxedType.FullName.ToLower();
                if (targetName1 == sourceName1) {
                    return val;
                }

                var targetName2 = val.DataType.FullName.ToLower();
                if (targetName2 == sourceName1) {
                    return val;
                }

                if (targetName2 == boxedType) {
                    return val;
                }

                var targetName3 = val.DataType.Name;
                if (targetName3 == boxedType) {
                    return val;
                }
            }

            return null;
        }
    }
} // End of namespace