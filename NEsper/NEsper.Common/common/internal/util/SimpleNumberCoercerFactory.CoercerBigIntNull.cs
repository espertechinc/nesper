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

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        private class CoercerBigIntNull : Coercer,
            BigIntegerCoercer
        {
            public static readonly CoercerBigIntNull INSTANCE = new CoercerBigIntNull();

            private CoercerBigIntNull()
            {
            }

            public CodegenExpression CoerceBoxedBigIntCodegen(
                CodegenExpression expr,
                Type type)
            {
                return expr;
            }

            public BigInteger CoerceBoxedBigInt(object numToCoerce)
            {
                return (BigInteger) numToCoerce;
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type ReturnType => typeof(BigInteger);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return value;
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression value,
                Type valueType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return value;
            }
        }
    }
}