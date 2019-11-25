///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeRootChildEval : ExprEvaluator,
        ExprEnumerationEval
    {
        private readonly ExprDotEval[] evalIteratorEventBean;
        private readonly ExprDotEval[] evalUnpacking;
        private readonly ExprDotNodeForgeRootChild forge;
        private readonly ExprDotEvalRootChildInnerEval innerEvaluator;

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

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object inner = innerEvaluator.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.forgesIteratorEventBean,
                    evalIteratorEventBean,
                    inner,
                    eventsPerStream,
                    isNewData,
                    context);
                if (inner is ICollection<EventBean>) {
                    return (ICollection<EventBean>) inner;
                }
            }

            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object inner = innerEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.forgesIteratorEventBean,
                    evalIteratorEventBean,
                    inner,
                    eventsPerStream,
                    isNewData,
                    context);
                if (inner is ICollection<object>) {
                    return (ICollection<object>) inner;
                }
            }

            return null;
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
            ExprEvaluatorContext context)
        {
            var inner = innerEvaluator.Evaluate(eventsPerStream, isNewData, context);
            if (inner != null) {
                inner = ExprDotNodeUtility.EvaluateChain(
                    forge.forgesUnpacking,
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
            var innerType = forge.innerForge.TypeInfo.GetCodegenReturnType().GetBoxedType();
            var evaluationType = forge.EvaluationType.GetBoxedType();
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    innerType,
                    "inner",
                    forge.innerForge.CodegenEvaluate(methodNode, exprSymbol, codegenClassScope));
            if (innerType.CanBeNull() && evaluationType != typeof(void)) {
                block.IfRefNullReturnNull("inner");
            }

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
            }

            block.Apply(
                InstrumentationCode.Instblock(
                    codegenClassScope,
                    "qExprDotChain",
                    typeInformation,
                    Ref("inner"),
                    Constant(forge.forgesUnpacking.Length)));
            var expression = ExprDotNodeUtility.EvaluateChainCodegen(
                methodNode,
                exprSymbol,
                codegenClassScope,
                Ref("inner"),
                innerType,
                forge.forgesUnpacking,
                null);
            if (evaluationType == typeof(void)) {
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

        public static CodegenExpression CodegenEvaluateGetROCollectionEvents(
            ExprDotNodeForgeRootChild forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.EvaluationType;
            if (evaluationType != typeof(ICollection<EventBean>)) {
                throw new IllegalStateException("ROCollectionEvents missing event collection");
            }
            
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
            }

            methodNode.Block
                .DeclareVar<ICollection<EventBean>>(
                    "inner",
                    forge.innerForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .Apply(
                    InstrumentationCode.Instblock(
                        codegenClassScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("inner"),
                        Constant(forge.forgesUnpacking.Length)))
                .IfRefNull("inner")
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .BlockReturn(ConstantNull())
                .DeclareVar(
                    evaluationType,
                    "result",
                    ExprDotNodeUtility.EvaluateChainCodegen(
                        methodNode,
                        exprSymbol,
                        codegenClassScope,
                        Ref("inner"),
                        typeof(ICollection<EventBean>),
                        forge.forgesIteratorEventBean,
                        null))
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }

        public static CodegenExpression CodegenEvaluateGetROCollectionScalar(
            ExprDotNodeForgeRootChild forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeRootChildEval),
                codegenClassScope);

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                    new EPTypeCodegenSharable(forge.innerForge.TypeInfo, codegenClassScope));
            }

            methodNode.Block.DeclareVar<ICollection<EventBean>>(
                    "inner",
                    forge.innerForge.EvaluateGetROCollectionScalarCodegen(methodNode, exprSymbol, codegenClassScope))
                .Apply(
                    InstrumentationCode.Instblock(
                        codegenClassScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("inner"),
                        Constant(forge.forgesUnpacking.Length)))
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
                        typeof(ICollection<EventBean>),
                        forge.forgesIteratorEventBean,
                        null))
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace