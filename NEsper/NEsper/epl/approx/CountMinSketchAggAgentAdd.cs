///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchAggAgentAdd : AggregationAgent
    {
        protected readonly ExprEvaluator StringEvaluator;
    
        public CountMinSketchAggAgentAdd(ExprEvaluator stringEvaluator) {
            StringEvaluator = stringEvaluator;
        }
    
        public virtual void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState)
        {
            var value = StringEvaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            var state = (CountMinSketchAggState) aggregationState;
            state.Add(value);
        }
    
        public virtual void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
        }
    }
}
