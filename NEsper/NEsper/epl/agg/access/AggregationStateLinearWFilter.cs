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
    public class AggregationStateLinearWFilter : AggregationStateLinearImpl
    {
        private readonly ExprEvaluator _filter;

        public AggregationStateLinearWFilter(int streamId, ExprEvaluator filter)
            : base(streamId)
        {
            _filter = filter;
        }

        public override void Clear()
        {
            Events.Clear();
        }

        public override void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            var pass = _filter.Evaluate(new EvaluateParams(eventsPerStream, false, exprEvaluatorContext));
            if (true.Equals(pass)) Events.Remove(theEvent);
        }

        public override void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[StreamId];
            if (theEvent == null) return;
            var pass = _filter.Evaluate(new EvaluateParams(eventsPerStream, false, exprEvaluatorContext));
            if (true.Equals(pass)) Events.Add(theEvent);
        }
    }
} // end of namespace