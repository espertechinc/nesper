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
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsAllAnyNodeForgeEvalAllWColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprEqualsAllAnyNodeForge forge;

        public ExprEqualsAllAnyNodeForgeEvalAllWColl(ExprEqualsAllAnyNodeForge forge, ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
            return result;
        }

        private object EvaluateInternal(
            EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var leftResult = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            // coerce early if testing without collections
            if (forge.IsMustCoerce && leftResult != null) {
                leftResult = forge.Coercer.CoerceBoxed(leftResult);
            }

            return CompareAll(
                forge.ForgeRenderable.IsNot, leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAll(
            bool isNot, object leftResult, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var len = forge.ForgeRenderable.ChildNodes.Length - 1;
            var hasNonNullRow = false;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult == null) {
                    hasNullRow = true;
                }
                else if (rightResult is Array array) {
                    var arrayLength = array.Length;
                    for (var index = 0; index < arrayLength; index++)
                    {
                        object item = array.GetValue(index);
                        if (item == null)
                        {
                            hasNullRow = true;
                            continue;
                        }

                        if (leftResult == null)
                        {
                            return null;
                        }

                        hasNonNullRow = true;
                        if (!forge.IsMustCoerce)
                        {
                            if (!isNot && !leftResult.Equals(item) || isNot && leftResult.Equals(item))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!(item.IsNumber()))
                            {
                                continue;
                            }

                            var left = forge.Coercer.CoerceBoxed(leftResult);
                            var right = forge.Coercer.CoerceBoxed(item);
                            if (!isNot && !left.Equals(right) || isNot && left.Equals(right))
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (rightResult.GetType().IsGenericDictionary()) {
                    if (leftResult == null) {
                        return null;
                    }

                    hasNonNullRow = true;
                    var coll = rightResult.UnwrapDictionary();
                    if (!isNot && !coll.ContainsKey(leftResult) || isNot && coll.ContainsKey(leftResult)) {
                        return false;
                    }
                }
                else if (rightResult.GetType().IsGenericCollection())
                {
                    if (leftResult == null)
                    {
                        return null;
                    }

                    hasNonNullRow = true;
                    var coll = rightResult.Unwrap<object>();
                    if (!isNot && !coll.Contains(leftResult) || isNot && coll.Contains(leftResult))
                    {
                        return false;
                    }
                }
                else {
                    if (leftResult == null) {
                        return null;
                    }

                    if (!forge.IsMustCoerce) {
                        if (!isNot && !leftResult.Equals(rightResult) || isNot && leftResult.Equals(rightResult)) {
                            return false;
                        }
                    }
                    else {
                        var left = forge.Coercer.CoerceBoxed(leftResult);
                        var right = forge.Coercer.CoerceBoxed(rightResult);
                        if (!isNot && !left.Equals(right) || isNot && left.Equals(right)) {
                            return false;
                        }
                    }
                }
            }

            if (!hasNonNullRow || hasNullRow) {
                return null;
            }

            return true;
        }

        public static CodegenExpression Codegen(
            ExprEqualsAllAnyNodeForge forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forges = ExprNodeUtilityQuery.GetForges(forge.ForgeRenderable.ChildNodes);
            var isNot = forge.ForgeRenderable.IsNot;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?), typeof(ExprEqualsAllAnyNodeForgeEvalAllWColl), codegenClassScope);

            var block = methodNode.Block;

            var leftTypeUncoerced = forges[0].EvaluationType;
            block.DeclareVar(
                leftTypeUncoerced, "left",
                forges[0].EvaluateCodegen(leftTypeUncoerced, methodNode, exprSymbol, codegenClassScope));
            block.DeclareVar(
                forge.CoercionTypeBoxed, "leftCoerced",
                !forge.IsMustCoerce
                    ? Ref("left")
                    : forge.Coercer.CoerceCodegenMayNullBoxed(
                        Ref("left"), leftTypeUncoerced, methodNode, codegenClassScope));
            block.DeclareVar(typeof(bool), "hasNonNullRow", ConstantFalse());
            block.DeclareVar(typeof(bool), "hasNullRow", ConstantFalse());

            for (var i = 1; i < forges.Length; i++) {
                var refforge = forges[i];
                var refname = "r" + i;
                var reftype = forges[i].EvaluationType;

                if (reftype.IsArray)
                {
                    var arrayBlock = block.IfRefNullReturnNull("left")
                        .DeclareVar(
                            reftype, refname,
                            refforge.EvaluateCodegen(reftype, methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse();

                    var forLoop = arrayBlock.ForLoopIntSimple("i", ArrayLength(Ref(refname)));
                    var arrayAtIndex = ArrayAtIndex(Ref(refname), Ref("i"));
                    forLoop.DeclareVar(
                        forge.CoercionTypeBoxed, "item",
                        forge.Coercer == null
                            ? arrayAtIndex
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                arrayAtIndex, reftype.GetElementType(), methodNode, codegenClassScope));

                    var forLoopElse = forLoop.IfCondition(EqualsNull(Ref("item")))
                        .AssignRef("hasNullRow", ConstantTrue()).IfElse();
                    forLoopElse.AssignRef("hasNonNullRow", ConstantTrue());
                    forLoopElse.IfCondition(
                            NotOptional(!isNot, ExprDotMethod(Ref("leftCoerced"), "equals", Ref("item"))))
                        .BlockReturn(ConstantFalse());
                }
                else if (reftype.IsGenericCollection()) {
                    block.IfRefNullReturnNull("left")
                        .DeclareVar(
                            typeof(ICollection<object>), refname,
                            refforge.EvaluateCodegen(typeof(ICollection<object>), methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(!isNot, ExprDotMethod(Ref(refname), "contains", Ref("left"))))
                        .BlockReturn(ConstantFalse());
                }
                else if (reftype.IsGenericDictionary()) {
                    block.IfRefNullReturnNull("left")
                        .DeclareVar(
                            typeof(IDictionary<object, object>), refname,
                            refforge.EvaluateCodegen(typeof(IDictionary<object, object>), methodNode, exprSymbol, codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refname)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(!isNot, ExprDotMethod(Ref(refname), "containsKey", Ref("left"))))
                        .BlockReturn(ConstantFalse());
                }
                else {
                    block.IfRefNullReturnNull("leftCoerced");
                    block.DeclareVar(
                        forge.CoercionTypeBoxed, refname,
                        forge.Coercer == null
                            ? refforge.EvaluateCodegen(
                                forge.CoercionTypeBoxed, methodNode, exprSymbol, codegenClassScope)
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                refforge.EvaluateCodegen(
                                    forge.CoercionTypeBoxed, methodNode, exprSymbol, codegenClassScope), reftype,
                                methodNode, codegenClassScope));
                    var ifRightNotNull = block.IfRefNotNull(refname);
                    {
                        ifRightNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                        ifRightNotNull
                            .IfCondition(NotOptional(!isNot, ExprDotMethod(Ref("leftCoerced"), "equals", Ref(refname))))
                            .BlockReturn(ConstantFalse());
                    }
                    ifRightNotNull.IfElse()
                        .AssignRef("hasNullRow", ConstantTrue());
                }
            }

            block.IfCondition(Or(Not(Ref("hasNonNullRow")), Ref("hasNullRow"))).BlockReturn(ConstantNull());
            block.MethodReturn(ConstantTrue());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace