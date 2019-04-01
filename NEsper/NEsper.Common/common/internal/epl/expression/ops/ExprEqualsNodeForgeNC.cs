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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNC : ExprEqualsNodeForge
    {
        public ExprEqualsNodeForgeNC(ExprEqualsNodeImpl parent) : base(parent)
        {
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprEvaluator ExprEvaluator {
            get {
                var lhs = ForgeRenderable.ChildNodes[0].Forge;
                var rhs = ForgeRenderable.ChildNodes[1].Forge;
                if (!ForgeRenderable.IsIs) {
                    return new ExprEqualsNodeForgeNCEvalEquals(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                }

                return new ExprEqualsNodeForgeNCEvalIs(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
            }
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, ForgeRenderable.IsIs ? "ExprIs" : "ExprEquals", requiredType, codegenMethodScope,
                exprSymbol, codegenClassScope).Build();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var lhs = ForgeRenderable.ChildNodes[0].Forge;
            var rhs = ForgeRenderable.ChildNodes[1].Forge;
            if (!ForgeRenderable.IsIs) {
                if (lhs.EvaluationType == null || rhs.EvaluationType == null) {
                    return ConstantNull();
                }

                return LocalMethod(
                    ExprEqualsNodeForgeNCEvalEquals.Codegen(
                        this, codegenMethodScope, exprSymbol, codegenClassScope, lhs, rhs));
            }

            return LocalMethod(
                ExprEqualsNodeForgeNCEvalIs.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope, lhs, rhs));
        }
    }
} // end of namespace