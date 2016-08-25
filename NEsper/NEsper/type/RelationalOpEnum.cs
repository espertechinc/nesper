///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Enum representing relational types of operation.
    /// </summary>

    public enum RelationalOpEnum
    {
        /// <summary>Greater then.</summary>
        GT,
        /// <summary>Greater equals.</summary>
        GE,
        /// <summary>Lesser then.</summary>
        LT,
        /// <summary>Lesser equals.</summary>
        LE
    }

    public static class RelationalOpEnumExtensions
    {
        public static string GetExpressionText(this RelationalOpEnum value)
        {
            switch(value)
            {
                case RelationalOpEnum.GT:
                    return ">";
                case RelationalOpEnum.GE:
                    return ">=";
                case RelationalOpEnum.LT:
                    return "<";
                case RelationalOpEnum.LE:
                    return "<=";
            }

            throw new ArgumentException("invalid value", "value");
        }

        public static int GetOrdinal(this RelationalOpEnum value)
        {
            return (int) value;
        }

        private static readonly IDictionary<MultiKeyUntyped, Computer> Computers;

        /// <summary>
        /// Reverses this instance.
        /// </summary>
        /// <returns></returns>
        public static RelationalOpEnum Reversed(this RelationalOpEnum value)
        {
            switch(value)
            {
                case RelationalOpEnum.GT:
                    return RelationalOpEnum.LT;
                case RelationalOpEnum.GE:
                    return RelationalOpEnum.LE;
                case RelationalOpEnum.LT:
                    return RelationalOpEnum.GT;
                case RelationalOpEnum.LE:
                    return RelationalOpEnum.GE;
            }

            throw new ArgumentException("invalid value", "value");
        }

        /// <summary>
        /// Parses the operator and returns an enum for the operator.
        /// </summary>
        /// <param name="op">operand to parse</param>
        /// <returns>enum representing relational operation</returns>
        public static RelationalOpEnum Parse(string op)
        {
            switch (op)
            {
                case "<":
                    return RelationalOpEnum.LT;
                case ">":
                    return RelationalOpEnum.GT;
                case ">=":
                case "=>":
                    return RelationalOpEnum.GE;
                case "<=":
                case "=<":
                    return RelationalOpEnum.LE;
            }

            throw new ArgumentException("Invalid relational operator '" + op + "'");
        }

        static RelationalOpEnumExtensions()
        {
            Computers = new Dictionary<MultiKeyUntyped, Computer>();
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(string), RelationalOpEnum.GT }), GTStringComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(string), RelationalOpEnum.GE }), GEStringComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(string), RelationalOpEnum.LT }), LTStringComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(string), RelationalOpEnum.LE }), LEStringComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTime), RelationalOpEnum.GT }), GTDateTimeComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTime), RelationalOpEnum.GE }), GEDateTimeComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTime), RelationalOpEnum.LT }), LTDateTimeComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTime), RelationalOpEnum.LE }), LEDateTimeComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTimeOffset), RelationalOpEnum.GT }), GTDateTimeOffsetComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTimeOffset), RelationalOpEnum.GE }), GEDateTimeOffsetComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTimeOffset), RelationalOpEnum.LT }), LTDateTimeOffsetComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(DateTimeOffset), RelationalOpEnum.LE }), LEDateTimeOffsetComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short), RelationalOpEnum.GT }), GTInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short), RelationalOpEnum.GE }), GEInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short), RelationalOpEnum.LT }), LTInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(short), RelationalOpEnum.LE }), LEInt16Computer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), RelationalOpEnum.GT }), GTInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), RelationalOpEnum.GE }), GEInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), RelationalOpEnum.LT }), LTInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), RelationalOpEnum.LE }), LEInt32Computer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ushort), RelationalOpEnum.GT }), GTUInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ushort), RelationalOpEnum.GE }), GEUInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ushort), RelationalOpEnum.LT }), LTUInt16Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ushort), RelationalOpEnum.LE }), LEUInt16Computer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), RelationalOpEnum.GT }), GTUInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), RelationalOpEnum.GE }), GEUInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), RelationalOpEnum.LT }), LTUInt32Computer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), RelationalOpEnum.LE }), LEUInt32Computer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), RelationalOpEnum.GT }), GTLongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), RelationalOpEnum.GE }), GELongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), RelationalOpEnum.LT }), LTLongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), RelationalOpEnum.LE }), LELongComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), RelationalOpEnum.GT }), GTULongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), RelationalOpEnum.GE }), GEULongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), RelationalOpEnum.LT }), LTULongComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), RelationalOpEnum.LE }), LEULongComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), RelationalOpEnum.GT }), GTSingleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), RelationalOpEnum.GE }), GESingleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), RelationalOpEnum.LT }), LTSingleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), RelationalOpEnum.LE }), LESingleComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), RelationalOpEnum.GT }), GTDoubleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), RelationalOpEnum.GE }), GEDoubleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), RelationalOpEnum.LT }), LTDoubleComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), RelationalOpEnum.LE }), LEDoubleComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), RelationalOpEnum.GT }), GTDecimalComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), RelationalOpEnum.GE }), GEDecimalComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), RelationalOpEnum.LT }), LTDecimalComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), RelationalOpEnum.LE }), LEDecimalComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), RelationalOpEnum.GT }), GTBigIntegerComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), RelationalOpEnum.GE }), GEBigIntegerComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), RelationalOpEnum.LT }), LTBigIntegerComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), RelationalOpEnum.LE }), LEBigIntegerComputer);

            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(IComparable), RelationalOpEnum.GT }), GTComparableComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(IComparable), RelationalOpEnum.GE }), GEComparableComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(IComparable), RelationalOpEnum.LT }), LTComparableComputer);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(IComparable), RelationalOpEnum.LE }), LEComparableComputer);
        }

        /// <summary>
        /// Returns the computer to use for the relational operation based on the
        /// coercion type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="coercedType">is the object type</param>
        /// <param name="typeOne">The type one.</param>
        /// <param name="typeTwo">The type two.</param>
        /// <returns>
        /// computer for performing the relational op
        /// </returns>

        public static Computer GetComputer(this RelationalOpEnum value, Type coercedType, Type typeOne, Type typeTwo)
        {
            var t = Nullable.GetUnderlyingType(coercedType);
            if (t != null) {
                coercedType = t;
            }

            var key = new MultiKeyUntyped(new Object[] { coercedType, value });
            var computer = Computers.Get(key, null);
            if (computer == null)
            {
                if (typeOne.IsComparable() && typeTwo.IsComparable())
                {
                    key = new MultiKeyUntyped(new object[]{ typeof(IComparable), value });
                    computer = Computers.Get(key, null);
                    if (computer != null)
                    {
                        return computer;
                    }
                }

                throw new ArgumentException("Unsupported type for relational op compare, type " + coercedType);
            }

            return computer;
        }

        /// <summary>
        /// Delegate for computing a relational operation on two objects.
        /// </summary>
        /// <param name="objOne"></param>
        /// <param name="objTwo"></param>
        /// <returns></returns>

        public delegate Boolean Computer(Object objOne, Object objTwo);

        #region String
        /// <summary>
        /// Greater than string computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTStringComputer(Object objOne, Object objTwo)
        {
            var s1 = (String)objOne;
            var s2 = (String)objTwo;
            int result = s1.CompareTo(s2);
            return result > 0;
        }

        /// <summary>
        /// Greater-than or equal to string computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEStringComputer(Object objOne, Object objTwo)
        {
            var s1 = (String)objOne;
            var s2 = (String)objTwo;
            return s1.CompareTo(s2) >= 0;
        }

        /// <summary>
        /// Less-than or equal to string computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEStringComputer(Object objOne, Object objTwo)
        {
            var s1 = (String)objOne;
            var s2 = (String)objTwo;
            return s1.CompareTo(s2) <= 0;
        }

        /// <summary>
        /// Less-than string computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTStringComputer(Object objOne, Object objTwo)
        {
            var s1 = (String)objOne;
            var s2 = (String)objTwo;
            return s1.CompareTo(s2) < 0;
        }
        #endregion

        #region Int16
        /// <summary>
        /// Greater-than int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTInt16Computer(Object objOne, Object objTwo)
        {
            Int16 s1 = Convert.ToInt16(objOne);
            Int16 s2 = Convert.ToInt16(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEInt16Computer(Object objOne, Object objTwo)
        {
            Int16 s1 = Convert.ToInt16(objOne);
            Int16 s2 = Convert.ToInt16(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTInt16Computer(Object objOne, Object objTwo)
        {
            Int16 s1 = Convert.ToInt16(objOne);
            Int16 s2 = Convert.ToInt16(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEInt16Computer(Object objOne, Object objTwo)
        {
            Int16 s1 = Convert.ToInt16(objOne);
            Int16 s2 = Convert.ToInt16(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region UInt16
        /// <summary>
        /// Greater-than int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTUInt16Computer(Object objOne, Object objTwo)
        {
            UInt16 s1 = Convert.ToUInt16(objOne);
            UInt16 s2 = Convert.ToUInt16(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEUInt16Computer(Object objOne, Object objTwo)
        {
            UInt16 s1 = Convert.ToUInt16(objOne);
            UInt16 s2 = Convert.ToUInt16(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTUInt16Computer(Object objOne, Object objTwo)
        {
            UInt16 s1 = Convert.ToUInt16(objOne);
            UInt16 s2 = Convert.ToUInt16(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to int16 computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEUInt16Computer(Object objOne, Object objTwo)
        {
            UInt16 s1 = Convert.ToUInt16(objOne);
            UInt16 s2 = Convert.ToUInt16(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region Int32
        /// <summary>
        /// Greater-than int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTInt32Computer(Object objOne, Object objTwo)
        {
            Int32 s1 = Convert.ToInt32(objOne);
            Int32 s2 = Convert.ToInt32(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEInt32Computer(Object objOne, Object objTwo)
        {
            Int32 s1 = Convert.ToInt32(objOne);
            Int32 s2 = Convert.ToInt32(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTInt32Computer(Object objOne, Object objTwo)
        {
            Int32 s1 = Convert.ToInt32(objOne);
            Int32 s2 = Convert.ToInt32(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEInt32Computer(Object objOne, Object objTwo)
        {
            Int32 s1 = Convert.ToInt32(objOne);
            Int32 s2 = Convert.ToInt32(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Greater-than int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTUInt32Computer(Object objOne, Object objTwo)
        {
            UInt32 s1 = Convert.ToUInt32(objOne);
            UInt32 s2 = Convert.ToUInt32(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEUInt32Computer(Object objOne, Object objTwo)
        {
            UInt32 s1 = Convert.ToUInt32(objOne);
            UInt32 s2 = Convert.ToUInt32(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTUInt32Computer(Object objOne, Object objTwo)
        {
            UInt32 s1 = Convert.ToUInt32(objOne);
            UInt32 s2 = Convert.ToUInt32(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to int computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEUInt32Computer(Object objOne, Object objTwo)
        {
            UInt32 s1 = Convert.ToUInt32(objOne);
            UInt32 s2 = Convert.ToUInt32(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region Int64
        /// <summary>
        /// Greater-than long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTLongComputer(Object objOne, Object objTwo)
        {
            Int64 s1 = Convert.ToInt64(objOne);
            Int64 s2 = Convert.ToInt64(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GELongComputer(Object objOne, Object objTwo)
        {
            Int64 s1 = Convert.ToInt64(objOne);
            Int64 s2 = Convert.ToInt64(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTLongComputer(Object objOne, Object objTwo)
        {
            Int64 s1 = Convert.ToInt64(objOne);
            Int64 s2 = Convert.ToInt64(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LELongComputer(Object objOne, Object objTwo)
        {
            Int64 s1 = Convert.ToInt64(objOne);
            Int64 s2 = Convert.ToInt64(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Greater-than unsigned long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTULongComputer(Object objOne, Object objTwo)
        {
            UInt64 s1 = Convert.ToUInt64(objOne);
            UInt64 s2 = Convert.ToUInt64(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to unsigned long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEULongComputer(Object objOne, Object objTwo)
        {
            UInt64 s1 = Convert.ToUInt64(objOne);
            UInt64 s2 = Convert.ToUInt64(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than unsigned long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTULongComputer(Object objOne, Object objTwo)
        {
            UInt64 s1 = Convert.ToUInt64(objOne);
            UInt64 s2 = Convert.ToUInt64(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to unsigned long computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEULongComputer(Object objOne, Object objTwo)
        {
            UInt64 s1 = Convert.ToUInt64(objOne);
            UInt64 s2 = Convert.ToUInt64(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region Float
        /// <summary>
        /// Greater-than float computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTSingleComputer(Object objOne, Object objTwo)
        {
            float s1 = Convert.ToSingle(objOne);
            float s2 = Convert.ToSingle(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to float computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GESingleComputer(Object objOne, Object objTwo)
        {
            float s1 = Convert.ToSingle(objOne);
            float s2 = Convert.ToSingle(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than float computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTSingleComputer(Object objOne, Object objTwo)
        {
            float s1 = Convert.ToSingle(objOne);
            float s2 = Convert.ToSingle(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to float computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LESingleComputer(Object objOne, Object objTwo)
        {
            float s1 = Convert.ToSingle(objOne);
            float s2 = Convert.ToSingle(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region Double
        /// <summary>
        /// Greater-than double computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTDoubleComputer(Object objOne, Object objTwo)
        {
            double s1 = Convert.ToDouble(objOne);
            double s2 = Convert.ToDouble(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to double computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEDoubleComputer(Object objOne, Object objTwo)
        {
            double s1 = Convert.ToDouble(objOne);
            double s2 = Convert.ToDouble(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than double computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTDoubleComputer(Object objOne, Object objTwo)
        {
            double s1 = Convert.ToDouble(objOne);
            double s2 = Convert.ToDouble(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to double computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEDoubleComputer(Object objOne, Object objTwo)
        {
            double s1 = Convert.ToDouble(objOne);
            double s2 = Convert.ToDouble(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region Decimal
        /// <summary>
        /// Greater-than decimal computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTDecimalComputer(Object objOne, Object objTwo)
        {
            decimal s1 = Convert.ToDecimal(objOne);
            decimal s2 = Convert.ToDecimal(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to decimal computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEDecimalComputer(Object objOne, Object objTwo)
        {
            decimal s1 = Convert.ToDecimal(objOne);
            decimal s2 = Convert.ToDecimal(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than decimal computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTDecimalComputer(Object objOne, Object objTwo)
        {
            decimal s1 = Convert.ToDecimal(objOne);
            decimal s2 = Convert.ToDecimal(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to decimal computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEDecimalComputer(Object objOne, Object objTwo)
        {
            decimal s1 = Convert.ToDecimal(objOne);
            decimal s2 = Convert.ToDecimal(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Greater-than datetime computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTDateTimeComputer(Object objOne, Object objTwo)
        {
            DateTime s1 = Convert.ToDateTime(objOne);
            DateTime s2 = Convert.ToDateTime(objTwo);
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to datetime computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEDateTimeComputer(Object objOne, Object objTwo)
        {
            DateTime s1 = Convert.ToDateTime(objOne);
            DateTime s2 = Convert.ToDateTime(objTwo);
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than datetime computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTDateTimeComputer(Object objOne, Object objTwo)
        {
            DateTime s1 = Convert.ToDateTime(objOne);
            DateTime s2 = Convert.ToDateTime(objTwo);
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to datetime computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEDateTimeComputer(Object objOne, Object objTwo)
        {
            DateTime s1 = Convert.ToDateTime(objOne);
            DateTime s2 = Convert.ToDateTime(objTwo);
            return s1 <= s2;
        }
        #endregion

        #region DateTimeOffset
        /// <summary>
        /// Greater-than datetime offset computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTDateTimeOffsetComputer(Object objOne, Object objTwo)
        {
            DateTimeOffset s1 = objOne.AsDateTimeOffset();
            DateTimeOffset s2 = objTwo.AsDateTimeOffset();
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to datetime offset computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEDateTimeOffsetComputer(Object objOne, Object objTwo)
        {
            DateTimeOffset s1 = objOne.AsDateTimeOffset();
            DateTimeOffset s2 = objTwo.AsDateTimeOffset();
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than datetime offset computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTDateTimeOffsetComputer(Object objOne, Object objTwo)
        {
            DateTimeOffset s1 = objOne.AsDateTimeOffset();
            DateTimeOffset s2 = objTwo.AsDateTimeOffset();
            return s1 < s2;
        }

        /// <summary>
        /// Less-than or equal to datetime offset computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEDateTimeOffsetComputer(Object objOne, Object objTwo)
        {
            DateTimeOffset s1 = objOne.AsDateTimeOffset();
            DateTimeOffset s2 = objTwo.AsDateTimeOffset();
            return s1 <= s2;
        }
        #endregion

        #region BigInteger
        /// <summary>
        /// Greater than BigInteger computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTBigIntegerComputer(Object objOne, Object objTwo)
        {
            var s1 = objOne.AsBigInteger();
            var s2 = objTwo.AsBigInteger();
            return s1 > s2;
        }

        /// <summary>
        /// Greater-than or equal to BigInteger computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEBigIntegerComputer(Object objOne, Object objTwo)
        {
            var s1 = objOne.AsBigInteger();
            var s2 = objTwo.AsBigInteger();
            return s1 >= s2;
        }

        /// <summary>
        /// Less-than or equal to BigInteger computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEBigIntegerComputer(Object objOne, Object objTwo)
        {
            var s1 = objOne.AsBigInteger();
            var s2 = objTwo.AsBigInteger();
            return s1 <= s2;
        }

        /// <summary>
        /// Less-than BigInteger computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTBigIntegerComputer(Object objOne, Object objTwo)
        {
            var s1 = objOne.AsBigInteger();
            var s2 = objTwo.AsBigInteger();
            return s1 < s2;
        }
        #endregion

        #region IComparable
        /// <summary>
        /// Greater than IComparable computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GTComparableComputer(Object objOne, Object objTwo)
        {
            var s1 = (IComparable)objOne;
            var s2 = (IComparable)objTwo;
            int result = s1.CompareTo(s2);
            return result > 0;
        }

        /// <summary>
        /// Greater-than or equal to IComparable computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool GEComparableComputer(Object objOne, Object objTwo)
        {
            var s1 = (IComparable)objOne;
            var s2 = (IComparable)objTwo;
            return s1.CompareTo(s2) >= 0;
        }

        /// <summary>
        /// Less-than or equal to IComparable computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LEComparableComputer(Object objOne, Object objTwo)
        {
            var s1 = (IComparable)objOne;
            var s2 = (IComparable)objTwo;
            return s1.CompareTo(s2) <= 0;
        }

        /// <summary>
        /// Less-than IComparable computer.
        /// </summary>
        /// <param name="objOne">The obj one.</param>
        /// <param name="objTwo">The obj two.</param>
        /// <returns></returns>
        public static bool LTComparableComputer(Object objOne, Object objTwo)
        {
            var s1 = (IComparable)objOne;
            var s2 = (IComparable)objTwo;
            return s1.CompareTo(s2) < 0;
        }
        #endregion

    }
}
