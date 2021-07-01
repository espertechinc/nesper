///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentDefaultWFilter : AggregationMultiFunctionAgent
    {
        private readonly ExprEvaluator filterEval;

        public AggregationAgentDefaultWFilter(ExprEvaluator filterEval)
        {
            this.filterEval = filterEval;
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            var pass = filterEval.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            if (pass != null && true.Equals(pass)) {
                row.EnterAccess(column, eventsPerStream, exprEvaluatorContext);
            }
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            var pass = filterEval.Evaluate(eventsPerStream, false, exprEvaluatorContext);
            if (pass != null && true.Equals(pass)) {
                row.LeaveAccess(column, eventsPerStream, exprEvaluatorContext);
            }
        }
    }
} // end of namespace