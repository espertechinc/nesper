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
        public class CoercerDecimal : Coercer
        {
            public static readonly CoercerDecimal INSTANCE = new CoercerDecimal();

            private CoercerDecimal()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsDecimal();
            }

            public Type ReturnType => typeof(decimal);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenDecimal(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if ((param is CodegenExpressionConstantNull) ||
                    (param is CodegenExpressionConstant exprConstant &&
                     exprConstant.IsNull)) {
                    return param;
                }
                
                return valueType.IsDecimal() 
                    ? CodegenExpressionBuilder.ExprDotMethod(param, "AsBoxedDecimal")
                    : param;

//                return CodegenCoerceMayNull(
//                    typeof(decimal),
//                    typeof(decimal?),
//                    "AsDecimal",
//                    param,
//                    valueTypeMustNumeric,
//                    codegenMethodScope,
//                    typeof(CoercerDecimal),
//                    codegenClassScope);
            }

            public static CodegenExpression CodegenDecimal(
                CodegenExpression param,
                Type type)
            {
                return type != typeof(decimal)
                    ? CodegenExpressionBuilder.ExprDotMethod(param, "AsDecimal")
                    : param;

//                return CodegenCoerceNonNull(typeof(decimal), typeof(decimal?), "AsDecimal", param, type);
            }
        }
    }
}