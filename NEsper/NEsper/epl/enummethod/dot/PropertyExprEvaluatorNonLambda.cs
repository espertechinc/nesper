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
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.dot
{
    [Serializable]
    public class PropertyExprEvaluatorNonLambda : ExprEvaluator
    {
        private readonly int _streamId;
        private readonly EventPropertyGetter _getter;
        private readonly Type _returnType;
    
        public PropertyExprEvaluatorNonLambda(int streamId, EventPropertyGetter getter, Type returnType)
        {
            _streamId = streamId;
            _getter = getter;
            _returnType = returnType;
        }
    
        public object Evaluate(EvaluateParams evaluateParams)
        {
            var eventInQuestion = evaluateParams.EventsPerStream[_streamId];
            if (eventInQuestion == null) {
                return null;
            }
            return _getter.Get(eventInQuestion);
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}
