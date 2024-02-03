///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
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
                var schedulesStart = ((ContextControllerConditionCrontab)startCondition).Schedules;

                var endCron = (ContextConditionDescriptorCrontab)spec.EndCondition;
                var schedulesEnd = new ScheduleSpec[endCron.EvaluatorsPerCrontab.Length];
                for (var i = 0; i < schedulesEnd.Length; i++) {
                    schedulesEnd[i] = ScheduleExpressionUtil.CrontabScheduleBuild(
                        endCron.EvaluatorsPerCrontab[i],
                        controller.Realization.AgentInstanceContextCreate);
                }

                var importService = controller.Realization.AgentInstanceContextCreate.ImportServiceRuntime;
                var time = controller.Realization.AgentInstanceContextCreate.SchedulingService.Time;
                var nextScheduledStartTime = ComputeScheduleMinimumNextOccurance(schedulesStart, time, importService);
                var nextScheduledEndTime = ComputeScheduleMinimumNextOccurance(schedulesEnd, time, importService);

                return nextScheduledStartTime >= nextScheduledEndTime;
            }

            if (startCondition.Descriptor is ContextConditionDescriptorTimePeriod descriptor) {
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
            var startTime = controller.Realization.AgentInstanceContextCreate.SchedulingService.Time;
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
            var filter = start as ContextConditionDescriptorFilter;
            if (filter?.OptionalFilterAsName != null) {
                identifier.Properties = Collections.SingletonDataMap(
                    filter.OptionalFilterAsName,
                    key.TriggeringEvent);
            }

            if (controller.Factory.FactoryEnv.IsLeaf) {
                identifier.ContextPartitionId = subpathIdOrCPId;
            }

            return identifier;
        }

        public static long ComputeScheduleMinimumNextOccurance(
            ScheduleSpec[] schedules,
            long time,
            ImportServiceRuntime importService)
        {
            var value = long.MaxValue;
            foreach (var spec in schedules) {
                var computed = ScheduleComputeHelper.ComputeNextOccurance(
                    spec,
                    time,
                    importService.TimeZone,
                    importService.TimeAbacus);
                if (computed < value) {
                    value = computed;
                }
            }

            return value;
        }

        public static long ComputeScheduleMinimumDelta(
            ScheduleSpec[] schedules,
            long time,
            ImportServiceRuntime importService)
        {
            var value = long.MaxValue;
            foreach (var spec in schedules) {
                var computed = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                    spec,
                    time,
                    importService.TimeZone,
                    importService.TimeAbacus);
                if (computed < value) {
                    value = computed;
                }
            }

            return value;
        }
    }
} // end of namespace