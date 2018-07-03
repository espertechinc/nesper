///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    /// <seealso cref="com.espertech.esper.epl.agg.access.AggregationStateSortedImpl" />
    public class AggregationStateSortedJoinWFilter : AggregationStateSortedJoin
    {
        public AggregationStateSortedJoinWFilter(AggregationStateSortedSpec spec) : base(spec)
        {
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }

            var pass = Spec.OptionalFilter.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            if (true.Equals(pass)) {
                ReferenceAdd(theEvent, eventsPerStream, exprEvaluatorContext);
            }
        }

        public override void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null) {
                return;
            }

            var pass = Spec.OptionalFilter.Evaluate(new EvaluateParams(eventsPerStream, false, exprEvaluatorContext));
            if (true.Equals(pass)) {
                DereferenceRemove(theEvent, eventsPerStream, exprEvaluatorContext);
            }
        }
    }
}
