///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    public class AggregationAgentRewriteStream : AggregationAgent
    {
        private readonly int _streamNum;

        public AggregationAgentRewriteStream(int streamNum)
        {
            _streamNum = streamNum;
        }

        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext,
            AggregationState aggregationState)
        {
            var rewrite = new[] {eventsPerStream[_streamNum]};
            aggregationState.ApplyEnter(rewrite, exprEvaluatorContext);
        }

        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext,
            AggregationState aggregationState)
        {
            var rewrite = new[] {eventsPerStream[_streamNum]};
            aggregationState.ApplyLeave(rewrite, exprEvaluatorContext);
        }
    }
}