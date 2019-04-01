///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextConditionDescriptorTimePeriod : ContextConditionDescriptor
    {
        public TimePeriodCompute TimePeriodCompute { get; set; }

        public int ScheduleCallbackId { get; set; } = -1;

        public bool IsImmediate { get; set; }

        public void AddFilterSpecActivatable(IList<FilterSpecActivatable> activatables)
        {
            // none here
        }

        public long? GetExpectedEndTime(ContextManagerRealization realization)
        {
            var agentInstanceContext = realization.AgentInstanceContextCreate;
            var current = agentInstanceContext.SchedulingService.Time;
            var msec = TimePeriodCompute.DeltaAdd(current, null, true, agentInstanceContext);
            return current + msec;
        }
    }
} // end of namespace