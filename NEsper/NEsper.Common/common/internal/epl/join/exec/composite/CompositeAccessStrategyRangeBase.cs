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

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public abstract class CompositeAccessStrategyRangeBase
    {
        internal readonly ExprEvaluator end;

        private readonly EventBean[] events;
        internal readonly bool includeEnd;
        internal readonly bool includeStart;

        private readonly bool isNWOnTrigger;
        private readonly int lookupStream;
        internal readonly ExprEvaluator start;

        protected CompositeAccessStrategyRangeBase(
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

            if (lookupStream != -1) {
                events = new EventBean[lookupStream + 1];
            }
            else {
                events = new EventBean[numStreams + 1];
            }

            this.lookupStream = lookupStream;
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
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return start.Evaluate(eventPerStream, true, context);
            }

            Array.Copy(eventPerStream, 0, events, 1, eventPerStream.Length);
            return start.Evaluate(events, true, context);
        }

        public object EvaluatePerStreamEnd(
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return end.Evaluate(eventPerStream, true, context);
            }

            Array.Copy(eventPerStream, 0, events, 1, eventPerStream.Length);
            return end.Evaluate(events, true, context);
        }
    }
} // end of namespace