///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     Observer that will wait a certain interval before indicating true (raising an event).
    /// </summary>
    public class TimerIntervalObserver : EventObserver,
        ScheduleHandleCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "timer-interval";

        private readonly long deltaTime;
        private readonly ObserverEventEvaluator observerEventEvaluator;
        private readonly long scheduleSlot;

        private bool isTimerActive;
        private EPStatementHandleCallbackSchedule scheduleHandle;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="deltaTime">the time deltaTime</param>
        /// <param name="beginState">start state</param>
        /// <param name="observerEventEvaluator">receiver for events</param>
        public TimerIntervalObserver(
            long deltaTime,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator)
        {
            this.deltaTime = deltaTime;
            BeginState = beginState;
            this.observerEventEvaluator = observerEventEvaluator;
            scheduleSlot = observerEventEvaluator.Context.AgentInstanceContext.ScheduleBucket.AllocateSlot();
        }

        public MatchedEventMap BeginState { get; }

        public void StartObserve()
        {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }

            if (deltaTime <= 0) {
                observerEventEvaluator.ObserverEvaluateTrue(BeginState, true);
            }
            else {
                var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
                scheduleHandle = new EPStatementHandleCallbackSchedule(
                    agentInstanceContext.EpStatementAgentInstanceHandle,
                    this);
                agentInstanceContext.AuditProvider.ScheduleAdd(
                    deltaTime,
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.pattern,
                    NAME_AUDITPROVIDER_SCHEDULE);
                observerEventEvaluator.Context.StatementContext.SchedulingService.Add(
                    deltaTime,
                    scheduleHandle,
                    scheduleSlot);
                isTimerActive = true;
            }
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
            visitor.VisitObserver(BeginState, 10, scheduleSlot);
        }

        public void ScheduledTrigger()
        {
            var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverScheduledEval();
            agentInstanceContext.AuditProvider.ScheduleFire(
                agentInstanceContext,
                ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            observerEventEvaluator.ObserverEvaluateTrue(BeginState, true);
            isTimerActive = false;
            agentInstanceContext.InstrumentationProvider.APatternObserverScheduledEval();
        }
    }
} // end of namespace