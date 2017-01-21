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

namespace com.espertech.esper.epl.datetime.interval
{
    public class ExprEvaluatorStreamLongPropFragment : ExprEvaluator
    {
        private readonly EventPropertyGetter _getterFragment;
        private readonly EventPropertyGetter _getterTimestamp;
        private readonly int _streamId;

        public ExprEvaluatorStreamLongPropFragment(
            int streamId,
            EventPropertyGetter getterFragment,
            EventPropertyGetter getterTimestamp)
        {
            _streamId = streamId;
            _getterFragment = getterFragment;
            _getterTimestamp = getterTimestamp;
        }

        public Type ReturnType
        {
            get { return typeof (long); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var theEvent = evaluateParams.EventsPerStream[_streamId];
            if (theEvent == null)
            {
                return null;
            }

            var @event = _getterFragment.GetFragment(theEvent);
            if (!(@event is EventBean))
            {
                return null;
            }
            return _getterTimestamp.Get((EventBean) @event);
        }
    }
}