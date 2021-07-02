///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.inner;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeRootChild : ExprDotNodeForge,
        ExprEnumerationForge
    {
        internal readonly ExprDotForge[] forgesIteratorEventBean;
        internal readonly ExprDotForge[] forgesUnpacking;
        internal readonly ExprDotEvalRootChildInnerForge innerForge;

        public ExprDotNodeForgeRootChild(
            ExprDotNodeImpl parent,
            FilterExprAnalyzerAffector filterExprAnalyzerAffector,
            int? streamNumReferenced,
            string rootPropertyName,
            bool hasEnumerationMethod,
            ExprForge rootNodeForge,
            ExprEnumerationForge rootLambdaEvaluator,
            EPChainableType typeInfo,
            ExprDotForge[] forgesIteratorEventBean,
            ExprDotForge[] forgesUnpacking,
            bool checkedUnpackEvent)
        {
            if (forgesUnpacking.Length == 0) {
                throw new ArgumentException("Empty forges-unpacking");
            }
            
            Parent = parent;
            FilterExprAnalyzerAffector = filterExprAnalyzerAffector;
            StreamNumReferenced = streamNumReferenced;
            RootPropertyName = rootPropertyName;
            if (rootLambdaEvaluator != null) {
                if (typeInfo is EPChainableTypeEventMulti typeEventMulti) {
                    innerForge = new InnerDotEnumerableEventCollectionForge(
                        rootLambdaEvaluator,
                        typeEventMulti.Component);
                }
                else if (typeInfo is EPChainableTypeEventSingle typeEventSingle) {
                    innerForge = new InnerDotEnumerableEventBeanForge(
                        rootLambdaEvaluator,
                        typeEventSingle.EventType);
                }
                else {
                    var type = (EPChainableTypeClass) typeInfo;
                    var component = TypeHelper.GetSingleParameterTypeOrObject(type.Clazz);
                    innerForge = new InnerDotEnumerableScalarCollectionForge(
                        rootLambdaEvaluator,
                        component);
                }
            }
            else {
                if (checkedUnpackEvent) {
                    innerForge = new InnerDotScalarUnpackEventForge(rootNodeForge);
                }
                else {
                    var returnType = rootNodeForge.EvaluationType;
                    if (hasEnumerationMethod && returnType.IsArray) {
                        if (returnType.GetElementType().CanNotBeNull()) {
                            innerForge = new InnerDotArrPrimitiveToCollForge(rootNodeForge);
                        }
                        else {
                            innerForge = new InnerDotArrObjectToCollForge(rootNodeForge);
                        }
                    }
                    else if (hasEnumerationMethod && returnType.IsGenericCollection()) {
                        innerForge = new InnerDotCollForge(rootNodeForge);
                    }
                    else {
                        innerForge = new InnerDotScalarForge(rootNodeForge);
                    }
                }
            }

            this.forgesUnpacking = forgesUnpacking;
            this.forgesIteratorEventBean = forgesIteratorEventBean;
        }

        public override ExprEvaluator ExprEvaluator =>
            new ExprDotNodeForgeRootChildEval(
                this,
                innerForge.InnerEvaluator,
                ExprDotNodeUtility.GetEvaluators(forgesIteratorEventBean),
                ExprDotNodeUtility.GetEvaluators(forgesUnpacking));

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType {
            get {
                var lastDotForge = forgesUnpacking[forgesUnpacking.Length - 1];
                var lastTypeInfo = lastDotForge.TypeInfo;
                return lastTypeInfo.GetNormalizedClass();
            }
        }

        public ExprDotNodeImpl Parent { get; }

        public override bool IsReturnsConstantResult => false;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector { get; }

        public override int? StreamNumReferenced { get; }

        public override string RootPropertyName { get; }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeRootChildEval.CodegenEvaluateGetROCollectionEvents(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeRootChildEval.CodegenEvaluateGetROCollectionScalar(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return innerForge.EventTypeCollection;
        }

        public Type ComponentTypeCollection => innerForge.ComponentTypeCollection;

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => (ExprEnumerationEval) ExprEvaluator;

        public override ExprNodeRenderable ExprForgeRenderable => Parent;

        public ExprNodeRenderable EnumForgeRenderable => Parent;

        public override bool IsLocalInlinedClass => false;

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprDot",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeRootChildEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace