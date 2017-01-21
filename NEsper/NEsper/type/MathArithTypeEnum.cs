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

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.type
{
    public enum MathArithTypeEnum
    {
        /// <summary>
        /// Plus.
        /// </summary>
        ADD,
        /// <summary>
        /// Minus
        /// </summary>
        SUBTRACT,
        /// <summary>
        /// Divide
        /// </summary>
        DIVIDE,
        /// <summary>
        /// Multiply.
        /// </summary>
        MULTIPLY,
        /// <summary>
        /// Modulo.
        /// </summary>
        MODULO
    }

    /// <summary>
    /// Enumeration for the type of arithmatic to use.
    /// </summary>

    [Serializable]
    public static class MathArithTypeEnumExtensions
    {
        private static readonly IDictionary<MultiKeyUntyped, Computer> Computers;

        static MathArithTypeEnumExtensions()
        {
            Computers = new Dictionary<MultiKeyUntyped, Computer>();
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), MathArithTypeEnum.ADD }), AddBigInteger);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), MathArithTypeEnum.ADD }), AddDecimal);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), MathArithTypeEnum.ADD }), AddDouble);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), MathArithTypeEnum.ADD }), AddSingle);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), MathArithTypeEnum.ADD }), AddInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), MathArithTypeEnum.ADD }), AddInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), MathArithTypeEnum.ADD }), AddUInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), MathArithTypeEnum.ADD }), AddUInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), MathArithTypeEnum.SUBTRACT }), SubtractBigInteger);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), MathArithTypeEnum.SUBTRACT }), SubtractDecimal);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), MathArithTypeEnum.SUBTRACT }), SubtractDouble);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), MathArithTypeEnum.SUBTRACT }), SubtractSingle);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), MathArithTypeEnum.SUBTRACT }), SubtractInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), MathArithTypeEnum.SUBTRACT }), SubtractInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), MathArithTypeEnum.SUBTRACT }), SubtractUInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), MathArithTypeEnum.SUBTRACT }), SubtractUInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), MathArithTypeEnum.MULTIPLY }), MultiplyBigInteger);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), MathArithTypeEnum.MULTIPLY }), MultiplyDecimal);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), MathArithTypeEnum.MULTIPLY }), MultiplyDouble);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), MathArithTypeEnum.MULTIPLY }), MultiplySingle);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), MathArithTypeEnum.MULTIPLY }), MultiplyInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), MathArithTypeEnum.MULTIPLY }), MultiplyInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), MathArithTypeEnum.MULTIPLY }), MultiplyUInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), MathArithTypeEnum.MULTIPLY }), MultiplyUInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(BigInteger), MathArithTypeEnum.MODULO }), ModuloBigInteger);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(decimal), MathArithTypeEnum.MODULO }), ModuloDecimal);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(double), MathArithTypeEnum.MODULO }), ModuloDouble);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(float), MathArithTypeEnum.MODULO }), ModuloSingle);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(long), MathArithTypeEnum.MODULO }), ModuloInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(int), MathArithTypeEnum.MODULO }), ModuloInt32);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(ulong), MathArithTypeEnum.MODULO }), ModuloUInt64);
            Computers.Add(new MultiKeyUntyped(new Object[] { typeof(uint), MathArithTypeEnum.MODULO }), ModuloUInt32);
        }

        /// <summary>Returns string representation of enum.</summary>
        /// <returns>text for enum</returns>

        public static String GetExpressionText(this MathArithTypeEnum value)
        {
            switch(value)
            {
                case MathArithTypeEnum.ADD:
                    return "+";
                case MathArithTypeEnum.SUBTRACT:
                    return "-";
                case MathArithTypeEnum.DIVIDE:
                    return "/";
                case MathArithTypeEnum.MULTIPLY:
                    return "*";
                case MathArithTypeEnum.MODULO:
                    return "%";
            }

            throw new ArgumentException("invalid value", "value");
        }

        /// <summary>
        /// Returns the math operator for the string.
        /// </summary>
        /// <param name="value">operator to parse</param>
        /// <returns>math enum</returns>
        public static MathArithTypeEnum ParseOperator(string value)
        {
            switch(value)
            {
                case "+":
                    return MathArithTypeEnum.ADD;
                case "-":
                    return MathArithTypeEnum.SUBTRACT;
                case "/":
                    return MathArithTypeEnum.DIVIDE;
                case "*":
                    return MathArithTypeEnum.MULTIPLY;
                case "%":
                    return MathArithTypeEnum.MODULO;
            }

            throw new ArgumentException("Unknown operator '" + value + "'");
        }

        /// <summary>
        /// Interface for number cruncher.
        /// </summary>

        public delegate Object Computer(Object d1, Object d2);

        /// <summary>
        /// Returns number cruncher for the target coercion type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="coercedType">target type</param>
        /// <param name="typeOne">the LHS type</param>
        /// <param name="typeTwo">the RHS type</param>
        /// <param name="isIntegerDivision">false for division returns double, true for using standard integer division</param>
        /// <param name="isDivisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
        /// <param name="optionalMathContext">The optional math context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Expected base numeric type for computation result but got type  +  coercedType
        /// or
        /// Could not determine process or type  + value +  type  + coercedType
        /// or
        /// Could not determine process or type  + value +  type  + coercedType
        /// </exception>
        public static Computer GetComputer(this MathArithTypeEnum value, Type coercedType, Type typeOne, Type typeTwo, bool isIntegerDivision, bool isDivisionByZeroReturnsNull, MathContext optionalMathContext)
        {
            Type t = Nullable.GetUnderlyingType(coercedType);
            if (t != null)
                coercedType = t;

            if ((coercedType != typeof (BigInteger)) && 
                (coercedType != typeof (decimal)) &&
                (coercedType != typeof (double)) &&
                (coercedType != typeof (float)) &&
                (coercedType != typeof (long)) &&
                (coercedType != typeof (int))) {
                throw new ArgumentException("Expected base numeric type for computation result but got type " + coercedType);
            }

            if (value != MathArithTypeEnum.DIVIDE) {
                var key = new MultiKeyUntyped(new Object[] {coercedType, value});
                var computer = Computers.Get(key);
                if (computer == null) {
                    throw new ArgumentException("Could not determine process or type " + value + " type " + coercedType);
                }
                return computer;
            }

            if (!isIntegerDivision) {
                if (coercedType == typeof(decimal))
                    return isDivisionByZeroReturnsNull ? (Computer)DivideDecimalChecked : DivideDecimalUnchecked;

                return isDivisionByZeroReturnsNull ? (Computer)DivideDoubleChecked : DivideDoubleUnchecked;
            }

            if (coercedType == typeof (BigInteger))
                return DivideBigInteger;
            if (coercedType == typeof (double))
                return isDivisionByZeroReturnsNull ? (Computer)DivideDoubleChecked : DivideDoubleUnchecked; 
            if (coercedType == typeof (float))
                return DivideSingle;
            if (coercedType == typeof (long))
                return DivideInt64;
            if (coercedType == typeof (int))
                return DivideInt32;
            if (coercedType == typeof (decimal))
                return isDivisionByZeroReturnsNull ? (Computer) DivideDecimalChecked : DivideDecimalUnchecked;

            throw new ArgumentException("Could not determine process or type " + value + " type " + coercedType);
        }

        /// <summary>
        /// Adds big integers.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddBigInteger(Object d1, Object d2)
        {
            var nd1 = d1.AsBigInteger();
            var nd2 = d2.AsBigInteger();
            return nd1 + nd2;
        }

        /// <summary>
        /// Adds decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddDecimal(Object d1, Object d2)
        {
            return d1.AsDecimal() + d2.AsDecimal();
        }

        /// <summary>
        /// Adds doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddDouble(Object d1, Object d2)
        {
            return d1.AsDouble() + d2.AsDouble();
        }

        /// <summary>
        /// Adds singles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddSingle(Object d1, Object d2)
        {
            return d1.AsFloat() + d2.AsFloat();
        }

        /// <summary>
        /// Adds int64s.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddInt64(Object d1, Object d2)
        {
            return d1.AsLong() + d2.AsLong();
        }

        /// <summary>
        /// Adds int32s.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddInt32(Object d1, Object d2)
        {
            return d1.AsInt() + d2.AsInt();
        }

        /// <summary>
        /// Adds unsigned int64s.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddUInt64(Object d1, Object d2)
        {
            return Convert.ToUInt64(d1) + Convert.ToUInt64(d2);
        }

        /// <summary>
        /// Adds unsigned 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object AddUInt32(Object d1, Object d2)
        {
            return Convert.ToUInt32(d1) + Convert.ToUInt32(d2);
        }

        /// <summary>
        /// Subtracts big integers.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractBigInteger(Object d1, Object d2)
        {
            var nd1 = d1.AsBigInteger();
            var nd2 = d2.AsBigInteger();
            return nd1 - nd2;
        }

        /// <summary>
        /// Subtracts decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractDecimal(Object d1, Object d2)
        {
            return d1.AsDecimal() - d2.AsDecimal();
        }

        /// <summary>
        /// Subtracts doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractDouble(Object d1, Object d2)
        {
            return d1.AsDouble() - d2.AsDouble();
        }

        /// <summary>
        /// Subtracts singles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractSingle(Object d1, Object d2)
        {
            return d1.AsFloat() - d2.AsFloat();
        }

        /// <summary>
        /// Subtracts 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractInt64(Object d1, Object d2)
        {
            return d1.AsLong() - d2.AsLong();
        }

        /// <summary>
        /// Subtracts 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractInt32(Object d1, Object d2)
        {
            return d1.AsInt() - d2.AsInt();
        }

        /// <summary>
        /// Subtracts unsigned 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractUInt64(Object d1, Object d2)
        {
            return Convert.ToUInt64(d1) - Convert.ToUInt64(d2);
        }

        /// <summary>
        /// Subtracts unsigned 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object SubtractUInt32(Object d1, Object d2)
        {
            return Convert.ToUInt32(d1) - Convert.ToUInt32(d2);
        }

        /// <summary>
        /// Divides big integers.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideBigInteger(Object d1, Object d2)
        {
            var nd1 = d1.AsBigInteger();
            var nd2 = d2.AsBigInteger();
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideDecimalChecked(Object d1, Object d2)
        {
            var nd1 = d1.AsDecimal();
            var nd2 = d2.AsDecimal();
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideDecimalUnchecked(Object d1, Object d2)
        {
            var nd1 = d1.AsDecimal();
            var nd2 = d2.AsDecimal();
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideDoubleUnchecked(Object d1, Object d2)
        {
            var nd1 = d1.AsDouble();
            var nd2 = d2.AsDouble();
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideDoubleChecked(Object d1, Object d2)
        {
            var nd1 = d1.AsDouble();
            var nd2 = d2.AsDouble();
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides singles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideSingle(Object d1, Object d2)
        {
            var nd1 = d1.AsFloat();
            var nd2 = d2.AsFloat();
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideInt64(Object d1, Object d2)
        {
            var nd1 = d1.AsLong();
            var nd2 = d2.AsLong();
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideInt32(Object d1, Object d2)
        {
            var nd1 = d1.AsInt();
            var nd2 = d2.AsInt();
            if (nd2 == 0) return null;
            return nd1 / nd2;

        }

        /// <summary>
        /// Divides unsigned 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideUInt64(Object d1, Object d2)
        {
            var nd1 = Convert.ToUInt64(d1);
            var nd2 = Convert.ToUInt64(d2);
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Divides unsigned 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object DivideUInt32(Object d1, Object d2)
        {
            var nd1 = Convert.ToUInt32(d1);
            var nd2 = Convert.ToUInt32(d2);
            if (nd2 == 0) return null;
            return nd1 / nd2;
        }

        /// <summary>
        /// Multiplies big integers.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyBigInteger(Object d1, Object d2)
        {
            var nd1 = d1.AsBigInteger();
            var nd2 = d2.AsBigInteger();
            return nd1 * nd2;
        }

        /// <summary>
        /// Multiplies decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyDecimal(Object d1, Object d2)
        {
            return d1.AsDecimal() * d2.AsDecimal();
        }

        /// <summary>
        /// Multiplies doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyDouble(Object d1, Object d2)
        {
            return d1.AsDouble() * d2.AsDouble();
        }

        /// <summary>
        /// Multiplies singles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplySingle(Object d1, Object d2)
        {
            return d1.AsFloat() * d2.AsFloat();
        }

        /// <summary>
        /// Multiplies 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyInt64(Object d1, Object d2)
        {
            return d1.AsLong() * d2.AsLong();
        }

        /// <summary>
        /// Multiplies 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyInt32(Object d1, Object d2)
        {
            return d1.AsInt() * d2.AsInt();
        }

        /// <summary>
        /// Multiplies unsigned 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyUInt64(Object d1, Object d2)
        {
            return Convert.ToUInt64(d1) * Convert.ToUInt64(d2);
        }

        /// <summary>
        /// Multiplies unsigned 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object MultiplyUInt32(Object d1, Object d2)
        {
            return Convert.ToUInt32(d1) * Convert.ToUInt32(d2);
        }

        /// <summary>
        /// Moduloes big integers.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloBigInteger(Object d1, Object d2)
        {
            var nd1 = d1.AsBigInteger();
            var nd2 = d2.AsBigInteger();
            return nd1 % nd2;
        }

        /// <summary>
        /// Moduloes decimals.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloDecimal(Object d1, Object d2)
        {
            return d1.AsDecimal() % d2.AsDecimal();
        }

        /// <summary>
        /// Moduloes doubles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloDouble(Object d1, Object d2)
        {
            return d1.AsDouble() % d2.AsDouble();
        }

        /// <summary>
        /// Moduloes singles.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloSingle(Object d1, Object d2)
        {
            return d1.AsFloat() % d2.AsFloat();
        }

        /// <summary>
        /// Moduloes 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloInt64(Object d1, Object d2)
        {
            return d1.AsLong() % d2.AsLong();
        }

        /// <summary>
        /// Moduloes 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloInt32(Object d1, Object d2)
        {
            return d1.AsInt() % d2.AsInt();
        }

        /// <summary>
        /// Moduloes unsigned 64-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloUInt64(Object d1, Object d2)
        {
            return Convert.ToUInt64(d1) % Convert.ToUInt64(d2);
        }

        /// <summary>
        /// Moduloes unsigned 32-bit ints.
        /// </summary>
        /// <param name="d1">The d1.</param>
        /// <param name="d2">The d2.</param>
        /// <returns></returns>
        public static Object ModuloUInt32(Object d1, Object d2)
        {
            return Convert.ToUInt32(d1) % Convert.ToUInt32(d2);
        }
    }
}
