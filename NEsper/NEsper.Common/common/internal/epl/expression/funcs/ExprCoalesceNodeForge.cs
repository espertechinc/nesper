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

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCoalesceNodeForge : ExprForgeInstrumentable
    {
        public ExprCoalesceNodeForge(ExprCoalesceNode parent, Type resultType, bool[] isNumericCoercion)
        {
            ForgeRenderable = parent;
            EvaluationType = resultType;
            IsNumericCoercion = isNumericCoercion;
        }

        public ExprCoalesceNode ForgeRenderable { get; }

        public bool[] IsNumericCoercion { get; }

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator => new ExprCoalesceNodeForgeEval(
            this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes));

        public Type EvaluationType { get; }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprCoalesceNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprCoalesce", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }
    }
} // end of namespace