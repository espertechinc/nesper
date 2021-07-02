///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoMayVoid
    {
        public static CodegenExpression ExpressionMayVoid(
            Type requiredType,
            ExprForge forge,
            CodegenMethod parentNode,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            requiredType = requiredType.IsNullTypeSafe()
                ? typeof(object)
                : requiredType;

            var evalType = forge.EvaluationType;
            if (!evalType.IsVoid()) {
                return forge.EvaluateCodegen(requiredType, parentNode, exprSymbol, codegenClassScope);
            }

            var methodNode = parentNode.MakeChild(
                typeof(object),
                typeof(CodegenLegoMayVoid),
                codegenClassScope);
            
            methodNode.Block
                .Expression(forge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(ConstantNull());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace