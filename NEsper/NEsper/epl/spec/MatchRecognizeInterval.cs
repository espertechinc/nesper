///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Interval specification within match_recognize.
    /// </summary>
    [Serializable]
    public class MatchRecognizeInterval : MetaDefItem
    {
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="timePeriodExpr">time period</param>
        /// <param name="orTerminated">if set to <c>true</c> [or terminated].</param>
        public MatchRecognizeInterval(ExprTimePeriod timePeriodExpr, bool orTerminated)
        {
            TimePeriodExpr = timePeriodExpr;
            IsOrTerminated = orTerminated;
        }

        /// <summary>Returns the time period. </summary>
        /// <value>time period</value>
        public ExprTimePeriod TimePeriodExpr { get; private set; }

        /// <summary>
        /// Returns the number of milliseconds.
        /// </summary>
        public long GetScheduleForwardDelta(long fromTime, AgentInstanceContext agentInstanceContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegIntervalValue(TimePeriodExpr); }
            if (_timeDeltaComputation == null)
            {
                _timeDeltaComputation = TimePeriodExpr.ConstEvaluator(new ExprEvaluatorContextStatement(agentInstanceContext.StatementContext, false));
            }
            long result = _timeDeltaComputation.DeltaMillisecondsAdd(fromTime);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalValue(result); }
            return result;
        }

        /// <summary>
        /// Returns the number of milliseconds.
        /// </summary>
        /// <returns></returns>
        public long GetScheduleBackwardDelta(long fromTime, AgentInstanceContext agentInstanceContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegIntervalValue(TimePeriodExpr); }
            if (_timeDeltaComputation == null)
            {
                _timeDeltaComputation = TimePeriodExpr.ConstEvaluator(new ExprEvaluatorContextStatement(agentInstanceContext.StatementContext, false));
            }
            long result = _timeDeltaComputation.DeltaMillisecondsSubtract(fromTime);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalValue(result); }
            return result;
        }

        public bool IsOrTerminated { get; private set; }

        public void Validate(ExprValidationContext validationContext)
        {
            TimePeriodExpr = (ExprTimePeriod)ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGINTERVAL, TimePeriodExpr, validationContext);
        }
    }
}
