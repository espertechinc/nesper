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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalSelectStreamBaseObjectArray : EvalSelectStreamBase, SelectExprProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EvalSelectStreamBaseObjectArray(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
        }
    
        public abstract EventBean ProcessSpecific(Object[] props, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext);

        public override EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Evaluate all expressions and build a map of name-value pairs
            int size = (base.IsUsingWildcard && eventsPerStream.Length > 1) ? eventsPerStream.Length : 0;
            size += base.SelectExprContext.ExpressionNodes.Length + base.NamedStreams.Count;
            var props = new Object[size];
            int count = 0;
            foreach (ExprEvaluator expressionNode in base.SelectExprContext.ExpressionNodes)
            {
                var evalResult = expressionNode.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                props[count] = evalResult;
                count++;
            }
            foreach (SelectClauseStreamCompiledSpec element in base.NamedStreams)
            {
                EventBean theEvent = eventsPerStream[element.StreamNumber];
                props[count] = theEvent;
                count++;
            }
            if (base.IsUsingWildcard && eventsPerStream.Length > 1)
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
} // end of namespace
