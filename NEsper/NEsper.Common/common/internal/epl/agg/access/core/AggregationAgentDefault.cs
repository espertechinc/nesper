///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentDefault : AggregationMultiFunctionAgent,
        AggregationAgentForge
    {
        public static readonly AggregationAgentDefault INSTANCE = new AggregationAgentDefault();

        private AggregationAgentDefault()
        {
        }

        public ExprForge OptionalFilter => null;

        public CodegenExpression Make(CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(AggregationAgentDefault), "INSTANCE");
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column)
        {
            row.EnterAccess(column, eventsPerStream, exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationRow row, int column)
        {
            row.LeaveAccess(column, eventsPerStream, exprEvaluatorContext);
        }
    }
} // end of namespace