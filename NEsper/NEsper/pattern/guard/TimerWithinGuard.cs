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

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    /// Guard implementation that keeps a timer instance and quits when the timer expired, letting all <seealso cref="MatchedEventMap"/> instances pass until then.
    /// </summary>
    public class TimerWithinGuard : Guard, ScheduleHandleCallback
    {
        private readonly long _msec;
        private readonly Quitable _quitable;
        private readonly ScheduleSlot _scheduleSlot;
    
        private bool _isTimerActive;
        private EPStatementHandleCallback _scheduleHandle;
    
        /// <summary>Ctor. </summary>
        /// <param name="msec">number of millisecond to guard expiration</param>
        /// <param name="quitable">to use to indicate that the gaurd quitted</param>
        public TimerWithinGuard(long msec, Quitable quitable)
        {
            _msec = msec;
            _quitable = quitable;
            _scheduleSlot = quitable.Context.PatternContext.ScheduleBucket.AllocateSlot();
        }
    
        public void StartGuard()
        {
            if (_isTimerActive)
            {
                throw new IllegalStateException("Timer already active");
            }
    
            // Start the stopwatch timer
            _scheduleHandle = new EPStatementHandleCallback(_quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            _quitable.Context.PatternContext.SchedulingService.Add(_msec, _scheduleHandle, _scheduleSlot);
            _isTimerActive = true;
        }
    
        public void StopGuard()
        {
            if (_isTimerActive)
            {
                _quitable.Context.PatternContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
                _scheduleHandle = null;
                _isTimerActive = false;
            }
        }
    
        public bool Inspect(MatchedEventMap matchEvent)
        {
            // no need to test: for timing only, if the timer expired the guardQuit stops any events from coming here
            return true;
        }
    
        public void ScheduledTrigger(ExtensionServicesContext extensionServicesContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardScheduledEval();}
            // Timer callback is automatically removed when triggering
            _isTimerActive = false;
            _quitable.GuardQuit();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardScheduledEval();}
        }
    
        public void Accept(EventGuardVisitor visitor) {
            visitor.VisitGuard(10, _scheduleSlot);
        }
    }
}
