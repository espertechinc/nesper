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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    public class SelectExprProcessorEvalStreamInsertUnd : ExprEvaluator
    {
        private readonly Type _returnType;
        private readonly int _streamNum;
        private readonly ExprStreamUnderlyingNode _undNode;

        public SelectExprProcessorEvalStreamInsertUnd(ExprStreamUnderlyingNode undNode, int streamNum, Type returnType)
        {
            _undNode = undNode;
            _streamNum = streamNum;
            _returnType = returnType;
        }

        public int StreamNum
        {
            get { return _streamNum; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var eventsPerStream = evaluateParams.EventsPerStream;

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QExprStreamUndSelectClause(_undNode);
                var @event = eventsPerStream == null ? null : eventsPerStream[_streamNum];
                InstrumentationHelper.Get().AExprStreamUndSelectClause(@event);
                return @event;
            }

            return eventsPerStream == null ? null : eventsPerStream[_streamNum];
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }
    }
} // end of namespace