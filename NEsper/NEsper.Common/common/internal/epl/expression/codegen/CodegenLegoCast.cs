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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoCast
    {
        public static CodegenExpression CastSafeFromObjectType(
            Type targetType,
            CodegenExpression value)
        {
            if (targetType == null) {
                return ConstantNull();
            }

            if (targetType == typeof(object)) {
                return value;
            }

            if (targetType == typeof(void)) {
                throw new ArgumentException("Invalid void target type for cast");
            }

            if (targetType.IsPrimitive) {
                return Cast(Boxing.GetBoxedType(targetType), value);
            }

            return Cast(targetType, value);
        }

        public static void AsDoubleNullReturnNull(
            CodegenBlock block,
            string variable,
            ExprForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type type = forge.EvaluationType;
            if (type == typeof(double)) {
                block.DeclareVar(
                    type,
                    variable,
                    forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
                return;
            }

            string holder = variable + "_";
            block.DeclareVar(
                type,
                holder,
                forge.EvaluateCodegen(type, codegenMethodScope, exprSymbol, codegenClassScope));
            if (!type.IsPrimitive) {
                block.IfRefNullReturnNull(holder);
            }

            block.DeclareVar<double>(
                variable,
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(@Ref(holder), type));
        }
    }
} // end of namespace