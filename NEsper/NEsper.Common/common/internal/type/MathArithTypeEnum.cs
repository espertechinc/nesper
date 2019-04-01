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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Enumeration for the type of arithmatic to use.
    /// </summary>
    public class MathArithTypeEnum
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
            Type coercedType, Type typeOne, Type typeTwo, bool isIntegerDivision, bool isDivisionByZeroReturnsNull,
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
                return MakedecimalComputer(typeOne, typeTwo, isDivisionByZeroReturnsNull, optionalMathContext);
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
                return new DivideDouble(isDivisionByZeroReturnsNull);
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

        private Computer MakedecimalComputer(
            Type typeOne, Type typeTwo, bool divisionByZeroReturnsNull, MathContext optionalMathContext)
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

            SimpleNumberDecimalCoercer convertorOne = SimpleNumberCoercerFactory.GetCoercerDecimal(typeOne);
            SimpleNumberDecimalCoercer convertorTwo = SimpleNumberCoercerFactory.GetCoercerDecimal(typeTwo);
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

        private Computer MakeBigIntegerComputer(Type typeOne, Type typeTwo)
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
            for (var i = 0; i < Values.Count; i++) {
                MathArithTypeEnum val = Values[i];
                if (val.ExpressionText.Equals(@operator)) {
                    return Values[i];
                }
            }

            throw new ArgumentException("Unknown operator '" + @operator + "'");
        }

        public static CodegenExpression CodegenAsLong(CodegenExpression @ref, Type type)
        {
            return SimpleNumberCoercerFactory.SimpleNumberCoercerLong.CodegenLong(@ref, type);
        }

        public static CodegenExpression CodegenAsDouble(CodegenExpression @ref, Type type)
        {
            return SimpleNumberCoercerFactory.SimpleNumberCoercerDouble.CodegenDouble(@ref, type);
        }

        public static CodegenExpression CodegenAsInt(CodegenExpression @ref, Type type)
        {
            return SimpleNumberCoercerFactory.SimpleNumberCoercerInt.CodegenInt(@ref, type);
        }

        public static CodegenExpression CodegenAsFloat(CodegenExpression @ref, Type type)
        {
            return SimpleNumberCoercerFactory.SimpleNumberCoercerFloat.CodegenFloat(@ref, type);
        }

        /// <summary>
        ///     Interface for number cruncher.
        /// </summary>
        public interface Computer
        {
            /// <summary>
            ///     Computes using the 2 numbers a result number.
            /// </summary>
            /// <param name="d1">is the first number</param>
            /// <param name="d2">is the second number</param>
            /// <returns>result</returns>
            object Compute(object d1, object d2);

            CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype);
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddDouble : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDouble() + d2.AsDouble();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsDouble(left, ltype), "+", CodegenAsDouble(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddFloat : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsFloat() + d2.AsFloat();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "+", CodegenAsFloat(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddLong : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsLong() + d2.AsLong();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsLong(left, ltype), "+", CodegenAsLong(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsInt() + d2.AsInt();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsInt(left, ltype), "+", CodegenAsInt(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddBigInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                var b1 = (BigInteger) d1;
                var b2 = (BigInteger) d2;
                return b1.Add(b2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "add", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class AddDecimal : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDecimal() + d2.AsDecimal();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "add", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractDouble : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDouble() - d2.AsDouble();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsDouble(left, ltype), "-", CodegenAsDouble(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractFloat : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsFloat() - d2.AsFloat();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "-", CodegenAsFloat(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractLong : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsLong() - d2.AsLong();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsLong(left, ltype), "-", CodegenAsLong(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsInt() - d2.AsInt();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsInt(left, ltype), "-", CodegenAsInt(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractBigInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                var b1 = (BigInteger) d1;
                var b2 = (BigInteger) d2;
                return b1.Subtract(b2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "subtract", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class SubtractDecimal : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDecimal() - d2.AsDecimal();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "subtract", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideDouble : Computer
        {
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDouble(bool divisionByZeroReturnsNull)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(object d1, object d2)
            {
                var d2Double = d2.AsDouble();
                if (divisionByZeroReturnsNull && d2Double == 0) {
                    return null;
                }

                return d1.AsDouble() / d2Double;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                if (!divisionByZeroReturnsNull) {
                    return Op(CodegenAsDouble(left, ltype), "/", CodegenAsDouble(right, rtype));
                }

                var method = codegenMethodScope.MakeChild(typeof(double?), typeof(DivideDouble), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(double), "d2Double", CodegenAsDouble(Ref("d2"), rtype))
                    .IfCondition(EqualsIdentity(Ref("d2Double"), Constant(0)))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(Op(CodegenAsDouble(Ref("d1"), ltype), "/", Ref("d2Double")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideFloat : Computer
        {
            public object Compute(object d1, object d2)
            {
                var d2Float = d2.AsFloat();
                if (d2Float == 0) {
                    return null;
                }

                return d1.AsFloat() / d2Float;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "/", CodegenAsFloat(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideLong : Computer
        {
            public object Compute(object d1, object d2)
            {
                var d2Long = d2.AsLong();
                if (d2Long == 0) {
                    return null;
                }

                return d1.AsLong() / d2Long;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsLong(left, ltype), "/", CodegenAsLong(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideInt : Computer
        {
            public object Compute(object i1, object i2)
            {
                var i2int = i2.AsInt();
                if (i2int == 0) {
                    return null;
                }

                return i1.AsInt() / i2int;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var method = codegenMethodScope.MakeChild(typeof(int?), typeof(DivideInt), codegenClassScope)
                    .AddParam(typeof(int), "i1").AddParam(typeof(int), "i2").Block
                    .IfCondition(EqualsIdentity(Ref("i2"), Constant(0)))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(Op(Ref("i1"), "/", Ref("i2")));
                return LocalMethod(method, CodegenAsInt(left, ltype), CodegenAsInt(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideDecimal : Computer
        {
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDecimal(bool divisionByZeroReturnsNull)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(object d1, object d2)
            {
                var b1 = d1.AsDecimal();
                var b2 = d2.AsDecimal();
                if (b2 == 0.0m) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    return b1 / 0.0m; // serves to create the right sign for infinity
                }

                return b1 / b2;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var block = codegenMethodScope.MakeChild(typeof(decimal?), typeof(DivideDecimal), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1").AddParam(typeof(decimal?), "b2").Block;
                var ifBlock = block.IfCondition(EqualsIdentity(ExprDotMethod(Ref("b1"), "doubleValue"), Constant(0d)));
                if (divisionByZeroReturnsNull) {
                    ifBlock.BlockReturn(ConstantNull());
                }
                else {
                    ifBlock.BlockReturn(
                        NewInstance(typeof(decimal?), Op(ExprDotMethod(Ref("b1"), "doubleValue"), "/", Constant(0d))));
                }

                var method = block.MethodReturn(ExprDotMethod(Ref("b1"), "divide", Ref("b2")));
                return LocalMethod(method, left, right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideDecimalWMathContext : Computer
        {
            private readonly bool divisionByZeroReturnsNull;
            private readonly MathContext mathContext;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            /// <param name="mathContext">math context</param>
            public DivideDecimalWMathContext(bool divisionByZeroReturnsNull, MathContext mathContext)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
                this.mathContext = mathContext;
            }

            public object Compute(object d1, object d2)
            {
                var b1 = d1.AsDecimal();
                var b2 = d2.AsDecimal();
                if (b2.AsDouble() == 0) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = b1.AsDouble() / 0; // serves to create the right sign for infinity
                    return new decimal(result);
                }

                return b1.Divide(b2, mathContext);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                CodegenExpression math =
                    codegenClassScope.AddOrGetFieldSharable(new MathContextCodegenField(mathContext));
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimalWMathContext), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1").AddParam(typeof(decimal?), "b2").Block;
                var ifZero = block.IfCondition(EqualsIdentity(ExprDotMethod(Ref("b2"), "doubleValue"), Constant(0)));
                {
                    if (divisionByZeroReturnsNull) {
                        ifZero.BlockReturn(ConstantNull());
                    }
                    else {
                        ifZero.BlockReturn(
                            NewInstance(
                                typeof(decimal?), Op(ExprDotMethod(Ref("b1"), "doubleValue"), "/", Constant(0))));
                    }
                }
                var method = block.MethodReturn(ExprDotMethod(Ref("b1"), "divide", Ref("b2"), math));
                return LocalMethod(method, left, right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyDouble : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDouble() * d2.AsDouble();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsDouble(left, ltype), "*", CodegenAsDouble(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyFloat : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsFloat() * d2.AsFloat();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "*", CodegenAsFloat(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyLong : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsLong() * d2.AsLong();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsLong(left, ltype), "*", CodegenAsLong(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsInt() * d2.AsInt();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsInt(left, ltype), "*", CodegenAsInt(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyBigInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                var b1 = (BigInteger) d1;
                var b2 = (BigInteger) d2;
                return b1.Multiply(b2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "multiply", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyDecimal : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDecimal() * d2.AsDecimal();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return ExprDotMethod(left, "multiply", right);
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class ModuloDouble : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsDouble() % d2.AsDouble();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsDouble(left, ltype), "%", CodegenAsDouble(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class ModuloFloat : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsFloat() % d2.AsFloat();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "%", CodegenAsFloat(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class ModuloLong : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsLong() % d2.AsLong();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsLong(left, ltype), "%", CodegenAsLong(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class ModuloInt : Computer
        {
            public object Compute(object d1, object d2)
            {
                return d1.AsInt() % d2.AsInt();
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                return Op(CodegenAsInt(left, ltype), "%", CodegenAsInt(right, rtype));
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class AddDecimalConvComputer : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public AddDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                decimal s1 = convOne.CoerceBoxedDecimal(d1);
                decimal s2 = convTwo.CoerceBoxedDecimal(d2);
                return s1.Add(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var leftAsBig = convOne.CoerceBoxedDecimalCodegen(left, ltype);
                var rightAsBig = convTwo.CoerceBoxedDecimalCodegen(right, rtype);
                return ExprDotMethod(leftAsBig, "add", rightAsBig);
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class SubtractDecimalConvComputer : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public SubtractDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                decimal s1 = convOne.CoerceBoxedDecimal(d1);
                decimal s2 = convTwo.CoerceBoxedDecimal(d2);
                return s1.Subtract(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var method = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(SubtractDecimalConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(decimal?), "s1", convOne.CoerceBoxedDecimalCodegen(Ref("d1"), ltype))
                    .DeclareVar(typeof(decimal?), "s2", convTwo.CoerceBoxedDecimalCodegen(Ref("d2"), rtype))
                    .MethodReturn(ExprDotMethod(Ref("s1"), "subtract", Ref("s2")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class MultiplyDecimalConvComputer : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public MultiplyDecimalConvComputer(SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                decimal s1 = convOne.CoerceBoxedDecimal(d1);
                decimal s2 = convTwo.CoerceBoxedDecimal(d2);
                return s1.Multiply(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var method = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(MultiplyDecimalConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(decimal?), "s1", convOne.CoerceBoxedDecimalCodegen(Ref("d1"), ltype))
                    .DeclareVar(typeof(decimal?), "s2", convTwo.CoerceBoxedDecimalCodegen(Ref("d2"), rtype))
                    .MethodReturn(ExprDotMethod(Ref("s1"), "multiply", Ref("s2")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public abstract class DivideDecimalConvComputerBase : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDecimalConvComputerBase(
                SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo, bool divisionByZeroReturnsNull)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(object d1, object d2)
            {
                decimal s1 = convOne.CoerceBoxedDecimal(d1);
                decimal s2 = convTwo.CoerceBoxedDecimal(d2);
                if (s2.AsDouble() == 0) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = s1.AsDouble() / 0;
                    return new decimal(result);
                }

                return DoDivide(s1, s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimalConvComputerBase), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(decimal?), "s1", convOne.CoerceBoxedDecimalCodegen(Ref("d1"), ltype))
                    .DeclareVar(typeof(decimal?), "s2", convTwo.CoerceBoxedDecimalCodegen(Ref("d2"), rtype));
                var ifZeroDivisor =
                    block.IfCondition(EqualsIdentity(ExprDotMethod(Ref("s2"), "doubleValue"), Constant(0)));
                if (divisionByZeroReturnsNull) {
                    ifZeroDivisor.BlockReturn(ConstantNull());
                }
                else {
                    ifZeroDivisor.DeclareVar(
                            typeof(double), "result", Op(ExprDotMethod(Ref("s1"), "doubleValue"), "/", Constant(0)))
                        .BlockReturn(NewInstance(typeof(decimal?), Ref("result")));
                }

                var method = block.MethodReturn(DoDivideCodegen(Ref("s1"), Ref("s2"), codegenClassScope));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }

            public abstract object DoDivide(decimal s1, decimal s2);

            public abstract CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1, CodegenExpressionRef s2, CodegenClassScope codegenClassScope);
        }

        public class DivideDecimalConvComputerNoMathCtx : DivideDecimalConvComputerBase
        {
            public DivideDecimalConvComputerNoMathCtx(
                SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo, bool divisionByZeroReturnsNull)
                : base(convOne, convTwo, divisionByZeroReturnsNull)
            {
            }

            public override object DoDivide(decimal s1, decimal s2)
            {
                return s1.Divide(s2);
            }

            public override CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1, CodegenExpressionRef s2, CodegenClassScope codegenClassScope)
            {
                return ExprDotMethod(s1, "divide", s2);
            }
        }

        public class DivideDecimalConvComputerWithMathCtx : DivideDecimalConvComputerBase
        {
            private readonly MathContext mathContext;

            public DivideDecimalConvComputerWithMathCtx(
                SimpleNumberDecimalCoercer convOne, SimpleNumberDecimalCoercer convTwo, bool divisionByZeroReturnsNull,
                MathContext mathContext) : base(convOne, convTwo, divisionByZeroReturnsNull)
            {
                this.mathContext = mathContext;
            }

            public override object DoDivide(decimal s1, decimal s2)
            {
                return s1.Divide(s2, mathContext);
            }

            public override CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1, CodegenExpressionRef s2, CodegenClassScope codegenClassScope)
            {
                CodegenExpression math =
                    codegenClassScope.AddOrGetFieldSharable(new MathContextCodegenField(mathContext));
                return ExprDotMethod(s1, "divide", s2, math);
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class AddBigIntConvComputer : Computer
        {
            private readonly SimpleNumberBigIntegerCoercer convOne;
            private readonly SimpleNumberBigIntegerCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public AddBigIntConvComputer(SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                var s1 = convOne.CoerceBoxedBigInt(d1);
                var s2 = convTwo.CoerceBoxedBigInt(d2);
                return s1.Add(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var leftAsBig = convOne.CoerceBoxedBigIntCodegen(left, ltype);
                var rightAsBig = convTwo.CoerceBoxedBigIntCodegen(right, rtype);
                return ExprDotMethod(leftAsBig, "add", rightAsBig);
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class SubtractBigIntConvComputer : Computer
        {
            private readonly SimpleNumberBigIntegerCoercer convOne;
            private readonly SimpleNumberBigIntegerCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public SubtractBigIntConvComputer(
                SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                var s1 = convOne.CoerceBoxedBigInt(d1);
                var s2 = convTwo.CoerceBoxedBigInt(d2);
                return s1.Subtract(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var leftAsBig = convOne.CoerceBoxedBigIntCodegen(left, ltype);
                var rightAsBig = convTwo.CoerceBoxedBigIntCodegen(right, rtype);
                return ExprDotMethod(leftAsBig, "subtract", rightAsBig);
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class MultiplyBigIntConvComputer : Computer
        {
            private readonly SimpleNumberBigIntegerCoercer convOne;
            private readonly SimpleNumberBigIntegerCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public MultiplyBigIntConvComputer(
                SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                var s1 = convOne.CoerceBoxedBigInt(d1);
                var s2 = convTwo.CoerceBoxedBigInt(d2);
                return s1.Multiply(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var leftAsBig = convOne.CoerceBoxedBigIntCodegen(left, ltype);
                var rightAsBig = convTwo.CoerceBoxedBigIntCodegen(right, rtype);
                return ExprDotMethod(leftAsBig, "multiply", rightAsBig);
            }
        }

        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class DivideBigIntConvComputer : Computer
        {
            private readonly SimpleNumberBigIntegerCoercer convOne;
            private readonly SimpleNumberBigIntegerCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public DivideBigIntConvComputer(
                SimpleNumberBigIntegerCoercer convOne, SimpleNumberBigIntegerCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(object d1, object d2)
            {
                var s1 = convOne.CoerceBoxedBigInt(d1);
                var s2 = convTwo.CoerceBoxedBigInt(d2);
                if (s2.AsDouble() == 0) {
                    return null;
                }

                return s1.Divide(s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope, CodegenExpressionRef left,
                CodegenExpressionRef right, Type ltype, Type rtype)
            {
                var method = codegenMethodScope
                    .MakeChild(typeof(BigInteger), typeof(DivideBigIntConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(BigInteger), "s1", convOne.CoerceBoxedBigIntCodegen(Ref("d1"), ltype))
                    .DeclareVar(typeof(BigInteger), "s2", convTwo.CoerceBoxedBigIntCodegen(Ref("d2"), rtype))
                    .IfCondition(EqualsIdentity(ExprDotMethod(Ref("s2"), "doubleValue"), Constant(0)))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(ExprDotMethod(Ref("s1"), "divide", Ref("s2")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }
    }
} // end of namespace