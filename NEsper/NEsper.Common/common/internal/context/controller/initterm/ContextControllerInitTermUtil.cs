///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
	public class ContextControllerInitTermUtil {
	    public static ContextControllerInitTermSvc GetService(ContextControllerInitTermFactory factory) {
	        if (factory.FactoryEnv.IsRoot) {
	            return new ContextControllerInitTermSvcLevelOne();
	        }
	        return new ContextControllerInitTermSvcLevelAny();
	    }

	    public static bool DetermineCurrentlyRunning(ContextControllerCondition startCondition, ContextControllerInitTerm controller) {
	        if (startCondition.IsImmediate) {
	            return true;
	        }

	        ContextControllerDetailInitiatedTerminated spec = controller.Factory.InitTermSpec;
	        if (spec.IsOverlapping) {
	            return false;
	        }

	        // we are not currently running if either of the endpoints is not crontab-triggered
	        if ((spec.StartCondition is ContextConditionDescriptorCrontab) &&
	                ((spec.EndCondition is ContextConditionDescriptorCrontab))) {
	            ScheduleSpec scheduleStart = ((ContextControllerConditionCrontab) startCondition).Schedule;

	            ContextConditionDescriptorCrontab endCron = (ContextConditionDescriptorCrontab) spec.EndCondition;
	            ScheduleSpec scheduleEnd = ScheduleExpressionUtil.CrontabScheduleBuild(endCron.Evaluators, controller.Realization.AgentInstanceContextCreate);

	            ImportServiceRuntime importService = controller.Realization.AgentInstanceContextCreate.ImportServiceRuntime;
	            long time = controller.Realization.AgentInstanceContextCreate.SchedulingService.Time;
	            long nextScheduledStartTime = ScheduleComputeHelper.ComputeNextOccurance(scheduleStart, time, importService.TimeZone, importService.TimeAbacus);
	            long nextScheduledEndTime = ScheduleComputeHelper.ComputeNextOccurance(scheduleEnd, time, importService.TimeZone, importService.TimeAbacus);
	            return nextScheduledStartTime >= nextScheduledEndTime;
	        }

	        if (startCondition.Descriptor is ContextConditionDescriptorTimePeriod) {
	            ContextConditionDescriptorTimePeriod descriptor = (ContextConditionDescriptorTimePeriod) startCondition.Descriptor;
	            long? endTime = descriptor.GetExpectedEndTime(controller.Realization);
	            if (endTime != null && endTime <= 0) {
	                return true;
	            }
	        }

	        return startCondition is ContextConditionDescriptorImmediate;
	    }

	    public static ContextControllerInitTermPartitionKey BuildPartitionKey(EventBean optionalTriggeringEvent, IDictionary<string, object> optionalTriggeringPattern, ContextControllerCondition endCondition, ContextControllerInitTerm controller) {
	        long startTime = controller.realization.AgentInstanceContextCreate.SchedulingService.Time;
	        long? expectedEndTime = endCondition.ExpectedEndTime;
	        return new ContextControllerInitTermPartitionKey(optionalTriggeringEvent, optionalTriggeringPattern, startTime, expectedEndTime);
	    }

	    public static ContextPartitionIdentifierInitiatedTerminated KeyToIdentifier(int subpathIdOrCPId, ContextControllerInitTermPartitionKey key, ContextControllerInitTerm controller) {
	        ContextPartitionIdentifierInitiatedTerminated identifier = new ContextPartitionIdentifierInitiatedTerminated();
	        identifier.StartTime = key.StartTime;
	        identifier.EndTime = key.ExpectedEndTime;

	        ContextConditionDescriptor start = controller.Factory.InitTermSpec.StartCondition;
	        if (start is ContextConditionDescriptorFilter) {
	            ContextConditionDescriptorFilter filter = (ContextConditionDescriptorFilter) start;
	            if (filter.OptionalFilterAsName != null) {
	                identifier.Properties = Collections.SingletonMap(filter.OptionalFilterAsName, key.TriggeringEvent);
	            }
	        }

	        if (controller.Factory.FactoryEnv.IsLeaf) {
	            identifier.ContextPartitionId = subpathIdOrCPId;
	        }

	        return identifier;
	    }
	}
} // end of namespace