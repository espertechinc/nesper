///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionFactory
    {
        public static ContextControllerConditionNonHA GetEndpoint(
            IntSeqKey conditionPath,
            object[] partitionKeys,
            ContextConditionDescriptor endpoint,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            if (endpoint is ContextConditionDescriptorFilter filter) {
                return new ContextControllerConditionFilter(conditionPath, partitionKeys, filter, callback, controller);
            }

            if (endpoint is ContextConditionDescriptorTimePeriod timePeriod) {
                var scheduleSlot = controller.Realization.AgentInstanceContextCreate.ScheduleBucket.AllocateSlot();
                return new ContextControllerConditionTimePeriod(
                    scheduleSlot,
                    timePeriod,
                    conditionPath,
                    callback,
                    controller);
            }

            if (endpoint is ContextConditionDescriptorCrontab crontab) {
                var schedules = new ScheduleSpec[crontab.EvaluatorsPerCrontab.Length];
                for (var i = 0; i < schedules.Length; i++) {
                    schedules[i] = ScheduleExpressionUtil.CrontabScheduleBuild(
                        crontab.EvaluatorsPerCrontab[i],
                        controller.Realization.AgentInstanceContextCreate);
                }

                var scheduleSlot = controller.Realization.AgentInstanceContextCreate.ScheduleBucket.AllocateSlot();
                return new ContextControllerConditionCrontabImpl(
                    conditionPath,
                    scheduleSlot,
                    schedules,
                    crontab,
                    callback,
                    controller);
            }

            if (endpoint is ContextConditionDescriptorPattern pattern) {
                return new ContextControllerConditionPattern(
                    conditionPath,
                    partitionKeys,
                    pattern,
                    callback,
                    controller);
            }

            if (endpoint is ContextConditionDescriptorNever) {
                return ContextControllerConditionNever.INSTANCE;
            }

            if (endpoint is ContextConditionDescriptorImmediate) {
                return ContextControllerConditionImmediate.INSTANCE;
            }

            throw new IllegalStateException("Unrecognized context range endpoint " + endpoint.GetType());
        }
    }
} // end of namespace