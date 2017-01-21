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

namespace com.espertech.esper.epl.join.exec.composite
{
    public abstract class CompositeAccessStrategyRangeBase
    {
        protected readonly ExprEvaluator Start;
        protected readonly bool IncludeStart;
    
        protected readonly ExprEvaluator End;
        protected readonly bool IncludeEnd;
    
        private readonly EventBean[] _events;
        private readonly int _lookupStream;
    
        protected readonly Type CoercionType;
        private readonly bool _isNwOnTrigger;
    
        protected CompositeAccessStrategyRangeBase(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator start, bool includeStart, ExprEvaluator end, bool includeEnd, Type coercionType)
        {
            Start = start;
            IncludeStart = includeStart;
            End = end;
            IncludeEnd = includeEnd;
            CoercionType = coercionType;
            _isNwOnTrigger = isNWOnTrigger;
    
            if (lookupStream != -1) {
                _events = new EventBean[lookupStream + 1];
            }
            else {
                _events = new EventBean[numStreams + 1];
            }
            _lookupStream = lookupStream;
        }
    
        public Object EvaluateLookupStart(EventBean theEvent, ExprEvaluatorContext context) {
            _events[_lookupStream] = theEvent;
            return Start.Evaluate(new EvaluateParams(_events, true, context));
        }
    
        public Object EvaluateLookupEnd(EventBean theEvent, ExprEvaluatorContext context) {
            _events[_lookupStream] = theEvent;
            return End.Evaluate(new EvaluateParams(_events, true, context));
        }
    
        public Object EvaluatePerStreamStart(EventBean[] eventPerStream, ExprEvaluatorContext context) {
            if (_isNwOnTrigger) {
                return Start.Evaluate(new EvaluateParams(eventPerStream, true, context));
            }
            else {
                Array.Copy(eventPerStream, 0, _events, 1, eventPerStream.Length);
                return Start.Evaluate(new EvaluateParams(_events, true, context));
            }
        }
    
        public Object EvaluatePerStreamEnd(EventBean[] eventPerStream, ExprEvaluatorContext context) {
            if (_isNwOnTrigger) {
                return End.Evaluate(new EvaluateParams(eventPerStream, true, context));
            }
            else {
                Array.Copy(eventPerStream, 0, _events, 1, eventPerStream.Length);
                return End.Evaluate(new EvaluateParams(_events, true, context));
            }
        }
    }
}
