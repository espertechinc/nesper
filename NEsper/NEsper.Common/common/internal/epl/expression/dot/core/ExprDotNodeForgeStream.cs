///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStream : ExprDotNodeForge
    {
        private readonly bool method;
        private readonly ExprDotNodeImpl parent;

        public ExprDotNodeForgeStream(
            ExprDotNodeImpl parent, FilterExprAnalyzerAffector filterExprAnalyzerAffector, int streamNumber,
            EventType eventType, ExprDotForge[] evaluators, bool method)
        {
            this.parent = parent;
            FilterExprAnalyzerAffector = filterExprAnalyzerAffector;
            StreamNumber = streamNumber;
            EventType = eventType;
            Evaluators = evaluators;
            this.method = method;

            var last = evaluators[evaluators.Length - 1];
            if (!method) {
                if (last.TypeInfo is ClassMultiValuedEPType) {
                    EvaluationType = EPTypeHelper.GetClassMultiValuedContainer(last.TypeInfo).GetBoxedType();
                }
                else {
                    EvaluationType = EPTypeHelper.GetClassSingleValued(last.TypeInfo).GetBoxedType();
                }
            }
            else {
                EvaluationType = EPTypeHelper.GetNormalizedClass(last.TypeInfo).GetBoxedType();
            }
        }

        public override ExprEvaluator ExprEvaluator {
            get {
                if (!method) {
                    return new ExprDotNodeForgeStreamEvalEventBean(this, ExprDotNodeUtility.GetEvaluators(Evaluators));
                }

                return new ExprDotNodeForgeStreamEvalMethod(this, ExprDotNodeUtility.GetEvaluators(Evaluators));
            }
        }

        public override Type EvaluationType { get; }

        public int StreamNumber { get; }

        public override bool IsReturnsConstantResult => false;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector { get; }

        public override int? StreamNumReferenced => StreamNumber;

        public override string RootPropertyName => null;

        public override ExprNodeRenderable ForgeRenderable => parent;

        public ExprDotForge[] Evaluators { get; }

        public EventType EventType { get; }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!method) {
                return ExprDotNodeForgeStreamEvalEventBean.Codegen(
                    this, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            return ExprDotNodeForgeStreamEvalMethod.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprDot", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }
    }
} // end of namespace