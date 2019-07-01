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
using com.espertech.esper.common.@internal.type;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprMathNodeForge : ExprForgeInstrumentable
    {
        public ExprMathNodeForge(
            ExprMathNode parent,
            MathArithType.Computer arithTypeEnumComputer,
            Type resultType)
        {
            ForgeRenderable = parent;
            ArithTypeEnumComputer = arithTypeEnumComputer;
            EvaluationType = resultType;
        }

        internal MathArithType.Computer ArithTypeEnumComputer { get; }

        public ExprMathNode ForgeRenderable { get; }

        public ExprEvaluator ExprEvaluator => new ExprMathNodeForgeEval(
            this, ForgeRenderable.ChildNodes[0].Forge.ExprEvaluator, ForgeRenderable.ChildNodes[1].Forge.ExprEvaluator);

        public Type EvaluationType { get; }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = ExprMathNodeForgeEval.Codegen(
                this, codegenMethodScope, exprSymbol, codegenClassScope, ForgeRenderable.ChildNodes[0], ForgeRenderable.ChildNodes[1]);
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(GetType(), this, "ExprMath", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Qparam(Constant(ForgeRenderable.MathArithTypeEnum.GetExpressionText())).Build();
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace