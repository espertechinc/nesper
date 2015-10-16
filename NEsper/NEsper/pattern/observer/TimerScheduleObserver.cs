///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.observer
{
	/// <summary>
	/// Observer implementation for indicating that a certain time arrived, similar to "crontab".
	/// </summary>
	public class TimerScheduleObserver : EventObserver, ScheduleHandleCallback
	{
        private readonly ScheduleSlot _scheduleSlot;
        private readonly MatchedEventMap _beginState;
        private readonly ObserverEventEvaluator _observerEventEvaluator;
	    private readonly TimerScheduleSpec _spec;
	    private readonly bool _isFilterChildNonQuitting;

	    // we always keep the anchor time, which could be engine time or the spec time, and never changes in computations
        private DateTimeEx _anchorTime;

	    // for fast computation, keep some last-value information around for the purpose of caching
	    private bool _isTimerActive = false;
        private DateTimeEx _cachedLastScheduled;
	    private long _cachedCountRepeated = 0;

	    private EPStatementHandleCallback _scheduleHandle;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <param name="beginState">start state</param>
        /// <param name="observerEventEvaluator">receiver for events</param>
        /// <param name="isFilterChildNonQuitting">if set to <c>true</c> [is filter child non quitting].</param>
	    public TimerScheduleObserver(TimerScheduleSpec spec, MatchedEventMap beginState, ObserverEventEvaluator observerEventEvaluator, bool isFilterChildNonQuitting)
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternObserverScheduledEval();}

	        // compute reschedule time
	        _isTimerActive = false;
	        var schedulingService = _observerEventEvaluator.Context.PatternContext.SchedulingService;
	        var nextScheduledTime = ComputeNextSetLastScheduled(schedulingService.Time);

	        var quit = !_isFilterChildNonQuitting || nextScheduledTime == -1;
	        _observerEventEvaluator.ObserverEvaluateTrue(_beginState, quit);

	        // handle no more invocations planned
	        if (nextScheduledTime == -1) {
	            StopObserve();
	            _observerEventEvaluator.ObserverEvaluateFalse(false);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverScheduledEval();}
	            return;
	        }

	        schedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
	        _isTimerActive = true;
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverScheduledEval();}
	    }

	    public void StartObserve()
	    {
	        if (_isTimerActive) {
	            throw new IllegalStateException("Timer already active");
	        }

	        var schedulingService = _observerEventEvaluator.Context.PatternContext.SchedulingService;
	        if (_anchorTime == null) {
	            if (_spec.OptionalDate == null)
	            {
	                _anchorTime = new DateTimeEx(
	                    schedulingService.Time.TimeFromMillis(_observerEventEvaluator.Context.StatementContext.MethodResolutionService.EngineImportService.TimeZone),
	                    _observerEventEvaluator.Context.StatementContext.MethodResolutionService.EngineImportService.TimeZone
                    );
	            }
	            else {
	                _anchorTime = _spec.OptionalDate;
	            }
	        }

	        var nextScheduledTime = ComputeNextSetLastScheduled(schedulingService.Time);
	        if (nextScheduledTime == -1) {
	            StopObserve();
	            _observerEventEvaluator.ObserverEvaluateFalse(false);
	            return;
	        }

	        _scheduleHandle = new EPStatementHandleCallback(_observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
	        schedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
	        _isTimerActive = true;
	    }

	    public void StopObserve()
	    {
	        if (_isTimerActive) {
	            _observerEventEvaluator.Context.PatternContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
	        }
	        _isTimerActive = false;
	        _scheduleHandle = null;
	        _cachedCountRepeated = long.MaxValue;
	        _cachedLastScheduled = null;
	        _anchorTime = null;
	    }

	    public void Accept(EventObserverVisitor visitor)
        {
	        visitor.VisitObserver(_beginState, 2, _scheduleSlot, _spec, _anchorTime, _cachedCountRepeated, _cachedLastScheduled, _isTimerActive);
	    }

	    private long ComputeNextSetLastScheduled(long currentTime)
        {
	        // handle already-stopped
	        if (_cachedCountRepeated == long.MaxValue) {
	            return -1;
	        }

	        // handle date-only-form: "<date>"
	        if (_spec.OptionalRepeatCount == null && _spec.OptionalDate != null && _spec.OptionalTimePeriod == null) {
	            _cachedCountRepeated = long.MaxValue;
                var anchorTimeInMillis = _anchorTime.TimeInMillis;
                if (anchorTimeInMillis > currentTime)
                {
	                return anchorTimeInMillis - currentTime;
	            }
	            return -1;
	        }

	        // handle period-only-form: "P<period>"
	        // handle partial-form-2: "<date>/<period>" (non-recurring)
	        if (_spec.OptionalRepeatCount == null && _spec.OptionalTimePeriod != null)
	        {
	            _cachedCountRepeated = long.MaxValue;
	            _cachedLastScheduled = new DateTimeEx(_anchorTime);
                CalendarOpPlusMinus.Action(_cachedLastScheduled, 1, _spec.OptionalTimePeriod);
                var cachedLastTimeInMillis = _cachedLastScheduled.TimeInMillis;
                if (cachedLastTimeInMillis > currentTime)
                {
	                return cachedLastTimeInMillis - currentTime;
	            }
	            return -1;
	        }

	        // handle partial-form-1: "R<?>/<period>"
	        // handle full form
	        if (_cachedLastScheduled == null) {
	            _cachedLastScheduled = new DateTimeEx(_anchorTime);
	            if (_spec.OptionalDate != null) {
	                _cachedCountRepeated = 1;
	            }
	        }

	        if (_spec.OptionalRepeatCount == -1)
	        {
                var nextDueInner = CalendarOpPlusFastAddHelper.ComputeNextDue(currentTime, _spec.OptionalTimePeriod, _cachedLastScheduled);
	            _cachedLastScheduled.Set(nextDueInner.Scheduled);
	            return _cachedLastScheduled.TimeInMillis - currentTime;
	        }

	        var nextDue = CalendarOpPlusFastAddHelper.ComputeNextDue(currentTime, _spec.OptionalTimePeriod, _cachedLastScheduled);
	        _cachedCountRepeated += nextDue.Factor;
	        if (_cachedCountRepeated <= _spec.OptionalRepeatCount) {
	            _cachedLastScheduled.Set(nextDue.Scheduled);
	            var cachedLastTimeInMillis = _cachedLastScheduled.TimeInMillis;
	            if (cachedLastTimeInMillis > currentTime) {
	                return cachedLastTimeInMillis - currentTime;
	            }
	        }
	        _cachedCountRepeated = long.MaxValue;
	        return -1;
	    }
	}
} // end of namespace
