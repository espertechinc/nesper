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
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeRootChildEval : ExprEvaluator,
        ExprEnumerationEval
    {
        private readonly ExprDotNodeForgeRootChild forge;
        private readonly ExprDotEvalRootChildInnerEval innerEvaluator;
        private readonly ExprDotEval[] evalIteratorEventBean;
        private readonly ExprDotEval[] evalUnpacking;

        public ExprDotNodeForgeRootChildEval(
            ExprDotNodeForgeRootChild forge,
            ExprDotEvalRootChildInnerEval innerEvaluator,
            ExprDotEval[] evalIteratorEventBean,
            ExprDotEval[] evalUnpacking)
        {
            this.forge = forge;
            this.innerEvaluator = innerEvaluator;
            this.evalIteratorEventBean = evalIteratorEventBean;
            this.evalUnpacking = evalUnpacking;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var inner = innerEvaluator.Evaluate(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.ForgesUnpacking,
                    evalUnpacking,
                    inner,
                    eventsPerStream,
                    isNewData,
                    context);
            }

            return inner;
        }

        public static CodegenExpression Codegen(
            ExprDotNodeForgeRootChild forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.EvaluationType;
            if (evaluationType == null) {
                return ConstantNull();
            }

            var evaluationClass = evaluationType;
            var innerType = forge.InnerForge.TypeInfo.GetCodegenReturnType();
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    innerType,
                    "inner",
                    forge.InnerForge.CodegenEvaluate(methodNode, exprSymbol, codegenClassScope));
            if (!innerType.IsPrimitive && !evaluationClass.IsTypeVoid()) {
                block.IfRefNullReturnNull("inner");
            }

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPChainableTypeCodegenSharable(forge.InnerForge.TypeInfo, codegenClassScope));
            }

            block.Apply(
                InstrumentationCode.Instblock(
                    codegenClassScope,
                    "qExprDotChain",
                    typeInformation,
                    Ref("inner"),
                    Constant(forge.ForgesUnpacking.Length)));
            var expression = ExprDotNodeUtility.EvaluateChainCodegen(
                methodNode,
                exprSymbol,
                codegenClassScope,
                Ref("inner"),
                innerType,
                forge.ForgesUnpacking,
                null);
            if (evaluationClass.IsTypeVoid()) {
                block.Expression(expression)
                    .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                    .MethodEnd();
            }
            else {
                block.DeclareVar(evaluationType, "result", expression)
                    .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                    .MethodReturn(Ref("result"));
            }

            return LocalMethod(methodNode);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object inner = innerEvaluator.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.ForgesIteratorEventBean,
                    evalIteratorEventBean,
                    inner,
                    eventsPerStream,
                    isNewData,
                    context);
                if (inner is ICollection<EventBean> beanCollection) {
                    return beanCollection;
                }
            }

            return null;
        }

        public static CodegenExpression CodegenEvaluateGetROCollectionEvents(
            ExprDotNodeForgeRootChild forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (forge.EvaluationType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                EPTYPE_COLLECTION_EVENTBEAN,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPChainableTypeCodegenSharable(forge.InnerForge.TypeInfo, codegenClassScope));
            }

            methodNode.Block
                .DeclareVar(
                    typeof(FlexCollection),
                    "inner",
                    forge.InnerForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .Apply(
                    InstrumentationCode.Instblock(
                        codegenClassScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("inner"),
                        Constant(forge.ForgesUnpacking.Length)))
                .IfRefNull("inner")
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .BlockReturn(ConstantNull())
                .DeclareVar(
                    EPTYPE_COLLECTION_EVENTBEAN,
                    "result",
                    ExprDotNodeUtility.EvaluateChainCodegen(
                        methodNode,
                        exprSymbol,
                        codegenClassScope,
                        Ref("inner"),
                        typeof(FlexCollection),
                        forge.ForgesIteratorEventBean,
                        null))
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object inner = innerEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.ForgesIteratorEventBean,
                    evalIteratorEventBean,
                    inner,
                    eventsPerStream,
                    isNewData,
                    context);
                if (inner.GetType().IsGenericCollection()) {
                    return inner.Unwrap<object>();
                }
            }

            return null;
        }

        public static CodegenExpression CodegenEvaluateGetROCollectionScalar(
            ExprDotNodeForgeRootChild forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (forge.EvaluationType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPChainableTypeCodegenSharable(forge.InnerForge.TypeInfo, codegenClassScope));
            }

            methodNode.Block.DeclareVar(
                    typeof(FlexCollection),
                    "inner",
                    forge.InnerForge.EvaluateGetROCollectionScalarCodegen(methodNode, exprSymbol, codegenClassScope))
                .Apply(
                    InstrumentationCode.Instblock(
                        codegenClassScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("inner"),
                        Constant(forge.ForgesUnpacking.Length)))
                .IfRefNull("inner")
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .BlockReturn(ConstantNull())
                .DeclareVar(
                    forge.EvaluationType,
                    "result",
                    ExprDotNodeUtility.EvaluateChainCodegen(
                        methodNode,
                        exprSymbol,
                        codegenClassScope,
                        Ref("inner"),
                        typeof(FlexCollection),
                        forge.ForgesIteratorEventBean,
                        null))
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .MethodReturn(Ref("result"));
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