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
    public static class RelationalOpEnumExtensions
    {
        private static readonly IDictionary<HashableMultiKey, RelationalOpEnumComputer> computers;

        static RelationalOpEnumExtensions()
        {
            computers = new Dictionary<HashableMultiKey, RelationalOpEnumComputer>();
            computers.Put(
                new HashableMultiKey(new object[] {typeof(string), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.StringComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(string), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.StringComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(string), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.StringComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(string), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.StringComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(int?), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.IntegerComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(int?), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.IntegerComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(int?), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.IntegerComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(int?), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.IntegerComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(long?), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.LongComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(long?), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.LongComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(long?), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.LongComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(long?), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.LongComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(double?), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.DoubleComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(double?), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.DoubleComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(double?), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.DoubleComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(double?), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.DoubleComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(float?), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.FloatComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(float?), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.FloatComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(float?), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.FloatComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(float?), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.FloatComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(decimal?), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.DecimalComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(decimal?), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.DecimalComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(decimal?), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.DecimalComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(decimal?), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.DecimalComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(BigInteger), RelationalOpEnum.GT}),
                new RelationalOpEnumGT.BigIntComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(BigInteger), RelationalOpEnum.GE}),
                new RelationalOpEnumGE.BigIntComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(BigInteger), RelationalOpEnum.LT}),
                new RelationalOpEnumLT.BigIntComputer());
            computers.Put(
                new HashableMultiKey(new object[] {typeof(BigInteger), RelationalOpEnum.LE}),
                new RelationalOpEnumLE.BigIntComputer());
        }


        /// <summary>
        ///     Returns string rendering of enum.
        /// </summary>
        /// <returns>relational op string</returns>
        public static string GetExpressionText(this RelationalOpEnum value)
        {
            switch (value) {
                case RelationalOpEnum.GT:
                    return ">";

                case RelationalOpEnum.GE:
                    return ">=";

                case RelationalOpEnum.LT:
                    return "<";

                case RelationalOpEnum.LE:
                    return "<=";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static RelationalOpEnum Reversed(this RelationalOpEnum value)
        {
            if (RelationalOpEnum.GT == value) {
                return RelationalOpEnum.LT;
            }
            else if (RelationalOpEnum.GE == value) {
                return RelationalOpEnum.LE;
            }
            else if (RelationalOpEnum.LE == value) {
                return RelationalOpEnum.GE;
            }

            return RelationalOpEnum.GT;
        }

        /// <summary>
        ///     Parses the operator and returns an enum for the operator.
        /// </summary>
        /// <param name="op">to parse</param>
        /// <returns>enum representing relational operation</returns>
        public static RelationalOpEnum Parse(string op)
        {
            switch (op) {
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

                default:
                    throw new ArgumentException("Invalid relational operator '" + op + "'");
            }
        }

        /// <summary>
        ///     Returns the computer to use for the relational operation based on the coercion type.
        /// </summary>
        /// <param name="coercedType">is the object type</param>
        /// <param name="typeOne">the compare-to type on the LHS</param>
        /// <param name="typeTwo">the compare-to type on the RHS</param>
        /// <returns>computer for performing the relational op</returns>
        public static RelationalOpEnumComputer GetComputer(
            this RelationalOpEnum value,
            Type coercedType,
            Type typeOne,
            Type typeTwo)
        {
            coercedType = coercedType.GetBoxedType();
            if (coercedType != typeof(double?) &&
                coercedType != typeof(float?) &&
                coercedType != typeof(int?) &&
                coercedType != typeof(long?) &&
                coercedType != typeof(string) &&
                coercedType != typeof(decimal?) &&
                coercedType != typeof(BigInteger?)) {
                throw new ArgumentException("Unsupported type for relational op compare, type " + coercedType);
            }

            if (coercedType.IsBigInteger()) {
                return MakeBigIntegerComputer(value, typeOne, typeTwo);
            }

            var key = new HashableMultiKey(new object[] {coercedType, value});
            return computers.Get(key);
        }

        public static RelationalOpEnumComputer MakeBigIntegerComputer(
            this RelationalOpEnum value,
            Type typeOne,
            Type typeTwo)
        {
            if (typeOne.IsBigInteger() && typeTwo.IsBigInteger()) {
                return computers.Get(new HashableMultiKey(new object[] {typeof(BigInteger), value}));
            }

            var convertorOne = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeOne);
            var convertorTwo = SimpleNumberCoercerFactory.GetCoercerBigInteger(typeTwo);
            if (value == RelationalOpEnum.GT) {
                return new RelationalOpEnumGT.BigIntConvComputer(convertorOne, convertorTwo);
            }

            if (value == RelationalOpEnum.LT) {
                return new RelationalOpEnumLT.BigIntConvComputer(convertorOne, convertorTwo);
            }

            if (value == RelationalOpEnum.GE) {
                return new RelationalOpEnumGE.BigIntConvComputer(convertorOne, convertorTwo);
            }

            return new RelationalOpEnumLE.BigIntConvComputer(convertorOne, convertorTwo);
        }

        public static CodegenExpression CodegenLong(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return CodegenExpressionBuilder.Op(
                MathArithType.CodegenAsLong(lhs, lhsType),
                op.GetExpressionText(),
                MathArithType.CodegenAsLong(rhs, rhsType));
        }

        public static CodegenExpression CodegenDecimal(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return CodegenExpressionBuilder.Op(
                MathArithType.CodegenAsDecimal(lhs, lhsType),
                op.GetExpressionText(),
                MathArithType.CodegenAsDecimal(rhs, rhsType));
        }

        public static CodegenExpression CodegenDouble(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return CodegenExpressionBuilder.Op(
                MathArithType.CodegenAsDouble(lhs, lhsType),
                op.GetExpressionText(),
                MathArithType.CodegenAsDouble(rhs, rhsType));
        }

        public static CodegenExpression CodegenFloat(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return CodegenExpressionBuilder.Op(
                MathArithType.CodegenAsFloat(lhs, lhsType),
                op.GetExpressionText(),
                MathArithType.CodegenAsFloat(rhs, rhsType));
        }

        public static CodegenExpression CodegenInt(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            RelationalOpEnum op)
        {
            return CodegenExpressionBuilder.Op(
                MathArithType.CodegenAsInt(lhs, lhsType),
                op.GetExpressionText(),
                MathArithType.CodegenAsInt(rhs, rhsType));
        }

        public static CodegenExpression CodegenStringCompare(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType,
            CodegenExpressionRelational.CodegenRelational rel)
        {
            return CodegenExpressionBuilder.Relational(
                CodegenExpressionBuilder.ExprDotMethod(
                    RelationalOpEnumExtensions.CodegenAsString(lhs, lhsType),
                    "Compare",
                    RelationalOpEnumExtensions.CodegenAsString(rhs, rhsType)),
                rel,
                CodegenExpressionBuilder.Constant(0));
        }

        public static CodegenExpression CodegenAsString(
            CodegenExpression @ref,
            Type type)
        {
            if (type == typeof(string)) {
                return @ref;
            }

            return CodegenExpressionBuilder.Cast(typeof(string), @ref);
        }

        public static CodegenExpression CodegenComparable(
            CodegenExpression lhs,
            CodegenExpression rhs,
            CodegenExpressionRelational.CodegenRelational rel)
        {
            return CodegenExpressionBuilder.Relational(
                CodegenExpressionBuilder.ExprDotMethod(lhs, "Compare", rhs),
                rel,
                CodegenExpressionBuilder.Constant(0));
        }

        public static CodegenExpression CodegenBigIntConv(
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
            return CodegenExpressionBuilder.Relational(
                CodegenExpressionBuilder.ExprDotMethod(leftConv, "Compare", rightConv),
                rel,
                CodegenExpressionBuilder.Constant(0));
        }
    }
}