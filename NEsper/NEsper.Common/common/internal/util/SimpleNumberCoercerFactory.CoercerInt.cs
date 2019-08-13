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
        public class CoercerInt : SimpleNumberCoercer
        {
            public static readonly CoercerInt INSTANCE = new CoercerInt();

            private CoercerInt()
            {
            }

            public object CoerceBoxed(object numToCoerce)
            {
                return numToCoerce.AsInt();
            }

            public Type ReturnType => typeof(int);

            public CodegenExpression CoerceCodegen(
                CodegenExpression value,
                Type valueType)
            {
                return CodegenInt(value, valueType);
            }

            public CodegenExpression CoerceCodegenMayNullBoxed(
                CodegenExpression param,
                Type valueTypeMustNumeric,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(int),
                    typeof(int?),
                    "AsInt",
                    param,
                    valueTypeMustNumeric,
                    codegenMethodScope,
                    typeof(CoercerInt),
                    codegenClassScope);
            }

            public static CodegenExpression CodegenInt(
                CodegenExpression param,
                Type type)
            {
                return CodegenCoerceNonNull(typeof(int), typeof(int), "AsInt", param, type);
            }

            public static CodegenExpression CoerceCodegenMayNull(
                CodegenExpression param,
                Type type,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                return CodegenCoerceMayNull(
                    typeof(int),
                    typeof(int?),
                    "AsInt",
                    param,
                    type,
                    codegenMethodScope,
                    typeof(CoercerInt),
                    codegenClassScope);
            }
        }
    }
}