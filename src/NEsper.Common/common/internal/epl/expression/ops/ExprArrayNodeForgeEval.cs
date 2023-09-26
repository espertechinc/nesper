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
using com.espertech.esper.common.client.collection;
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
        private const string PRIMITIVE_ARRAY_NULL_MSG =
            "new-array received a null value as an array element of an array of primitives";

        private readonly ExprArrayNodeForge forge;
        private readonly ExprEvaluator[] evaluators;

        public ExprArrayNodeForgeEval(
            ExprArrayNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var array = Arrays.CreateInstanceChecked(forge.ArrayReturnType, evaluators.Length);
            var index = 0;
            var requiresPrimitive = forge.Parent.OptionalRequiredType != null &&
                                    forge.Parent.OptionalRequiredType.IsPrimitive;
            foreach (var child in evaluators) {
                var result = child.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                if (result != null) {
                    if (forge.IsMustCoerce) {
                        var boxed = result;
                        var coercedResult = forge.Coercer.CoerceBoxed(boxed);
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
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprArrayNodeForgeEval),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    forge.EvaluationType,
                    "array",
                    NewArrayByLength(forge.ArrayReturnType, Constant(forge.ForgeRenderableArray.ChildNodes.Length)));
            var requiresPrimitive = forge.Parent.OptionalRequiredType != null &&
                                    forge.Parent.OptionalRequiredType.IsPrimitive;
            for (var i = 0; i < forge.ForgeRenderableArray.ChildNodes.Length; i++) {
                ExprForge child = forge.ForgeRenderableArray.ChildNodes[i].Forge;
                var childType = child.EvaluationType;

                if (childType == null) {
                    // no action
                }
                else {
                    var childTypeClass = childType;
                    var refname = "r" + i;
                    block.DeclareVar(
                        childTypeClass,
                        refname,
                        child.EvaluateCodegen(childTypeClass, methodNode, exprSymbol, codegenClassScope));

                    if (childTypeClass.IsPrimitive) {
                        if (!forge.IsMustCoerce) {
                            block.AssignArrayElement("array", Constant(i), Ref(refname));
                        }
                        else {
                            block.AssignArrayElement(
                                "array",
                                Constant(i),
                                forge.Coercer.CoerceCodegen(Ref(refname), childTypeClass));
                        }
                    }
                    else {
                        var ifNotNull = block.IfCondition(NotEqualsNull(Ref(refname)));
                        if (!forge.IsMustCoerce) {
                            ifNotNull.AssignArrayElement("array", Constant(i), Ref(refname));
                        }
                        else {
                            ifNotNull.AssignArrayElement(
                                "array",
                                Constant(i),
                                forge.Coercer.CoerceCodegen(Ref(refname), childTypeClass));
                        }

                        if (requiresPrimitive) {
                            block.IfCondition(EqualsNull(Ref(refname)))
                                .BlockThrow(NewInstance(typeof(EPException), Constant(PRIMITIVE_ARRAY_NULL_MSG)));
                        }
                    }
                }
            }

            block.MethodReturn(Ref("array"));
            return LocalMethod(methodNode);
        }

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
            if (forge.ForgeRenderableArray.ChildNodes.Length == 0) {
                return EmptyList<object>.Instance;
            }

            var resultList = new ArrayDeque<object>(evaluators.Length);
            foreach (var child in evaluators) {
                var result = child.Evaluate(eventsPerStream, isNewData, context);
                if (result != null) {
                    if (forge.IsMustCoerce) {
                        var boxed = result;
                        var coercedResult = forge.Coercer.CoerceBoxed(boxed);
                        resultList.Add(coercedResult);
                    }
                    else {
                        resultList.Add(result);
                    }
                }
            }

            return resultList;
        }

        public static CodegenExpression CodegenEvaluateGetROCollectionScalar(
            ExprArrayNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            ExprNode[] children = forge.ForgeRenderableArray.ChildNodes;
            if (children.Length == 0) {
                return StaticMethod(typeof(Collections), "GetEmptyList");
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(FlexCollection),
                typeof(ExprArrayNodeForgeEval),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    typeof(ArrayDeque<object>),
                    "resultList",
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

                var returnClass = returnType;
                block.DeclareVar(
                    returnClass,
                    refname,
                    childForge.EvaluateCodegen(returnType, methodNode, exprSymbol, codegenClassScope));
                var nonNullTest =
                    returnClass.IsPrimitive ? ConstantTrue() : NotEqualsNull(Ref(refname));
                var blockIfNotNull = block.IfCondition(nonNullTest);
                CodegenExpression added = Ref(refname);
                if (forge.IsMustCoerce) {
                    added = forge.Coercer.CoerceCodegen(Ref(refname), childForge.EvaluationType);
                }

                blockIfNotNull.Expression(ExprDotMethod(Ref("resultList"), "Add", added));
            }

            block.MethodReturn(Ref("resultList"));
            return LocalMethod(methodNode);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace