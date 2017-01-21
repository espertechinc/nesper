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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalBaseMap : EvalBase, SelectExprProcessor
    {
        protected EvalBaseMap(SelectExprContext selectExprContext, EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        public abstract EventBean ProcessSpecific(IDictionary<String, Object> props, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext);

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            ExprEvaluator[] expressionNodes = SelectExprContext.ExpressionNodes;
            String[] columnNames = SelectExprContext.ColumnNames;

            // Evaluate all expressions and build a map of name-value pairs
            IDictionary<String, Object> props;
            if (expressionNodes.Length == 0)
            {
                props = Collections.EmptyDataMap;
            }
            else
            {
                props = new Dictionary<String, Object>();
                for (int i = 0; i < expressionNodes.Length; i++)
                {
                    Object evalResult = expressionNodes[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    props.Put(columnNames[i], evalResult);
                }
            }

            return ProcessSpecific(props, eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
        }
    }
}
