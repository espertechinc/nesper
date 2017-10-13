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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>Factory for <seealso cref="TimerWithinGuard" /> instances.</summary>
    [Serializable]
    public class TimerWithinGuardFactory
        : GuardFactory
          ,
          MetaDefItem
    {
        /// <summary>Number of milliseconds.</summary>
        private ExprNode _timeExpr;

        /// <summary>For converting matched-events maps to events-per-stream.</summary>
        [NonSerialized] private MatchedEventConvertor _convertor;

        public void SetGuardParameters(IList<ExprNode> parameters, MatchedEventConvertor convertor)
        {
            const string errorMessage = "Timer-within guard requires a single numeric or time period parameter";
            if (parameters.Count != 1)
            {
                throw new GuardParameterException(errorMessage);
            }

            if (!TypeHelper.IsNumeric(parameters[0].ExprEvaluator.ReturnType))
            {
                throw new GuardParameterException(errorMessage);
            }

            _convertor = convertor;
            _timeExpr = parameters[0];
        }

        public long ComputeTime(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (_timeExpr is ExprTimePeriod)
            {
                var timePeriod = (ExprTimePeriod) _timeExpr;
                return timePeriod.NonconstEvaluator()
                    .DeltaUseEngineTime(_convertor.Convert(beginState), context.AgentInstanceContext);
            }
            else
            {
                var time = PatternExpressionUtil.Evaluate(
                    "Timer-within guard", beginState, _timeExpr, _convertor, context.AgentInstanceContext);
                if (time == null)
                {
                    throw new EPException("Timer-within guard expression returned a null-value");
                }
                return context.StatementContext.TimeAbacus.DeltaForSecondsNumber(time);
            }
        }

        public Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap matchedEventMap,
            Quitable quitable,
            EvalStateNodeNumber stateNodeId,
            Object guardState)
        {
            return new TimerWithinGuard(ComputeTime(matchedEventMap, context), quitable);
        }
    }
} // end of namespace
