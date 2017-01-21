///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class EvalSelectStreamWUnderlyingRecastObjectArray : SelectExprProcessor
    {
        private readonly EventType _resultType;
        private readonly SelectExprContext _selectExprContext;
        private readonly int _underlyingStreamNumber;

        public EvalSelectStreamWUnderlyingRecastObjectArray(SelectExprContext selectExprContext,
                                                            int underlyingStreamNumber,
                                                            EventType resultType)
        {
            _selectExprContext = selectExprContext;
            _underlyingStreamNumber = underlyingStreamNumber;
            _resultType = resultType;
        }

        public EventType ResultEventType
        {
            get { return _resultType; }
        }

        public EventBean Process(EventBean[] eventsPerStream,
                                 bool isNewData,
                                 bool isSynthesize,
                                 ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = (ObjectArrayBackedEventBean) eventsPerStream[_underlyingStreamNumber];
            return _selectExprContext.EventAdapterService.AdapterForTypedObjectArray(theEvent.Properties, _resultType);
        }
    }
}