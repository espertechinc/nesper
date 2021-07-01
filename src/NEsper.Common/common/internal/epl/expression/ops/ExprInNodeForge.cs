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
            this._parent = parent;
            this._mustCoerce = mustCoerce;
            this._coercer = coercer;
            this._coercionType = coercionType;
            this._hasCollectionOrArray = hasCollectionOrArray;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                ExprEvaluator[] evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
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

        public Type EvaluationType {
            get => typeof(bool?);
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprInNodeImpl ForgeRenderable {
            get => _parent;
        }

        public bool IsMustCoerce {
            get => _mustCoerce;
        }

        public Coercer Coercer {
            get => _coercer;
        }

        public Type CoercionType {
            get => _coercionType;
        }

        public bool HasCollectionOrArray {
            get => _hasCollectionOrArray;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace