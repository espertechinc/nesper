///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.runtime;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public abstract class ExprDeclaredForgeBase : ExprForgeInstrumentable,
        ExprTypableReturnForge,
        ExprTypableReturnEval,
        ExprEnumerationForge,
        ExprEnumerationEval
    {
        private readonly ExprDeclaredNodeImpl _parent;
        private readonly ExprForge _innerForge;
        private readonly bool _isCache;
        private readonly bool _audit;
        private readonly string _statementName;

        [JsonIgnore] private ExprEvaluator _innerEvaluatorLazy;
        [JsonIgnore] private ExprEnumerationEval _innerEvaluatorLambdaLazy;
        [JsonIgnore] private ExprTypableReturnEval _innerEvaluatorTypableLazy;

        public abstract EventBean[] GetEventsPerStreamRewritten(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        protected abstract CodegenExpression CodegenEventsPerStreamRewritten(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public ExprDeclaredForgeBase(
            ExprDeclaredNodeImpl parent,
            ExprForge innerForge,
            bool isCache,
            bool audit,
            string statementName)
        {
            _parent = parent;
            _innerForge = innerForge;
            _isCache = isCache;
            _audit = audit;
            _statementName = statementName;
        }

        public ExprTypableReturnEval TypableReturnEvaluator => this;

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => _innerForge.EvaluationType;

        public ExprForge InnerForge => _innerForge;

        public IDictionary<string, object> RowProperties {
            get {
                if (_innerForge is ExprTypableReturnForge forge) {
                    return forge.RowProperties;
                }

                return null;
            }
        }

        public bool? IsMultirow {
            get {
                if (_innerForge is ExprTypableReturnForge forge) {
                    return forge.IsMultirow;
                }

                return null;
            }
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorTypable();
            return _innerEvaluatorTypableLazy.EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprTypableReturnForge)_innerForge).EvaluateTypableSingleCodegen(
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorTypable();
            return _innerEvaluatorTypableLazy.EvaluateTypableMulti(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprTypableReturnForge)_innerForge).EvaluateTypableMultiCodegen(
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluator();
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream, isNewData, context);
            return _innerEvaluatorLazy.Evaluate(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!_audit) {
                return EvaluateCodegenNoAudit(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            var inner = _innerForge.EvaluationType;
            if (inner == null) {
                return ConstantNull();
            }

            var evaluationType = requiredType == typeof(object) ? typeof(object) : inner;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            methodNode.Block
                .DeclareVar(
                    evaluationType,
                    "result",
                    EvaluateCodegenNoAudit(requiredType, methodNode, exprSymbol, codegenClassScope))
                .Expression(
                    ExprDotMethodChain(exprSymbol.GetAddExprEvalCtx(methodNode))
                        .Get("AuditProvider")
                        .Add(
                            "Exprdef",
                            Constant(_parent.Prototype.Name),
                            Ref("result"),
                            exprSymbol.GetAddExprEvalCtx(methodNode)))
                .MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }

        public virtual ExprForgeConstantType ForgeConstantType => null;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprDeclared",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope).Qparams(GetInstrumentationQParams(_parent, codegenClassScope))
                .Build();
        }

        internal static CodegenExpression[] GetInstrumentationQParams(
            ExprDeclaredNodeImpl parent,
            CodegenClassScope codegenClassScope)
        {
            string expressionText = null;
            if (codegenClassScope.IsInstrumented) {
                expressionText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(parent.ExpressionBodyCopy);
            }

            return new CodegenExpression[] {
                Constant(parent.Prototype.Name),
                Constant(expressionText),
                Constant(parent.Prototype.ParametersNames)
            };
        }

        private CodegenExpression EvaluateCodegenNoAudit(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var inner = _innerForge.EvaluationType;
            if (inner == null) {
                return ConstantNull();
            }

            var evaluationType = requiredType == typeof(object) ? typeof(object) : inner;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateCodegenRewritten(requiredType, methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        private Type DetermineEvaluationType(Type requiredType)
        {
            var evaluationType = requiredType == typeof(object) ? typeof(object) : InnerForge.EvaluationType;
            if (evaluationType != requiredType) {
                if (evaluationType.GetBoxedType() == requiredType) {
                    evaluationType = evaluationType.GetBoxedType();
                }
                else {
                    throw new IllegalStateException("requiredType incompatible with evaluationType");
                }
            }

            return evaluationType;
        }
        
        private CodegenMethod EvaluateCodegenRewritten(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var nodeObject = GetNodeObject(codegenClassScope);
            var evaluationType = DetermineEvaluationType(requiredType);

            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(evaluationType, typeof(ExprDeclaredForgeBase), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEps(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue = _innerForge.EvaluateCodegen(
                requiredType,
                methodNode,
                scope,
                codegenClassScope);

            // produce derived symbols
            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (_isCache) {
                CodegenExpression eval = ExprDotName(Ref("entry"), "Result");
                if (evaluationType != typeof(object)) {
                    eval = Cast(_innerForge.EvaluationType, eval);
                }

                block
                    .DeclareVar<ExpressionResultCacheForDeclaredExprLastValue>("cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Get("ExpressionResultCacheService")
                            .Get("AllocateDeclaredExprLastValue"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndObj>("entry",
                        ExprDotMethod(Ref("cache"), "GetDeclaredExpressionLastValue", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(eval)
                    .DeclareVar(evaluationType, "result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "SaveDeclaredExpressionLastValue",
                            nodeObject,
                            refEPS,
                            Ref("result")));
            }
            else {
                block.DeclareVar(evaluationType, "result", innerValue);
            }

            block.MethodReturn(Ref("result"));
            return methodNode;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream, isNewData, context);
            var result =
                _innerEvaluatorLambdaLazy.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
            return result;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            // PREVIOUSLY: FlexCollection
            var returnType = typeof(ICollection<EventBean>);
            
            var methodNode = codegenMethodScope.MakeChild(
                returnType,
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateGetROCollectionEventsCodegenRewritten(methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        private CodegenMethod EvaluateGetROCollectionEventsCodegenRewritten(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var nodeObject = GetNodeObject(codegenClassScope);
            // PREVIOUSLY: FlexCollection
            var returnType = typeof(ICollection<EventBean>);

            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    returnType,
                    typeof(ExprDeclaredForgeBase),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEps(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue =
                ((ExprEnumerationForge)_innerForge).EvaluateGetROCollectionEventsCodegen(
                    methodNode,
                    scope,
                    codegenClassScope);

            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (_isCache) {
                block.DeclareVar<ExpressionResultCacheForDeclaredExprLastColl>(
                        "cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Get("ExpressionResultCacheService")
                            .Get("AllocateDeclaredExprLastColl"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndCollBean>(
                        "entry",
                        ExprDotMethod(Ref("cache"), "GetDeclaredExpressionLastColl", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(ExprDotName(Ref("entry"), "Result"))
                    .DeclareVar(returnType, "result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "SaveDeclaredExpressionLastColl",
                            nodeObject,
                            refEPS,
                            Ref("result")));
            }
            else {
                block.DeclareVar(returnType, "result", innerValue);
            }

            block.MethodReturn(Ref("result"));
            return methodNode;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream, isNewData, context);
            return _innerEvaluatorLambdaLazy.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var enumerationForge = (ExprEnumerationForge) _innerForge;
            var enumerationCollectionType = enumerationForge.ComponentTypeCollection;
            var collectionType = typeof(ICollection<>).MakeGenericType(enumerationCollectionType);
            
            var methodNode = codegenMethodScope.MakeChild(
                collectionType,
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateGetROCollectionScalarCodegenRewritten(methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        private CodegenMethod EvaluateGetROCollectionScalarCodegenRewritten(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var enumerationForge = (ExprEnumerationForge) _innerForge;
            var enumerationCollectionType = enumerationForge.ComponentTypeCollection;
            var collectionType = typeof(ICollection<>).MakeGenericType(enumerationCollectionType);
            
            var nodeObject = GetNodeObject(codegenClassScope);
            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    collectionType,
                    typeof(ExprDeclaredForgeBase),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEps(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue = enumerationForge.EvaluateGetROCollectionScalarCodegen(
                    methodNode,
                    scope,
                    codegenClassScope);

            // produce derived symbols
            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (_isCache) {
                block.DeclareVar<ExpressionResultCacheForDeclaredExprLastColl>("cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Get("ExpressionResultCacheService")
                            .Get("AllocateDeclaredExprLastColl"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndCollBean>("entry",
                        ExprDotMethod(Ref("cache"), "GetDeclaredExpressionLastColl", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(
                        Unwrap(enumerationCollectionType, ExprDotName(Ref("entry"), "Result")))
                    .DeclareVar(collectionType, "result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "SaveDeclaredExpressionLastColl",
                            nodeObject,
                            refEPS,
                            Unwrap<EventBean>(Ref("result"))));
            }
            else {
                block.DeclareVar(collectionType, "result", innerValue);
            }

            block.MethodReturn(Ref("result"));
            return methodNode;
        }

        public Type ComponentTypeCollection {
            get {
                if (_innerForge is ExprEnumerationForge forge) {
                    return forge.ComponentTypeCollection;
                }

                return null;
            }
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (_innerForge is ExprEnumerationForge forge) {
                return forge.GetEventTypeCollection(statementRawInfo, compileTimeServices);
            }

            return null;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (_innerForge is ExprEnumerationForge forge) {
                return forge.GetEventTypeSingle(statementRawInfo, compileTimeServices);
            }

            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            return _innerEvaluatorLambdaLazy.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprEnumerationForge)_innerForge).EvaluateGetEventBeanCodegen(
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public ExprNodeRenderable ExprForgeRenderable => _parent;
        public ExprNodeRenderable EnumForgeRenderable => _parent;

        private void InitInnerEvaluator()
        {
            if (_innerEvaluatorLazy == null) {
                _innerEvaluatorLazy = _innerForge.ExprEvaluator;
            }
        }

        private void InitInnerEvaluatorLambda()
        {
            if (_innerForge is ExprEnumerationForge forge && _innerEvaluatorLambdaLazy == null) {
                _innerEvaluatorLambdaLazy = forge.ExprEvaluatorEnumeration;
            }
        }

        private void InitInnerEvaluatorTypable()
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        private CodegenExpressionInstanceField GetNodeObject(CodegenClassScope codegenClassScope)
        {
            return ExpressionDeployTimeResolver.MakeRuntimeCacheKeyField(
                Ref("statementFields"),
                _parent.PrototypeWVisibility,
                codegenClassScope,
                GetType());
        }
    }
} // end of namespace