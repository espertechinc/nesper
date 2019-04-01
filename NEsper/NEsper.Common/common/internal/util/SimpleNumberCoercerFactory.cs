///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Factory for conversion/coercion and widening implementations for numbers.
    /// </summary>
    public class SimpleNumberCoercerFactory
    {
        /// <summary>
        ///     Returns a coercer/widener to BigInteger for a given type.
        /// </summary>
        /// <param name="fromType">to widen</param>
        /// <returns>widener</returns>
        public static SimpleNumberBigIntegerCoercer GetCoercerBigInteger(Type fromType)
        {
            if (fromType == typeof(BigInteger)) {
                return SimpleNumberCoercerBigIntNull.INSTANCE;
            }

            return SimpleNumberCoercerBigInt.INSTANCE;
        }

        /// <summary>
        ///     Returns a coercer/widener/narrower to a result number type from a given type.
        /// </summary>
        /// <param name="fromType">to widen/narrow, can be null to indicate that no shortcut-coercer is used</param>
        /// <param name="resultBoxedType">type to widen/narrow to</param>
        /// <returns>widener/narrower</returns>
        public static SimpleNumberCoercer GetCoercer(Type fromType, Type resultBoxedType)
        {
            if (fromType == resultBoxedType) {
                return SimpleNumberCoercerNull.INSTANCE;
            }

            if (resultBoxedType == typeof(decimal?)) {
                return SimpleNumberCoercerDecimal.INSTANCE;
            }

            if (resultBoxedType == typeof(double?)) {
                return SimpleNumberCoercerDouble.INSTANCE;
            }

            if (resultBoxedType == typeof(long?)) {
                return SimpleNumberCoercerLong.INSTANCE;
            }

            if (resultBoxedType == typeof(float?)) {
                return SimpleNumberCoercerFloat.INSTANCE;
            }

            if (resultBoxedType == typeof(int?)) {
                return SimpleNumberCoercerInt.INSTANCE;
            }

            if (resultBoxedType == typeof(short?)) {
                return SimpleNumberCoercerShort.INSTANCE;
            }

            if (resultBoxedType == typeof(byte?)) {
                return SimpleNumberCoercerByte.INSTANCE;
            }

            if (resultBoxedType == typeof(BigInteger)) {
                return SimpleNumberCoercerBigInt.INSTANCE;
            }

            if (resultBoxedType == typeof(object)) {
                return SimpleNumberCoercerNull.INSTANCE;
            }

            throw new ArgumentException("Cannot coerce to number subtype " + resultBoxedType.Name);
        }

        private static CodegenExpression CodegenCoerceNonNull(
            Type primitive, Type boxed, string numberValueMethodName, CodegenExpression param, Type type)
        {
            if (type == primitive || type == boxed) {
                return param;
            }

            if (type.IsPrimitive) {
                return Cast(primitive, param);
            }

            return ExprDotMethod(param, numberValueMethodName);
        }

        private static CodegenExpression CodegenCoerceMayNull(
            Type primitive, Type boxed, string numberValueMethodName, CodegenExpression param, Type type,
            CodegenMethodScope codegenMethodScope, Type generator, CodegenClassScope codegenClassScope)
        {
            if (type == primitive || type == boxed) {
                return param;
            }

            if (type == null) {
                return ConstantNull();
            }

            if (type.IsPrimitive) {
                return Cast(primitive, param);
            }

            var method = codegenMethodScope
                .MakeChild(boxed, generator, codegenClassScope)
                .AddParam(type, "value")
                .Block
                .IfRefNullReturnNull("value")
                .MethodReturn(ExprDotMethod(Ref("value"), numberValueMethodName));
            return LocalMethod(method, param);
        }

        private class SimpleNumberCoercerNull : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerNull INSTANCE = new SimpleNumberCoercerNull();

            private SimpleNumberCoercerNull()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce;
            }

            public Type ReturnType => typeof(object);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return value;
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return value;
            }
        }

        public class SimpleNumberCoercerDecimal : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerDecimal INSTANCE = new SimpleNumberCoercerDecimal();

            private SimpleNumberCoercerDecimal()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsDecimal();
            }

            public Type ReturnType => typeof(decimal?);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenDecimal(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(decimal), typeof(decimal?), "AsDecimal", param, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerDecimal), codegenClassScope);
            }

            public static CodegenExpression CodegenDecimal(CodegenExpression param, Type type)
            {
                return CodegenCoerceNonNull(typeof(decimal), typeof(decimal?), "AsDecimal", param, type);
            }

            public static CodegenExpression CodegenDoubleMayNullBoxedIncludeBig(
                CodegenExpression value, Type valueType, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType == typeof(BigInteger) || valueType == typeof(decimal)) {
                    return ExprDotMethod(value, "AsDecimal");
                }

                return CodegenCoerceMayNull(
                    typeof(decimal), typeof(decimal?), "AsDecimal", value, valueType, codegenMethodScope,
                    typeof(SimpleNumberCoercerDecimal), codegenClassScope);
            }
        }

        public class SimpleNumberCoercerDouble : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerDouble INSTANCE = new SimpleNumberCoercerDouble();

            private SimpleNumberCoercerDouble()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsDouble();
            }

            public Type ReturnType => typeof(double?);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenDouble(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(double), typeof(double?), "doubleValue", param, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerDouble), codegenClassScope);
            }

            public static CodegenExpression CodegenDouble(CodegenExpression param, Type type)
            {
                return CodegenCoerceNonNull(typeof(double), typeof(double?), "doubleValue", param, type);
            }

            public static CodegenExpression CodegenDoubleMayNullBoxedIncludeBig(
                CodegenExpression value, Type valueType, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType == typeof(BigInteger) || valueType == typeof(decimal))
                {
                    return ExprDotMethod(value, "doubleValue");
                }

                return CodegenCoerceMayNull(
                    typeof(double), typeof(double?), "doubleValue", value, valueType, codegenMethodScope,
                    typeof(SimpleNumberCoercerDouble), codegenClassScope);
            }
        }

        public class SimpleNumberCoercerLong : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerLong INSTANCE = new SimpleNumberCoercerLong();

            private SimpleNumberCoercerLong()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsLong();
            }

            public Type ReturnType => typeof(long);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenLong(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenLongMayNullBox(param, valueTypeMustNumeric, codegenMethodScope, codegenClassScope);
            }

            public static CodegenExpression CodegenLong(CodegenExpression param, Type type)
            {
                return CodegenCoerceNonNull(typeof(long), typeof(long), "longValue", param, type);
            }

            public static CodegenExpression CodegenLongMayNullBox(
                CodegenExpression param, Type type, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(long), typeof(long), "longValue", param, type, codegenMethodScope,
                    typeof(SimpleNumberCoercerLong), codegenClassScope);
            }
        }

        public class SimpleNumberCoercerInt : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerInt INSTANCE = new SimpleNumberCoercerInt();

            private SimpleNumberCoercerInt()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsInt();
            }

            public Type ReturnType => typeof(int);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(int), typeof(int), "intValue", param, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerInt), codegenClassScope);
            }

            public static CodegenExpression CodegenInt(CodegenExpression param, Type type)
            {
                return CodegenCoerceNonNull(typeof(int), typeof(int), "intValue", param, type);
            }

            public static CodegenExpression CoerceCodegenMayNull(
                CodegenExpression param, Type type, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(int), typeof(int), "intValue", param, type, codegenMethodScope,
                    typeof(SimpleNumberCoercerInt), codegenClassScope);
            }
        }

        public class SimpleNumberCoercerFloat : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerFloat INSTANCE = new SimpleNumberCoercerFloat();

            private SimpleNumberCoercerFloat()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsFloat();
            }

            public Type ReturnType => typeof(float?);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenFloat(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(float), typeof(float?), "floatValue", value, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerFloat), codegenClassScope);
            }

            public static CodegenExpression CodegenFloat(CodegenExpression @ref, Type type)
            {
                return CodegenCoerceNonNull(typeof(float), typeof(float?), "floatValue", @ref, type);
            }
        }

        public class SimpleNumberCoercerShort : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerShort INSTANCE = new SimpleNumberCoercerShort();

            private SimpleNumberCoercerShort()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsShort();
            }

            public Type ReturnType => typeof(short?);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenShort(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(short), typeof(short?), "shortValue", value, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerShort), codegenClassScope);
            }

            public static CodegenExpression CodegenShort(CodegenExpression input, Type inputType)
            {
                return CodegenCoerceNonNull(typeof(short), typeof(short?), "shortValue", input, inputType);
            }
        }

        public class SimpleNumberCoercerByte : SimpleNumberCoercer
        {
            public static readonly SimpleNumberCoercerByte INSTANCE = new SimpleNumberCoercerByte();

            private SimpleNumberCoercerByte()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsByte();
            }

            public Type ReturnType => typeof(byte);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenByte(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(byte), typeof(byte), "bteValue", value, valueTypeMustNumeric, codegenMethodScope,
                    typeof(SimpleNumberCoercerByte), codegenClassScope);
            }

            public static CodegenExpression CodegenByte(CodegenExpression input, Type inputType)
            {
                return CodegenCoerceNonNull(typeof(byte), typeof(byte), "byteValue", input, inputType);
            }
        }

        public class SimpleNumberCoercerBigInt : SimpleNumberCoercer,
            SimpleNumberBigIntegerCoercer
        {
            public static readonly SimpleNumberCoercerBigInt INSTANCE = new SimpleNumberCoercerBigInt();

            private SimpleNumberCoercerBigInt()
            {
            }

            public CodegenExpression CoerceBoxedBigIntCodegen(CodegenExpression expr, Type type)
            {
                return CoerceCodegen(expr, type);
            }

            public BigInteger CoerceBoxedBigInt(object numToCoerce)
            {
                return numToCoerce.AsBigInteger();
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsBigInteger();
            }

            public Type ReturnType => typeof(long);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return CodegenBigInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueTypeMustNumeric == null) {
                    return value;
                }

                if (valueTypeMustNumeric.IsPrimitive) {
                    return CodegenBigInt(value, valueTypeMustNumeric);
                }

                if (valueTypeMustNumeric == typeof(BigInteger)) {
                    return value;
                }

                var method = codegenMethodScope
                    .MakeChild(typeof(BigInteger), typeof(SimpleNumberCoercerBigInt), codegenClassScope)
                    .AddParam(valueTypeMustNumeric, "value").Block
                    .IfRefNullReturnNull("value")
                    .MethodReturn(CodegenBigInt(Ref("value"), valueTypeMustNumeric));
                return LocalMethod(method, value);
            }

            public static CodegenExpression CodegenBigInt(CodegenExpression value, Type valueType)
            {
                if (valueType == typeof(BigInteger)) {
                    return value;
                }

                if (valueType == typeof(long) || valueType == typeof(long)) {
                    return StaticMethod(typeof(BigInteger), "valueOf", value);
                }

                if (valueType.IsPrimitive) {
                    return StaticMethod(typeof(BigInteger), "valueOf", Cast(typeof(long), value));
                }

                return StaticMethod(typeof(BigInteger), "valueOf", ExprDotMethod(value, "longValue"));
            }
        }

        private class SimpleNumberCoercerBigIntNull : SimpleNumberCoercer,
            SimpleNumberBigIntegerCoercer
        {
            public static readonly SimpleNumberCoercerBigIntNull INSTANCE = new SimpleNumberCoercerBigIntNull();

            private SimpleNumberCoercerBigIntNull()
            {
            }

            public CodegenExpression CoerceBoxedBigIntCodegen(CodegenExpression expr, Type type)
            {
                return expr;
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce;
            }

            public Type ReturnType => typeof(BigInteger);

            public CodegenExpression CoerceCodegen(CodegenExpression value, Type valueType)
            {
                return value;
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value, Type valueTypeMustNumeric, CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return value;
            }

            public BigInteger CoerceBoxedBigInt(object numToCoerce)
            {
                return (BigInteger) numToCoerce;
            }
        }
    }
} // end of namespace