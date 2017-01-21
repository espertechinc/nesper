///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<string, object>;

    public abstract class CompositeAccessStrategyRelOpBase
    {
        protected ExprEvaluator Key;
        protected Type CoercionType;

        private readonly EventBean[] _events;
        private readonly int _lookupStream;
        private readonly bool _isNwOnTrigger;

        protected CompositeAccessStrategyRelOpBase(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator key, Type coercionType)
        {
            Key = key;
            CoercionType = coercionType;

            if (lookupStream != -1)
            {
                _events = new EventBean[lookupStream + 1];
            }
            else
            {
                _events = new EventBean[numStreams + 1];
            }
            
            _lookupStream = lookupStream;
            _isNwOnTrigger = isNWOnTrigger;
        }

        public Object EvaluateLookup(EventBean theEvent, ExprEvaluatorContext context)
        {
            _events[_lookupStream] = theEvent;
            return Key.Evaluate(new EvaluateParams(_events, true, context));
        }

        public Object EvaluatePerStream(EventBean[] eventPerStream, ExprEvaluatorContext context)
        {
            if (_isNwOnTrigger)
            {
                return Key.Evaluate(new EvaluateParams(eventPerStream, true, context));
            }
            else
            {
                Array.Copy(eventPerStream, 0, _events, 1, eventPerStream.Length);
                return Key.Evaluate(new EvaluateParams(_events, true, context));
            }
        }
    }
}
