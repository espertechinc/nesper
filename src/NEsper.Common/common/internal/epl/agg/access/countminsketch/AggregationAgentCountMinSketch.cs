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
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationAgentCountMinSketch : AggregationMultiFunctionAgent
    {
        private ExprEvaluator _optionalFilterEval;
        private ExprEvaluator _stringEval;

        public ExprEvaluator StringEval {
            get => _stringEval;
            set => _stringEval = value;
        }

        public ExprEvaluator OptionalFilterEval {
            get => _optionalFilterEval;
            set => _optionalFilterEval = value;
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
            if (_optionalFilterEval != null) {
                var pass = _optionalFilterEval.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (pass == null || false.Equals(pass)) {
                    return;
                }
            }

            var value = _stringEval.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            if (value == null) {
                return;
            }

            var state = (CountMinSketchAggState) row.GetAccessState(column);
            state.Add(value);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column)
        {
        }
    }
} // end of namespace