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
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Enum representing relational types of operation.
    /// </summary>
    public partial class RelationalOpEnum
    {
        /// <summary>
        ///     Greater then.
        /// </summary>
        public static readonly RelationalOpEnum GT = new RelationalOpEnum(">");

        /// <summary>
        ///     Greater equals.
        /// </summary>
        public static readonly RelationalOpEnum GE = new RelationalOpEnum(">=");

        /// <summary>
        ///     Lesser then.
        /// </summary>
        public static readonly RelationalOpEnum LT = new RelationalOpEnum("<");

        /// <summary>
        ///     Lesser equals.
        /// </summary>
        public static readonly RelationalOpEnum LE = new RelationalOpEnum("<=");

        private static readonly IDictionary<HashableMultiKey, Computer> computers;

        static RelationalOpEnum()
        {
            computers = new Dictionary<HashableMultiKey, Computer>();
            computers.Put(new HashableMultiKey(new object[] {typeof(string), GT}), new GTStringComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(string), GE}), new GEStringComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(string), LT}), new LTStringComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(string), LE}), new LEStringComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), GT}), new GTIntegerComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), GE}), new GEIntegerComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), LT}), new LTIntegerComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(int?), LE}), new LEIntegerComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), GT}), new GTLongComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), GE}), new GELongComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), LT}), new LTLongComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(long?), LE}), new LELongComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), GT}), new GTDoubleComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), GE}), new GEDoubleComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), LT}), new LTDoubleComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(double?), LE}), new LEDoubleComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), GT}), new GTFloatComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), GE}), new GEFloatComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), LT}), new LTFloatComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(float?), LE}), new LEFloatComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), GT}), new GTDecimalComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), GE}), new GEDecimalComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), LT}), new LTDecimalComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(decimal?), LE}), new LEDecimalComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), GT}), new GTBigIntComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), GE}), new GEBigIntComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), LT}), new LTBigIntComputer());
            computers.Put(new HashableMultiKey(new object[] {typeof(BigInteger), LE}), new LEBigIntComputer());
        }

        private RelationalOpEnum(string expressionText)
        {
            ExpressionText = expressionText;
        }

        /// <summary>
        ///     Returns string rendering of enum.
        /// </summary>
        /// <returns>relational op string</returns>
        public string ExpressionText { get; }

        public RelationalOpEnum Reversed()
        {
            if (GT == this) {
                return LT;
            }

            if (GE == this) {
                return LE;
            }

            if (LE == this) {
                return GE;
            }

            return GT;
        }

        /// <summary>
        ///     Parses the operator and returns an enum for the operator.
        /// </summary>
        /// <param name="op">to parse</param>
        /// <returns>enum representing relational operation</returns>
        public static RelationalOpEnum Parse(string op)
        {
            if (op.Equals("<")) {
                return LT;
            }

            if (op.Equals(">")) {
                return GT;
            }

            if (op.Equals(">=") || op.Equals("=>")) {
                return GE;
            }

            if (op.Equals("<=") || op.Equals("=<")) {
                return LE;
            }

            throw new ArgumentException("Invalid relational operator '" + op + "'");
        }

        /// <summary>
        ///     Returns the computer to use for the relational operation based on the coercion type.
        /// </summary>
        /// <param name="coercedType">is the object type</param>
        /// <param name="typeOne">the compare-to type on the LHS</param>
        /// <param name="typeTwo">the compare-to type on the RHS</param>
        /// <returns>computer for performing the relational op</returns>
        public Computer GetComputer(
            Type coercedType,
            Type typeOne,
            Type typeTwo)
        {
            if (coercedType != typeof(double?) &&
                coercedType != typeof(float?) &&
                coercedType != typeof(int?) &&
                coercedType != typeof(long?) &&
                coercedType != typeof(string) &&
                coercedType != typeof(decimal?) &&
                coercedType != typeof(BigInteger)) {
                throw new ArgumentException("Unsupported type for relational op compare, type " + coercedType);
            }

            if (coercedType == typeof(decimal?)) {
                return MakeDecimalComputer(typeOne, typeTwo);
            }

            if (coercedType == typeof(BigInteger)) {
                return MakeBigIntegerComputer(typeOne, typeTwo);
            }

            var key = new HashableMultiKey(new object[] {coercedType, this});
            return computers.Get(key);
        }

        private Computer MakeDecimalComputer(
            Type typeOne,
            Type typeTwo)
        {
            if (typeOne == typeof(decimal?) && typeTwo == typeof(decimal?)) {
                return computers.Get(new HashableMultiKey(new object[] {typeof(decimal?), this}));
            }

            SimpleNumberDecimalCoercer convertorOne = SimpleNumberCoercerFactory.GetCoercerDecimal(typeOne);
            SimpleNumberDecimalCoercer convertorTwo = SimpleNumberCoercerFactory.GetCoercerDecimal(typeTwo);
            if (this == GT) {
                return new GTDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (this == LT) {
                return new LTDecimalConvComputer(convertorOne, convertorTwo);
            }

            if (this == GE) {
                return new GEDecimalConvComputer(convertorOne, convertorTwo);
            }

            return new LEDecimalConvComputer(convertorOne, convertorTwo);
        }

        private Computer MakeBigIntegerComputer(
            Type typeOne,
            Type typeTwo)
        {
            if (typeOne == typeof(BigInteger) && typeTwo == typeof(BigInteger)) {
                return computers.Get(new HashableMultiKey(new object[] {typeof(BigInteger), this}));
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeOne);
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeTwo);
            if (this == GT) {
                return new GTBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (this == LT) {
                return new LTBigIntConvComputer(convertorOne, convertorTwo);
            }

            if (this == GE) {
                return new GEBigIntConvComputer(convertorOne, convertorTwo);
            }

            return new LEBigIntConvComputer(convertorOne, convertorTwo);
        }

        private static CodegenExpression CodegenLong(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return Op(
                MathArithTypeEnum.CodegenAsLong(lhs, lhsType), op.ExpressionText,
                MathArithTypeEnum.CodegenAsLong(rhs, rhsType));
        }

        private static CodegenExpression CodegenDecimal(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return Op(
                MathArithTypeEnum.CodegenAsDecimal(lhs, lhsType), op.ExpressionText,
                MathArithTypeEnum.CodegenAsDecimal(rhs, rhsType));
        }

        private static CodegenExpression CodegenDouble(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return Op(
                MathArithTypeEnum.CodegenAsDouble(lhs, lhsType), op.ExpressionText,
                MathArithTypeEnum.CodegenAsDouble(rhs, rhsType));
        }

        private static CodegenExpression CodegenFloat(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return Op(
                MathArithTypeEnum.CodegenAsFloat(lhs, lhsType), op.ExpressionText,
                MathArithTypeEnum.CodegenAsFloat(rhs, rhsType));
        }

        private static CodegenExpression CodegenInt(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return Op(
                MathArithTypeEnum.CodegenAsInt(lhs, lhsType), op.ExpressionText,
                MathArithTypeEnum.CodegenAsInt(rhs, rhsType));
        }

        private static CodegenExpression CodegenStringCompare(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            CodegenExpressionRelational.CodegenRelational rel)
        {
            return Relational(
                ExprDotMethod(
                    CodegenAsString(lhs, lhsType), "Compare",
                    CodegenAsString(rhs, rhsType)), rel,
                Constant(0));
        }

        private static CodegenExpression CodegenAsString(
            CodegenExpression @ref,
            Type type)
        {
            if (type == typeof(string)) {
                return @ref;
            }

            return Cast(typeof(string), @ref);
        }

        private static CodegenExpression CodegenComparable(
            CodegenExpression lhs,
            CodegenExpression rhs,
            CodegenExpressionRelational.CodegenRelational rel)
        {
            return Relational(ExprDotMethod(lhs, "Compare", rhs), rel, Constant(0));
        }

        private static CodegenExpression CodegenBigIntConv(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            BigIntegerCoercer convLeft,
            BigIntegerCoercer convRight,
            CodegenExpressionRelational.CodegenRelational rel)
        {
            var leftConv = convLeft.CoerceBoxedBigIntCodegen(lhs, lhsType);
            var rightConv = convRight.CoerceBoxedBigIntCodegen(rhs, rhsType);
            return Relational(ExprDotMethod(leftConv, "Compare", rightConv), rel, Constant(0));
        }
    }
} // end of namespace