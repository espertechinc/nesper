///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStream : ExprDotNodeForge
    {
        private readonly ExprDotNodeImpl _parent;
        private readonly FilterExprAnalyzerAffector _filterExprAnalyzerAffector;
        private readonly int _streamNumber;
        private readonly EventType _eventType;
        private readonly ExprDotForge[] _evaluators;
        private readonly bool _method;
        private readonly Type _evaluationType;

        public ExprDotNodeForgeStream(
            ExprDotNodeImpl parent,
            FilterExprAnalyzerAffector filterExprAnalyzerAffector,
            int streamNumber,
            EventType eventType,
            ExprDotForge[] evaluators,
            bool method)
        {
            _parent = parent;
            _filterExprAnalyzerAffector = filterExprAnalyzerAffector;
            _streamNumber = streamNumber;
            _eventType = eventType;
            _evaluators = evaluators;
            _method = method;
            
            var last = evaluators[^1];
            var lastType = last.TypeInfo;
            var evaluationTypeUnboxed = !method ? ((EPChainableTypeClass)lastType).Clazz : lastType.GetNormalizedType();

            _evaluationType = evaluationTypeUnboxed.GetBoxedType();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!_method) {
                return ExprDotNodeForgeStreamEvalEventBean.Codegen(
                    this,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            return ExprDotNodeForgeStreamEvalMethod.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
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

        public override bool IsReturnsConstantResult => false;

        public EventType EventType => _eventType;

        public override bool IsLocalInlinedClass => false;

        public override ExprEvaluator ExprEvaluator {
            get {
                if (!_method) {
                    return new ExprDotNodeForgeStreamEvalEventBean(this, ExprDotNodeUtility.GetEvaluators(_evaluators));
                }

                return new ExprDotNodeForgeStreamEvalMethod(this, ExprDotNodeUtility.GetEvaluators(_evaluators));
            }
        }

        public override Type EvaluationType => _evaluationType;

        public int StreamNumber => _streamNumber;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => _filterExprAnalyzerAffector;

        public override int? StreamNumReferenced => _streamNumber;

        public override string RootPropertyName => null;

        public ExprDotNodeImpl ForgeRenderable => _parent;

        public override ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public ExprDotForge[] Evaluators => _evaluators;
    }
} // end of namespace