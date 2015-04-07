///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprNodeUtilExprEvalStreamNumEvent
        : ExprEvaluator
    {
        private readonly int _streamNum;

        public ExprNodeUtilExprEvalStreamNumEvent(int streamNum)
        {
            _streamNum = streamNum;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return evaluateParams.EventsPerStream[_streamNum];
        }

        public Type ReturnType
        {
            get { return typeof (EventBean); }
        }
    }
}