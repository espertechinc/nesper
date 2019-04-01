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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Like-Node Form-1: constant pattern
    /// </summary>
    public class ExprLikeNodeForgeConst : ExprLikeNodeForge
    {
        public ExprLikeNodeForgeConst(
            ExprLikeNode parent, bool isNumericValue, LikeUtil likeUtil, CodegenExpression likeUtilInit) : base(
            parent, isNumericValue)
        {
            LikeUtil = likeUtil;
            LikeUtilInit = likeUtilInit;
        }

        public override ExprEvaluator ExprEvaluator => new ExprLikeNodeForgeConstEval(
            this, ForgeRenderable.ChildNodes[0].Forge.ExprEvaluator);

        public CodegenExpression LikeUtilInit { get; }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public LikeUtil LikeUtil { get; }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = ExprLikeNodeForgeConstEval.Codegen(
                this, ForgeRenderable.ChildNodes[0], codegenMethodScope, exprSymbol, codegenClassScope);
            return LocalMethod(methodNode);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprLike", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }
    }
} // end of namespace