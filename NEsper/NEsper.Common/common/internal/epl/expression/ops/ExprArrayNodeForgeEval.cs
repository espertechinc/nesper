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
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprArrayNodeForgeEval : ExprEvaluator,
        ExprEnumerationEval
    {
        private readonly ExprEvaluator[] _evaluators;

        private readonly ExprArrayNodeForge _forge;

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
            var array = Array.CreateInstance(_forge.ArrayReturnType, _evaluators.Length);
            var index = 0;
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
                forge.EvaluationType, typeof(ExprArrayNodeForgeEval), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    forge.EvaluationType, "array",
                    NewArrayByLength(forge.ArrayReturnType, Constant(forgeRenderable.ChildNodes.Length)));
            for (var i = 0; i < forgeRenderable.ChildNodes.Length; i++) {
                var child = forgeRenderable.ChildNodes[i].Forge;
                var childType = child.EvaluationType;
                var refname = "r" + i;
                block.DeclareVar(
                    childType, refname, child.EvaluateCodegen(childType, methodNode, exprSymbol, codegenClassScope));

                if (child.EvaluationType.IsPrimitive) {
                    if (!forge.IsMustCoerce) {
                        block.AssignArrayElement("array", Constant(i), Ref(refname));
                    }
                    else {
                        block.AssignArrayElement(
                            "array", Constant(i), forge.Coercer.CoerceCodegen(Ref(refname), child.EvaluationType));
                    }
                }
                else {
                    var ifNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                    if (!forge.IsMustCoerce) {
                        ifNotNull.AssignArrayElement("array", Constant(i), Ref(refname));
                    }
                    else {
                        ifNotNull.AssignArrayElement(
                            "array", Constant(i), forge.Coercer.CoerceCodegen(Ref(refname), child.EvaluationType));
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
                return StaticMethod(typeof(Collections), "emptyList");
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>), typeof(ExprArrayNodeForgeEval), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    typeof(ArrayDeque<object>), "resultList",
                    NewInstance(typeof(ArrayDeque<object>), Constant(children.Length)));
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
                    returnType, refname,
                    childForge.EvaluateCodegen(returnType, methodNode, exprSymbol, codegenClassScope));
                var nonNullTest = returnType.IsPrimitive ? ConstantTrue() : NotEqualsNull(Ref(refname));
                var blockIfNotNull = block.IfCondition(nonNullTest);
                CodegenExpression added = Ref(refname);
                if (forge.IsMustCoerce) {
                    added = forge.Coercer.CoerceCodegen(Ref(refname), childForge.EvaluationType);
                }

                blockIfNotNull.Expression(ExprDotMethod(Ref("resultList"), "add", added));
            }

            block.MethodReturn(Ref("resultList"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace