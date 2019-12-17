///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportReferenceCountedMapAgent : AggregationMultiFunctionAgent
    {
        private readonly ExprEvaluator evaluator;

        public SupportReferenceCountedMapAgent(ExprEvaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            Apply(true, eventsPerStream, exprEvaluatorContext, row, column);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            Apply(false, eventsPerStream, exprEvaluatorContext, row, column);
        }

        private void Apply(
            bool enter,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            var state = (SupportReferenceCountedMapState) row.GetAccessState(column);
            var value = evaluator.Evaluate(eventsPerStream, enter, exprEvaluatorContext);
            if (enter) {
                state.Enter(value);
            }
            else {
                state.Leave(value);
            }
        }
    }
} // end of namespace