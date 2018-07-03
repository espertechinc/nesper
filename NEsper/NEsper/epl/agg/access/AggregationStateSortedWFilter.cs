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
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateSortedWFilter : AggregationStateSortedImpl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationStateSortedWFilter"/> class.
        /// </summary>
        /// <param name="spec">aggregation spec</param>
        public AggregationStateSortedWFilter(AggregationStateSortedSpec spec) : base(spec)
        {
        }

        public override void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null)
            {
                return;
            }
            var pass = Spec.OptionalFilter.Evaluate(new EvaluateParams(eventsPerStream, false, exprEvaluatorContext));
            if (true.Equals(pass))
            {
                base.ReferenceAdd(theEvent, eventsPerStream, exprEvaluatorContext);
            }
        }

        protected override bool ReferenceEvent(EventBean theEvent)
        {
            // no action
            return true;
        }

        protected override bool DereferenceEvent(EventBean theEvent)
        {
            // no action
            return true;
        }

        public override void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null)
            {
                return;
            }
            var pass = Spec.OptionalFilter.Evaluate(new EvaluateParams(eventsPerStream, false, exprEvaluatorContext));
            if (true.Equals(pass))
            {
                base.DereferenceRemove(theEvent, eventsPerStream, exprEvaluatorContext);
            }
        }
    }
}
