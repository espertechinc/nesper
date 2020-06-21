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
        public class CoercerShort : Coercer
        {
            public static readonly CoercerShort INSTANCE = new CoercerShort();

            private CoercerShort()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsInt16();
            }

            public Type ReturnType => typeof(short);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenShort(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return ((valueType != typeof(short)) &&
                        (valueType != typeof(short?)))
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedInt16")
                    : value;

//                return CodegenCoerceMayNull(
//                    typeof(short),
//                    typeof(short?),
//                    "AsInt16",
//                    value,
//                    valueTypeMustNumeric,
//                    codegenMethodScope,
//                    typeof(CoercerShort),
//                    codegenClassScope);
            }

            public static CodegenExpression CodegenShort(
                CodegenExpression input,
                Type inputType)
            {
                return ((inputType != typeof(short)) &&
                        (inputType != typeof(short?)))
                    ? CodegenExpressionBuilder.ExprDotMethod(input, "AsInt16")
                    : input;
                //return CodegenCoerceNonNull(typeof(short), typeof(short?), "AsInt16", input, inputType);
            }
        }
    }
}