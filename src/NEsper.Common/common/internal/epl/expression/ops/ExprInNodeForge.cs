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
    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    public class ExprInNodeForge : ExprForgeInstrumentable
    {
        private readonly ExprInNodeImpl _parent;
        private readonly bool _mustCoerce;
        private readonly Coercer _coercer;
        private readonly Type _coercionType;
        private readonly bool _hasCollectionOrArray;

        public ExprInNodeForge(
            ExprInNodeImpl parent,
            bool mustCoerce,
            Coercer coercer,
            Type coercionType,
            bool hasCollectionOrArray)
        {
            _parent = parent;
            _mustCoerce = mustCoerce;
            _coercer = coercer;
            _coercionType = coercionType;
            _hasCollectionOrArray = hasCollectionOrArray;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
                if (_hasCollectionOrArray) {
                    return new ExprInNodeForgeEvalWColl(this, evaluators);
                }

                return new ExprInNodeForgeEvalNoColl(this, evaluators);
            }
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprInNodeForgeEvalWColl.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprIn",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public Type EvaluationType => typeof(bool?);

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprInNodeImpl ForgeRenderable => _parent;

        public bool IsMustCoerce => _mustCoerce;

        public Coercer Coercer => _coercer;

        public Type CoercionType => _coercionType;

        public bool HasCollectionOrArray => _hasCollectionOrArray;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace