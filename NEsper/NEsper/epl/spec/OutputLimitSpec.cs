///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>Spec for defining an output rate </summary>
    [Serializable]
    public class OutputLimitSpec : MetaDefItem
    {
        /// <summary>
        /// Ctor. For batching events by event count.
        /// </summary>
        /// <param name="rate">is the fixed output rate, or null if by variable</param>
        /// <param name="variableForRate">an optional variable name instead of the rate</param>
        /// <param name="rateType">type of the rate</param>
        /// <param name="displayLimit">indicates whether to output only the first, only the last, or all events</param>
        /// <param name="whenExpressionNode">for controlling output by a bool expression</param>
        /// <param name="thenExpressions">variable assignments, if null if none</param>
        /// <param name="crontabAtSchedule">crontab parameters</param>
        /// <param name="timePeriodExpr">the time period, or null if none</param>
        /// <param name="afterTimePeriodExpr">after-keyword time period</param>
        /// <param name="afterNumberOfEvents">after-keyword number of events</param>
        /// <param name="isAndAfterTerminate">if set to <c>true</c> [and after terminate].</param>
        /// <param name="andAfterTerminateExpr">The and after terminate expr.</param>
        /// <param name="andAfterTerminateSetExpressions">The and after terminate set expressions.</param>
        public OutputLimitSpec(double? rate,
                               String variableForRate,
                               OutputLimitRateType rateType,
                               OutputLimitLimitType displayLimit,
                               ExprNode whenExpressionNode,
                               IList<OnTriggerSetAssignment> thenExpressions,
                               IList<ExprNode> crontabAtSchedule,
                               ExprTimePeriod timePeriodExpr,
                               ExprTimePeriod afterTimePeriodExpr,
                               int? afterNumberOfEvents,
                               bool isAndAfterTerminate,
                               ExprNode andAfterTerminateExpr,
                               IList<OnTriggerSetAssignment> andAfterTerminateSetExpressions)
    	{
    		Rate = rate;
    		DisplayLimit = displayLimit;
            VariableName = variableForRate;
            RateType = rateType;
            CrontabAtSchedule = crontabAtSchedule;
            WhenExpressionNode = whenExpressionNode;
            ThenExpressions = thenExpressions;
            TimePeriodExpr = timePeriodExpr;
            AfterTimePeriodExpr = afterTimePeriodExpr;
            AfterNumberOfEvents = afterNumberOfEvents;
            IsAndAfterTerminate = isAndAfterTerminate;
            AndAfterTerminateExpr = andAfterTerminateExpr;
            AndAfterTerminateThenExpressions = andAfterTerminateSetExpressions;
        }

        public OutputLimitSpec(OutputLimitLimitType displayLimit, OutputLimitRateType rateType)
            : this(null, null, rateType, displayLimit, null, null, null, null, null, null, false, null, null)
        {
        }

        public OutputLimitLimitType DisplayLimit { get; private set; }

        public OutputLimitRateType RateType { get; private set; }

        public double? Rate { get; private set; }

        public string VariableName { get; private set; }

        public ExprNode WhenExpressionNode { get; set; }

        public IList<OnTriggerSetAssignment> ThenExpressions { get; private set; }

        public IList<ExprNode> CrontabAtSchedule { get; private set; }

        public ExprTimePeriod TimePeriodExpr { get; set; }

        public ExprTimePeriod AfterTimePeriodExpr { get; set; }

        public int? AfterNumberOfEvents { get; private set; }

        public bool IsAndAfterTerminate { get; private set; }

        public ExprNode AndAfterTerminateExpr { get; set; }

        public IList<OnTriggerSetAssignment> AndAfterTerminateThenExpressions { get; set; }
    }
}
