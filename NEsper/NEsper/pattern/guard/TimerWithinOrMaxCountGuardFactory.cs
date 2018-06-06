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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.guard
{
    [Serializable]
    public class TimerWithinOrMaxCountGuardFactory
        : GuardFactory
        , MetaDefItem
    {
        /// <summary>For converting matched-events maps to events-per-stream.</summary>
        [NonSerialized] private MatchedEventConvertor _convertor;

        /// <summary>Number of count-to max.</summary>
        private ExprNode _numCountToExpr;

        /// <summary>Number of milliseconds.</summary>
        private ExprNode _timeExpr;

        public Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            Quitable quitable,
            EvalStateNodeNumber stateNodeId,
            Object guardState)
        {
            return new TimerWithinOrMaxCountGuard(
                ComputeTime(beginState, context), ComputeNumCountTo(beginState, context), quitable);
        }

        public void SetGuardParameters(IList<ExprNode> parameters, MatchedEventConvertor convertor)
        {
            const string message = "Timer-within-or-max-count guard requires two parameters: "
                                   + "numeric or time period parameter and an integer-value expression parameter";

            if (parameters.Count != 2)
            {
                throw new GuardParameterException(message);
            }

            if (!parameters[0].ExprEvaluator.ReturnType.IsNumeric())
            {
                throw new GuardParameterException(message);
            }

            if (parameters[1].ExprEvaluator.ReturnType.IsNotInt32())
            {
                throw new GuardParameterException(message);
            }

            _timeExpr = parameters[0];
            _numCountToExpr = parameters[1];
            _convertor = convertor;
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
                Object time = PatternExpressionUtil.Evaluate(
                    "Timer-Within-Or-Max-Count guard", beginState, _timeExpr, _convertor, context.AgentInstanceContext);
                if (null == time)
                {
                    throw new EPException("Timer-within-or-max first parameter evaluated to a null-value");
                }
                return context.StatementContext.TimeAbacus.DeltaForSecondsNumber(time);
            }
        }

        public int ComputeNumCountTo(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            Object numCountToVal = PatternExpressionUtil.Evaluate(
                "Timer-Within-Or-Max-Count guard", beginState, _numCountToExpr, _convertor, context.AgentInstanceContext);
            if (null == numCountToVal)
            {
                throw new EPException("Timer-within-or-max second parameter evaluated to a null-value");
            }
            return numCountToVal.AsInt();
        }
    }
} // end of namespace