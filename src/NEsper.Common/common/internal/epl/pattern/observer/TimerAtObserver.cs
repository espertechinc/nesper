///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    /// Observer implementation for indicating that a certain time arrived, similar to "crontab".
    /// </summary>
    public class TimerAtObserver : EventObserver,
        ScheduleHandleCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "timer-at";

        private readonly ScheduleSpec scheduleSpec;
        private readonly long scheduleSlot;
        private readonly MatchedEventMap beginState;
        private readonly ObserverEventEvaluator observerEventEvaluator;

        private bool isTimerActive = false;
        private EPStatementHandleCallbackSchedule scheduleHandle;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="scheduleSpec">specification containing the crontab schedule</param>
        /// <param name="beginState">start state</param>
        /// <param name="observerEventEvaluator">receiver for events</param>
        public TimerAtObserver(
            ScheduleSpec scheduleSpec,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator)
        {
            this.scheduleSpec = scheduleSpec;
            this.beginState = beginState;
            this.observerEventEvaluator = observerEventEvaluator;
            scheduleSlot = observerEventEvaluator.Context.AgentInstanceContext.ScheduleBucket.AllocateSlot();
        }

        public MatchedEventMap BeginState {
            get => beginState;
        }

        public void ScheduledTrigger()
        {
            var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverScheduledEval();
            agentInstanceContext.AuditProvider.ScheduleFire(
                agentInstanceContext,
                ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            observerEventEvaluator.ObserverEvaluateTrue(beginState, true);
            isTimerActive = false;
            agentInstanceContext.InstrumentationProvider.APatternObserverScheduledEval();
        }

        public void StartObserve()
        {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }

            scheduleHandle = new EPStatementHandleCallbackSchedule(
                observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
            var schedulingService = agentInstanceContext.SchedulingService;
            var importService = agentInstanceContext.ImportServiceRuntime;
            var nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                scheduleSpec,
                schedulingService.Time,
                importService.TimeZone,
                importService.TimeAbacus);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                nextScheduledTime,
                agentInstanceContext,
                scheduleHandle,
                ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            schedulingService.Add(nextScheduledTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
        }

        public void StopObserve()
        {
            if (isTimerActive) {
                var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.pattern,
                    NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
                isTimerActive = false;
                scheduleHandle = null;
            }
        }

        public void Accept(EventObserverVisitor visitor)
        {
            visitor.VisitObserver(beginState, 2, scheduleSlot, scheduleSpec);
        }
    }
} // end of namespace