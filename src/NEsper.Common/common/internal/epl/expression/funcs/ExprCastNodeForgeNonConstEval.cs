///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeForgeNonConstEval : ExprEvaluator
    {
        private readonly ExprCastNode.CasterParserComputer _casterParserComputer;
        private readonly ExprEvaluator _evaluator;
        private readonly ExprCastNodeForge _forge;

        public ExprCastNodeForgeNonConstEval(
            ExprCastNodeForge forge,
            ExprEvaluator evaluator,
            ExprCastNode.CasterParserComputer casterParserComputer)
        {
            _forge = forge;
            _evaluator = evaluator;
            _casterParserComputer = casterParserComputer;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = _evaluator.Evaluate(eventsPerStream, isNewData, context);
            if (result != null) {
                result = _casterParserComputer.Compute(result, eventsPerStream, isNewData, context);
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

            var child = forge.ForgeRenderableCast.ChildNodes[0];
            var childType = child.Forge.EvaluationType;
            if (childType.IsNullTypeSafe()) {
                return ConstantNull();
            }

            var forgeEvaluationType = forge.EvaluationType;
            // TBD: Should all generated classes be boxed to nullable to allow
            //    for nullable values in case we do primitive conversion?
            if (childType.CanBeNull()) {
                forgeEvaluationType = forgeEvaluationType.GetBoxedType();
            }

            var methodNode = codegenMethodScope.MakeChild(
                forgeEvaluationType,
                typeof(ExprCastNodeForgeNonConstEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    childType,
                    "result",
                    child.Forge.EvaluateCodegen(childType, methodNode, exprSymbol, codegenClassScope));
            if (childType.CanBeNull()) {
                block.IfRefNullReturnNull("result");
            }

            var cast = forge.CasterParserComputerForge.CodegenPremade(
                forgeEvaluationType,
                Ref("result"),
                childType,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(cast);
            return LocalMethod(methodNode);
        }
    }
} // end of namespace