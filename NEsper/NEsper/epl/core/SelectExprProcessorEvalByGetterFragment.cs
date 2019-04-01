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

namespace com.espertech.esper.epl.core
{
    public class SelectExprProcessorEvalByGetterFragment : ExprEvaluator
    {
        private readonly EventPropertyGetter _getter;
        private readonly Type _returnType;
        private readonly int _streamNum;

        public SelectExprProcessorEvalByGetterFragment(int streamNum, EventPropertyGetter getter, Type returnType)
        {
            _streamNum = streamNum;
            _getter = getter;
            _returnType = returnType;
        }

        public EventPropertyGetter Getter
        {
            get { return _getter; }
        }

        public int StreamNum
        {
            get { return _streamNum; }
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            EventBean streamEvent = evaluateParams.EventsPerStream[_streamNum];
            if (streamEvent == null)
            {
                return null;
            }
            return _getter.GetFragment(streamEvent);
        }
    }
} // end of namespace