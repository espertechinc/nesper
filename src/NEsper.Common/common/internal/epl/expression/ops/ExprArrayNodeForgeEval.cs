///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
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
    public class ExprArrayNodeForgeEval : ExprEvaluator,
        ExprEnumerationEval
    {
        private const string PRIMITIVE_ARRAY_NULL_MSG = "new-array received a null value as an array element of an array of primitives";
        
        private readonly ExprArrayNodeForge _forge;
        private readonly ExprEvaluator[] _evaluators;

        public ExprArrayNodeForgeEval(
            ExprArrayNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            _forge = forge;
            _evaluators = evaluators;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (_forge.ForgeRenderableArray.ChildNodes.Length == 0) {
                return Collections.GetEmptyList<object>();
            }

            var resultList = new ArrayDeque<object>(_evaluators.Length);
            foreach (var child in _evaluators) {
                var result = child.Evaluate(eventsPerStream, isNewData, context);
                if (result != null) {
                    if (_forge.IsMustCoerce) {
                        var coercedResult = _forge.Coercer.CoerceBoxed(result);
                        resultList.Add(coercedResult);
                    }
                    else {
                        resultList.Add(result);
                    }
                }
            }

            return resultList;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var array = Arrays.CreateInstanceChecked(_forge.ArrayReturnType, _evaluators.Length);
            var index = 0;
            var requiresPrimitive =
                _forge.Parent.OptionalRequiredType != null &&
                _forge.Parent.OptionalRequiredType.IsPrimitive;

            foreach (var child in _evaluators) {
                var result = child.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                if (result != null) {
                    if (_forge.IsMustCoerce) {
                        var coercedResult = _forge.Coercer.CoerceBoxed(result);
                        array.SetValue(coercedResult, index);
                    }
                    else {
                        array.SetValue(result, index);
                    }
                }
                else {
                    if (requiresPrimitive) {
                        throw new EPException(PRIMITIVE_ARRAY_NULL_MSG);
                    }
                }

                index++;
            }

            return array;
        }

        public static CodegenExpression Codegen(
            ExprArrayNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forgeRenderable = forge.ForgeRenderableArray;
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprArrayNodeForgeEval),
                codegenClassScope);

            var arrayType = forge.EvaluationType;
            var block = methodNode.Block
                .DeclareVar(
                    arrayType,
                    "array",
                    NewArrayByLength(forge.ArrayReturnType, Constant(forgeRenderable.ChildNodes.Length)));
            var requiresPrimitive =
                forge.Parent.OptionalRequiredType != null &&
                forge.Parent.OptionalRequiredType.IsPrimitive;

            for (var i = 0; i < forgeRenderable.ChildNodes.Length; i++) {
                var child = forgeRenderable.ChildNodes[i].Forge;
                var childType = child.EvaluationType;
                if (childType.IsNullTypeSafe()) {
                    // no action
                }
                else {
                    var refname = "r" + i;

                    block.DeclareVar(
                        childType,
                        refname,
                        child.EvaluateCodegen(childType, methodNode, exprSymbol, codegenClassScope));

                    if (child.EvaluationType.CanNotBeNull()) {
                        if (!forge.IsMustCoerce) {
                            block
                                .AssignArrayElement(
                                    "array",
                                    Constant(i),
                                    Unbox(Ref(refname), childType));
                        }
                        else {
                            block
                                .AssignArrayElement(
                                    "array",
                                    Constant(i),
                                    forge.Coercer.CoerceCodegen(Ref(refname), childType));
                        }
                    }
                    else {
                        var ifNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                        if (!forge.IsMustCoerce) {
                            ifNotNull
                                .AssignArrayElement(
                                    "array",
                                    Constant(i),
                                    Unbox(Ref(refname), childType));
                        }
                        else {
                            ifNotNull
                                .AssignArrayElement(
                                    "array",
                                    Constant(i),
                                    forge.Coercer.CoerceCodegen(Ref(refname), childType));
                        }

                        if (requiresPrimitive) {
                            block.IfCondition(
                                    EqualsNull(Ref(refname)))
                                .BlockThrow(
                                    NewInstance(typeof(EPException), Constant(PRIMITIVE_ARRAY_NULL_MSG)));
                        }
                    }
                }
            }

            block.MethodReturn(Ref("array"));
            return LocalMethod(methodNode);
        }

        public static CodegenExpression CodegenEvaluateGetROCollectionScalar(
            ExprArrayNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var children = forge.ForgeRenderableArray.ChildNodes;
            if (children.Length == 0) {
                return StaticMethod(typeof(Collections), "GetEmptyList");
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(FlexCollection),
                typeof(ExprArrayNodeForgeEval),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<ArrayDeque<object>>(
                    "resultList",
                    NewInstance<ArrayDeque<object>>(Constant(children.Length)));
            var count = -1;
            foreach (var child in children) {
                count++;
                var refname = "r" + count;
                var childForge = child.Forge;
                var returnType = childForge.EvaluationType;
                if (returnType == null) {
                    continue;
                }

                block.DeclareVar(
                    returnType,
                    refname,
                    childForge.EvaluateCodegen(returnType, methodNode, exprSymbol, codegenClassScope));
                var nonNullTest = returnType.CanNotBeNull() ? ConstantTrue() : NotEqualsNull(Ref(refname));
                var blockIfNotNull = block.IfCondition(nonNullTest);
                CodegenExpression added = Ref(refname);
                if (forge.IsMustCoerce) {
                    added = forge.Coercer.CoerceCodegen(Ref(refname), childForge.EvaluationType);
                }

                blockIfNotNull.Expression(ExprDotMethod(Ref("resultList"), "Add", added));
            }

            block.MethodReturn(FlexWrap(Ref("resultList")));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace