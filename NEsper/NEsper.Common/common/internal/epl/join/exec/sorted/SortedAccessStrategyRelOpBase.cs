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
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    public abstract class SortedAccessStrategyRelOpBase
    {
        private readonly EventBean[] events;
        private readonly bool isNWOnTrigger;
        private readonly ExprEvaluator keyEval;
        private readonly int lookupStream;

        protected SortedAccessStrategyRelOpBase(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator keyEval)
        {
            this.lookupStream = lookupStream;
            this.keyEval = keyEval;
            this.isNWOnTrigger = isNWOnTrigger;
            if (lookupStream != -1) {
                events = new EventBean[lookupStream + 1];
            }
            else {
                events = new EventBean[numStreams + 1];
            }
        }

        public object EvaluateLookup(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            events[lookupStream] = theEvent;
            return keyEval.Evaluate(events, true, context);
        }

        public object EvaluatePerStream(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return keyEval.Evaluate(eventsPerStream, true, context);
            }

            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
            return keyEval.Evaluate(events, true, context);
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() + " key " + keyEval.GetType().GetSimpleName();
        }
    }
} // end of namespace