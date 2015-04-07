///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertCoercionMap : SelectExprProcessor
    {
        private readonly EventType _resultEventType;
        private readonly EventAdapterService _eventAdapterService;
    
        public EvalInsertCoercionMap(EventType resultEventType, EventAdapterService eventAdapterService) {
            _resultEventType = resultEventType;
            _eventAdapterService = eventAdapterService;
        }
    
        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext) {
            var theEvent = (MappedEventBean) eventsPerStream[0];
            return _eventAdapterService.AdapterForTypedMap(theEvent.Properties, _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }
    }
}
