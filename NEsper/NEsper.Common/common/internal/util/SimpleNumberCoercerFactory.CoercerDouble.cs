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
        public class CoercerDouble : SimpleNumberCoercer
        {
            public static readonly CoercerDouble INSTANCE = new CoercerDouble();

            private CoercerDouble()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsDouble();
            }

            public Type ReturnType => typeof(double?);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenDouble(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueTypeMustNumeric,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(double),
                    typeof(double?),
                    "AsDouble",
                    param,
                    valueTypeMustNumeric,
                    codegenMethodScope,
                    typeof(CoercerDouble),
                    codegenClassScope);
            }

            public static CodegenExpression CodegenDouble(
                CodegenExpression param,
                Type type)
            {
                return CodegenCoerceNonNull(typeof(double), typeof(double?), "AsDouble", param, type);
            }

            public static CodegenExpression CodegenDoubleMayNullBoxedIncludeBig(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                if (valueType == typeof(BigInteger) || valueType == typeof(decimal)) {
                    return CodegenExpressionBuilder.ExprDotMethod(value, "DoubleValue");
                }

                return CodegenCoerceMayNull(
                    typeof(double),
                    typeof(double?),
                    "AsDouble",
                    value,
                    valueType,
                    codegenMethodScope,
                    typeof(CoercerDouble),
                    codegenClassScope);
            }
        }
    }
}