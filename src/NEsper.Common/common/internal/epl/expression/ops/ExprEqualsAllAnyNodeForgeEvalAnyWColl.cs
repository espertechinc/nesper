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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsAllAnyNodeForgeEvalAnyWColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprEqualsAllAnyNodeForge _forge;

        public ExprEqualsAllAnyNodeForgeEvalAnyWColl(
            ExprEqualsAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            _forge = forge;
            _evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var leftResult = _evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            // coerce early if testing without collections
            if (_forge.IsMustCoerce && leftResult != null) {
                leftResult = _forge.Coercer.CoerceBoxed(leftResult);
            }

            return CompareAny(leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAny(
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var isNot = _forge.ForgeRenderable.IsNot;
            var len = _forge.ForgeRenderable.ChildNodes.Length - 1;
            var hasNonNullRow = false;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

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
                        if (!_forge.IsMustCoerce) {
                            if (!isNot && leftResult.Equals(item) || isNot && !leftResult.Equals(item)) {
                                return true;
                            }
                        }
                        else {
                            if (!item.IsNumber()) {
                                continue;
                            }

                            var left = _forge.Coercer.CoerceBoxed(leftResult);
                            var right = _forge.Coercer.CoerceBoxed(item);
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
                    if (!_forge.IsMustCoerce) {
                        if (!isNot && leftResult.Equals(rightResult) || isNot && !leftResult.Equals(rightResult)) {
                            return true;
                        }
                    }
                    else {
                        var left = _forge.Coercer.CoerceBoxed(leftResult);
                        var right = _forge.Coercer.CoerceBoxed(rightResult);
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
                var refForge = forges[i];
                var refName = "r" + i;
                var refType = forges[i].EvaluationType.TypeNormalized();
 
                if (refType != null && refType.IsArray) {
                    var leftInitializer = refForge.EvaluateCodegen(refType, methodNode, exprSymbol, codegenClassScope);
                    var arrayBlock = block.IfRefNullReturnNull("left")
                        .DeclareVar(refType, refName, leftInitializer)
                        .IfCondition(EqualsNull(Ref(refName)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse();

                    var forLoop = arrayBlock.ForLoopIntSimple("i", ArrayLength(Ref(refName)));
                    var arrayAtIndex = ArrayAtIndex(Ref(refName), Ref("i"));
                    
                    forLoop.DeclareVar(
                        forge.CoercionTypeBoxed,
                        "item",
                        forge.Coercer == null
                            ? arrayAtIndex
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                arrayAtIndex,
                                refType.GetElementType(),
                                methodNode,
                                codegenClassScope));

                    var forLoopElse = forLoop
                        .IfCondition(EqualsNull(Ref("item")))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue());

                    forLoopElse
                        .IfCondition(
                            NotOptional(isNot, StaticMethod<object>("Equals", Ref("leftCoerced"), Ref("item"))))
                        .BlockReturn(ConstantTrue());
                }
                else if (refType != null && refType.IsGenericCollection()) {
                    var leftInitializer = refForge.EvaluateCodegen(
                        refType,
                        methodNode,
                        exprSymbol,
                        codegenClassScope);
                    
                    var leftWithBoxing = ExprEqualsAllAnyNodeForgeHelper.ItemToCollectionUnboxing(
                        Ref("left"), leftTypeUncoerced, refType.GetCollectionItemType());
                    
                    block.IfRefNullReturnNull("left")
                        .DeclareVar(refType, refName, leftInitializer)
                        .IfCondition(EqualsNull(Ref(refName)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(isNot, ExprDotMethod(Ref(refName), "CheckedContains", leftWithBoxing)))
                        .BlockReturn(ConstantTrue());
                }
                else if (refType != null && refType.IsGenericDictionary()) {
                    var leftWithBoxing = ExprEqualsAllAnyNodeForgeHelper.ItemToCollectionUnboxing(
                        Ref("left"), leftTypeUncoerced, refType.GetDictionaryKeyType());
                    
                    block.IfRefNullReturnNull("left")
                        .DeclareVar(
                            refType,
                            refName,
                            refForge.EvaluateCodegen(
                                typeof(IDictionary<object, object>),
                                methodNode,
                                exprSymbol,
                                codegenClassScope))
                        .IfCondition(EqualsNull(Ref(refName)))
                        .AssignRef("hasNullRow", ConstantTrue())
                        .IfElse()
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(isNot, ExprDotMethod(Ref(refName), "CheckedContainsKey", leftWithBoxing)))
                        .BlockReturn(ConstantTrue());
                }
                else {
                    var rhs = ConstantNull();
                    if (refType != null) {
                        rhs = forge.Coercer == null
                            ? refForge.EvaluateCodegen(forge.CoercionTypeBoxed, methodNode, exprSymbol, codegenClassScope)
                            : forge.Coercer.CoerceCodegenMayNullBoxed(
                                refForge.EvaluateCodegen(forge.CoercionTypeBoxed, methodNode, exprSymbol, codegenClassScope),
                                refType,
                                methodNode,
                                codegenClassScope);
                    }

                    var leftCoercedInitializer = refForge.EvaluateCodegen(
                        forge.CoercionTypeBoxed,
                        methodNode,
                        exprSymbol,
                        codegenClassScope);
                    
                    block.IfRefNullReturnNull("leftCoerced");
                    block.DeclareVar(forge.CoercionTypeBoxed, refName, rhs);

                    block
                        .IfRefNotNull(refName)
                        .AssignRef("hasNonNullRow", ConstantTrue())
                        .IfCondition(NotOptional(isNot, StaticMethod<object>("Equals", Ref("leftCoerced"), Ref(refName))))
                        .BlockReturn(ConstantTrue())
                        .IfElse().AssignRef("hasNullRow", ConstantTrue());
                }
            }

            block.IfCondition(Or(Not(Ref("hasNonNullRow")), Ref("hasNullRow"))).BlockReturn(ConstantNull());
            block.MethodReturn(ConstantFalse());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace