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
    public class PropertyExprEvaluatorNonLambdaMapped : ExprEvaluator
    {
        private readonly int _streamId;
        private readonly EventPropertyGetterMapped _mappedGetter;
        private readonly ExprEvaluator _paramEval;
        private readonly Type _returnType;

        public PropertyExprEvaluatorNonLambdaMapped(
            int streamId,
            EventPropertyGetterMapped mappedGetter,
            ExprEvaluator paramEval,
            Type returnType)
        {
            _streamId = streamId;
            _mappedGetter = mappedGetter;
            _paramEval = paramEval;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var key = (String) _paramEval.Evaluate(evaluateParams);
            var eventInQuestion = evaluateParams.EventsPerStream[_streamId];
            return eventInQuestion == null ? null : _mappedGetter.Get(eventInQuestion, key);
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
}
