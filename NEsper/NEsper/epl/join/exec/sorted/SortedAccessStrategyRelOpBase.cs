///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.exec.sorted
{
    public abstract class SortedAccessStrategyRelOpBase
    {
        private readonly ExprEvaluator keyEval;
        private readonly EventBean[] events;
        private readonly int lookupStream;
        private readonly bool isNWOnTrigger;
    
        protected SortedAccessStrategyRelOpBase(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator keyEval)
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
    
        public Object EvaluateLookup(EventBean theEvent, ExprEvaluatorContext context) {
            events[lookupStream] = theEvent;
            return keyEval.Evaluate(new EvaluateParams(events, true, context));
        }
    
        public Object EvaluatePerStream(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            if (isNWOnTrigger) {
                return keyEval.Evaluate(new EvaluateParams(eventsPerStream, true, context));
            }
            else {
                Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
                return keyEval.Evaluate(new EvaluateParams(events, true, context));
            }
        }
    
        public String ToQueryPlan() {
            return this.GetType().FullName + " key " + keyEval.GetType().Name;
        }
    }
}
