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
    public class DatabaseTypeEnum
    {
        public static readonly IDictionary<DatabaseTypeEnum, DatabaseTypeBinding> BINDINGS;

        /// <summary>Boolean type.</summary>
        public static readonly DatabaseTypeEnum BOOLEAN = new DatabaseTypeEnum(typeof(bool));

        /// <summary>Byte type.</summary>
        public static readonly DatabaseTypeEnum BYTE = new DatabaseTypeEnum(typeof(byte));

        /// <summary>Byte array type.</summary>
        public static readonly DatabaseTypeEnum BYTE_ARRAY = new DatabaseTypeEnum(typeof(byte[]));

        /// <summary>Big decimal.</summary>
        public static readonly DatabaseTypeEnum DECIMAL = new DatabaseTypeEnum(typeof(decimal));

        /// <summary>Double type.</summary>
        public static readonly DatabaseTypeEnum DOUBLE = new DatabaseTypeEnum(typeof(double));

        /// <summary>Float type.</summary>
        public static readonly DatabaseTypeEnum FLOAT = new DatabaseTypeEnum(typeof(float));

        /// <summary>Integer type.</summary>
        public static readonly DatabaseTypeEnum INT32 = new DatabaseTypeEnum(typeof(int));

        /// <summary>Long type.</summary>
        public static readonly DatabaseTypeEnum INT64 = new DatabaseTypeEnum(typeof(long));

        /// <summary>Short type.</summary>
        public static readonly DatabaseTypeEnum INT16 = new DatabaseTypeEnum(typeof(short));

        /// <summary>String type.</summary>
        public static readonly DatabaseTypeEnum STRING = new DatabaseTypeEnum(typeof(string));

        /// <summary>timestamp type.</summary>
        public static readonly DatabaseTypeEnum TIMESTAMP = new DatabaseTypeEnum(typeof(DateTime));

        public static readonly DatabaseTypeEnum[] VALUES = {
            STRING,
            DECIMAL,
            DOUBLE,
            FLOAT,
            INT64,
            INT32,
            INT16,
            BOOLEAN,
            BYTE,
            BYTE_ARRAY,
            TIMESTAMP
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

        static DatabaseTypeEnum()
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
            BINDINGS.Put(STRING, InstanceBindingString);
            BINDINGS.Put(DECIMAL, InstanceBindingDecimal);
            BINDINGS.Put(DOUBLE, InstanceBindingDouble);
            BINDINGS.Put(FLOAT, InstanceBindingFloat);
            BINDINGS.Put(INT64, InstanceBindingInt64);
            BINDINGS.Put(INT32, InstanceBindingInt32);
            BINDINGS.Put(INT16, InstanceBindingInt16);
            BINDINGS.Put(BYTE, InstanceBindingByte);
            BINDINGS.Put(BYTE_ARRAY, InstanceBindingByteArray);
            BINDINGS.Put(BOOLEAN, InstanceBindingBoolean);

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
        public DatabaseTypeBinding Binding => BINDINGS.Get(this);

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

            foreach (var val in VALUES) {
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