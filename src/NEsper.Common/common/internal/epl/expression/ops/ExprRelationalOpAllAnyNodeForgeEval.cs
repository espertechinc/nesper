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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRelationalOpAllAnyNodeForgeEval : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evaluators;

        private readonly ExprRelationalOpAllAnyNodeForge _forge;

        public ExprRelationalOpAllAnyNodeForgeEval(
            ExprRelationalOpAllAnyNodeForge forge,
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

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_evaluators.Length == 1) {
                return false;
            }

            var isAll = _forge.ForgeRenderable.IsAll;
            var computer = _forge.Computer;
            var valueLeft = _evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var len = _evaluators.Length - 1;

            if (_forge.IsCollectionOrArray) {
                var hasNonNullRow = false;
                var hasRows = false;
                for (var i = 1; i <= len; i++) {
                    var valueRight = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                    if (valueRight == null) {
                        continue;
                    }

                    if (valueRight is Array valueRightArray) {
                        hasRows = true;
                        var arrayLength = valueRightArray.Length;
                        for (var index = 0; index < arrayLength; index++) {
                            var item = valueRightArray.GetValue(index);
                            if (item == null) {
                                if (isAll) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight is IDictionary<object, object>) {
                        var coll = (IDictionary<object, object>) valueRight;
                        hasRows = true;
                        foreach (var item in coll.Keys) {
                            if (!(item.IsNumber())) {
                                if (isAll && item == null) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (valueRight is ICollection<object>) {
                        var coll = (ICollection<object>) valueRight;
                        hasRows = true;
                        foreach (var item in coll) {
                            if (!(item.IsNumber())) {
                                if (isAll && item == null) {
                                    return null;
                                }

                                continue;
                            }

                            hasNonNullRow = true;
                            if (valueLeft != null) {
                                if (isAll) {
                                    if (!computer.Compare(valueLeft, item)) {
                                        return false;
                                    }
                                }
                                else {
                                    if (computer.Compare(valueLeft, item)) {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (!(valueRight.IsNumber())) {
                        if (isAll) {
                            return null;
                        }
                    }
                    else {
                        hasNonNullRow = true;
                        if (isAll) {
                            if (!computer.Compare(valueLeft, valueRight)) {
                                return false;
                            }
                        }
                        else {
                            if (computer.Compare(valueLeft, valueRight)) {
                                return true;
                            }
                        }
                    }
                }

                if (isAll) {
                    if (!hasRows) {
                        return true;
                    }

                    if (!hasNonNullRow || valueLeft == null) {
                        return null;
                    }

                    return true;
                }

                if (!hasRows) {
                    return false;
                }

                if (!hasNonNullRow || valueLeft == null) {
                    return null;
                }

                return false;
            }
            else {
                var hasNonNullRow = false;
                var hasRows = false;
                for (var i = 1; i <= len; i++) {
                    var valueRight = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    hasRows = true;

                    if (valueRight != null) {
                        hasNonNullRow = true;
                    }
                    else {
                        if (isAll) {
                            return null;
                        }
                    }

                    if (valueRight != null && valueLeft != null) {
                        if (isAll) {
                            if (!computer.Compare(valueLeft, valueRight)) {
                                return false;
                            }
                        }
                        else {
                            if (computer.Compare(valueLeft, valueRight)) {
                                return true;
                            }
                        }
                    }
                }

                if (isAll) {
                    if (!hasRows) {
                        return true;
                    }

                    if (!hasNonNullRow || valueLeft == null) {
                        return null;
                    }

                    return true;
                }

                if (!hasRows) {
                    return false;
                }

                if (!hasNonNullRow || valueLeft == null) {
                    return null;
                }

                return false;
            }
        }

        public static CodegenExpression Codegen(
            ExprRelationalOpAllAnyNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forges = ExprNodeUtilityQuery.GetForges(forge.ForgeRenderable.ChildNodes);
            var valueLeftType = forges[0].EvaluationType;
            var isAll = forge.ForgeRenderable.IsAll;
            if (forges.Length == 1) {
                return Constant(isAll);
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRelationalOpAllAnyNodeForgeEval),
                codegenClassScope);

            // when null-type value and "all" the result is always null
            if (isAll) {
                for (int i = 1; i < forges.Length; i++) {
                    var refType = forges[i].EvaluationType;
                    if (refType.IsNullTypeSafe()) {
                        methodNode.Block.MethodReturn(ConstantNull());
                        return LocalMethod(methodNode);
                    }
                }
            }
            
            var block = methodNode.Block
                .DeclareVar<bool>("hasNonNullRow", ConstantFalse())
                .DeclareVar(
                    valueLeftType,
                    "valueLeft",
                    forges[0].EvaluateCodegen(valueLeftType, methodNode, exprSymbol, codegenClassScope));

            for (var i = 1; i < forges.Length; i++) {
                var refForge = forges[i];
                var refName = "r" + i;
                var refType = refForge.EvaluationType;
                if (refType.IsNullTypeSafe() && !isAll) {
                    continue;
                }
                
                block.DeclareVar(
                    refType,
                    refName,
                    refForge.EvaluateCodegen(refType, methodNode, exprSymbol, codegenClassScope));

                if (refType.IsArray) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refName)));
                    {
                        var componentType = refType.GetElementType().GetBoxedType();
                        var forLoopArray = blockIfNotNull.ForLoopIntSimple("index", ArrayLength(Ref(refName)));
                        {
                            forLoopArray.DeclareVar(
                                componentType,
                                "item",
                                ArrayAtIndex(Ref(refName), Ref("index")));
                            var ifItemNull = forLoopArray.IfCondition(EqualsNull(Ref("item")));
                            {
                                if (isAll) {
                                    ifItemNull.IfReturn(ConstantNull());
                                }
                            }
                            var ifItemNotNull = ifItemNull.IfElse();
                            {
                                ifItemNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifItemNotNull.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Ref("item"),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (refType.IsGenericDictionary()) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refName)));
                    {
                        var forEach = blockIfNotNull.ForEach(
                            typeof(object),
                            "item",
                            ExprDotName(Ref(refName), "Keys"));
                        {
                            var ifNotNumber = forEach.IfCondition(Not(InstanceOf(Ref("item"), typeof(object))));
                            {
                                if (isAll) {
                                    ifNotNumber.IfRefNullReturnNull("item");
                                }
                            }
                            var ifNotNumberElse = ifNotNumber.IfElse();
                            {
                                ifNotNumberElse.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifNotNumberElse.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Cast(typeof(object), Ref("item")),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (refType.IsGenericCollection()) {
                    var blockIfNotNull = block.IfCondition(NotEqualsNull(Ref(refName)));
                    {
                        var forEach = blockIfNotNull.ForEach(typeof(object), "item", Ref(refName));
                        {
                            var ifNotNumber = forEach.IfCondition(Not(InstanceOf(Ref("item"), typeof(object))));
                            {
                                if (isAll) {
                                    ifNotNumber.IfRefNullReturnNull("item");
                                }
                            }
                            var ifNotNumberElse = ifNotNumber.IfElse();
                            {
                                ifNotNumberElse.AssignRef("hasNonNullRow", ConstantTrue());
                                var ifLeftNotNull = ifNotNumberElse.IfCondition(NotEqualsNull(Ref("valueLeft")));
                                {
                                    ifLeftNotNull.IfCondition(
                                            NotOptional(
                                                isAll,
                                                forge.Computer.Codegen(
                                                    Ref("valueLeft"),
                                                    valueLeftType,
                                                    Cast(typeof(object), Ref("item")),
                                                    typeof(object))))
                                        .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                                }
                            }
                        }
                    }
                }
                else if (!TypeHelper.IsSubclassOrImplementsInterface(
                    refType.GetBoxedType(),
                    typeof(object))) {
                    if (refType.CanBeNull()) {
                        block.IfRefNullReturnNull(refName);
                    }

                    block.AssignRef("hasNonNullRow", ConstantTrue());
                    if (isAll) {
                        block.BlockReturn(ConstantNull());
                    }
                }
                else {
                    if (refType.CanNotBeNull()) {
                        block.AssignRef("hasNonNullRow", ConstantTrue());
                        block.IfCondition(
                                NotOptional(
                                    isAll,
                                    forge.Computer.Codegen(Ref("valueLeft"), valueLeftType, Ref(refName), refType)))
                            .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                    }
                    else {
                        if (isAll) {
                            block.IfRefNullReturnNull(refName);
                        }

                        var ifRefNotNull = block.IfRefNotNull(refName);
                        {
                            ifRefNotNull.AssignRef("hasNonNullRow", ConstantTrue());
                            var ifLeftNotNull = ifRefNotNull.IfCondition(NotEqualsNull(Ref("valueLeft")));
                            ifLeftNotNull.IfCondition(
                                    NotOptional(
                                        isAll,
                                        forge.Computer.Codegen(
                                            Ref("valueLeft"),
                                            valueLeftType,
                                            Ref(refName),
                                            typeof(object))))
                                .BlockReturn(isAll ? ConstantFalse() : ConstantTrue());
                        }
                    }
                }
            }

            block.IfCondition(Not(Ref("hasNonNullRow")))
                .BlockReturn(ConstantNull());
            if (valueLeftType.CanBeNull()) {
                block.IfRefNullReturnNull("valueLeft");
            }

            block.MethodReturn(Constant(isAll));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace