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

namespace com.espertech.esper.epl.core
{
    public class BindProcessorEvaluatorStream : ExprEvaluator
    {
        private readonly int _streamNum;
        private readonly Type _returnType;
    
        public BindProcessorEvaluatorStream(int streamNum, Type returnType)
        {
            _streamNum = streamNum;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            EventBean theEvent = evaluateParams.EventsPerStream[_streamNum];
            if (theEvent != null) {
                return theEvent.Underlying;
            }
            return null;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}
