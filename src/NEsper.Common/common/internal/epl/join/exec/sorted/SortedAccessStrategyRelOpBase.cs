///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly EventBean[] _events;
        private readonly bool _isNwOnTrigger;
        private readonly ExprEvaluator _keyEval;
        private readonly int _lookupStream;

        protected SortedAccessStrategyRelOpBase(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator keyEval)
        {
            _lookupStream = lookupStream;
            _keyEval = keyEval;
            _isNwOnTrigger = isNWOnTrigger;
            if (lookupStream != -1) {
                _events = new EventBean[lookupStream + 1];
            }
            else {
                _events = new EventBean[numStreams + 1];
            }
        }

        public object EvaluateLookup(
            EventBean theEvent,
            ExprEvaluatorContext context)
        {
            _events[_lookupStream] = theEvent;
            return _keyEval.Evaluate(_events, true, context);
        }

        public object EvaluatePerStream(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (_isNwOnTrigger) {
                return _keyEval.Evaluate(eventsPerStream, true, context);
            }

            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return _keyEval.Evaluate(_events, true, context);
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() + " key " + _keyEval.GetType().GetSimpleName();
        }
    }
} // end of namespace