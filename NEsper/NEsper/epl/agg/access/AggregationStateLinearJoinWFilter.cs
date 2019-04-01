///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>Implementation of access function for joins.</summary>
    [Serializable]
    public class AggregationStateLinearJoinWFilter : AggregationStateLinearJoinImpl
    {
        private readonly ExprEvaluator _filterEval;

        public AggregationStateLinearJoinWFilter(int streamId, ExprEvaluator filterEval)
            : base(streamId)
        {
            _filterEval = filterEval;
        }

        public override void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            var pass = _filterEval.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            if (true.Equals(pass)) AddEvent(theEvent);
        }

        public override void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            var pass = _filterEval.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            if (true.Equals(pass)) RemoveEvent(theEvent);
        }
    }
} // end of namespace