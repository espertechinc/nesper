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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Like-Node Form-1: non-constant pattern
    /// </summary>
    public class ExprLikeNodeForgeNonconst : ExprLikeNodeForge
    {
        public ExprLikeNodeForgeNonconst(
            ExprLikeNode parent,
            bool isNumericValue)
            : base(parent, isNumericValue)
        {
        }

        public override ExprEvaluator ExprEvaluator => new ExprLikeNodeFormNonconstEval(
            this,
            ForgeRenderable.ChildNodes[0].Forge.ExprEvaluator,
            ForgeRenderable.ChildNodes[1].Forge.ExprEvaluator,
            ForgeRenderable.ChildNodes.Length == 2 ? null : ForgeRenderable.ChildNodes[2].Forge.ExprEvaluator);

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = ExprLikeNodeFormNonconstEval.Codegen(
                this,
                ForgeRenderable.ChildNodes[0],
                ForgeRenderable.ChildNodes[1],
                ForgeRenderable.ChildNodes.Length == 2 ? null : ForgeRenderable.ChildNodes[2],
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
            return LocalMethod(methodNode);
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
                "ExprLike",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }
    }
} // end of namespace