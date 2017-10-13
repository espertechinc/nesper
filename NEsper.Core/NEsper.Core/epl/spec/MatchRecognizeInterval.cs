///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// <summary>Interval specification within match_recognize.</summary>
    [Serializable]
    public class MatchRecognizeInterval : MetaDefItem
    {
        private ExprTimePeriod _timePeriodExpr;
        private readonly bool _orTerminated;
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="timePeriodExpr">time period</param>
        /// <param name="orTerminated">or-terminated indicator</param>
        public MatchRecognizeInterval(ExprTimePeriod timePeriodExpr, bool orTerminated)
        {
            _timePeriodExpr = timePeriodExpr;
            _orTerminated = orTerminated;
        }

        /// <summary>
        /// Returns the time period.
        /// </summary>
        /// <value>time period</value>
        public ExprTimePeriod TimePeriodExpr
        {
            get { return _timePeriodExpr; }
        }

        /// <summary>
        /// Returns the number of milliseconds.
        /// </summary>
        /// <param name="fromTime">from-time</param>
        /// <param name="agentInstanceContext">context</param>
        /// <returns>msec</returns>
        public long GetScheduleForwardDelta(long fromTime, AgentInstanceContext agentInstanceContext)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QRegIntervalValue(_timePeriodExpr);
            }
            if (_timeDeltaComputation == null) {
                _timeDeltaComputation = _timePeriodExpr.ConstEvaluator(new ExprEvaluatorContextStatement(agentInstanceContext.StatementContext, false));
            }
            long result = _timeDeltaComputation.DeltaAdd(fromTime);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().ARegIntervalValue(result);
            }
            return result;
        }
    
        /// <summary>
        /// Returns the number of milliseconds.
        /// </summary>
        /// <param name="fromTime">from-time</param>
        /// <param name="agentInstanceContext">context</param>
        /// <returns>msec</returns>
        public long GetScheduleBackwardDelta(long fromTime, AgentInstanceContext agentInstanceContext) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QRegIntervalValue(_timePeriodExpr);
            }
            if (_timeDeltaComputation == null) {
                _timeDeltaComputation = _timePeriodExpr.ConstEvaluator(new ExprEvaluatorContextStatement(agentInstanceContext.StatementContext, false));
            }
            long result = _timeDeltaComputation.DeltaSubtract(fromTime);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().ARegIntervalValue(result);
            }
            return result;
        }

        public bool IsOrTerminated
        {
            get { return _orTerminated; }
        }

        public void Validate(ExprValidationContext validationContext)
        {
            _timePeriodExpr = (ExprTimePeriod) ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.MATCHRECOGINTERVAL, _timePeriodExpr, validationContext);
        }
    }
} // end of namespace
