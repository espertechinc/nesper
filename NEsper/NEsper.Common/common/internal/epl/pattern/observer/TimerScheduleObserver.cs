///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Observer implementation for indicating that a certain time arrived, similar to "crontab".
    /// </summary>
    public class TimerScheduleObserver : EventObserver,
        ScheduleHandleCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "timer-at";
        private readonly bool isFilterChildNonQuitting;
        internal readonly ObserverEventEvaluator observerEventEvaluator;
        internal readonly long scheduleSlot;
        private readonly TimerScheduleSpec spec;
        internal long anchorRemainder;

        // we always keep the anchor time, which could be runtime time or the spec time, and never changes in computations
        internal DateTimeEx anchorTime;
        internal MatchedEventMap beginState;
        private long cachedCountRepeated;
        private DateTimeEx cachedLastScheduled;

        // for fast computation, keep some last-value information around for the purpose of caching
        internal bool isTimerActive;

        internal EPStatementHandleCallbackSchedule scheduleHandle;

        public TimerScheduleObserver(
            TimerScheduleSpec spec, MatchedEventMap beginState, ObserverEventEvaluator observerEventEvaluator,
            bool isFilterChildNonQuitting)
        {
            this.beginState = beginState;
            this.observerEventEvaluator = observerEventEvaluator;
            scheduleSlot = observerEventEvaluator.Context.AgentInstanceContext.ScheduleBucket.AllocateSlot();
            this.spec = spec;
            this.isFilterChildNonQuitting = isFilterChildNonQuitting;
        }

        public MatchedEventMap BeginState => beginState;

        public void StartObserve()
        {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }

            var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
            var schedulingService = agentInstanceContext.SchedulingService;
            var timeAbacus = agentInstanceContext.ImportServiceRuntime.TimeAbacus;

            if (anchorTime == null) {
                if (spec.OptionalDate == null) {
                    anchorTime = DateTimeEx.GetInstance(
                        observerEventEvaluator.Context.StatementContext.ImportServiceRuntime.TimeZone);
                    anchorRemainder = timeAbacus.DateTimeSet(schedulingService.Time, anchorTime);
                }
                else {
                    anchorTime = spec.OptionalDate;
                    anchorRemainder = spec.OptionalRemainder.GetValueOrDefault(0);
                }
            }

            var nextScheduledTime = ComputeNextSetLastScheduled(schedulingService.Time, timeAbacus);
            if (nextScheduledTime == -1) {
                StopObserve();
                observerEventEvaluator.ObserverEvaluateFalse(false);
                return;
            }

            scheduleHandle = new EPStatementHandleCallbackSchedule(
                observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                nextScheduledTime, agentInstanceContext, scheduleHandle, ScheduleObjectType.context,
                NAME_AUDITPROVIDER_SCHEDULE);
            schedulingService.Add(nextScheduledTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
        }

        public void StopObserve()
        {
            if (isTimerActive) {
                var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext, scheduleHandle, ScheduleObjectType.pattern, NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }

            isTimerActive = false;
            scheduleHandle = null;
            cachedCountRepeated = long.MaxValue;
            cachedLastScheduled = null;
            anchorTime = null;
        }

        public void Accept(EventObserverVisitor visitor)
        {
            visitor.VisitObserver(
                beginState, 2, scheduleSlot, spec, anchorTime, cachedCountRepeated, cachedLastScheduled, isTimerActive);
        }

        public void ScheduledTrigger()
        {
            var agentInstanceContext = observerEventEvaluator.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverScheduledEval();
            agentInstanceContext.AuditProvider.ScheduleFire(
                agentInstanceContext, ScheduleObjectType.pattern, NAME_AUDITPROVIDER_SCHEDULE);

            // compute reschedule time
            isTimerActive = false;
            var schedulingService = agentInstanceContext.SchedulingService;
            var nextScheduledTime = ComputeNextSetLastScheduled(
                schedulingService.Time, agentInstanceContext.ImportServiceRuntime.TimeAbacus);

            var quit = !isFilterChildNonQuitting || nextScheduledTime == -1;
            observerEventEvaluator.ObserverEvaluateTrue(beginState, quit);

            // handle no more invocations planned
            if (nextScheduledTime == -1) {
                StopObserve();
                observerEventEvaluator.ObserverEvaluateFalse(false);
                agentInstanceContext.InstrumentationProvider.APatternObserverScheduledEval();
                return;
            }

            agentInstanceContext.AuditProvider.ScheduleAdd(
                nextScheduledTime, agentInstanceContext, scheduleHandle, ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            schedulingService.Add(nextScheduledTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;

            agentInstanceContext.InstrumentationProvider.APatternObserverScheduledEval();
        }

        private long ComputeNextSetLastScheduled(long currentTime, TimeAbacus timeAbacus)
        {
            // handle already-stopped
            if (cachedCountRepeated == long.MaxValue) {
                return -1;
            }

            // handle date-only-form: "<date>"
            if (spec.OptionalRepeatCount == null && spec.OptionalDate != null && spec.OptionalTimePeriod == null) {
                cachedCountRepeated = long.MaxValue;
                var computed = timeAbacus.DateTimeGet(anchorTime, anchorRemainder);
                if (computed > currentTime) {
                    return computed - currentTime;
                }

                return -1;
            }

            // handle period-only-form: "P<period>"
            // handle partial-form-2: "<date>/<period>" (non-recurring)
            if (spec.OptionalRepeatCount == null && spec.OptionalTimePeriod != null) {
                cachedCountRepeated = long.MaxValue;
                cachedLastScheduled = anchorTime.Clone();
                CalendarPlusMinusForgeOp.ActionCalendarPlusMinusTimePeriod(
                    cachedLastScheduled, 1, spec.OptionalTimePeriod);
                var computed = timeAbacus.DateTimeGet(cachedLastScheduled, anchorRemainder);
                if (computed > currentTime) {
                    return computed - currentTime;
                }

                return -1;
            }

            // handle partial-form-1: "R<?>/<period>"
            // handle full form
            if (cachedLastScheduled == null) {
                cachedLastScheduled = anchorTime.Clone();
                if (spec.OptionalDate != null) {
                    cachedCountRepeated = 1;
                }
            }

            var nextDue = CalendarOpPlusFastAddHelper.ComputeNextDue(
                currentTime, spec.OptionalTimePeriod, cachedLastScheduled, timeAbacus, anchorRemainder);

            if (spec.OptionalRepeatCount == -1) {
                cachedLastScheduled = nextDue.Scheduled;
                var computed = timeAbacus.DateTimeGet(cachedLastScheduled, anchorRemainder);
                return computed - currentTime;
            }

            cachedCountRepeated += nextDue.Factor;
            if (cachedCountRepeated <= spec.OptionalRepeatCount) {
                cachedLastScheduled = nextDue.Scheduled;
                var computed = timeAbacus.DateTimeGet(cachedLastScheduled, anchorRemainder);
                if (computed > currentTime) {
                    return computed - currentTime;
                }
            }

            cachedCountRepeated = long.MaxValue;
            return -1;
        }
    }
} // end of namespace