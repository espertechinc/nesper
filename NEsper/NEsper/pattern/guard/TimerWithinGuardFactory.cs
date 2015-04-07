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
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    /// Factory for <seealso cref="TimerWithinGuard" /> instances.
    /// </summary>
    [Serializable]
    public class TimerWithinGuardFactory : GuardFactory, MetaDefItem
    {
        /// <summary>Number of milliseconds. </summary>
        protected ExprNode MillisecondsExpr;
    
        /// <summary>For converting matched-events maps to events-per-stream. </summary>
        [NonSerialized] protected MatchedEventConvertor Convertor;
    
        public void SetGuardParameters(IList<ExprNode> parameters, MatchedEventConvertor convertor)
        {
            String errorMessage = "Timer-within guard requires a single numeric or time period parameter";
            if (parameters.Count != 1)
            {
                throw new GuardParameterException(errorMessage);
            }
    
            if (!parameters[0].ExprEvaluator.ReturnType.IsNumeric())
            {
                throw new GuardParameterException(errorMessage);
            }
    
            Convertor = convertor;
            MillisecondsExpr = parameters[0];
        }

        protected long ComputeMilliseconds(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (MillisecondsExpr is ExprTimePeriod)
            {
                ExprTimePeriod timePeriod = (ExprTimePeriod) MillisecondsExpr;
                return timePeriod.NonconstEvaluator()
                    .DeltaMillisecondsUseEngineTime(Convertor.Convert(beginState), context.AgentInstanceContext);
            }
            else
            {
                Object millisecondVal = PatternExpressionUtil.Evaluate(
                    "Timer-within guard", beginState, MillisecondsExpr, Convertor, context.AgentInstanceContext);

                if (millisecondVal == null)
                {
                    throw new EPException("Timer-within guard expression returned a null-value");
                }

                return (long) Math.Round(1000d*millisecondVal.AsDouble());
            }
        }

        public Guard MakeGuard(PatternAgentInstanceContext context, MatchedEventMap matchedEventMap, Quitable quitable, EvalStateNodeNumber stateNodeId, Object guardState)
        {
            return new TimerWithinGuard(ComputeMilliseconds(matchedEventMap, context), quitable);
        }
    }
}
