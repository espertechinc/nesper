///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval.deltaexpr
{
    public class IntervalDeltaExprMSecConstForge : IntervalDeltaExprForge,
        IntervalDeltaExprEvaluator
    {
        private readonly long _msec;

        public IntervalDeltaExprMSecConstForge(long msec)
        {
            _msec = msec;
        }

        public long Evaluate(
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return _msec;
        }

        public IntervalDeltaExprEvaluator MakeEvaluator()
        {
            return this;
        }

        public CodegenExpression Codegen(
            CodegenExpression reference,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return Constant(_msec);
        }
    }
} // end of namespace