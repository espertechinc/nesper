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
        public class CoercerFloat : SimpleNumberCoercer
        {
            public static readonly CoercerFloat INSTANCE = new CoercerFloat();

            private CoercerFloat()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsFloat();
            }

            public Type ReturnType => typeof(float?);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenFloat(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return ((valueType != typeof(float)) &&
                        (valueType != typeof(float?)))
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedFloat")
                    : value;

//                return CodegenCoerceMayNull(
//                    typeof(float),
//                    typeof(float?),
//                    "AsFloat",
//                    value,
//                    valueTypeMustNumeric,
//                    codegenMethodScope,
//                    typeof(CoercerFloat),
//                    codegenClassScope);
            }

            public static CodegenExpression CodegenFloat(
                CodegenExpression value,
                Type valueType)
            {
                return ((valueType != typeof(float)))
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsFloat")
                    : value;

                //return CodegenCoerceNonNull(typeof(float), typeof(float?), "AsFloat", @ref, type);
            }
        }
    }
}