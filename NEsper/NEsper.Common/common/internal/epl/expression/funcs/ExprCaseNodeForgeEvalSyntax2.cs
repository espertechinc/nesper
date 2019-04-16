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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.funcs.ExprCaseNodeForgeEvalSyntax1;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForgeEvalSyntax2 : ExprEvaluator
    {
        private readonly ExprEvaluator compareExprNode;

        private readonly ExprCaseNodeForge forge;
        private readonly ExprEvaluator optionalElseExprNode;
        private readonly IList<UniformPair<ExprEvaluator>> whenThenNodeList;

        internal ExprCaseNodeForgeEvalSyntax2(
            ExprCaseNodeForge forge,
            IList<UniformPair<ExprEvaluator>> whenThenNodeList,
            ExprEvaluator compareExprNode,
            ExprEvaluator optionalElseExprNode)
        {
            this.forge = forge;
            this.whenThenNodeList = whenThenNodeList;
            this.compareExprNode = compareExprNode;
            this.optionalElseExprNode = optionalElseExprNode;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Case 2 expression example:
            //      case p when p1 then x [when p2 then y...] [else z]
            var checkResult = compareExprNode.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            object caseResult = null;
            var matched = false;
            foreach (var p in whenThenNodeList) {
                var whenResult = p.First.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (Compare(checkResult, whenResult)) {
                    caseResult = p.Second.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    matched = true;
                    break;
                }
            }

            if (!matched && optionalElseExprNode != null) {
                caseResult = optionalElseExprNode.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            if (caseResult == null) {
                return null;
            }

            if (caseResult.GetType() != forge.EvaluationType && forge.IsNumericResult) {
                caseResult = TypeHelper.CoerceBoxed(caseResult, forge.EvaluationType);
            }

            return caseResult;
        }

        public static CodegenExpression Codegen(
            ExprCaseNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.EvaluationType == null
                ? typeof(IDictionary<object, object>)
                : forge.EvaluationType;
            var compareType = forge.OptionalCompareExprNode.Forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType, typeof(ExprCaseNodeForgeEvalSyntax2), codegenClassScope);

            var checkResultType = compareType == null ? typeof(object) : compareType;
            var block = methodNode.Block
                .DeclareVar(
                    checkResultType, "checkResult",
                    forge.OptionalCompareExprNode.Forge.EvaluateCodegen(
                        checkResultType, methodNode, exprSymbol, codegenClassScope));
            var num = 0;
            foreach (var pair in forge.WhenThenNodeList) {
                var refname = "r" + num;
                var lhsType = pair.First.Forge.EvaluationType;
                var lhsDeclaredType = lhsType == null ? typeof(object) : lhsType;
                block.DeclareVar(
                    lhsDeclaredType, refname,
                    pair.First.Forge.EvaluateCodegen(lhsDeclaredType, methodNode, exprSymbol, codegenClassScope));
                var compareExpression = CodegenCompare(
                    Ref("checkResult"), compareType, Ref(refname), pair.First.Forge.EvaluationType, forge, methodNode,
                    codegenClassScope);
                block.IfCondition(compareExpression)
                    .BlockReturn(CodegenToType(forge, pair.Second, methodNode, exprSymbol, codegenClassScope));
                num++;
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

        private bool Compare(
            object leftResult,
            object rightResult)
        {
            if (leftResult == null) {
                return rightResult == null;
            }

            if (rightResult == null) {
                return false;
            }

            if (!forge.IsMustCoerce) {
                return leftResult.Equals(rightResult);
            }

            var left = forge.Coercer.CoerceBoxed(leftResult);
            var right = forge.Coercer.CoerceBoxed(rightResult);
            return left.Equals(right);
        }

        private static CodegenExpression CodegenCompare(
            CodegenExpressionRef lhs,
            Type lhsType,
            CodegenExpressionRef rhs,
            Type rhsType,
            ExprCaseNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (lhsType == null) {
                return EqualsNull(rhs);
            }

            if (rhsType == null) {
                return EqualsNull(lhs);
            }

            if (lhsType.IsPrimitive && rhsType.IsPrimitive && !forge.IsMustCoerce) {
                return CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(lhs, lhsType, rhs, rhsType);
            }

            var block = codegenMethodScope
                .MakeChild(typeof(bool), typeof(ExprCaseNodeForgeEvalSyntax2), codegenClassScope)
                .AddParam(lhsType, "leftResult").AddParam(rhsType, "rightResult").Block;
            if (!lhsType.IsPrimitive) {
                var ifBlock = block.IfCondition(EqualsNull(Ref("leftResult")));
                if (rhsType.IsPrimitive) {
                    ifBlock.BlockReturn(ConstantFalse());
                }
                else {
                    ifBlock.BlockReturn(EqualsNull(Ref("rightResult")));
                }
            }

            if (!rhsType.IsPrimitive) {
                block.IfCondition(EqualsNull(Ref("rightResult"))).BlockReturn(ConstantFalse());
            }

            CodegenMethod method;
            if (!forge.IsMustCoerce) {
                method = block.MethodReturn(
                    CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                        Ref("leftResult"), lhsType, Ref("rightResult"), rhsType));
            }
            else {
                block.DeclareVar(typeof(object), "left", forge.Coercer.CoerceCodegen(Ref("leftResult"), lhsType));
                block.DeclareVar(typeof(object), "right", forge.Coercer.CoerceCodegen(Ref("rightResult"), rhsType));
                method = block.MethodReturn(ExprDotMethod(Ref("left"), "equals", Ref("right")));
            }

            return LocalMethodBuild(method).Pass(lhs).Pass(rhs).Call();
        }
    }
} // end of namespace