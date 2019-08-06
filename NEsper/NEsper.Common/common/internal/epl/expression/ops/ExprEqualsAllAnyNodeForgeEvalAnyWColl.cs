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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsAllAnyNodeForgeEvalAnyWColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprEqualsAllAnyNodeForge forge;

        public ExprEqualsAllAnyNodeForgeEvalAnyWColl(
            ExprEqualsAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
            return result;
        }

        private object EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var leftResult = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            // coerce early if testing without collections
            if (forge.IsMustCoerce && leftResult != null) {
                leftResult = forge.Coercer.CoerceBoxed(leftResult);
            }

            return CompareAny(leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAny(
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var isNot = forge.ForgeRenderable.IsNot;
            var len = forge.ForgeRenderable.ChildNodes.Length - 1;
            var hasNonNullRow = false;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult == null) {
                    hasNullRow = true;
                    continue;
                }

                if (rightResult is Array rightResultArray) {
                    var arrayLength = rightResultArray.Length;
                    if (arrayLength > 0 && leftResult == null) {
                        return null;
                    }

                    for (var index = 0; index < arrayLength; index++) {
                        var item = rightResultArray.GetValue(index);
                        if (item == null) {
                            hasNullRow = true;
                            continue;
                        }

                        hasNonNullRow = true;
                        if (!forge.IsMustCoerce) {
                            if (!isNot && leftResult.Equals(item) || isNot && !leftResult.Equals(item)) {
                                return true;
                            }
                        }
                        else {
                            if (!item.IsNumber()) {
                                continue;
                            }

                            var left = forge.Coercer.CoerceBoxed(leftResult);
                            var right = forge.Coercer.CoerceBoxed(item);
                            if (!isNot && left.Equals(right) || isNot && !left.Equals(right)) {
                                return true;
                            }
                        }
                    }
                }
                else if (rightResult is IDictionary<object, object>) {
                    if (leftResult == null) {
                        return null;
                    }

                    var coll = (IDictionary<object, object>) rightResult;
                    if (!isNot && coll.ContainsKey(leftResult) || isNot && !coll.ContainsKey(leftResult)) {
                        return true;
                    }

                    hasNonNullRow = true;
                }
                else if (rightResult is ICollection<object>) {
                    if (leftResult == null) {
                        return null;
                    }

                    var coll = (ICollection<object>) rightResult;
                    if (!isNot && coll.Contains(leftResult) || isNot && !coll.Contains(leftResult)) {
                        return true;
                    }

                    hasNonNullRow = true;
                }
                else {
                    if (leftResult == null) {
                        return null;
                    }

                    hasNonNullRow = true;
                    if (!forge.IsMustCoerce) {
                        if (!isNot && leftResult.Equals(rightResult) || isNot && !leftResult.Equals(rightResult)) {
                            return true;
                        }
                    }
                    else {
                        var left = forge.Coercer.CoerceBoxed(leftResult);
                        var right = forge.Coercer.CoerceBoxed(rightResult);
                        if (!isNot && left.Equals(right) || isNot && !left.Equals(right)) {
                            return true;
                        }
                    }
                }
            }

            if (!hasNonNullRow || hasNullRow) {
                return null;
            }

            return false;
        }

        public static CodegenExpression Codegen(
            ExprEqualsAllAnyNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forges = ExprNodeUtilityQuery.GetForges(forge.ForgeRenderable.ChildNodes);
            var isNot = forge.ForgeRenderable.IsNot;

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprEqualsAllAnyNodeForgeEvalAnyWColl),
                codegenClassScope);

            var block = methodNode.Block;
            var leftTypeUncoerced = forges[0].EvaluationType;
            block.DeclareVar(
                leftTypeUncoerced,
                "left",
                forges[0].EvaluateCodegen(leftTypeUncoerced, methodNode, exprSymbol, codegenClassScope));
            block.DeclareVar(
                forge.CoercionTypeBoxed,
                "leftCoerced",
                !forge.IsMustCoerce
                    ? Ref("left")
                    : forge.Coercer.CoerceCodegenMayNullBoxed(
                        Ref("left"),
                        leftTypeUncoerced,
                        methodNode,
                        codegenClassScope));
            block.DeclareVar<bool>("hasNonNullRow", ConstantFalse());
            block.DeclareVar<bool>("hasNullRow", ConstantFalse());

            for (var i = 1; i < forges.Length; i++) {
                var refforge = forges[i];
                var refname = "r" + i;
                var reftype = forges[i].EvaluationType;

                if (reftype != null && reftype.IsImplementsInterface(typeof(ICollection<object>))) {
                    block.IfRefNullReturnNull("left")
                        .DeclareVar<ICollection<object>>(
                            refname,
                            refforge.EvaluateCodegen(
                                typeof(ICollection<object>),
                                methodNode,
                                exprSymbol,
                                codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(isNot, ExprDotMethod(Ref(refname), "Contains", Ref("left"))))
                        .BlockReturn(ConstantTrue());
                }
                else if (reftype != null && reftype.IsImplementsInterface(typeof(IDictionary<object, object>))) {
                    block.IfRefNullReturnNull("left")
                        .DeclareVar<IDictionary<object, object>>(
                            refname,
                            refforge.EvaluateCodegen(
                                typeof(IDictionary<object, object>),
                                methodNode,
                                exprSymbol,
                                codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(isNot, ExprDotMethod(Ref(refname), "containsKey", Ref("left"))))
                        .BlockReturn(ConstantTrue());
                }
                else if (reftype != null && reftype.IsArray) {
                    var arrayBlock = block.IfRefNullReturnNull("left")
                        .DeclareVar(
                            reftype,
                            refname,
                            refforge.EvaluateCodegen(reftype, methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse();

                    var forLoop = arrayBlock.ForLoopIntSimple("i", ArrayLength(Ref(refname)));
                    var arrayAtIndex = ArrayAtIndex(Ref(refname), Ref("i"));
                    forLoop.DeclareVar(
                        forge.CoercionTypeBoxed,
                        "item",
                        forge.Coercer == null
                            ? arrayAtIndex
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                arrayAtIndex,
                                reftype.GetElementType(),
                                methodNode,
                                codegenClassScope));

                    var forLoopElse = forLoop.IfCondition(EqualsNull(Ref("item")))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse();
                    forLoopElse.AssignRef("hasNonNullRow", ConstantTrue());
                    forLoopElse.IfCondition(
                            NotOptional(isNot, ExprDotMethod(Ref("leftCoerced"), "Equals", Ref("item"))))
                        .BlockReturn(ConstantTrue());
                }
                else {
                    block.IfRefNullReturnNull("leftCoerced");
                    block.DeclareVar(
                        forge.CoercionTypeBoxed,
                        refname,
                        forge.Coercer == null
                            ? refforge.EvaluateCodegen(
                                forge.CoercionTypeBoxed,
                                methodNode,
                                exprSymbol,
                                codegenClassScope)
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                refforge.EvaluateCodegen(
                                    forge.CoercionTypeBoxed,
                                    methodNode,
                                    exprSymbol,
                                    codegenClassScope),
                                reftype,
                                methodNode,
                                codegenClassScope));
                    var ifRightNotNull = block.IfRefNotNull(refname);
                    {
                        ifRightNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                        ifRightNotNull
                            .IfCondition(NotOptional(isNot, ExprDotMethod(Ref("leftCoerced"), "Equals", Ref(refname))))
                            .BlockReturn(ConstantTrue());
                    }
                    ifRightNotNull.IfElse()
                        .AssignRef("hasNullRow", ConstantTrue());
                }
            }

            block.IfCondition(Or(Not(Ref("hasNonNullRow")), Ref("hasNullRow"))).BlockReturn(ConstantNull());
            block.MethodReturn(ConstantFalse());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace