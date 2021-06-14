///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition that is satisfied at the end
    ///     of every time interval of a given length.
    /// </summary>
    public class OutputConditionTimeFactory : OutputConditionFactory
    {
        public OutputConditionTimeFactory(
            bool hasVariable,
            TimePeriodCompute timePeriodCompute,
            bool isStartConditionOnCreation,
            int scheduleCallbackId)
        {
            IsVariable = hasVariable;
            TimePeriodCompute = timePeriodCompute;
            IsStartConditionOnCreation = isStartConditionOnCreation;
            ScheduleCallbackId = scheduleCallbackId;
        }

        public bool IsVariable { get; }

        public TimePeriodCompute TimePeriodCompute { get; }

        public bool IsStartConditionOnCreation { get; }

        public int ScheduleCallbackId { get; }

        public OutputCondition InstantiateOutputCondition(
            AgentInstanceContext agentInstanceContext,
            OutputCallback outputCallback)
        {
            return new OutputConditionTime(outputCallback, agentInstanceContext, this, IsStartConditionOnCreation);
        }
    }
} // end of namespace