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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeCoercion : ExprEqualsNodeForge
    {
        private readonly Coercer _coercerLHS;
        private readonly Coercer _coercerRHS;
        private readonly Type _typeLHS;
        private readonly Type _typeRHS;

        public ExprEqualsNodeForgeCoercion(
            ExprEqualsNodeImpl parent,
            Coercer coercerLhs,
            Coercer coercerRhs,
            Type typeLhs,
            Type typeRhs) : base(parent)
        {
            _coercerLHS = coercerLhs;
            _coercerRHS = coercerRhs;
            _typeLHS = typeLhs;
            _typeRHS = typeRhs;
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var lhs = ForgeRenderable.ChildNodes[0];
            var rhs = ForgeRenderable.ChildNodes[1];
            var method = ExprEqualsNodeForgeCoercionEval.Codegen(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope,
                lhs,
                rhs);
            return LocalMethod(method);
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
                ForgeRenderable.IsIs ? "ExprIs" : "ExprEquals",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public override ExprEvaluator ExprEvaluator {
            get {
                var lhs = ForgeRenderable.ChildNodes[0];
                var rhs = ForgeRenderable.ChildNodes[1];
                return new ExprEqualsNodeForgeCoercionEval(
                    ForgeRenderable,
                    lhs.Forge.ExprEvaluator,
                    rhs.Forge.ExprEvaluator,
                    _coercerLHS,
                    _coercerRHS);
            }
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Coercer CoercerLHS => _coercerLHS;

        public Coercer CoercerRHS => _coercerRHS;

        public Type TypeLhs => _typeLHS;

        public Type TypeRhs => _typeRHS;
    }
} // end of namespace