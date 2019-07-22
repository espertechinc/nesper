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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    public class ExprInNodeForge : ExprForgeInstrumentable
    {
        private readonly ExprInNodeImpl parent;
        private readonly bool mustCoerce;
        private readonly SimpleNumberCoercer coercer;
        private readonly Type coercionType;
        private readonly bool hasCollectionOrArray;

        public ExprInNodeForge(
            ExprInNodeImpl parent,
            bool mustCoerce,
            SimpleNumberCoercer coercer,
            Type coercionType,
            bool hasCollectionOrArray)
        {
            this.parent = parent;
            this.mustCoerce = mustCoerce;
            this.coercer = coercer;
            this.coercionType = coercionType;
            this.hasCollectionOrArray = hasCollectionOrArray;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                ExprEvaluator[] evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes);
                if (hasCollectionOrArray) {
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
                    this.GetType(),
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
            get => parent;
        }

        public bool IsMustCoerce {
            get => mustCoerce;
        }

        public SimpleNumberCoercer Coercer {
            get => coercer;
        }

        public Type CoercionType {
            get => coercionType;
        }

        public bool HasCollectionOrArray {
            get => hasCollectionOrArray;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace