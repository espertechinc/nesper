///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        public class CoercerLong : Coercer
        {
            public static readonly CoercerLong INSTANCE = new CoercerLong();

            private CoercerLong()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsInt64();
            }

            public Type ReturnType => typeof(long);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenLong(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenLongMayNullBox(param, valueType, codegenMethodScope, codegenClassScope);
            }

            public static CodegenExpression CodegenLong(
                CodegenExpression value,
                Type valueType)
            {
                return valueType != typeof(short) &&
                       valueType != typeof(int) &&
                       valueType != typeof(long)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsInt64")
                    : value;

                //return CodegenCoerceNonNull(typeof(long), typeof(long), "AsInt64", param, type);
            }

            public static CodegenExpression CodegenLongMayNullBox(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return valueType != typeof(short) &&
                       valueType != typeof(int) &&
                       valueType != typeof(long) &&
                       valueType != typeof(long?)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedInt64")
                    : value;

//                return CodegenCoerceMayNull(
//                    typeof(long),
//                    typeof(long?),
//                    "AsInt64",
//                    param,
//                    type,
//                    codegenMethodScope,
//                    typeof(CoercerLong),
//                    codegenClassScope);
            }
        }
    }
}