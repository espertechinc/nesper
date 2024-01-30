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
        private readonly ExprDotNodeImpl _parent;
        private readonly FilterExprAnalyzerAffector _filterExprAnalyzerAffector;
        private readonly int? _streamNumReferenced;
        private readonly string _rootPropertyName;
        private readonly ExprDotEvalRootChildInnerForge _innerForge;
        private readonly ExprDotForge[] _forgesIteratorEventBean;
        private readonly ExprDotForge[] _forgesUnpacking;

        public ExprDotForge[] ForgesUnpacking => _forgesUnpacking;

        public ExprDotForge[] ForgesIteratorEventBean => _forgesIteratorEventBean;

        public ExprDotEvalRootChildInnerForge InnerForge => _innerForge;

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

            _parent = parent;
            _filterExprAnalyzerAffector = filterExprAnalyzerAffector;
            _streamNumReferenced = streamNumReferenced;
            _rootPropertyName = rootPropertyName;
            if (rootLambdaEvaluator != null) {
                if (typeInfo is EPChainableTypeEventMulti multi) {
                    _innerForge = new InnerDotEnumerableEventCollectionForge(rootLambdaEvaluator, multi.Component);
                }
                else if (typeInfo is EPChainableTypeEventSingle single) {
                    _innerForge = new InnerDotEnumerableEventBeanForge(rootLambdaEvaluator, single.EventType);
                }
                else {
                    var type = (EPChainableTypeClass)typeInfo;
                    var component = type.Clazz.GetComponentType();
                    _innerForge = new InnerDotEnumerableScalarCollectionForge(rootLambdaEvaluator, component);
                }
            }
            else {
                if (checkedUnpackEvent) {
                    _innerForge = new InnerDotScalarUnpackEventForge(rootNodeForge);
                }
                else {
                    var returnType = rootNodeForge.EvaluationType;
                    if (hasEnumerationMethod && returnType is Type type && type.IsArray) {
                        if (type.GetElementType().IsPrimitive) {
                            _innerForge = new InnerDotArrPrimitiveToCollForge(rootNodeForge);
                        }
                        else {
                            _innerForge = new InnerDotArrObjectToCollForge(rootNodeForge);
                        }
                    }
                    else if (hasEnumerationMethod &&
                             returnType is Type &&
                             returnType.IsGenericCollection()) {
                        _innerForge = new InnerDotCollForge(rootNodeForge);
                    }
                    else {
                        _innerForge = new InnerDotScalarForge(rootNodeForge);
                    }
                }
            }

            _forgesUnpacking = forgesUnpacking;
            _forgesIteratorEventBean = forgesIteratorEventBean;
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
        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => _filterExprAnalyzerAffector;
        public override int? StreamNumReferenced => _streamNumReferenced;
        public override string RootPropertyName => _rootPropertyName;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var last = _forgesIteratorEventBean[^1];
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
        public ExprNodeRenderable ForgeRenderable => _parent;

        public override ExprNodeRenderable ExprForgeRenderable => _parent;
        public ExprNodeRenderable EnumForgeRenderable => _parent;
        public override bool IsLocalInlinedClass => false;

        public override ExprEvaluator ExprEvaluator => ExprEvaluatorCovariant;

        public ExprDotNodeForgeRootChildEval ExprEvaluatorCovariant => new ExprDotNodeForgeRootChildEval(
            this,
            _innerForge.InnerEvaluator,
            ExprDotNodeUtility.GetEvaluators(_forgesIteratorEventBean),
            ExprDotNodeUtility.GetEvaluators(_forgesUnpacking));

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType {
            get {
                var last = _forgesUnpacking[^1];
                var type = last.TypeInfo;
                return type.GetNormalizedType();
            }
        }

        public ExprDotNodeImpl Parent => _parent;

        public Type ComponentTypeCollection {
            get {
                var last = _forgesUnpacking[^1];
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