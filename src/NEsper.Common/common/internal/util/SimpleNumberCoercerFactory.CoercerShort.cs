///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        public class CoercerShort : Coercer
        {
            public static readonly CoercerShort INSTANCE = new CoercerShort();

            private CoercerShort()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsBoxedInt16();
            }

            public Type GetReturnType(Type valueType)
            {
                return valueType.CanBeNull() ? typeof(short?) : typeof(short);
            }
            
            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return valueType.CanBeNull() 
                    ? CoerceCodegenMayNullBoxed(value, valueType, codegenMethodScope, codegenClassScope)
                    : CodegenShort(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType == typeof(short) ||
                    valueType == typeof(short?)) {
                    return value;
                }

                return CodegenCoerceMayNull(
                    typeof(short),
                    typeof(short?),
                    "AsInt16",
                    value,
                    valueType,
                    codegenMethodScope,
                    typeof(CoercerShort),
                    codegenClassScope);
            }

            public static CodegenExpression CodegenShort(
                CodegenExpression input,
                Type inputType)
            {
                return inputType != typeof(short) &&
                       inputType != typeof(short?)
                    ? CodegenExpressionBuilder.ExprDotMethod(input, "AsInt16")
                    : input;
                //return CodegenCoerceNonNull(typeof(short), typeof(short?), "AsInt16", input, inputType);
            }
        }
    }
}