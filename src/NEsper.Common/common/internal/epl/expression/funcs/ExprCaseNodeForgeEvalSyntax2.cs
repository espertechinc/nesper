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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.funcs.ExprCaseNodeForgeEvalSyntax1;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForgeEvalSyntax2 : ExprEvaluator
    {
        private readonly ExprEvaluator _compareExprNode;

        private readonly ExprCaseNodeForge _forge;
        private readonly ExprEvaluator _optionalElseExprNode;
        private readonly IList<UniformPair<ExprEvaluator>> _whenThenNodeList;

        internal ExprCaseNodeForgeEvalSyntax2(
            ExprCaseNodeForge forge,
            IList<UniformPair<ExprEvaluator>> whenThenNodeList,
            ExprEvaluator compareExprNode,
            ExprEvaluator optionalElseExprNode)
        {
            this._forge = forge;
            this._whenThenNodeList = whenThenNodeList;
            this._compareExprNode = compareExprNode;
            this._optionalElseExprNode = optionalElseExprNode;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Case 2 expression example:
            //      case p when p1 then x [when p2 then y...] [else z]
            var checkResult = _compareExprNode.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            object caseResult = null;
            var matched = false;
            foreach (var p in _whenThenNodeList) {
                var whenResult = p.First.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (Compare(checkResult, whenResult)) {
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

            if (caseResult.GetType() != _forge.EvaluationType && _forge.IsNumericResult) {
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
            var evaluationType = forge.EvaluationType == null
                ? typeof(IDictionary<object, object>)
                : forge.EvaluationType;
            var compareType = forge.OptionalCompareExprNode.Forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprCaseNodeForgeEvalSyntax2),
                codegenClassScope);

            var checkResultType = compareType == null ? typeof(object) : compareType;
            var block = methodNode.Block
                .DeclareVar(
                    checkResultType,
                    "checkResult",
                    forge.OptionalCompareExprNode.Forge.EvaluateCodegen(
                        checkResultType,
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            var num = 0;
            foreach (var pair in forge.WhenThenNodeList) {
                var refname = "r" + num;
                var lhsType = pair.First.Forge.EvaluationType;
                var lhsTypeClass = lhsType.IsNullTypeSafe() ? null : lhsType;
                var lhsDeclaredType = lhsTypeClass == null ? typeof(object) : lhsType;
                block.DeclareVar(
                    lhsDeclaredType,
                    refname,
                    pair.First.Forge.EvaluateCodegen(lhsDeclaredType, methodNode, exprSymbol, codegenClassScope));
                var compareExpression = CodegenCompare(
                    Ref("checkResult"),
                    compareType,
                    Ref(refname),
                    lhsType,
                    forge,
                    methodNode,
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

            if (!_forge.IsMustCoerce) {
                return leftResult.Equals(rightResult);
            }

            var left = _forge.Coercer.CoerceBoxed(leftResult);
            var right = _forge.Coercer.CoerceBoxed(rightResult);
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
            if (lhsType.IsNullTypeSafe()) {
                return EqualsNull(rhs);
            }

            if (rhsType.IsNullTypeSafe()) {
                return EqualsNull(lhs);
            }

            if (lhsType.CanNotBeNull() && rhsType.CanNotBeNull() && !forge.IsMustCoerce) {
                return CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(lhs, lhsType, rhs, rhsType);
            }

            var block = codegenMethodScope
                .MakeChild(typeof(bool), typeof(ExprCaseNodeForgeEvalSyntax2), codegenClassScope)
                .AddParam(lhsType, "leftResult")
                .AddParam(rhsType, "rightResult")
                .Block;
            if (lhsType.CanBeNull()) {
                var ifBlock = block.IfCondition(EqualsNull(Ref("leftResult")));
                if (rhsType.CanNotBeNull()) {
                    ifBlock.BlockReturn(ConstantFalse());
                }
                else {
                    ifBlock.BlockReturn(EqualsNull(Ref("rightResult")));
                }
            }

            if (rhsType.CanBeNull()) {
                block.IfCondition(EqualsNull(Ref("rightResult"))).BlockReturn(ConstantFalse());
            }

            CodegenMethod method;
            if (!forge.IsMustCoerce) {
                method = block.MethodReturn(
                    CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                        Ref("leftResult"),
                        lhsType,
                        Ref("rightResult"),
                        rhsType));
            }
            else {
                block.DeclareVar<object>("left", forge.Coercer.CoerceCodegen(Ref("leftResult"), lhsType));
                block.DeclareVar<object>("right", forge.Coercer.CoerceCodegen(Ref("rightResult"), rhsType));
                method = block.MethodReturn(StaticMethod<object>("Equals", Ref("left"), Ref("right")));
            }

            return LocalMethodBuild(method).Pass(lhs).Pass(rhs).Call();
        }
    }
} // end of namespace