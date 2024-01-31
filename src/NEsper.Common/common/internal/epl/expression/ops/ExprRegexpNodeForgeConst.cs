///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.RegularExpressions;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Regex-Node Form-1: constant pattern
    /// </summary>
    public class ExprRegexpNodeForgeConst : ExprRegexpNodeForge
    {
        public ExprRegexpNodeForgeConst(
            ExprRegexpNode parent,
            bool isNumericValue,
            Regex pattern,
            CodegenExpression patternInit)
            : base(parent, isNumericValue)
        {
            Pattern = pattern;
            PatternInit = patternInit;
        }

        public override ExprEvaluator ExprEvaluator => new ExprRegexpNodeForgeConstEval(
            this,
            ForgeRenderable.ChildNodes[0].Forge.ExprEvaluator);

        internal Regex Pattern { get; }

        public CodegenExpression PatternInit { get; }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = ExprRegexpNodeForgeConstEval.Codegen(
                this,
                ForgeRenderable.ChildNodes[0],
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
                "ExprRegexp",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }
    }
} // end of namespace