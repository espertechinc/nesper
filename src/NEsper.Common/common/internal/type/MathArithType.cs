///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        private static readonly IDictionary<MathArithDesc, Computer> computers;

        static MathArithType()
        {
            computers = new Dictionary<MathArithDesc, Computer>();
            computers.Put(
                new MathArithDesc(typeof(double?), MathArithTypeEnum.ADD),
                new AddDouble());
            computers.Put(
                new MathArithDesc(typeof(float?), MathArithTypeEnum.ADD),
                new AddFloat());
            computers.Put(
                new MathArithDesc(typeof(long?), MathArithTypeEnum.ADD),
                new AddLong());
            computers.Put(
                new MathArithDesc(typeof(int?), MathArithTypeEnum.ADD),
                new AddInt());
            computers.Put(
                new MathArithDesc(typeof(decimal?), MathArithTypeEnum.ADD),
                new AddDecimal());
            computers.Put(
                new MathArithDesc(typeof(BigInteger), MathArithTypeEnum.ADD),
                new AddBigInt());
            computers.Put(
                new MathArithDesc(typeof(double?), MathArithTypeEnum.SUBTRACT),
                new SubtractDouble());
            computers.Put(
                new MathArithDesc(typeof(float?), MathArithTypeEnum.SUBTRACT),
                new SubtractFloat());
            computers.Put(
                new MathArithDesc(typeof(long?), MathArithTypeEnum.SUBTRACT),
                new SubtractLong());
            computers.Put(
                new MathArithDesc(typeof(int?), MathArithTypeEnum.SUBTRACT),
                new SubtractInt());
            computers.Put(
                new MathArithDesc(typeof(decimal?), MathArithTypeEnum.SUBTRACT),
                new SubtractDecimal());
            computers.Put(
                new MathArithDesc(typeof(BigInteger), MathArithTypeEnum.SUBTRACT),
                new SubtractBigInt());
            computers.Put(
                new MathArithDesc(typeof(double?), MathArithTypeEnum.MULTIPLY),
                new MultiplyDouble());
            computers.Put(
                new MathArithDesc(typeof(float?), MathArithTypeEnum.MULTIPLY),
                new MultiplyFloat());
            computers.Put(
                new MathArithDesc(typeof(long?), MathArithTypeEnum.MULTIPLY),
                new MultiplyLong());
            computers.Put(
                new MathArithDesc(typeof(int?), MathArithTypeEnum.MULTIPLY),
                new MultiplyInt());
            computers.Put(
                new MathArithDesc(typeof(decimal?), MathArithTypeEnum.MULTIPLY),
                new MultiplyDecimal());
            computers.Put(
                new MathArithDesc(typeof(BigInteger), MathArithTypeEnum.MULTIPLY),
                new MultiplyBigInt());
            computers.Put(
                new MathArithDesc(typeof(double?), MathArithTypeEnum.MODULO),
                new ModuloDouble());
            computers.Put(
                new MathArithDesc(typeof(float?), MathArithTypeEnum.MODULO),
                new ModuloFloat());
            computers.Put(
                new MathArithDesc(typeof(long?), MathArithTypeEnum.MODULO),
                new ModuloLong());
            computers.Put(new MathArithDesc(typeof(int?), MathArithTypeEnum.MODULO), new ModuloInt());
            computers.Put(
                new MathArithDesc(typeof(decimal?), MathArithTypeEnum.MODULO),
                new ModuloDouble());
            computers.Put(
                new MathArithDesc(typeof(BigInteger), MathArithTypeEnum.MODULO),
                new ModuloLong());
        }

        /// <summary>
        ///     Returns number cruncher for the target coercion type.
        /// </summary>
        /// <param name="operation">Arithmetic operation.</param>
        /// <param name="coercedType">target type</param>
        /// <param name="typeOne">the LHS type</param>
        /// <param name="typeTwo">the RHS type</param>
        /// <param name="isIntegerDivision">false for division returns double, true for using standard integer division</param>
        /// <param name="isDivisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
        /// <param name="optionalMathContext">math context or null</param>
        /// <returns>number cruncher</returns>
        public static Computer GetComputer(
            MathArithTypeEnum operation,
            Type coercedType,
            Type typeOne,
            Type typeTwo,
            bool isIntegerDivision,
            bool isDivisionByZeroReturnsNull,
            MathContext optionalMathContext)
        {
            coercedType = coercedType.GetBoxedType();
            if (coercedType != typeof(double?) &&
                coercedType != typeof(float?) &&
                coercedType != typeof(long?) &&
                coercedType != typeof(int?) &&
                coercedType != typeof(decimal?) &&
                coercedType != typeof(BigInteger?) &&
                coercedType != typeof(short?) &&
                coercedType != typeof(byte)) {
                throw new ArgumentException(
                    $"Expected base numeric type for computation result but got type {coercedType}");
            }

            if (coercedType.IsTypeDecimal()) {
                return MakeDecimalComputer(
                    operation,
                    typeOne,
                    typeTwo,
                    isDivisionByZeroReturnsNull,
                    optionalMathContext);
            }

            if (coercedType.IsTypeBigInteger()) {
                return MakeBigIntegerComputer(
                    operation,
                    typeOne,
                    typeTwo);
            }

            if (operation != MathArithTypeEnum.DIVIDE) {
                var key = new MathArithDesc(coercedType, operation);
                var computer = computers.Get(key);
                if (computer == null) {
                    throw new ArgumentException(
                        $"Could not determine process or type {operation} type {coercedType}");
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

            throw new ArgumentException($"Could not determine process or type {operation} type {coercedType}");
        }

        private static Computer MakeDecimalComputer(
            MathArithTypeEnum operation,
            Type typeOne,
            Type typeTwo,
            bool divisionByZeroReturnsNull,
            MathContext optionalMathContext)
        {
            if (typeOne.IsTypeDecimal() && typeTwo.IsTypeDecimal()) {
                if (operation == MathArithTypeEnum.DIVIDE) {
                    if (optionalMathContext != null) {
                        return new DivideDecimalWMathContext(divisionByZeroReturnsNull, optionalMathContext);
                    }

                    return new DivideDecimal(divisionByZeroReturnsNull);
                }

                return computers.Get(new MathArithDesc(typeof(decimal?), operation));
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercer(typeOne, typeof(decimal?));
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercer(typeTwo, typeof(decimal?));
            if (operation == MathArithTypeEnum.ADD) {
                return new AddDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.SUBTRACT) {
                return new SubtractDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.MULTIPLY) {
                return new MultiplyDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.DIVIDE) {
                if (optionalMathContext == null) {
                    return new DivideDecimalConvComputerNoMathCtx(
                        convertorOne,
                        convertorTwo,
                        divisionByZeroReturnsNull);
                }

                return new DivideDecimalConvComputerWithMathCtx(
                    convertorOne,
                    convertorTwo,
                    divisionByZeroReturnsNull,
                    optionalMathContext);
            }

            return new ModuloDouble();
        }

        private static Computer MakeBigIntegerComputer(
            MathArithTypeEnum operation,
            Type typeOne,
            Type typeTwo)
        {
            if (typeOne.IsTypeDecimal() && typeTwo.IsTypeDecimal()) {
                return computers.Get(new MathArithDesc(typeof(decimal?), operation));
            }

            if (typeOne.IsTypeBigInteger() && typeTwo.IsTypeBigInteger()) {
                var computer = computers.Get(new MathArithDesc(typeof(BigInteger), operation));
                if (computer != null) {
                    return computer;
                }
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeOne);
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeTwo);
            if (operation == MathArithTypeEnum.ADD) {
                return new AddBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.SUBTRACT) {
                return new SubtractBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.MULTIPLY) {
                return new MultiplyBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (operation == MathArithTypeEnum.DIVIDE) {
                return new DivideBigIntConvComputer(convertorOne, convertorTwo);
            }

            return new ModuloLong();
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
}