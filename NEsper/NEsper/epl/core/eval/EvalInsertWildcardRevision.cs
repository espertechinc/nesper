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
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalInsertWildcardRevision 
        : EvalBase
        , SelectExprProcessor
    {
        private readonly ValueAddEventProcessor _vaeProcessor;

        public EvalInsertWildcardRevision(SelectExprContext selectExprContext, EventType resultEventType, ValueAddEventProcessor vaeProcessor)
            : base(selectExprContext, resultEventType)
        {
            _vaeProcessor = vaeProcessor;
        }

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[0];
            return _vaeProcessor.GetValueAddEventBean(theEvent);
        }
    }
}