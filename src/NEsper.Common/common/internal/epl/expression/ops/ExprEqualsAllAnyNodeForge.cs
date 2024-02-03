///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsAllAnyNodeForge : ExprForgeInstrumentable
    {
        private readonly bool _hasCollectionOrArray;

        public ExprEqualsAllAnyNodeForge(
            ExprEqualsAllAnyNode parent,
            bool mustCoerce,
            Coercer coercer,
            Type coercionTypeBoxed,
            bool hasCollectionOrArray)
        {
            ForgeRenderable = parent;
            IsMustCoerce = mustCoerce;
            Coercer = coercer;
            CoercionTypeBoxed = coercionTypeBoxed;
            _hasCollectionOrArray = hasCollectionOrArray;
        }

        public ExprEqualsAllAnyNode ForgeRenderable { get; }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public bool IsMustCoerce { get; }

        public Coercer Coercer { get; }

        public Type CoercionTypeBoxed { get; }

        public ExprEvaluator ExprEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
                if (ForgeRenderable.IsAll) {
                    if (!_hasCollectionOrArray) {
                        return new ExprEqualsAllAnyNodeForgeEvalAllNoColl(this, evaluators);
                    }

                    return new ExprEqualsAllAnyNodeForgeEvalAllWColl(this, evaluators);
                }

                if (!_hasCollectionOrArray) {
                    return new ExprEqualsAllAnyNodeForgeEvalAnyNoColl(this, evaluators);
                }

                return new ExprEqualsAllAnyNodeForgeEvalAnyWColl(this, evaluators);
            }
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprEqualsAnyOrAll",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (ForgeRenderable.IsAll) {
                return ExprEqualsAllAnyNodeForgeEvalAllWColl.Codegen(
                    this,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            return ExprEqualsAllAnyNodeForgeEvalAnyWColl.Codegen(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public Type EvaluationType => typeof(bool?);
    }
} // end of namespace