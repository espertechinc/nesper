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

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalByGetterFragmentAvro : ExprEvaluator
    {
        private readonly EventPropertyGetter _getter;
        private readonly Type _returnType;
        private readonly int _streamNum;

        public SelectExprProcessorEvalByGetterFragmentAvro(int streamNum, EventPropertyGetter getter, Type returnType)
        {
            _streamNum = streamNum;
            _getter = getter;
            _returnType = returnType;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var streamEvent = evaluateParams.EventsPerStream[_streamNum];
            if (streamEvent == null)
            {
                return null;
            }
            return _getter.Get(streamEvent);
        }
    }
} // end of namespace