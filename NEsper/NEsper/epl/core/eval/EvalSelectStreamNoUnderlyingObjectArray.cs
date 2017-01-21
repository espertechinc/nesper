///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamNoUnderlyingObjectArray 
        : EvalSelectStreamBase
        , SelectExprProcessor
    {
        public EvalSelectStreamNoUnderlyingObjectArray(SelectExprContext selectExprContext, EventType resultEventType, IList<SelectClauseStreamCompiledSpec> namedStreams, bool usingWildcard)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
        }
    
        public EventBean ProcessSpecific(Object[] props, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            return SelectExprContext.EventAdapterService.AdapterForTypedObjectArray(props, base.ResultEventType);
        }
    
        public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Evaluate all expressions and build a map of name-value pairs
            int size = (IsUsingWildcard && eventsPerStream.Length > 1) ? eventsPerStream.Length : 0;
            size += SelectExprContext.ExpressionNodes.Length + NamedStreams.Count;
            Object[] props = new Object[size];
            int count = 0;
            foreach (ExprEvaluator expressionNode in SelectExprContext.ExpressionNodes)
            {
                Object evalResult = expressionNode.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                props[count] = evalResult;
                count++;
            }
            foreach (SelectClauseStreamCompiledSpec element in NamedStreams)
            {
                EventBean theEvent = eventsPerStream[element.StreamNumber];
                props[count] = theEvent;
                count++;
            }
            if (IsUsingWildcard && eventsPerStream.Length > 1)
            {
                foreach (EventBean anEventsPerStream in eventsPerStream)
                {
                    props[count] = anEventsPerStream;
                    count++;
                }
            }
    
            return ProcessSpecific(props, eventsPerStream, exprEvaluatorContext);
        }
    }
}
