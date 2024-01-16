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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        public class CoercerInt : Coercer
        {
            public static readonly CoercerInt INSTANCE = new CoercerInt();

            private CoercerInt()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsBoxedInt32();
            }

            public Type ReturnType => typeof(int?);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return valueType.CanBeNull() 
                    ? CoerceCodegenMayNullBoxed(value, valueType, null, null)
                    : CodegenInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(int),
                    typeof(int?),
                    "AsInt32",
                    param,
                    valueType,
                    codegenMethodScope,
                    typeof(CoercerInt),
                    codegenClassScope);
            }

            public static CodegenExpression CodegenInt(
                CodegenExpression value,
                Type valueType)
            {
                return valueType != typeof(short) &&
                       valueType != typeof(int)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsInt32")
                    : value;

//                return CodegenCoerceNonNull(
//                    typeof(int), 
//                    typeof(int?), 
//                    "AsInt32", 
//                    param, 
//                    type);
            }

            public static CodegenExpression CoerceCodegenMayNull(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope = null,
                CodegenClassScope codegenClassScope = null)
            {
                if (valueType == null) {
                    return CodegenExpressionBuilder.ConstantNull();
                }

                return valueType != typeof(short) &&
                       valueType != typeof(int) &&
                       valueType != typeof(int?)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedInt32")
                    : value;
            }
        }
    }
}