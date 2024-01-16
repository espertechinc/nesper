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
        public class CoercerDouble : Coercer
        {
            public static readonly CoercerDouble INSTANCE = new CoercerDouble();

            private CoercerDouble()
            {
            }

            public object CoerceBoxed(object value)
            {
                return value.AsBoxedDouble();
            }

            public Type ReturnType => typeof(double?);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return valueType.CanBeNull() 
                    ? CoerceCodegenMayNullBoxed(value, valueType, null, null)
                    : CodegenDouble(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return valueType != typeof(float) &&
                       valueType != typeof(double) &&
                       valueType != typeof(double?)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedDouble")
                    : value;
            }

            public static CodegenExpression CodegenDouble(
                CodegenExpression value,
                Type valueType)
            {
                return valueType != typeof(float) &&
                       valueType != typeof(double)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsDouble")
                    : value;
            }

            public static CodegenExpression CodegenDoubleMayNullBoxedIncludeBig(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType.IsTypeBigInteger() || valueType.IsTypeDecimal()) {
                    return CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedDouble");
                }

                return valueType != typeof(double) || valueType != typeof(double?)
                    ? CodegenExpressionBuilder.ExprDotMethod(value, "AsBoxedDouble")
                    : value;
            }
        }
    }
}