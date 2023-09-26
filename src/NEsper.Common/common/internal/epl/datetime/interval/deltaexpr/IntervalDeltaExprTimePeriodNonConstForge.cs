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
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval.deltaexpr
{
    public class IntervalDeltaExprTimePeriodNonConstForge : IntervalDeltaExprForge,
        IntervalDeltaExprEvaluator
    {
        private readonly ExprTimePeriod timePeriod;
        private readonly TimeAbacus timeAbacus;

        public IntervalDeltaExprTimePeriodNonConstForge(
            ExprTimePeriod timePeriod,
            TimeAbacus timeAbacus)
        {
            this.timePeriod = timePeriod;
            this.timeAbacus = timeAbacus;
        }

        public IntervalDeltaExprEvaluator MakeEvaluator()
        {
            return this;
        }

        public long Evaluate(
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var sec = timePeriod.EvaluateAsSeconds(eventsPerStream, isNewData, context);
            return timeAbacus.DeltaForSecondsDouble(sec);
        }

        public CodegenExpression Codegen(
            CodegenExpression reference,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    typeof(long),
                    typeof(IntervalDeltaExprTimePeriodNonConstForge),
                    codegenClassScope)
                .AddParam<long>("reference");

            methodNode.Block
                .DeclareVar<double>(
                    "sec",
                    timePeriod.EvaluateAsSecondsCodegen(methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    timeAbacus.DeltaForSecondsDoubleCodegen(
                        Ref("sec"),
                        codegenClassScope));
            return LocalMethod(methodNode, reference);
        }
    }
} // end of namespace