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
        private readonly bool audit;
        private readonly bool isCache;
        private readonly ExprDeclaredNodeImpl parent;
        private readonly string statementName;
        [NonSerialized] private ExprEnumerationEval innerEvaluatorLambdaLazy;

        [NonSerialized] private ExprEvaluator innerEvaluatorLazy;
        [NonSerialized] private ExprTypableReturnEval innerEvaluatorTypableLazy;

        public ExprDeclaredForgeBase(
            ExprDeclaredNodeImpl parent,
            ExprForge innerForge,
            bool isCache,
            bool audit,
            string statementName)
        {
            this.parent = parent;
            InnerForge = innerForge;
            this.isCache = isCache;
            this.audit = audit;
            this.statementName = statementName;
        }

        public ExprTypableReturnEval TypableReturnEvaluator => this;

        public ExprForge InnerForge { get; }

        public IDictionary<string, object> RowProperties {
            get {
                if (InnerForge is ExprTypableReturnForge) {
                    return ((ExprTypableReturnForge) InnerForge).RowProperties;
                }

                return null;
            }
        }

        public ExprNodeRenderable ExprForgeRenderable => parent;
        public ExprNodeRenderable EnumForgeRenderable => parent;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream);
            var result =
                innerEvaluatorLambdaLazy.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
            return result;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream);
            return innerEvaluatorLambdaLazy.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            InitInnerEvaluatorLambda();
            return innerEvaluatorLambdaLazy.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>),
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(refEPS, methodNode, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateGetROCollectionEventsCodegenRewritten(methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>),
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(refEPS, methodNode, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateGetROCollectionScalarCodegenRewritten(methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        public Type ComponentTypeCollection {
            get {
                if (InnerForge is ExprEnumerationForge) {
                    return ((ExprEnumerationForge) InnerForge).ComponentTypeCollection;
                }

                return null;
            }
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (InnerForge is ExprEnumerationForge) {
                return ((ExprEnumerationForge) InnerForge).GetEventTypeCollection(
                    statementRawInfo,
                    compileTimeServices);
            }

            return null;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (InnerForge is ExprEnumerationForge) {
                return ((ExprEnumerationForge) InnerForge).GetEventTypeSingle(statementRawInfo, compileTimeServices);
            }

            return null;
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprEnumerationForge) InnerForge).EvaluateGetEventBeanCodegen(
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => InnerForge.EvaluationType;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!audit) {
                return EvaluateCodegenNoAudit(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            var evaluationType = requiredType == typeof(object) ? typeof(object) : InnerForge.EvaluationType;
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
                        .Add("getAuditProvider")
                        .Add(
                            "exprdef",
                            Constant(parent.Prototype.Name),
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
                    codegenClassScope)
                .Qparams(GetInstrumentationQParams(parent, codegenClassScope))
                .Build();
        }

        public abstract EventBean[] GetEventsPerStreamRewritten(EventBean[] eventsPerStream);

        protected abstract CodegenExpression CodegenEventsPerStreamRewritten(
            CodegenExpression eventsPerStream,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public bool? IsMultirow {
            get {
                if (InnerForge is ExprTypableReturnForge) {
                    return ((ExprTypableReturnForge) InnerForge).IsMultirow;
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
            return innerEvaluatorTypableLazy.EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprTypableReturnForge) InnerForge).EvaluateTypableSingleCodegen(
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
            return innerEvaluatorTypableLazy.EvaluateTypableMulti(eventsPerStream, isNewData, context);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ((ExprTypableReturnForge) InnerForge).EvaluateTypableMultiCodegen(
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
            eventsPerStream = GetEventsPerStreamRewritten(eventsPerStream);
            return innerEvaluatorLazy.Evaluate(eventsPerStream, isNewData, context);
        }

        protected internal static CodegenExpression[] GetInstrumentationQParams(
            ExprDeclaredNodeImpl parent,
            CodegenClassScope codegenClassScope)
        {
            string expressionText = null;
            if (codegenClassScope.IsInstrumented) {
                expressionText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(parent.ExpressionBodyCopy);
            }

            return new[] {
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
            var evaluationType = requiredType == typeof(object) ? typeof(object) : InnerForge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                evaluationType,
                typeof(ExprDeclaredForgeBase),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            methodNode.Block
                .DeclareVar<EventBean[]>(
                    "rewritten",
                    CodegenEventsPerStreamRewritten(refEPS, methodNode, codegenClassScope))
                .MethodReturn(
                    LocalMethod(
                        EvaluateCodegenRewritten(requiredType, methodNode, codegenClassScope),
                        Ref("rewritten"),
                        refIsNewData,
                        refExprEvalCtx));
            return LocalMethod(methodNode);
        }

        private CodegenMethod EvaluateCodegenRewritten(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var nodeObject = GetNodeObject(codegenClassScope);
            var evaluationType = requiredType == typeof(object) ? typeof(object) : InnerForge.EvaluationType;

            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(evaluationType, typeof(ExprDeclaredForgeBase), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEPS(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue = InnerForge.EvaluateCodegen(
                requiredType,
                methodNode,
                scope,
                codegenClassScope);

            // produce derived symbols
            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (isCache) {
                CodegenExpression eval = ExprDotName(Ref("entry"), "Result");
                if (evaluationType != typeof(object)) {
                    eval = Cast(InnerForge.EvaluationType.GetBoxedType(), eval);
                }

                block.DeclareVar<ExpressionResultCacheForDeclaredExprLastValue>(
                        "cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Add("getExpressionResultCacheService")
                            .Add("getAllocateDeclaredExprLastValue"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndObj>(
                        "entry",
                        ExprDotMethod(Ref("cache"), "getDeclaredExpressionLastValue", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(eval)
                    .DeclareVar(evaluationType, "result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "saveDeclaredExpressionLastValue",
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

        private CodegenMethod EvaluateGetROCollectionEventsCodegenRewritten(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var nodeObject = GetNodeObject(codegenClassScope);

            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(ExprDeclaredForgeBase),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEPS(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue =
                ((ExprEnumerationForge) InnerForge).EvaluateGetROCollectionEventsCodegen(
                    methodNode,
                    scope,
                    codegenClassScope);

            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (isCache) {
                block.DeclareVar<ExpressionResultCacheForDeclaredExprLastColl>(
                        "cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Add("getExpressionResultCacheService")
                            .Add("getAllocateDeclaredExprLastColl"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndCollBean>(
                        "entry",
                        ExprDotMethod(Ref("cache"), "getDeclaredExpressionLastColl", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(ExprDotName(Ref("entry"), "Result"))
                    .DeclareVar<ICollection<object>>("result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "saveDeclaredExpressionLastColl",
                            nodeObject,
                            refEPS,
                            Ref("result")));
            }
            else {
                block.DeclareVar<ICollection<object>>("result", innerValue);
            }

            block.MethodReturn(Ref("result"));
            return methodNode;
        }

        private CodegenMethod EvaluateGetROCollectionScalarCodegenRewritten(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var nodeObject = GetNodeObject(codegenClassScope);
            var scope = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(ExprDeclaredForgeBase),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression refEPS = scope.GetAddEPS(methodNode);
            CodegenExpression refExprEvalCtx = scope.GetAddExprEvalCtx(methodNode);

            // generate code for the inner value so we know its symbols and derived symbols
            var innerValue =
                ((ExprEnumerationForge) InnerForge).EvaluateGetROCollectionScalarCodegen(
                    methodNode,
                    scope,
                    codegenClassScope);

            // produce derived symbols
            var block = methodNode.Block;
            scope.DerivedSymbolsCodegen(methodNode, block, codegenClassScope);

            if (isCache) {
                block
                    .DeclareVar<ExpressionResultCacheForDeclaredExprLastColl>(
                        "cache",
                        ExprDotMethodChain(refExprEvalCtx)
                            .Add("getExpressionResultCacheService")
                            .Add("getAllocateDeclaredExprLastColl"))
                    .DeclareVar<ExpressionResultCacheEntryEventBeanArrayAndCollBean>(
                        "entry",
                        ExprDotMethod(Ref("cache"), "getDeclaredExpressionLastColl", nodeObject, refEPS))
                    .IfCondition(NotEqualsNull(Ref("entry")))
                    .BlockReturn(ExprDotName(Ref("entry"), "Result"))
                    .DeclareVar<ICollection<object>>("result", innerValue)
                    .Expression(
                        ExprDotMethod(
                            Ref("cache"),
                            "saveDeclaredExpressionLastColl",
                            nodeObject,
                            refEPS,
                            Ref("result")));
            }
            else {
                block.DeclareVar<ICollection<object>>("result", innerValue);
            }

            block.MethodReturn(Ref("result"));
            return methodNode;
        }

        private void InitInnerEvaluator()
        {
            if (innerEvaluatorLazy == null) {
                innerEvaluatorLazy = InnerForge.ExprEvaluator;
            }
        }

        private void InitInnerEvaluatorLambda()
        {
            if (InnerForge is ExprEnumerationForge && innerEvaluatorLambdaLazy == null) {
                innerEvaluatorLambdaLazy = ((ExprEnumerationForge) InnerForge).ExprEvaluatorEnumeration;
            }
        }

        private void InitInnerEvaluatorTypable()
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        private CodegenExpressionField GetNodeObject(CodegenClassScope codegenClassScope)
        {
            return ExpressionDeployTimeResolver.MakeRuntimeCacheKeyField(
                parent.PrototypeWVisibility,
                codegenClassScope,
                GetType());
        }
    }
} // end of namespace