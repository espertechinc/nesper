///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalSelectStreamBaseMap 
        : EvalSelectStreamBase
        , SelectExprProcessor
    {
        protected EvalSelectStreamBaseMap(SelectExprContext selectExprContext, EventType resultEventType, IList<SelectClauseStreamCompiledSpec> namedStreams, bool usingWildcard)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
        }

        public abstract EventBean ProcessSpecific(IDictionary<String, Object> props, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Evaluate all expressions and build a map of name-value pairs
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            int count = 0;
            foreach (ExprEvaluator expressionNode in SelectExprContext.ExpressionNodes)
            {
                Object evalResult = expressionNode.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                props.Put(SelectExprContext.ColumnNames[count], evalResult);
                count++;
            }
            foreach (SelectClauseStreamCompiledSpec element in NamedStreams)
            {
                EventBean theEvent = eventsPerStream[element.StreamNumber];
                if (element.TableMetadata != null)
                {
                    if (theEvent != null)
                    {
                        theEvent = element.TableMetadata.EventToPublic.Convert(theEvent, eventsPerStream, isNewData, exprEvaluatorContext);
                    }
                } 
                props.Put(SelectExprContext.ColumnNames[count], theEvent);
                count++;
            }
            if (IsUsingWildcard && eventsPerStream.Length > 1)
            {
                foreach (EventBean anEventsPerStream in eventsPerStream)
                {
                    props.Put(SelectExprContext.ColumnNames[count], anEventsPerStream);
                    count++;
                }
            }

            return ProcessSpecific(props, eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
}
