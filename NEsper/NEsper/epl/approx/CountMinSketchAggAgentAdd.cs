///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchAggAgentAdd : AggregationAgent
    {
        private readonly ExprEvaluator _stringEvaluator;
    
        public CountMinSketchAggAgentAdd(ExprEvaluator stringEvaluator) {
            _stringEvaluator = stringEvaluator;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState)
        {
            var value = _stringEvaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            var state = (CountMinSketchAggState) aggregationState;
            state.Add(value);
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, AggregationState aggregationState) {
    
        }
    }
}
