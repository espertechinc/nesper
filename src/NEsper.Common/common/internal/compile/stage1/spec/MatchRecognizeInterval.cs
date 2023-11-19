///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.time.node;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Interval specification within match_recognize.
    /// </summary>
    public class MatchRecognizeInterval
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="timePeriodExpr">time period</param>
        /// <param name="orTerminated">or-terminated indicator</param>
        public MatchRecognizeInterval(
            ExprTimePeriod timePeriodExpr,
            bool orTerminated)
        {
            TimePeriodExpr = timePeriodExpr;
            IsOrTerminated = orTerminated;
        }

        /// <summary>
        ///     Returns the time period.
        /// </summary>
        /// <returns>time period</returns>
        public ExprTimePeriod TimePeriodExpr { get; set; }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation { get; set; }

        public bool IsOrTerminated { get; set; }
    }
} // end of namespace