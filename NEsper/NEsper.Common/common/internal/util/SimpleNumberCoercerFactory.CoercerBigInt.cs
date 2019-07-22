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

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        public class CoercerBigInt : SimpleNumberCoercer,
            BigIntegerCoercer
        {
            public static readonly CoercerBigInt INSTANCE = new CoercerBigInt();

            private CoercerBigInt()
            {
            }

            public CodegenExpression CoerceBoxedBigIntCodegen(
                CodegenExpression expr,
                Type type)
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

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenBigInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueTypeMustNumeric,
                CodegenMethodScope codegenMethodScope,
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
                    .MakeChild(typeof(BigInteger), typeof(CoercerBigInt), codegenClassScope)
                    .AddParam(valueTypeMustNumeric, "value")
                    .Block
                    .IfRefNullReturnNull("value")
                    .MethodReturn(CodegenBigInt(CodegenExpressionBuilder.Ref("value"), valueTypeMustNumeric));
                return CodegenExpressionBuilder.LocalMethod(method, value);
            }

            public static CodegenExpression CodegenBigInt(
                CodegenExpression value,
                Type valueType)
            {
                if (valueType == typeof(BigInteger)) {
                    return value;
                }

                if (valueType == typeof(long) || valueType == typeof(long)) {
                    return CodegenExpressionBuilder.StaticMethod(typeof(BigInteger), "valueOf", value);
                }

                if (valueType.IsPrimitive) {
                    return CodegenExpressionBuilder.StaticMethod(
                        typeof(BigInteger),
                        "valueOf",
                        CodegenExpressionBuilder.Cast(typeof(long), value));
                }

                return CodegenExpressionBuilder.StaticMethod(
                    typeof(BigInteger),
                    "valueOf",
                    CodegenExpressionBuilder.ExprDotMethod(value, "longValue"));
            }
        }
    }
}