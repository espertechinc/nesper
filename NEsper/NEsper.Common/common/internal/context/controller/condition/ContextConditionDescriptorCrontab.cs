///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextConditionDescriptorCrontab : ContextConditionDescriptor
    {
        public int ScheduleCallbackId { get; set; } = -1;

        public ExprEvaluator[] Evaluators { get; set; }

        public bool IsImmediate { get; set; }

        public void AddFilterSpecActivatable(IList<FilterSpecActivatable> activatables)
        {
            // none here
        }

        public long? GetExpectedEndTime(
            ContextManagerRealization realization,
            ScheduleSpec scheduleSpec)
        {
            var classpathImportService = realization.AgentInstanceContextCreate.ImportServiceRuntime;
            return ScheduleComputeHelper.ComputeNextOccurance(
                scheduleSpec, realization.AgentInstanceContextCreate.TimeProvider.Time,
                classpathImportService.TimeZone,
                classpathImportService.TimeAbacus);
        }
    }
} // end of namespace