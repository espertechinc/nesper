///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
        private readonly ExprCaseNodeForge _forge;
        private readonly IList<UniformPair<ExprEvaluator>> _whenThenNodeList;
        private readonly ExprEvaluator _compareExprNode;
        private readonly ExprEvaluator _optionalElseExprNode;

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
            var evaluationType = forge.EvaluationType ?? typeof(IDictionary<string, object>);
            var compareType = forge.OptionalCompareExprNode.Forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprCaseNodeForgeEvalSyntax2),
                codegenClassScope);

            var checkResultType = compareType ?? typeof(object);
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
                var lhsTypeClass = lhsType;
                var lhsDeclaredType = lhsTypeClass ?? typeof(object);
                block.DeclareVar(
                    lhsDeclaredType,
                    refname,
                    pair.First.Forge.EvaluateCodegen(lhsDeclaredType, methodNode, exprSymbol, codegenClassScope));
                var compareExpression = CodegenCompare(
                    Ref("checkResult"),
                    compareType,
                    Ref(refname),
                    lhsTypeClass,
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
            else {
                var left = _forge.Coercer.CoerceBoxed(leftResult);
                var right = _forge.Coercer.CoerceBoxed(rightResult);
                return left.Equals(right);
            }
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

            var lhsClass = lhsType;
            var rhsClass = rhsType;
            if (lhsClass.IsPrimitive && rhsClass.IsPrimitive && !forge.IsMustCoerce) {
                return CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(lhs, lhsClass, rhs, rhsClass);
            }

            var block = codegenMethodScope
                .MakeChild(typeof(bool), typeof(ExprCaseNodeForgeEvalSyntax2), codegenClassScope)
                .AddParam(lhsClass, "leftResult")
                .AddParam(rhsClass, "rightResult")
                .Block;
            if (!lhsClass.IsPrimitive) {
                var ifBlock = block.IfCondition(EqualsNull(Ref("leftResult")));
                if (rhsClass.IsPrimitive) {
                    ifBlock.BlockReturn(ConstantFalse());
                }
                else {
                    ifBlock.BlockReturn(EqualsNull(Ref("rightResult")));
                }
            }

            if (!rhsClass.IsPrimitive) {
                block.IfCondition(EqualsNull(Ref("rightResult"))).BlockReturn(ConstantFalse());
            }

            CodegenMethod method;
            if (!forge.IsMustCoerce) {
                method = block.MethodReturn(
                    CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                        Ref("leftResult"),
                        lhsClass,
                        Ref("rightResult"),
                        rhsClass));
            }
            else {
                block.DeclareVar<object>(
                    "left",
                    forge.Coercer.CoerceCodegen(Ref("leftResult"), lhsClass, codegenMethodScope, codegenClassScope));
                block.DeclareVar<object>(
                    "right",
                    forge.Coercer.CoerceCodegen(Ref("rightResult"), rhsClass, codegenMethodScope, codegenClassScope));
                method = block.MethodReturn(ExprDotMethod(Ref("left"), "Equals", Ref("right")));
            }

            return LocalMethodBuild(method).Pass(lhs).Pass(rhs).Call();
        }
    }
} // end of namespace