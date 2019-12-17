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
    public class ExprBitWiseNodeForge : ExprForgeInstrumentable
    {
        public ExprBitWiseNodeForge(
            ExprBitWiseNode parent,
            Type resultType,
            BitWiseOpEnum.Computer computer)
        {
            ForgeRenderable = parent ?? throw new ArgumentNullException(nameof(parent));
            EvaluationType = resultType ?? throw new ArgumentNullException(nameof(resultType));
            Computer = computer ?? throw new ArgumentNullException(nameof(computer));
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprBitWiseNode ForgeRenderable { get; }

        public BitWiseOpEnum.Computer Computer { get; }

        public Type EvaluationType { get; }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator => new ExprBitWiseNodeForgeEval(
            this,
            ForgeRenderable.ChildNodes[0].Forge.ExprEvaluator,
            ForgeRenderable.ChildNodes[1].Forge.ExprEvaluator);

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprBitwise",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Qparam(Constant(ForgeRenderable.BitWiseOpEnum))
                .Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprBitWiseNodeForgeEval.Codegen(
                this,
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope,
                ForgeRenderable.ChildNodes[0],
                ForgeRenderable.ChildNodes[1]);
        }
    }
} // end of namespace