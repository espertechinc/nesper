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
    [Serializable]
    public class ExprEvaluatorStreamLongProp : ExprEvaluator
    {
        private readonly int _streamId;
        private readonly EventPropertyGetter _getter;

        public ExprEvaluatorStreamLongProp(int streamId, EventPropertyGetter getter)
        {
            _streamId = streamId;
            _getter = getter;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var theEvent = evaluateParams.EventsPerStream[_streamId];
            if (theEvent == null)
            {
                return null;
            }
            return _getter.Get(theEvent);
        }

        public Type ReturnType
        {
            get { return typeof (long); }
        }
    }
}
