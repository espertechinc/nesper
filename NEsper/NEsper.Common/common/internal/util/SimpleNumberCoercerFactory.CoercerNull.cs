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

namespace com.espertech.esper.common.@internal.util
{
    public partial class SimpleNumberCoercerFactory
    {
        private class CoercerNull : SimpleNumberCoercer
        {
            public static readonly CoercerNull INSTANCE = new CoercerNull(typeof(object));

            public CoercerNull(Type returnType)
            {
                ReturnType = returnType;
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce;
            }

            public Type ReturnType { get; }

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