///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    public class ExprInNodeForgeEvalWColl : ExprEvaluator
    {
        private readonly ExprInNodeForge _forge;
        private readonly ExprEvaluator[] _evaluators;

        public ExprInNodeForgeEvalWColl(
            ExprInNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this._forge = forge;
            this._evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var inPropResult = _evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var isNotIn = _forge.ForgeRenderable.IsNotIn;

            var len = _evaluators.Length - 1;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult == null) {
                    continue;
                }

                if (rightResult.GetType().IsGenericDictionary()) {
                    if (inPropResult == null) {
                        return null;
                    }

                    var coll = rightResult.UnwrapDictionary(MagicMarker.SingletonInstance);
                    if (coll.ContainsKey(inPropResult)) {
                        return !isNotIn;
                    }
                }
                else if (rightResult is Array array) {
                    var arrayLength = array.Length;
                    if (arrayLength > 0 && inPropResult == null) {
                        return null;
                    }

                    for (var index = 0; index < arrayLength; index++) {
                        var item = array.GetValue(index);
                        if (item == null) {
                            hasNullRow = true;
                            continue;
                        }

                        if (!_forge.IsMustCoerce) {
                            if (inPropResult.Equals(item)) {
                                return !isNotIn;
                            }
                        }
                        else {
                            if (!(item.IsNumber())) {
                                continue;
                            }

                            var left = _forge.Coercer.CoerceBoxed(inPropResult);
                            var right = _forge.Coercer.CoerceBoxed(item);
                            if (left.Equals(right)) {
                                return !isNotIn;
                            }
                        }
                    }
                }
                else if (rightResult.GetType().IsGenericCollection()) {
                    if (inPropResult == null) {
                        return null;
                    }

                    var coll = rightResult.Unwrap<object>();
                    if (coll.Contains(inPropResult)) {
                        return !isNotIn;
                    }
                }
                else if (rightResult.GetType().IsArray) {
                }
                else {
                    if (inPropResult == null) {
                        return null;
                    }

                    if (!_forge.IsMustCoerce) {
                        if (inPropResult.Equals(rightResult)) {
                            return !isNotIn;
                        }
                    }
                    else {
                        var left = _forge.Coercer.CoerceBoxed(inPropResult);
                        var right = _forge.Coercer.CoerceBoxed(rightResult);
                        if (left.Equals(right)) {
                            return !isNotIn;
                        }
                    }
                }
            }

            if (hasNullRow) {
                return null;
            }

            return isNotIn;
        }

        public static CodegenExpression Codegen(
            ExprInNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forges = ExprNodeUtilityQuery.GetForges(forge.ForgeRenderable.ChildNodes);
            var isNot = forge.ForgeRenderable.IsNotIn;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprInNodeForgeEvalWColl),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar<bool>("hasNullRow", ConstantFalse());

            var leftTypeUncoerced = forges[0].EvaluationType.GetBoxedType();
            var leftTypeCoerced = forge.CoercionType.GetBoxedType();

            block.DeclareVar(
                leftTypeUncoerced,
                "left",
                forges[0].EvaluateCodegen(leftTypeUncoerced, methodNode, exprSymbol, codegenClassScope));
            block.DeclareVar(
                forge.CoercionType,
                "leftCoerced",
                !forge.IsMustCoerce
                    ? Ref("left")
                    : forge.Coercer.CoerceCodegenMayNullBoxed(
                        Ref("left"),
                        leftTypeUncoerced,
                        methodNode,
                        codegenClassScope));

            for (var i = 1; i < forges.Length; i++) {
                var reftype = forges[i].EvaluationType.GetBoxedType();
                var refforge = forges[i];
                var refname = "r" + i;

                if (reftype == null) {
                    block.AssignRef("hasNullRow", ConstantTrue());
                    continue;
                }

                block.DeclareVar(
                    reftype,
                    refname,
                    refforge.EvaluateCodegen(reftype, methodNode, exprSymbol, codegenClassScope));

                if (reftype.IsArray) {
                    var ifRightNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        if (leftTypeUncoerced.CanBeNull()) {
                            ifRightNotNull.IfCondition(
                                    And(
                                        Relational(ArrayLength(Ref(refname)), GT, Constant(0)),
                                        EqualsNull(Ref("left"))))
                                .BlockReturn(ConstantNull());
                        }

                        var forLoop = ifRightNotNull.ForLoopIntSimple("index", ArrayLength(Ref(refname)));
                        {
                            forLoop.DeclareVar(
                                reftype.GetElementType(),
                                "item",
                                ArrayAtIndex(Ref(refname), Ref("index")));
                            forLoop.DeclareVar<bool>(
                                "itemNull",
                                reftype.GetElementType().CanNotBeNull() ? ConstantFalse() : EqualsNull(Ref("item")));
                            var itemNotNull = forLoop.IfCondition(Ref("itemNull"))
                                .AssignRef("hasNullRow", ConstantTrue())
                                .IfElse();
                            {
                                if (!forge.IsMustCoerce) {
                                    itemNotNull.IfCondition(
                                            CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                                                Ref("leftCoerced"),
                                                leftTypeCoerced,
                                                Ref("item"),
                                                reftype.GetElementType()))
                                        .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                                }
                                else {
                                    if (reftype.GetElementType().IsTypeNumeric()) {
                                        itemNotNull.IfCondition(
                                                CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                                                    Ref("leftCoerced"),
                                                    leftTypeCoerced,
                                                    forge.Coercer.CoerceCodegen(Ref("item"), reftype.GetElementType()),
                                                    forge.CoercionType))
                                            .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                                    }
                                }
                            }
                        }
                    }
                }
                else if (reftype.IsGenericDictionary()) {
                    var ifRightNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        if (leftTypeUncoerced.CanBeNull()) {
                            ifRightNotNull.IfRefNullReturnNull("left");
                        }

                        var leftWithBoxing = ExprEqualsAllAnyNodeForgeHelper.ItemToCollectionUnboxing(
                            Ref("left"), leftTypeUncoerced, reftype.GetDictionaryKeyType());
                        
                        ifRightNotNull.IfCondition(ExprDotMethod(Ref(refname), "CheckedContainsKey", leftWithBoxing))
                            .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                    }
                }
                else if (reftype.IsGenericCollection()) {
                    var ifRightNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    {
                        if (leftTypeUncoerced.CanBeNull()) {
                            ifRightNotNull.IfRefNullReturnNull("left");
                        }

                        var leftWithBoxing = ExprEqualsAllAnyNodeForgeHelper.ItemToCollectionUnboxing(
                            Ref("left"), leftTypeUncoerced, reftype.GetCollectionItemType());
                        
                        ifRightNotNull
                            .IfCondition(StaticMethod(typeof(Collections), "CheckedContains", Ref(refname), leftWithBoxing))
                            .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                    }
                }
                else {
                    var ifRightNotNull = reftype.CanNotBeNull() ? block : block.IfRefNotNull(refname);
                    {
                        if (leftTypeUncoerced.CanBeNull()) {
                            ifRightNotNull.IfRefNullReturnNull("left");
                        }

                        if (!forge.IsMustCoerce) {
                            ifRightNotNull.IfCondition(
                                    CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                                        Ref("leftCoerced"),
                                        leftTypeCoerced,
                                        Ref(refname),
                                        reftype))
                                .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                        }
                        else {
                            ifRightNotNull.IfCondition(
                                    CodegenLegoCompareEquals.CodegenEqualsNonNullNoCoerce(
                                        Ref("leftCoerced"),
                                        leftTypeCoerced,
                                        forge.Coercer.CoerceCodegen(Ref(refname), reftype),
                                        forge.CoercionType))
                                .BlockReturn(!isNot ? ConstantTrue() : ConstantFalse());
                        }
                    }
                    if (reftype.CanBeNull()) {
                        block.IfRefNull(refname).AssignRef("hasNullRow", ConstantTrue());
                    }
                }
            }

            block.IfCondition(Ref("hasNullRow")).BlockReturn(ConstantNull());
            block.MethodReturn(isNot ? ConstantTrue() : ConstantFalse());
            return LocalMethod(methodNode);
        }
    }
} // end of namespace