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
    public abstract class EvalBase
    {
        private readonly EventType _resultEventType;
        private readonly SelectExprContext _selectExprContext;

        protected EvalBase(SelectExprContext selectExprContext,
                           EventType resultEventType)
        {
            _selectExprContext = selectExprContext;
            _resultEventType = resultEventType;
        }

        public EventAdapterService EventAdapterService
        {
            get { return _selectExprContext.EventAdapterService; }
        }

        public ExprEvaluator[] ExprNodes
        {
            get { return _selectExprContext.ExpressionNodes; }
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }

        public SelectExprContext SelectExprContext
        {
            get { return _selectExprContext; }
        }
    }
}