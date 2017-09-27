///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Observer implementation for indicating that a certain time arrived, similar to "crontab".
    /// </summary>
    public class TimerAtObserver
        : EventObserver
        , ScheduleHandleCallback
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ScheduleSpec _scheduleSpec;
        private readonly long _scheduleSlot;
        private readonly MatchedEventMap _beginState;
        private readonly ObserverEventEvaluator _observerEventEvaluator;
        private bool _isTimerActive = false;
        private EPStatementHandleCallback _scheduleHandle;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="scheduleSpec">- specification containing the crontab schedule</param>
        /// <param name="beginState">- start state</param>
        /// <param name="observerEventEvaluator">- receiver for events</param>
        public TimerAtObserver(
            ScheduleSpec scheduleSpec,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator)
        {
            _scheduleSpec = scheduleSpec;
            _beginState = beginState;
            _observerEventEvaluator = observerEventEvaluator;
            _scheduleSlot = observerEventEvaluator.Context.PatternContext.ScheduleBucket.AllocateSlot();
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
            _observerEventEvaluator.ObserverEvaluateTrue(_beginState, true);
            _isTimerActive = false;
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

            _scheduleHandle = new EPStatementHandleCallback(_observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            var schedulingService = _observerEventEvaluator.Context.PatternContext.SchedulingService;
            var engineImportService =
                _observerEventEvaluator.Context.StatementContext.EngineImportService;
            var nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                _scheduleSpec, schedulingService.Time, engineImportService.TimeZone, engineImportService.TimeAbacus);
            schedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
            _isTimerActive = true;
        }

        public void StopObserve()
        {
            if (_isTimerActive)
            {
                _observerEventEvaluator.Context.PatternContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
                _isTimerActive = false;
                _scheduleHandle = null;
            }
        }

        public void Accept(EventObserverVisitor visitor)
        {
            visitor.VisitObserver(_beginState, 2, _scheduleSlot, _scheduleSpec);
        }
    }
} // end of namespace
