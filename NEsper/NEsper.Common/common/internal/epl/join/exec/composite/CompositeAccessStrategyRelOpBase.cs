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
    public abstract class CompositeAccessStrategyRelOpBase
    {
        private readonly EventBean[] events;
        private readonly bool isNWOnTrigger;
        private readonly int lookupStream;

        internal Type coercionType;
        internal ExprEvaluator key;

        internal CompositeAccessStrategyRelOpBase(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator key)
        {
            this.key = key;

            if (lookupStream != -1) {
                events = new EventBean[lookupStream + 1];
            }
            else {
                events = new EventBean[numStreams + 1];
            }

            this.lookupStream = lookupStream;
            this.isNWOnTrigger = isNWOnTrigger;
        }

        public object EvaluateLookup(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            events[lookupStream] = theEvent;
            return key.Evaluate(events, true, context);
        }

        public object EvaluatePerStream(
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (isNWOnTrigger) {
                return key.Evaluate(eventPerStream, true, context);
            }

            Array.Copy(eventPerStream, 0, events, 1, eventPerStream.Length);
            return key.Evaluate(events, true, context);
        }
    }
} // end of namespace