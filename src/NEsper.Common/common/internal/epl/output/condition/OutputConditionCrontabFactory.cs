///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition handling crontab-at schedule output.
    /// </summary>
    public class OutputConditionCrontabFactory : OutputConditionFactory
    {
        internal readonly bool isStartConditionOnCreation;
        internal readonly ExprEvaluator[] scheduleSpecEvaluators;

        public OutputConditionCrontabFactory(
            ExprEvaluator[] scheduleSpecExpressionList,
            bool isStartConditionOnCreation,
            int scheduleCallbackId)
        {
            scheduleSpecEvaluators = scheduleSpecExpressionList;
            this.isStartConditionOnCreation = isStartConditionOnCreation;
            ScheduleCallbackId = scheduleCallbackId;
        }

        public int ScheduleCallbackId { get; }

        public OutputCondition InstantiateOutputCondition(
            AgentInstanceContext agentInstanceContext,
            OutputCallback outputCallback)
        {
            var scheduleSpec = ScheduleExpressionUtil.CrontabScheduleBuild(
                scheduleSpecEvaluators,
                agentInstanceContext);
            return new OutputConditionCrontab(
                outputCallback,
                agentInstanceContext,
                isStartConditionOnCreation,
                scheduleSpec);
        }
    }
} // end of namespace