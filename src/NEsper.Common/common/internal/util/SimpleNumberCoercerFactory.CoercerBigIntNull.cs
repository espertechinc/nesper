///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private class CoercerBigIntNull : Coercer,
            BigIntegerCoercer
        {
            public static readonly CoercerBigIntNull INSTANCE = new CoercerBigIntNull();

            private CoercerBigIntNull()
            {
            }

            public CodegenExpression CoerceBoxedBigIntCodegen(
                CodegenExpression expr,
                Type type, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
            {
                return expr;
            }

            public BigInteger CoerceBoxedBigInt(object numToCoerce)
            {
                return numToCoerce.AsBigInteger();
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type GetReturnType(Type valueType) => typeof(BigInteger?);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
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