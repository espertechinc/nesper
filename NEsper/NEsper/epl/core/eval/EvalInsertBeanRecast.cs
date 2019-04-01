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
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertBeanRecast : SelectExprProcessor
    {

        private readonly EventType _eventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly int _streamNumber;

        public EvalInsertBeanRecast(EventType targetType, EventAdapterService eventAdapterService, int streamNumber, EventType[] typesPerStream)
        {
            _eventType = targetType;
            _eventAdapterService = eventAdapterService;
            _streamNumber = streamNumber;

            EventType sourceType = typesPerStream[streamNumber];
            Type sourceClass = sourceType.UnderlyingType;
            Type targetClass = targetType.UnderlyingType;
            if (!TypeHelper.IsSubclassOrImplementsInterface(sourceClass, targetClass))
            {
                throw EvalInsertUtil.MakeEventTypeCastException(sourceType, targetType);
            }
        }

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[_streamNumber];
            return _eventAdapterService.AdapterForTypedObject(theEvent.Underlying, _eventType);
        }

        public EventType ResultEventType
        {
            get { return _eventType; }
        }
    }
}