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
    public class AggregationAgentRewriteStreamWFilter : AggregationAgent
    {
        private readonly int _streamNum;
        private readonly ExprEvaluator _filterEval;
    
        public AggregationAgentRewriteStreamWFilter(int streamNum, ExprEvaluator filterEval)
        {
            _streamNum = streamNum;
            _filterEval = filterEval;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var pass = _filterEval.Evaluate(evaluateParams);
            if (true.Equals(pass)) {
                var rewrite = new EventBean[]{eventsPerStream[_streamNum]};
                aggregationState.ApplyEnter(rewrite, exprEvaluatorContext);
            }
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            var pass = _filterEval.Evaluate(evaluateParams);
            if (true.Equals(pass)) {
                var rewrite = new EventBean[]{eventsPerStream[_streamNum]};
                aggregationState.ApplyLeave(rewrite, exprEvaluatorContext);
            }
        }
    }
} // end of namespace
