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
            var variableReader =
                classScope.AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(forge.Variable));

            Type variableType;
            var metaData = forge.Variable;
            if (metaData.EventType != null) {
                variableType = typeof(EventBean);
            }
            else {
                variableType = metaData.Type;
            }

            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeVariableEval),
                classScope);

            var typeInformation = ConstantNull();
            if (classScope.IsInstrumented) {
                typeInformation = classScope.AddOrGetDefaultFieldSharable(
                    new EPChainableTypeCodegenSharable(new EPChainableTypeClass(variableType), classScope));
            }

            var block = methodNode.Block
                .DeclareVar(variableType, "result", Cast(variableType, ExprDotName(variableReader, "Value")))
                .Apply(
                    InstrumentationCode.Instblock(
                        classScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("result"),
                        Constant(forge.ChainForge.Length)));

            var chain = ExprDotNodeUtility.EvaluateChainCodegen(
                methodNode,
                exprSymbol,
                classScope,
                Ref("result"),
                variableType,
                forge.ChainForge,
                forge.ResultWrapLambda);

            if (forge.EvaluationType != typeof(void)) {
                block.DeclareVar(forge.EvaluationType, "returned", chain)
                    .Apply(InstrumentationCode.Instblock(classScope, "aExprDotChain"))
                    .MethodReturn(Ref("returned"));
            }
            else {
                block
                    .Expression(chain)
                    .Apply(InstrumentationCode.Instblock(classScope, "aExprDotChain"));
            }

            return LocalMethod(methodNode);
        }
    }
} // end of namespace