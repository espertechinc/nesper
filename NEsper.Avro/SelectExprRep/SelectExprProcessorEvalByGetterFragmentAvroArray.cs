///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalByGetterFragmentAvroArray : ExprEvaluator
    {
        private readonly EventPropertyGetter _getter;
        private readonly Type _returnType;
        private readonly int _streamNum;

        public SelectExprProcessorEvalByGetterFragmentAvroArray(
            int streamNum,
            EventPropertyGetter getter,
            Type returnType)
        {
            _streamNum = streamNum;
            _getter = getter;
            _returnType = returnType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            EventBean streamEvent = evaluateParams.EventsPerStream[_streamNum];
            if (streamEvent == null)
            {
                return null;
            }
            Object result = _getter.Get(streamEvent);
            if (result is Array)
            {
                return Collections.List((Object[]) result);
            }
            return null;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
} // end of namespace