///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Observer implementation for indicating that a certain time arrived, similar to "crontab".
    /// </summary>
    public class TimerScheduleObserver
        : EventObserver
            ,
            ScheduleHandleCallback
    {
        private readonly long _scheduleSlot;
        private readonly ObserverEventEvaluator _observerEventEvaluator;
        private readonly TimerScheduleSpec _spec;
        private readonly bool _isFilterChildNonQuitting;
        private readonly MatchedEventMap _beginState;
        // we always keep the anchor time, which could be engine time or the spec time, and never changes in computations
        private DateTimeEx _anchorTime;
        private long _anchorRemainder;

        // for fast computation, keep some last-value information around for the purpose of caching
        private bool _isTimerActive = false;
        private EPStatementHandleCallback _scheduleHandle;
        private DateTimeEx _cachedLastScheduled;
        private long _cachedCountRepeated = 0;

        public TimerScheduleObserver(
            TimerScheduleSpec spec,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            bool isFilterChildNonQuitting)
        {
            _beginState = beginState;
            _observerEventEvaluator = observerEventEvaluator;
            _scheduleSlot = observerEventEvaluator.Context.PatternContext.ScheduleBucket.AllocateSlot();
            _spec = spec;
            _isFilterChildNonQuitting = isFilterChildNonQuitting;
        }

        public MatchedEventMap BeginState
        {
            get { return _beginState; }
        }

        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QPatternObserverScheduledEval();
            }

            // compute reschedule time
            _isTimerActive = false;
            var schedulingService = _observerEventEvaluator.Context.PatternContext.SchedulingService;
            var nextScheduledTime = ComputeNextSetLastScheduled(
                schedulingService.Time, _observerEventEvaluator.Context.StatementContext.TimeAbacus);

            var quit = !_isFilterChildNonQuitting || nextScheduledTime == -1;
            _observerEventEvaluator.ObserverEvaluateTrue(_beginState, quit);

            // handle no more invocations planned
            if (nextScheduledTime == -1)
            {
                StopObserve();
                _observerEventEvaluator.ObserverEvaluateFalse(false);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().APatternObserverScheduledEval();
                }
                return;
            }

            schedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
            _isTimerActive = true;
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().APatternObserverScheduledEval();
            }
        }

        public void StartObserve()
        {
            if (_isTimerActive)
            {
                throw new IllegalStateException("Timer already active");
            }

            var schedulingService = _observerEventEvaluator.Context.PatternContext.SchedulingService;
            var timeAbacus = _observerEventEvaluator.Context.StatementContext.TimeAbacus;

            if (_anchorTime == null)
            {
                if (_spec.OptionalDate == null)
                {
                    _anchorTime =
                        DateTimeEx.GetInstance(
                            _observerEventEvaluator.Context.StatementContext.EngineImportService.TimeZone);
                    _anchorRemainder = timeAbacus.CalendarSet(schedulingService.Time, _anchorTime);
                }
                else
                {
                    _anchorTime = _spec.OptionalDate;
                    _anchorRemainder = _spec.OptionalRemainder.GetValueOrDefault(0);
                }
            }

            var nextScheduledTime = ComputeNextSetLastScheduled(schedulingService.Time, timeAbacus);
            if (nextScheduledTime == -1)
            {
                StopObserve();
                _observerEventEvaluator.ObserverEvaluateFalse(false);
                return;
            }

            _scheduleHandle =
                new EPStatementHandleCallback(
                    _observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            schedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
            _isTimerActive = true;
        }

        public void StopObserve()
        {
            if (_isTimerActive)
            {
                _observerEventEvaluator.Context.PatternContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
            }
            _isTimerActive = false;
            _scheduleHandle = null;
            _cachedCountRepeated = Int64.MaxValue;
            _cachedLastScheduled = null;
            _anchorTime = null;
        }

        public void Accept(EventObserverVisitor visitor)
        {
            visitor.VisitObserver(
                _beginState, 2, _scheduleSlot, _spec, _anchorTime, _cachedCountRepeated, _cachedLastScheduled,
                _isTimerActive);
        }

        private long ComputeNextSetLastScheduled(long currentTime, TimeAbacus timeAbacus)
        {

            // handle already-stopped
            if (_cachedCountRepeated == Int64.MaxValue)
            {
                return -1;
            }

            // handle date-only-form: "<date>"
            if (_spec.OptionalRepeatCount == null && _spec.OptionalDate != null && _spec.OptionalTimePeriod == null)
            {
                _cachedCountRepeated = Int64.MaxValue;
                var computed = timeAbacus.CalendarGet(_anchorTime, _anchorRemainder);
                if (computed > currentTime)
                {
                    return computed - currentTime;
                }
                return -1;
            }

            // handle period-only-form: "P<period>"
            // handle partial-form-2: "<date>/<period>" (non-recurring)
            if (_spec.OptionalRepeatCount == null && _spec.OptionalTimePeriod != null)
            {
                _cachedCountRepeated = Int64.MaxValue;
                _cachedLastScheduled = _anchorTime.Clone();
                CalendarOpPlusMinus.Action(_cachedLastScheduled, 1, _spec.OptionalTimePeriod);
                var computed = timeAbacus.CalendarGet(_cachedLastScheduled, _anchorRemainder);
                if (computed > currentTime)
                {
                    return computed - currentTime;
                }
                return -1;
            }

            // handle partial-form-1: "R<?>/<period>"
            // handle full form
            if (_cachedLastScheduled == null)
            {
                _cachedLastScheduled = _anchorTime.Clone();
                if (_spec.OptionalDate != null)
                {
                    _cachedCountRepeated = 1;
                }
            }

            var nextDue = CalendarOpPlusFastAddHelper.ComputeNextDue(
                currentTime, _spec.OptionalTimePeriod, _cachedLastScheduled, timeAbacus, _anchorRemainder);

            if (_spec.OptionalRepeatCount == -1)
            {
                _cachedLastScheduled = nextDue.Scheduled;
                var computed = timeAbacus.CalendarGet(_cachedLastScheduled, _anchorRemainder);
                return computed - currentTime;
            }

            _cachedCountRepeated += nextDue.Factor;
            if (_cachedCountRepeated <= _spec.OptionalRepeatCount)
            {
                _cachedLastScheduled = nextDue.Scheduled;
                var computed = timeAbacus.CalendarGet(_cachedLastScheduled, _anchorRemainder);
                if (computed > currentTime)
                {
                    return computed - currentTime;
                }
            }
            _cachedCountRepeated = Int64.MaxValue;
            return -1;
        }
    }
} // end of namespace
