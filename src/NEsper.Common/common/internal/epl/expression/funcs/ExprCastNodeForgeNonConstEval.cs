///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeForgeNonConstEval : ExprEvaluator
    {
        private readonly ExprCastNodeForge forge;
        private readonly ExprEvaluator evaluator;
        private readonly ExprCastNode.CasterParserComputer casterParserComputer;

        public ExprCastNodeForgeNonConstEval(
            ExprCastNodeForge forge,
            ExprEvaluator evaluator,
            ExprCastNode.CasterParserComputer casterParserComputer)
        {
            this.forge = forge;
            this.evaluator = evaluator;
            this.casterParserComputer = casterParserComputer;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = evaluator.Evaluate(eventsPerStream, isNewData, context);
            if (result != null) {
                result = casterParserComputer.Compute(result, eventsPerStream, isNewData, context);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            ExprCastNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (forge.EvaluationType == null) {
                return ConstantNull();
            }

            ExprNode child = forge.ForgeRenderableCast.ChildNodes[0];
            var type = child.Forge.EvaluationType;
            if (type == null) {
                return ConstantNull();
            }

            var typeClass = type;
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprCastNodeForgeNonConstEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    typeClass,
                    "result",
                    child.Forge.EvaluateCodegen(typeClass, methodNode, exprSymbol, codegenClassScope));
            if (!typeClass.IsPrimitive) {
                block.IfRefNullReturnNull("result");
            }

            var cast = forge.CasterParserComputerForge.CodegenPremade(
                forge.EvaluationType,
                Ref("result"),
                typeClass,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(cast);
            return LocalMethod(methodNode);
        }
    }
} // end of namespace