///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Enumeration for the type of arithmatic to use.
    /// </summary>
    public partial class MathArithTypeEnum
    {
        /// <summary>
        ///     Plus.
        /// </summary>
        public static readonly MathArithTypeEnum ADD = new MathArithTypeEnum("+");

        /// <summary>
        ///     Minus.
        /// </summary>
        public static readonly MathArithTypeEnum SUBTRACT = new MathArithTypeEnum("-");

        /// <summary>
        ///     Divide.
        /// </summary>
        public static readonly MathArithTypeEnum DIVIDE = new MathArithTypeEnum("/");

        /// <summary>
        ///     Multiply.
        /// </summary>
        public static readonly MathArithTypeEnum MULTIPLY = new MathArithTypeEnum("*");

        /// <summary>
        ///     Modulo.
        /// </summary>
        public static readonly MathArithTypeEnum MODULO = new MathArithTypeEnum("%");

        private static readonly ISet<MathArithTypeEnum> Values = new HashSet<MathArithTypeEnum>();

        private static readonly IDictionary<HashableMultiKey, Computer> computers;

        static MathArithTypeEnum()
        {
            computers = new Dictionary<HashableMultiKey, Computer>();
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), ADD}), new AddDouble());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), ADD}), new AddFloat());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), ADD}), new AddLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), ADD}), new AddInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), ADD}), new AddDecimal());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), ADD}), new AddBigInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), SUBTRACT}), new SubtractDouble());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), SUBTRACT}), new SubtractFloat());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), SUBTRACT}), new SubtractLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), SUBTRACT}), new SubtractInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), SUBTRACT}), new SubtractDecimal());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), SUBTRACT}), new SubtractBigInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), MULTIPLY}), new MultiplyDouble());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), MULTIPLY}), new MultiplyFloat());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), MULTIPLY}), new MultiplyLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), MULTIPLY}), new MultiplyInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), MULTIPLY}), new MultiplyDecimal());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), MULTIPLY}), new MultiplyBigInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), MODULO}), new ModuloDouble());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), MODULO}), new ModuloFloat());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), MODULO}), new ModuloLong());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), MODULO}), new ModuloInt());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), MODULO}), new ModuloDouble());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), MODULO}), new ModuloLong());
        }

        private MathArithTypeEnum(string expressionText)
        {
            ExpressionText = expressionText;
            Values.Add(this);
        }

        /// <summary>
        ///     Returns string representation of enum.
        /// </summary>
        /// <returns>text for enum</returns>
        public string ExpressionText { get; }

        /// <summary>
        ///     Returns number cruncher for the target coercion type.
        /// </summary>
        /// <param name="coercedType">target type</param>
        /// <param name="typeOne">the LHS type</param>
        /// <param name="typeTwo">the RHS type</param>
        /// <param name="isIntegerDivision">false for division returns double, true for using Java-standard integer division</param>
        /// <param name="isDivisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
        /// <param name="optionalMathContext">math context or null</param>
        /// <returns>number cruncher</returns>
        public Computer GetComputer(
            Type coercedType,
            Type typeOne,
            Type typeTwo,
            bool isIntegerDivision,
            bool isDivisionByZeroReturnsNull,
            MathContext optionalMathContext)
        {
            if (coercedType != typeof(double?)
                && coercedType != typeof(float?)
                && coercedType != typeof(long?)
                && coercedType != typeof(int?)
                && coercedType != typeof(decimal?)
                && coercedType != typeof(BigInteger)
                && coercedType != typeof(short?)
                && coercedType != typeof(byte)) {
                throw new ArgumentException(
                    "Expected base numeric type for computation result but got type " + coercedType);
            }

            if (coercedType == typeof(decimal?)) {
                return MakeDecimalComputer(typeOne, typeTwo, isDivisionByZeroReturnsNull, optionalMathContext);
            }

            if (coercedType == typeof(BigInteger)) {
                return MakeBigIntegerComputer(typeOne, typeTwo);
            }

            if (this != DIVIDE) {
                var key = new HashableMultiKey(new object[] {coercedType, this});
                var computer = computers.Get(key);
                if (computer == null) {
                    throw new ArgumentException("Could not determine process or type " + this + " type " + coercedType);
                }

                return computer;
            }

            if (!isIntegerDivision) {
                //return new DivideDecimal(isDivisionByZeroReturnsNull);
                return new DivideDouble(isDivisionByZeroReturnsNull);
            }

            if (coercedType == typeof(decimal?)) {
                return new DivideDecimal(isDivisionByZeroReturnsNull);
            }

            if (coercedType == typeof(double?)) {
                return new DivideDouble(isDivisionByZeroReturnsNull);
            }

            if (coercedType == typeof(float?)) {
                return new DivideFloat();
            }

            if (coercedType == typeof(long?)) {
                return new DivideLong();
            }

            if (coercedType == typeof(int?)) {
                return new DivideInt();
            }

            throw new ArgumentException("Could not determine process or type " + this + " type " + coercedType);
        }

        private Computer MakeDecimalComputer(
            Type typeOne,
            Type typeTwo,
            bool divisionByZeroReturnsNull,
            MathContext optionalMathContext)
        {
            if (typeOne == typeof(decimal?) && typeTwo == typeof(decimal?)) {
                if (this == DIVIDE) {
                    if (optionalMathContext != null) {
                        return new DivideDecimalWMathContext(divisionByZeroReturnsNull, optionalMathContext);
                    }

                    return new DivideDecimal(divisionByZeroReturnsNull);
                }

                return computers.Get(new HashableMultiKey(new object[] {typeof(decimal?), this}));
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercer(typeOne, typeof(decimal));
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercer(typeTwo, typeof(decimal));
            if (this == ADD) {
                return new AddDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (this == SUBTRACT) {
                return new SubtractDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (this == MULTIPLY) {
                return new MultiplyDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (this == DIVIDE) {
                if (optionalMathContext == null) {
                    return new DivideDecimalConvComputerNoMathCtx(
                        convertorOne, convertorTwo, divisionByZeroReturnsNull);
                }

                return new DivideDecimalConvComputerWithMathCtx(
                    convertorOne, convertorTwo, divisionByZeroReturnsNull, optionalMathContext);
            }

            return new ModuloDouble();
        }

        private Computer MakeBigIntegerComputer(
            Type typeOne,
            Type typeTwo)
        {
            if (typeOne == typeof(decimal?) && typeTwo == typeof(decimal?)) {
                return computers.Get(new HashableMultiKey(new object[] {typeof(decimal?), this}));
            }

            if (typeOne == typeof(BigInteger) && typeTwo == typeof(BigInteger)) {
                var computer = computers.Get(new HashableMultiKey(new object[] {typeof(BigInteger), this}));
                if (computer != null) {
                    return computer;
                }
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeOne);
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeTwo);
            if (this == ADD) {
                return new AddBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (this == SUBTRACT) {
                return new SubtractBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (this == MULTIPLY) {
                return new MultiplyBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (this == DIVIDE) {
                return new DivideBigIntConvComputer(convertorOne, convertorTwo);
            }

            return new ModuloLong();
        }

        /// <summary>
        ///     Returns the math operator for the string.
        /// </summary>
        /// <param name="operator">to parse</param>
        /// <returns>math enum</returns>
        public static MathArithTypeEnum ParseOperator(string @operator)
        {
            foreach (var value in Values) {
                if (value.ExpressionText == @operator) {
                    return value;
                }
            }

            throw new ArgumentException("Unknown operator '" + @operator + "'");
        }

        public static CodegenExpression CodegenAsLong(
            CodegenExpression @ref,
            Type type)
        {
            return SimpleNumberCoercerFactory.CoercerLong.CodegenLong(@ref, type);
        }

        public static CodegenExpression CodegenAsInt(
            CodegenExpression @ref,
            Type type)
        {
            return SimpleNumberCoercerFactory.CoercerInt.CodegenInt(@ref, type);
        }

        public static CodegenExpression CodegenAsDecimal(
            CodegenExpression @ref,
            Type type)
        {
            return SimpleNumberCoercerFactory.CoercerDecimal.CodegenDecimal(@ref, type);
        }

        public static CodegenExpression CodegenAsDouble(
            CodegenExpression @ref,
            Type type)
        {
            return SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(@ref, type);
        }

        public static CodegenExpression CodegenAsFloat(
            CodegenExpression @ref,
            Type type)
        {
            return SimpleNumberCoercerFactory.CoercerFloat.CodegenFloat(@ref, type);
        }
    }
} // end of namespace