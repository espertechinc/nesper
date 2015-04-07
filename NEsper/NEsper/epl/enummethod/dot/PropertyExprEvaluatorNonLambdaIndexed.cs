///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.dot
{
    [Serializable]
    public class PropertyExprEvaluatorNonLambdaIndexed : ExprEvaluator
    {
        private readonly int _streamId;
        private readonly EventPropertyGetterIndexed _indexedGetter;
        private readonly ExprEvaluator _paramEval;
        private readonly Type _returnType;
    
        public PropertyExprEvaluatorNonLambdaIndexed(int streamId, EventPropertyGetterIndexed indexedGetter, ExprEvaluator paramEval, Type returnType)
        {
            _streamId = streamId;
            _indexedGetter = indexedGetter;
            _paramEval = paramEval;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var key = _paramEval.Evaluate(evaluateParams).AsInt();
            var eventInQuestion = evaluateParams.EventsPerStream[_streamId];
            if (eventInQuestion == null) {
                return null;
            }
            return _indexedGetter.Get(eventInQuestion, key);
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}
