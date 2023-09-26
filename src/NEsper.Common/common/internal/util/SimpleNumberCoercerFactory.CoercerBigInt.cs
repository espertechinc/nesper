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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        public class CoercerBigInt : Coercer,
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

            public object CoerceBoxed(object value)
            {
                return value.AsBigInteger();
            }

            public Type ReturnType => typeof(BigInteger);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenBigInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType == null) {
                    return value;
                }

                if (valueType.IsValueType && valueType.CanNotBeNull()) {
                    return CodegenBigInt(value, valueType);
                }

                if (valueType.IsTypeBigInteger()) {
                    return value;
                }

                var method = codegenMethodScope
                    .MakeChild(typeof(BigInteger), typeof(CoercerBigInt), codegenClassScope)
                    .AddParam(valueType, "value")
                    .Block
                    .IfRefNullReturnNull("value")
                    .MethodReturn(CodegenBigInt(CodegenExpressionBuilder.Ref("value"), valueType));
                return CodegenExpressionBuilder.LocalMethod(method, value);
            }

            public static CodegenExpression CodegenBigInt(
                CodegenExpression value,
                Type valueType)
            {
                if (valueType.IsTypeBigInteger()) {
                    return value;
                }

                return CodegenExpressionBuilder.ExprDotMethod(value, "AsBigInteger");
            }
        }
    }
}