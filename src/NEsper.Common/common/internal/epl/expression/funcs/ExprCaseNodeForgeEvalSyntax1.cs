///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForgeEvalSyntax1 : ExprEvaluator
    {
        private readonly ExprCaseNodeForge _forge;
        private readonly IList<UniformPair<ExprEvaluator>> _whenThenNodeList;
        private readonly ExprEvaluator _optionalElseExprNode;

        public ExprCaseNodeForgeEvalSyntax1(
            ExprCaseNodeForge forge,
            IList<UniformPair<ExprEvaluator>> whenThenNodeList,
            ExprEvaluator optionalElseExprNode)
        {
            this._forge = forge;
            this._whenThenNodeList = whenThenNodeList;
            this._optionalElseExprNode = optionalElseExprNode;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Case 1 expression example:
            //      case when a=b then x [when c=d then y...] [else y]
            object caseResult = null;
            bool matched = false;
            foreach (UniformPair<ExprEvaluator> p in _whenThenNodeList) {
                var whenResult = p.First.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                // If the 'when'-expression returns true
                if ((whenResult != null) && true.Equals(whenResult)) {
                    caseResult = p.Second.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    matched = true;
                    break;
                }
            }

            if (!matched && _optionalElseExprNode != null) {
                caseResult = _optionalElseExprNode.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            if (caseResult == null) {
                return null;
            }

            if ((caseResult.GetType() != _forge.EvaluationType) && _forge.IsNumericResult) {
                caseResult = TypeHelper.CoerceBoxed(caseResult, _forge.EvaluationType);
            }

            return caseResult;
        }

        public static CodegenExpression Codegen(
            ExprCaseNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type evaluationType = forge.EvaluationType == null
                ? typeof(IDictionary<object, object>)
                : forge.EvaluationType;
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprCaseNodeForgeEvalSyntax1),
                codegenClassScope);

            CodegenBlock block = methodNode.Block.DeclareVar<bool?>("when", ConstantFalse());

            foreach (UniformPair<ExprNode> pair in forge.WhenThenNodeList) {
                block.AssignRef(
                    "when",
                    pair.First.Forge.EvaluateCodegen(typeof(bool?), methodNode, exprSymbol, codegenClassScope));
                block.IfCondition(And(NotEqualsNull(Ref("when")), ExprDotName(Ref("when"), "Value")))
                    .BlockReturn(CodegenToType(forge, pair.Second, methodNode, exprSymbol, codegenClassScope));
            }

            if (forge.OptionalElseExprNode != null) {
                block.MethodReturn(
                    CodegenToType(forge, forge.OptionalElseExprNode, methodNode, exprSymbol, codegenClassScope));
            }
            else {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode);
        }

        protected internal static CodegenExpression CodegenToType(
            ExprCaseNodeForge forge,
            ExprNode node,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var nodeEvaluationType = node.Forge.EvaluationType;
            if (nodeEvaluationType.IsNullTypeSafe()) {
                return ConstantNull();
            }
            
            if (nodeEvaluationType == forge.EvaluationType || !forge.IsNumericResult) {
                return node.Forge.EvaluateCodegen(nodeEvaluationType, methodNode, exprSymbol, codegenClassScope);
            }

            return TypeHelper.CoerceNumberToBoxedCodegen(
                node.Forge.EvaluateCodegen(nodeEvaluationType, methodNode, exprSymbol, codegenClassScope),
                nodeEvaluationType,
                forge.EvaluationType);
        }
    }
} // end of namespace