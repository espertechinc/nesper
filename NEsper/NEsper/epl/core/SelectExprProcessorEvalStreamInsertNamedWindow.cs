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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    public class SelectExprProcessorEvalStreamInsertNamedWindow : ExprEvaluator
    {
        private readonly EventType _namedWindowAsType;
        private readonly EventAdapterService _eventAdapterService;

        public SelectExprProcessorEvalStreamInsertNamedWindow(
            int streamNum,
            EventType namedWindowAsType,
            Type returnType,
            EventAdapterService eventAdapterService)
        {
            StreamNum = streamNum;
            _namedWindowAsType = namedWindowAsType;
            ReturnType = returnType;
            _eventAdapterService = eventAdapterService;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[StreamNum];
            return @event == null ? null : _eventAdapterService.AdapterForType(@event.Underlying, _namedWindowAsType);
        }

        public Type ReturnType { get; private set; }

        public int StreamNum { get; private set; }
    }
} // end of namespace
