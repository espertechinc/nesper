///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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
    public enum DatabaseTypeEnum
    {
        /// <summary>Boolean type.</summary>
        BOOLEAN,

        /// <summary>Byte type.</summary>
        BYTE,

        /// <summary>Byte array type.</summary>
        BYTE_ARRAY,

        /// <summary>Big decimal.</summary>
        DECIMAL,

        /// <summary>Double type.</summary>
        DOUBLE,

        /// <summary>Float type.</summary>
        FLOAT,

        /// <summary>Integer type.</summary>
        INT32,

        /// <summary>Long type.</summary>
        INT64,

        /// <summary>Short type.</summary>
        INT16,

        /// <summary>String type.</summary>
        STRING,

        /// <summary>timestamp type.</summary>
        TIMESTAMP
    }

    public static class DatabaseTypeEnumExtensions
    {
        public static readonly IDictionary<DatabaseTypeEnum, DatabaseTypeBinding> BINDINGS;

        public static readonly DatabaseTypeEnum[] VALUES = {
            DatabaseTypeEnum.STRING,
            DatabaseTypeEnum.DECIMAL,
            DatabaseTypeEnum.DOUBLE,
            DatabaseTypeEnum.FLOAT,
            DatabaseTypeEnum.INT64,
            DatabaseTypeEnum.INT32,
            DatabaseTypeEnum.INT16,
            DatabaseTypeEnum.BOOLEAN,
            DatabaseTypeEnum.BYTE,
            DatabaseTypeEnum.BYTE_ARRAY,
            DatabaseTypeEnum.TIMESTAMP
        };

        public static ProxyDatabaseTypeBinding<string> InstanceBindingString;
        public static ProxyDatabaseTypeBinding<decimal> InstanceBindingDecimal;
        public static ProxyDatabaseTypeBinding<double> InstanceBindingDouble;
        public static ProxyDatabaseTypeBinding<float> InstanceBindingFloat;
        public static ProxyDatabaseTypeBinding<long> InstanceBindingInt64;
        public static ProxyDatabaseTypeBinding<int> InstanceBindingInt32;
        public static ProxyDatabaseTypeBinding<short> InstanceBindingInt16;
        public static ProxyDatabaseTypeBinding<bool> InstanceBindingBoolean;
        public static ProxyDatabaseTypeBinding<byte> InstanceBindingByte;
        public static ProxyDatabaseTypeBinding<byte[]> InstanceBindingByteArray;
        public static ProxyDatabaseTypeBinding<byte[]> InstanceBindingTimestamp;

        static DatabaseTypeEnumExtensions()
        {
            InstanceBindingString = new ProxyDatabaseTypeBinding<string>(
                (
                    rawData,
                    columnName) => Convert.ToString(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingString"));

            InstanceBindingDecimal = new ProxyDatabaseTypeBinding<decimal>(
                (
                    rawData,
                    columnName) => Convert.ToDecimal(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingDecimal"));
            InstanceBindingDouble = new ProxyDatabaseTypeBinding<double>(
                (
                    rawData,
                    columnName) => Convert.ToDouble(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingDouble"));
            InstanceBindingFloat = new ProxyDatabaseTypeBinding<float>(
                (
                    rawData,
                    columnName) => Convert.ToSingle(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingFloat"));

            InstanceBindingInt64 = new ProxyDatabaseTypeBinding<long>(
                (
                    rawData,
                    columnName) => Convert.ToInt64(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingInt64"));
            InstanceBindingInt32 = new ProxyDatabaseTypeBinding<int>(
                (
                    rawData,
                    columnName) => Convert.ToInt32(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingInt32"));
            InstanceBindingInt16 = new ProxyDatabaseTypeBinding<short>(
                (
                    rawData,
                    columnName) => Convert.ToInt16(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingInt16"));


            InstanceBindingByte = new ProxyDatabaseTypeBinding<byte>(
                (
                    rawData,
                    columnName) => Convert.ToByte(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingByte"));
            InstanceBindingByteArray = new ProxyDatabaseTypeBinding<byte[]>(
                (
                    rawData,
                    columnName) => Convert.ChangeType(rawData, typeof(byte[])),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingByteArray"));

            InstanceBindingBoolean = new ProxyDatabaseTypeBinding<bool>(
                (
                    rawData,
                    columnName) => Convert.ToBoolean(rawData),
                () => CodegenExpressionBuilder.PublicConstValue(typeof(DatabaseTypeEnum), "InstanceBindingBoolean"));

            BINDINGS = new Dictionary<DatabaseTypeEnum, DatabaseTypeBinding>();
            BINDINGS.Put(DatabaseTypeEnum.STRING, InstanceBindingString);
            BINDINGS.Put(DatabaseTypeEnum.DECIMAL, InstanceBindingDecimal);
            BINDINGS.Put(DatabaseTypeEnum.DOUBLE, InstanceBindingDouble);
            BINDINGS.Put(DatabaseTypeEnum.FLOAT, InstanceBindingFloat);
            BINDINGS.Put(DatabaseTypeEnum.INT64, InstanceBindingInt64);
            BINDINGS.Put(DatabaseTypeEnum.INT32, InstanceBindingInt32);
            BINDINGS.Put(DatabaseTypeEnum.INT16, InstanceBindingInt16);
            BINDINGS.Put(DatabaseTypeEnum.BYTE, InstanceBindingByte);
            BINDINGS.Put(DatabaseTypeEnum.BYTE_ARRAY, InstanceBindingByteArray);
            BINDINGS.Put(DatabaseTypeEnum.BOOLEAN, InstanceBindingBoolean);

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

        /// <summary>
        ///     Given a type name, matches for simple and fully-qualified type name (case-insensitive)
        ///     as well as case-insensitive type name.
        /// </summary>
        /// <param name="type">is the named type</param>
        /// <returns>type enumeration value for type</returns>
        public static DatabaseTypeEnum GetEnum(string type)
        {
            var sourceName1 = type.ToLowerInvariant();

            foreach (var val in VALUES)
            {
                var targetName1 = val.GetName().ToLowerInvariant();
                if (targetName1 == sourceName1)
                {
                    return val;
                }

                var dataType = val.GetDataType();
                if (dataType != null) {
                    if ((sourceName1 == dataType.FullName?.ToLowerInvariant()) ||
                        (sourceName1 == dataType.GetBoxedType().FullName?.ToLowerInvariant())) {
                        return val;
                    }
                }
            }

            throw new ArgumentException($"unable to find a value for type \"{type}\"", nameof(type));
        }

        public static Type GetDataType(this DatabaseTypeEnum value)
        {
            switch (value)
            {
                case DatabaseTypeEnum.BOOLEAN:
                    return typeof(bool);
                case DatabaseTypeEnum.BYTE:
                    return typeof(byte);
                case DatabaseTypeEnum.BYTE_ARRAY:
                    return typeof(byte[]);
                case DatabaseTypeEnum.DECIMAL:
                    return typeof(decimal);
                case DatabaseTypeEnum.DOUBLE:
                    return typeof(double);
                case DatabaseTypeEnum.FLOAT:
                    return typeof(float);
                case DatabaseTypeEnum.INT32:
                    return typeof(int);
                case DatabaseTypeEnum.INT64:
                    return typeof(long);
                case DatabaseTypeEnum.INT16:
                    return typeof(short);
                case DatabaseTypeEnum.STRING:
                    return typeof(string);
                case DatabaseTypeEnum.TIMESTAMP:
                    return typeof(DateTime);
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static Type GetBoxedType(this DatabaseTypeEnum value)
        {
            switch (value)
            {
                case DatabaseTypeEnum.BOOLEAN:
                    return typeof(bool?);
                case DatabaseTypeEnum.BYTE:
                    return typeof(byte?);
                case DatabaseTypeEnum.BYTE_ARRAY:
                    return typeof(byte[]);
                case DatabaseTypeEnum.DECIMAL:
                    return typeof(decimal?);
                case DatabaseTypeEnum.DOUBLE:
                    return typeof(double?);
                case DatabaseTypeEnum.FLOAT:
                    return typeof(float?);
                case DatabaseTypeEnum.INT32:
                    return typeof(int?);
                case DatabaseTypeEnum.INT64:
                    return typeof(long?);
                case DatabaseTypeEnum.INT16:
                    return typeof(short?);
                case DatabaseTypeEnum.STRING:
                    return typeof(string);
                case DatabaseTypeEnum.TIMESTAMP:
                    return typeof(DateTime?);
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        /// <summary>
        ///     Returns the binding for this enumeration value for
        ///     reading the database result set and returning the right type.
        /// </summary>
        /// <returns>The binding.</returns>
        /// <returns>mapping of output column type to built-in</returns>
        public static DatabaseTypeBinding GetBinding(this DatabaseTypeEnum value)
        {
            return BINDINGS.Get(value);
        }
    }
} // End of namespace