///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectNoWildcardEmptyProps : SelectExprProcessor {
        private readonly SelectExprContext _selectExprContext;
        private readonly EventType _resultEventType;
    
        public EvalSelectNoWildcardEmptyProps(SelectExprContext selectExprContext, EventType resultEventType) {
            this._selectExprContext = selectExprContext;
            this._resultEventType = resultEventType;
        }
    
        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _selectExprContext.EventAdapterService.AdapterForTypedMap(new Dictionary<string, object>(), _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }
    }
}
