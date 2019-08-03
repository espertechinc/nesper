///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeVariableEval
    {
        public static CodegenExpression Codegen(
            ExprDotNodeForgeVariable forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope classScope)
        {
            CodegenExpressionField variableReader =
                classScope.AddOrGetFieldSharable(new VariableReaderCodegenFieldSharable(forge.Variable));

            Type variableType;
            VariableMetaData metaData = forge.Variable;
            if (metaData.EventType != null) {
                variableType = typeof(EventBean);
            }
            else {
                variableType = metaData.Type;
            }

            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeVariableEval),
                classScope);

            CodegenExpression typeInformation = ConstantNull();
            if (classScope.IsInstrumented) {
                typeInformation = classScope.AddOrGetFieldSharable(
                    new EPTypeCodegenSharable(new ClassEPType(variableType), classScope));
            }

            CodegenBlock block = methodNode.Block
                .DeclareVar(variableType, "result", Cast(variableType, ExprDotName(variableReader, "Value")))
                .Apply(
                    InstrumentationCode.Instblock(
                        classScope,
                        "qExprDotChain",
                        typeInformation,
                        @Ref("result"),
                        Constant(forge.ChainForge.Length)));
            CodegenExpression chain = ExprDotNodeUtility.EvaluateChainCodegen(
                methodNode,
                exprSymbol,
                classScope,
                @Ref("result"),
                variableType,
                forge.ChainForge,
                forge.ResultWrapLambda);
            block.DeclareVar(forge.EvaluationType, "returned", chain)
                .Apply(InstrumentationCode.Instblock(classScope, "aExprDotChain"))
                .MethodReturn(@Ref("returned"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace