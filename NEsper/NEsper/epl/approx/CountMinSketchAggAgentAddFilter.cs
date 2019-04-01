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
    public class CountMinSketchAggAgentAddFilter : CountMinSketchAggAgentAdd
    {
        private readonly ExprEvaluator _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountMinSketchAggAgentAddFilter"/> class.
        /// </summary>
        /// <param name="stringEvaluator">The string evaluator.</param>
        /// <param name="filter">The filter.</param>
        public CountMinSketchAggAgentAddFilter(ExprEvaluator stringEvaluator, ExprEvaluator filter)
            : base(stringEvaluator)
        {
            _filter = filter;
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream, 
            ExprEvaluatorContext exprEvaluatorContext, 
            AggregationState aggregationState)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            var pass = _filter.Evaluate(evaluateParams);
            if (true.Equals(pass))
            {
                var value = base.StringEvaluator.Evaluate(evaluateParams);
                var state = (CountMinSketchAggState)aggregationState;
                state.Add(value);
            }
        }
    }
}
