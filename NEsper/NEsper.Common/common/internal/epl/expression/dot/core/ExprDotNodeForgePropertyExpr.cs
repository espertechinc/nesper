///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgePropertyExpr : ExprDotNodeForge
    {
        private readonly string propertyName;
        private readonly string statementName;

        protected internal ExprDotNodeForgePropertyExpr(
            ExprDotNodeImpl parent,
            string statementName,
            string propertyName,
            int streamNum,
            ExprForge exprForge,
            Type propertyType,
            EventPropertyGetterIndexedSPI indexedGetter,
            EventPropertyGetterMappedSPI mappedGetter)
        {
            Parent = parent;
            this.statementName = statementName;
            this.propertyName = propertyName;
            StreamNum = streamNum;
            ExprForge = exprForge;
            EvaluationType = propertyType;
            IndexedGetter = indexedGetter;
            MappedGetter = mappedGetter;
        }

        public override ExprEvaluator ExprEvaluator {
            get {
                if (IndexedGetter != null) {
                    return new ExprDotNodeForgePropertyExprEvalIndexed(this, ExprForge.ExprEvaluator);
                }

                return new ExprDotNodeForgePropertyExprEvalMapped(this, ExprForge.ExprEvaluator);
            }
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType { get; }

        public Type Type => EvaluationType;

        public int StreamNum { get; }

        public EventPropertyGetterIndexedSPI IndexedGetter { get; }

        public EventPropertyGetterMappedSPI MappedGetter { get; }

        public ExprDotNodeImpl Parent { get; }

        public override bool IsReturnsConstantResult => false;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => StreamNum;

        public override string RootPropertyName => null;

        public ExprForge ExprForge { get; }

        public override ExprNodeRenderable ExprForgeRenderable => Parent;

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (IndexedGetter != null) {
                return ExprDotNodeForgePropertyExprEvalIndexed.Codegen(
                    this,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            return ExprDotNodeForgePropertyExprEvalMapped.Codegen(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
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

        protected internal string GetWarningText(
            string expectedType,
            object received)
        {
            var receivedText = received == null ? "null" : received.GetType().CleanName();
            return string.Format(
                "Statement '{0}' property {1} parameter expression expected a value of {2} but received {3}",
                statementName,
                propertyName,
                expectedType,
                receivedText);
        }
    }
} // end of namespace