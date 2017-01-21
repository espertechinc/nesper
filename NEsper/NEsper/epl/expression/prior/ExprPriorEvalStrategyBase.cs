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

namespace com.espertech.esper.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event function in an expression node tree.
    /// </summary>
    public abstract class ExprPriorEvalStrategyBase : ExprPriorEvalStrategy
    {
        public abstract EventBean GetSubstituteEvent(EventBean originalEvent, bool isNewData, int constantIndexNumber);

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, int streamNumber, ExprEvaluator evaluator, int constantIndexNumber)
        {
            var originalEvent = eventsPerStream[streamNumber];
            var substituteEvent = GetSubstituteEvent(originalEvent, isNewData, constantIndexNumber);

            // Substitute original event with prior event, evaluate inner expression
            eventsPerStream[streamNumber] = substituteEvent;
            Object evalResult = evaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            eventsPerStream[streamNumber] = originalEvent;

            return evalResult;
        }
    }
}
