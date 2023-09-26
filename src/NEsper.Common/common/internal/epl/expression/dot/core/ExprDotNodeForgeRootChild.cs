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
        private readonly ExprDotNodeImpl parent;
        private readonly FilterExprAnalyzerAffector filterExprAnalyzerAffector;
        private readonly int? streamNumReferenced;
        private readonly string rootPropertyName;
        private readonly ExprDotEvalRootChildInnerForge innerForge;
        private readonly ExprDotForge[] forgesIteratorEventBean;
        private readonly ExprDotForge[] forgesUnpacking;

        public ExprDotForge[] ForgesUnpacking => forgesUnpacking;

        public ExprDotForge[] ForgesIteratorEventBean => forgesIteratorEventBean;

        public ExprDotEvalRootChildInnerForge InnerForge => innerForge;

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

            this.parent = parent;
            this.filterExprAnalyzerAffector = filterExprAnalyzerAffector;
            this.streamNumReferenced = streamNumReferenced;
            this.rootPropertyName = rootPropertyName;
            if (rootLambdaEvaluator != null) {
                if (typeInfo is EPChainableTypeEventMulti multi) {
                    innerForge = new InnerDotEnumerableEventCollectionForge(rootLambdaEvaluator, multi.Component);
                }
                else if (typeInfo is EPChainableTypeEventSingle single) {
                    innerForge = new InnerDotEnumerableEventBeanForge(rootLambdaEvaluator, single.EventType);
                }
                else {
                    var type = (EPChainableTypeClass)typeInfo;
                    var component = type.Clazz.GetComponentType();
                    innerForge = new InnerDotEnumerableScalarCollectionForge(rootLambdaEvaluator, component);
                }
            }
            else {
                if (checkedUnpackEvent) {
                    innerForge = new InnerDotScalarUnpackEventForge(rootNodeForge);
                }
                else {
                    var returnType = rootNodeForge.EvaluationType;
                    if (hasEnumerationMethod && returnType is Type type && type.IsArray) {
                        if (type.GetElementType().IsPrimitive) {
                            innerForge = new InnerDotArrPrimitiveToCollForge(rootNodeForge);
                        }
                        else {
                            innerForge = new InnerDotArrObjectToCollForge(rootNodeForge);
                        }
                    }
                    else if (hasEnumerationMethod &&
                             returnType is Type &&
                             returnType.IsGenericCollection()) {
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
                codegenClassScope).Build();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeRootChildEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

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

        public override bool IsReturnsConstantResult => false;
        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => filterExprAnalyzerAffector;
        public override int? StreamNumReferenced => streamNumReferenced;
        public override string RootPropertyName => rootPropertyName;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var last = forgesIteratorEventBean[^1];
            var type = last.TypeInfo;
            if (type is EPChainableTypeEventMulti multi) {
                return multi.Component;
            }

            return null;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => ExprEvaluatorCovariant;
        public ExprNodeRenderable ForgeRenderable => parent;

        public override ExprNodeRenderable ExprForgeRenderable => parent;
        public ExprNodeRenderable EnumForgeRenderable => parent;
        public override bool IsLocalInlinedClass => false;

        public override ExprEvaluator ExprEvaluator => ExprEvaluatorCovariant;

        public ExprDotNodeForgeRootChildEval ExprEvaluatorCovariant => new ExprDotNodeForgeRootChildEval(
            this,
            innerForge.InnerEvaluator,
            ExprDotNodeUtility.GetEvaluators(forgesIteratorEventBean),
            ExprDotNodeUtility.GetEvaluators(forgesUnpacking));

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType {
            get {
                var last = forgesUnpacking[^1];
                var type = last.TypeInfo;
                return type.GetNormalizedType();
            }
        }

        public ExprDotNodeImpl Parent => parent;

        public Type ComponentTypeCollection {
            get {
                var last = forgesUnpacking[^1];
                var type = last.TypeInfo;
                var normalized = type.GetNormalizedType();
                if (normalized.IsGenericCollection()) {
                    return normalized.GetCollectionItemType();
                }

                return null;
            }
        }
    }
} // end of namespace