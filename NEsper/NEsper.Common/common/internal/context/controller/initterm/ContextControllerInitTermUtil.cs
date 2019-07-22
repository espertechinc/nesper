///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermUtil
    {
        public static ContextControllerInitTermSvc GetService(ContextControllerInitTermFactory factory)
        {
            if (factory.FactoryEnv.IsRoot) {
                return new ContextControllerInitTermSvcLevelOne();
            }

            return new ContextControllerInitTermSvcLevelAny();
        }

        public static bool DetermineCurrentlyRunning(
            ContextControllerCondition startCondition,
            ContextControllerInitTerm controller)
        {
            if (startCondition.IsImmediate) {
                return true;
            }

            var factory = controller.InitTermFactory;
            var spec = factory.InitTermSpec;
            if (spec.IsOverlapping) {
                return false;
            }

            // we are not currently running if either of the endpoints is not crontab-triggered
            if (spec.StartCondition is ContextConditionDescriptorCrontab &&
                spec.EndCondition is ContextConditionDescriptorCrontab) {
                var scheduleStart = ((ContextControllerConditionCrontab) startCondition).Schedule;

                var endCron = (ContextConditionDescriptorCrontab) spec.EndCondition;
                var scheduleEnd = ScheduleExpressionUtil.CrontabScheduleBuild(
                    endCron.Evaluators,
                    controller.Realization.AgentInstanceContextCreate);

                var importService = controller.Realization.AgentInstanceContextCreate.ImportServiceRuntime;
                var time = controller.Realization.AgentInstanceContextCreate.SchedulingService.Time;
                var nextScheduledStartTime = ScheduleComputeHelper.ComputeNextOccurance(
                    scheduleStart,
                    time,
                    importService.TimeZone,
                    importService.TimeAbacus);
                var nextScheduledEndTime = ScheduleComputeHelper.ComputeNextOccurance(
                    scheduleEnd,
                    time,
                    importService.TimeZone,
                    importService.TimeAbacus);
                return nextScheduledStartTime >= nextScheduledEndTime;
            }

            if (startCondition.Descriptor is ContextConditionDescriptorTimePeriod) {
                var descriptor = (ContextConditionDescriptorTimePeriod) startCondition.Descriptor;
                var endTime = descriptor.GetExpectedEndTime(controller.Realization);
                if (endTime != null && endTime <= 0) {
                    return true;
                }
            }

            return startCondition is ContextConditionDescriptorImmediate;
        }

        public static ContextControllerInitTermPartitionKey BuildPartitionKey(
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            ContextControllerCondition endCondition,
            ContextControllerInitTerm controller)
        {
            var startTime = controller.realization.AgentInstanceContextCreate.SchedulingService.Time;
            var expectedEndTime = endCondition.ExpectedEndTime;
            return new ContextControllerInitTermPartitionKey(
                optionalTriggeringEvent,
                optionalTriggeringPattern,
                startTime,
                expectedEndTime);
        }

        public static ContextPartitionIdentifierInitiatedTerminated KeyToIdentifier(
            int subpathIdOrCPId,
            ContextControllerInitTermPartitionKey key,
            ContextControllerInitTerm controller)
        {
            var identifier = new ContextPartitionIdentifierInitiatedTerminated();
            identifier.StartTime = key.StartTime;
            identifier.EndTime = key.ExpectedEndTime;

            var start = controller.InitTermFactory.InitTermSpec.StartCondition;
            if (start is ContextConditionDescriptorFilter) {
                var filter = (ContextConditionDescriptorFilter) start;
                if (filter.OptionalFilterAsName != null) {
                    identifier.Properties = Collections.SingletonDataMap(
                        filter.OptionalFilterAsName,
                        key.TriggeringEvent);
                }
            }

            if (controller.Factory.FactoryEnv.IsLeaf) {
                identifier.ContextPartitionId = subpathIdOrCPId;
            }

            return identifier;
        }
    }
} // end of namespace