///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    public abstract class SortedAccessStrategyRangeBase
    {
        private readonly EventBean[] events;

        private readonly bool isNWOnTrigger;
        private readonly int lookupStream;
        protected ExprEvaluator end;
        protected bool includeEnd;
        protected bool includeStart;
        protected ExprEvaluator start;

        protected SortedAccessStrategyRangeBase(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator start,
            bool includeStart,
            ExprEvaluator end,
            bool includeEnd)
        {
            this.start = start;
            this.includeStart = includeStart;
            this.end = end;
            this.includeEnd = includeEnd;
            this.isNWOnTrigger = isNWOnTrigger;

            this.lookupStream = lookupStream;
            if (lookupStream != -1) {
                events = new EventBean[lookupStream + 1];
            }
            else {
                events = new EventBean[numStreams + 1];
            }
        }

        public object EvaluateLookupStart(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            events[lookupStream] = theEvent;
            return start.Evaluate(events, true, context);
        }

        public object EvaluateLookupEnd(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            events[lookupStream] = theEvent;
            return end.Evaluate(events, true, context);
        }

        public object EvaluatePerStreamStart(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return start.Evaluate(eventsPerStream, true, context);
            }

            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
            return start.Evaluate(events, true, context);
        }

        public object EvaluatePerStreamEnd(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return end.Evaluate(eventsPerStream, true, context);
            }

            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
            return end.Evaluate(events, true, context);
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " start=" +
                   start.GetType().Name +
                   ", includeStart=" +
                   includeStart +
                   ", end=" +
                   end.GetType().Name +
                   ", includeEnd=" +
                   includeEnd;
        }
    }
} // end of namespace