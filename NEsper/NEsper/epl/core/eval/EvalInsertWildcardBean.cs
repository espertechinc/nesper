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

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertWildcardBean 
        : EvalBase
        , SelectExprProcessor
    {
        public EvalInsertWildcardBean(SelectExprContext selectExprContext, EventType resultEventType)
                    : base(selectExprContext, resultEventType)
        {
        }

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[0];
            return EventAdapterService.AdapterForTypedObject(theEvent.Underlying, ResultEventType);
        }
    }
}
