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

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalBaseFirstProp : SelectExprProcessor
    {
        private readonly SelectExprContext _selectExprContext;

        protected EvalBaseFirstProp(SelectExprContext selectExprContext, EventType resultEventType)
        {
            _selectExprContext = selectExprContext;
            ResultEventType = resultEventType;
        }

        public abstract EventBean ProcessFirstCol(Object result);

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            ExprEvaluator[] expressionNodes = _selectExprContext.ExpressionNodes;

            Object first = expressionNodes[0].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            return ProcessFirstCol(first);
        }

        public EventAdapterService EventAdapterService
        {
            get { return _selectExprContext.EventAdapterService; }
        }

        public EventType ResultEventType { get; private set; }
    }
}
