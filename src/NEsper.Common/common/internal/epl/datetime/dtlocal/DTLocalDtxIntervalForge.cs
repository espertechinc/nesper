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
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxIntervalForge : DTLocalForgeIntervalBase
    {
        public DTLocalDtxIntervalForge(IntervalForge intervalForge)
            : base(intervalForge)
        {
        }

        public override DTLocalEvaluator DTEvaluator => new DTLocalDtxIntervalEval(intervalForge.Op);

        public override DTLocalEvaluatorIntervalComp MakeEvaluatorComp()
        {
            return new DTLocalDtxIntervalEval(intervalForge.Op);
        }

        public override CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalDtxIntervalEval.Codegen(this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override CodegenExpression Codegen(
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalDtxIntervalEval.Codegen(this, start, end, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace