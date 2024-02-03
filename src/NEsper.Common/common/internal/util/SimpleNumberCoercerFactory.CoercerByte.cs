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
        public class CoercerByte : Coercer
        {
            public static readonly CoercerByte INSTANCE = new CoercerByte();

            private CoercerByte()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsBoxedByte();
            }

            public Type GetReturnType(Type valueType)
            {
                return valueType.CanBeNull() ? typeof(byte?) : typeof(byte);
            }

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType, 
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return valueType.CanBeNull() 
                    ? CoerceCodegenMayNullBoxed(value, valueType, codegenMethodScope, codegenClassScope)
                    : CodegenByte(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                // return valueType != typeof(byte) &&
                //        valueType != typeof(byte?)
                //     ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedByte")
                //     : value;

                if (valueType == typeof(byte) ||
                    valueType == typeof(byte?)) {
                    return value;
                }
                
                return CodegenCoerceMayNull(
                    typeof(byte),
                    typeof(byte?),
                    "AsByte",
                    value,
                    valueType,
                    codegenMethodScope,
                    typeof(CoercerByte),
                    codegenClassScope);
            }

            public static CodegenExpression CodegenByte(
                CodegenExpression input,
                Type inputType)
            {
                return inputType != typeof(byte)
                    ? CodegenExpressionBuilder.ExprDotMethod(input, "AsByte")
                    : input;

//                return CodegenCoerceNonNull(
//                    typeof(byte), 
//                    "AsByte", 
//                    input, 
//                    inputType);
            }
        }
    }
}