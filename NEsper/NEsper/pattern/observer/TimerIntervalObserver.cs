///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Observer that will wait a certain interval before indicating true (raising an event).
    /// </summary>
    public class TimerIntervalObserver : EventObserver, ScheduleHandleCallback
    {
        private readonly long _msec;
        private readonly MatchedEventMap _beginState;
        private readonly ObserverEventEvaluator _observerEventEvaluator;
        private readonly ScheduleSlot _scheduleSlot;
    
        private bool _isTimerActive = false;
        private EPStatementHandleCallback _scheduleHandle;
    
        /// <summary>Ctor. </summary>
        /// <param name="msec">number of milliseconds</param>
        /// <param name="beginState">start state</param>
        /// <param name="observerEventEvaluator">receiver for events</param>
        public TimerIntervalObserver(long msec, MatchedEventMap beginState, ObserverEventEvaluator observerEventEvaluator)
        {
            _msec = msec;
            _beginState = beginState;
            _observerEventEvaluator = observerEventEvaluator;
            _scheduleSlot = observerEventEvaluator.Context.PatternContext.ScheduleBucket.AllocateSlot();
        }
    
        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternObserverScheduledEval();}
            _observerEventEvaluator.ObserverEvaluateTrue(_beginState, true);
            _isTimerActive = false;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverScheduledEval();}
        }

        public MatchedEventMap BeginState
        {
            get { return _beginState; }
        }

        public void StartObserve()
        {
            if (_isTimerActive)
            {
                throw new IllegalStateException("Timer already active");
            }
    
            if (_msec <= 0)
            {
                _observerEventEvaluator.ObserverEvaluateTrue(_beginState, true);
            }
            else
            {
                _scheduleHandle = new EPStatementHandleCallback(_observerEventEvaluator.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
                _observerEventEvaluator.Context.PatternContext.SchedulingService.Add(_msec, _scheduleHandle, _scheduleSlot);
                _isTimerActive = true;
            }
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
    
        public void Accept(EventObserverVisitor visitor) {
            visitor.VisitObserver(_beginState, 10, _scheduleSlot);
        }
    }
}
