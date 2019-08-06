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
        public class CoercerLong : SimpleNumberCoercer
        {
            public static readonly CoercerLong INSTANCE = new CoercerLong();

            private CoercerLong()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsLong();
            }

            public Type ReturnType => typeof(long);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenLong(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueTypeMustNumeric,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenLongMayNullBox(param, valueTypeMustNumeric, codegenMethodScope, codegenClassScope);
            }

            public static CodegenExpression CodegenLong(
                CodegenExpression param,
                Type type)
            {
                return CodegenCoerceNonNull(typeof(long), typeof(long), "AsLong", param, type);
            }

            public static CodegenExpression CodegenLongMayNullBox(
                CodegenExpression param,
                Type type,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(long),
                    typeof(long),
                    "AsLong",
                    param,
                    type,
                    codegenMethodScope,
                    typeof(CoercerLong),
                    codegenClassScope);
            }
        }
    }
}