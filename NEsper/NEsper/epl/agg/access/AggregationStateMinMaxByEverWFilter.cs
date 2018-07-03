///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregationStateMinMaxByEverWFilter : AggregationStateMinMaxByEver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationStateMinMaxByEverWFilter"/> class.
        /// </summary>
        /// <param name="spec">The spec.</param>
        public AggregationStateMinMaxByEverWFilter(AggregationStateMinMaxByEverSpec spec) : base(spec)
        {
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[Spec.StreamId];
            if (theEvent == null)
            {
                return;
            }

            var pass = Spec.OptionalFilter.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            if (true.Equals(pass))
            {
                base.AddEvent(theEvent, eventsPerStream, exprEvaluatorContext);
            }
        }
    }
}
