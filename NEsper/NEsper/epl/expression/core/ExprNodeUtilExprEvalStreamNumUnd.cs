///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprNodeUtilExprEvalStreamNumUnd : ExprEvaluator
    {
        private readonly int _streamNum;
        private readonly Type _returnType;

        public ExprNodeUtilExprEvalStreamNumUnd(int streamNum, Type returnType)
        {
            _streamNum = streamNum;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return evaluateParams.EventsPerStream[_streamNum].Underlying;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}
