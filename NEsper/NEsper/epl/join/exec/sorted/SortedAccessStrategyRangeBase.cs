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
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.exec.sorted
{
    public abstract class SortedAccessStrategyRangeBase
    {
        protected ExprEvaluator Start;
        protected bool IncludeStart;
        protected ExprEvaluator End;
        protected bool IncludeEnd;
    
        private readonly bool _isNWOnTrigger;
        private readonly EventBean[] _events;
        private readonly int _lookupStream;
    
        protected SortedAccessStrategyRangeBase(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator start, bool includeStart, ExprEvaluator end, bool includeEnd)
        {
            Start = start;
            IncludeStart = includeStart;
            End = end;
            IncludeEnd = includeEnd;
            _isNWOnTrigger = isNWOnTrigger;
    
            _lookupStream = lookupStream;
            if (lookupStream != -1) {
                _events = new EventBean[lookupStream + 1];
            }
            else {
                _events = new EventBean[numStreams + 1];
            }
        }
    
        public Object EvaluateLookupStart(EventBean theEvent, ExprEvaluatorContext context) {
            _events[_lookupStream] = theEvent;
            return Start.Evaluate(new EvaluateParams(_events, true, context));
        }
    
        public Object EvaluateLookupEnd(EventBean theEvent, ExprEvaluatorContext context) {
            _events[_lookupStream] = theEvent;
            return End.Evaluate(new EvaluateParams(_events, true, context));
        }
    
        public Object EvaluatePerStreamStart(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            if (_isNWOnTrigger) {
                return Start.Evaluate(new EvaluateParams(eventsPerStream, true, context));
            }
            else {
                Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
                return Start.Evaluate(new EvaluateParams(_events, true, context));
            }
        }
    
        public Object EvaluatePerStreamEnd(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            if (_isNWOnTrigger) {
                return End.Evaluate(new EvaluateParams(eventsPerStream, true, context));
            }
            else {
                Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
                return End.Evaluate(new EvaluateParams(_events, true, context));
            }
        }
    
        public String ToQueryPlan() {
            return GetType().FullName + " start=" + Start.GetType().Name +
                    ", includeStart=" + IncludeStart +
                    ", end=" + End.GetType().Name +
                    ", includeEnd=" + IncludeEnd;
        }
    }
}
