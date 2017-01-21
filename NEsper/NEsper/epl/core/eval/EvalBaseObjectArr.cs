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

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalBaseObjectArr : EvalBase, SelectExprProcessor
    {
        protected EvalBaseObjectArr(SelectExprContext selectExprContext, EventType resultEventType)
            : base(selectExprContext, resultEventType)
        {
        }

        public abstract EventBean ProcessSpecific(Object[] props, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext);

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            ExprEvaluator[] expressionNodes = SelectExprContext.ExpressionNodes;

            Object[] result = new Object[expressionNodes.Length];
            for (int i = 0; i < expressionNodes.Length; i++)
            {
                result[i] = expressionNodes[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            }

            return ProcessSpecific(result, eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
        }
    }
}