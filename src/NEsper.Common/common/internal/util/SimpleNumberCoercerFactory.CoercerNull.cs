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

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        private class CoercerNull : Coercer
        {
            public static readonly CoercerNull INSTANCE = new CoercerNull(typeof(object));
            private readonly Type _returnType;

            public CoercerNull(Type returnType)
            {
                _returnType = returnType;
            }

            public object CoerceBoxed(object value)
            {
                return value;
            }

            public Type GetReturnType(Type valueType) => _returnType;

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