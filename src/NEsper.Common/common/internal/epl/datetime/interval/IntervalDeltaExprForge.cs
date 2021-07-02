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

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public interface IntervalDeltaExprForge
    {
        IntervalDeltaExprEvaluator MakeEvaluator();

        CodegenExpression Codegen(
            CodegenExpression reference,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }

    public class ProxyIntervalDeltaExprForge : IntervalDeltaExprForge
    {
        public Func<IntervalDeltaExprEvaluator> procMakeEvaluator;

        public Func<CodegenExpression, CodegenMethodScope, ExprForgeCodegenSymbol, CodegenClassScope, CodegenExpression>
            procCodegen;

        public IntervalDeltaExprEvaluator MakeEvaluator()
        {
            return procMakeEvaluator();
        }

        public CodegenExpression Codegen(
            CodegenExpression reference,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return procCodegen(
                reference,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace