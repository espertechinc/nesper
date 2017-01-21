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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern.guard
{
    [Serializable]
    public class TimerWithinOrMaxCountGuardFactory
        : GuardFactory
        , MetaDefItem
    {
        /// <summary>For converting matched-events maps to events-per-stream. </summary>
        [NonSerialized] private MatchedEventConvertor _convertor;

        /// <summary>Number of milliseconds. </summary>
        private ExprNode _millisecondsExpr;

        /// <summary>Number of count-to max. </summary>
        private ExprNode _numCountToExpr;

        #region GuardFactory Members

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

            if (parameters[1].ExprEvaluator.ReturnType.GetBoxedType() != typeof (int?))
            {
                throw new GuardParameterException(message);
            }

            _millisecondsExpr = parameters[0];
            _numCountToExpr = parameters[1];
            _convertor = convertor;
        }

        public Guard MakeGuard(PatternAgentInstanceContext context,
                               MatchedEventMap beginState,
                               Quitable quitable,
                               EvalStateNodeNumber stateNodeId,
                               Object guardState)
        {
            return new TimerWithinOrMaxCountGuard(
                ComputeMilliseconds(beginState, context), ComputeNumCountTo(beginState, context), quitable);
        }

        #endregion

        public long ComputeMilliseconds(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            if (_millisecondsExpr is ExprTimePeriod)
            {
                var timePeriod = (ExprTimePeriod)_millisecondsExpr;
                return timePeriod.NonconstEvaluator().DeltaMillisecondsUseEngineTime(
                    _convertor.Convert(beginState), context.AgentInstanceContext);
            }
            else
            {
                var millisecondVal = PatternExpressionUtil.Evaluate(
                    "Timer-Within-Or-Max-Count guard", beginState, _millisecondsExpr, _convertor, context.AgentInstanceContext);
                if (null == millisecondVal) {
                    throw new EPException("Timer-within-or-max first parameter evaluated to a null-value");
                }
                return (long) Math.Round(1000d * millisecondVal.AsDouble());
            }
        }

        public int ComputeNumCountTo(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            object numCountToVal = PatternExpressionUtil.Evaluate(
                "Timer-Within-Or-Max-Count guard", beginState, _numCountToExpr, _convertor, context.AgentInstanceContext);
            if (null == numCountToVal)
            {
                throw new EPException("Timer-within-or-max second parameter evaluated to a null-value");
            }
            return numCountToVal.AsInt();
        }
    }
}