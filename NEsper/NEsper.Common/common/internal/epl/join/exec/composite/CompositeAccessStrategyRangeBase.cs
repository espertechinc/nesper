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

        private readonly EventBean[] _events;
        internal readonly bool includeEnd;
        internal readonly bool includeStart;

        private readonly bool _isNwOnTrigger;
        private readonly int _lookupStream;
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
            this._isNwOnTrigger = isNWOnTrigger;

            if (lookupStream != -1) {
                _events = new EventBean[lookupStream + 1];
            }
            else {
                _events = new EventBean[numStreams + 1];
            }

            this._lookupStream = lookupStream;
        }

        public object EvaluateLookupStart(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            _events[_lookupStream] = theEvent;
            return start.Evaluate(_events, true, context);
        }

        public object EvaluateLookupEnd(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            _events[_lookupStream] = theEvent;
            return end.Evaluate(_events, true, context);
        }

        public object EvaluatePerStreamStart(
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (_isNwOnTrigger) {
                return start.Evaluate(eventPerStream, true, context);
            }

            Array.Copy(eventPerStream, 0, _events, 1, eventPerStream.Length);
            return start.Evaluate(_events, true, context);
        }

        public object EvaluatePerStreamEnd(
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (_isNwOnTrigger) {
                return end.Evaluate(eventPerStream, true, context);
            }

            Array.Copy(eventPerStream, 0, _events, 1, eventPerStream.Length);
            return end.Evaluate(_events, true, context);
        }
    }
} // end of namespace