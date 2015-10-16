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
    /// Guard implementation that keeps a timer instance and quits when the timer expired, and 
    /// also keeps a count of the number of matches so far, checking both count and timer, letting 
    /// all <seealso cref="com.espertech.esper.pattern.MatchedEventMap"/> instances pass until then.
    /// </summary>
    public class TimerWithinOrMaxCountGuard : Guard, ScheduleHandleCallback
    {
        private readonly long _msec;
        private readonly int _numCountTo;
        private readonly Quitable _quitable;
        private readonly ScheduleSlot _scheduleSlot;
    
        private int _counter;
        private bool _isTimerActive;
        private EPStatementHandleCallback _scheduleHandle;
    
        /// <summary>Ctor. </summary>
        /// <param name="msec">number of millisecond to guard expiration</param>
        /// <param name="numCountTo">max number of counts</param>
        /// <param name="quitable">to use to indicate that the gaurd quitted</param>
        public TimerWithinOrMaxCountGuard(long msec, int numCountTo, Quitable quitable) {
            this._msec = msec;
            this._numCountTo = numCountTo;
            this._quitable = quitable;
            this._scheduleSlot = quitable.Context.PatternContext.ScheduleBucket.AllocateSlot();
        }
    
        public void StartGuard() {
            if (_isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }
    
            _scheduleHandle = new EPStatementHandleCallback(_quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            _quitable.Context.PatternContext.SchedulingService.Add(_msec, _scheduleHandle, _scheduleSlot);
            _isTimerActive = true;
            _counter = 0;
        }
    
        public bool Inspect(MatchedEventMap matchEvent) {
            _counter++;
            if (_counter > _numCountTo) {
                _quitable.GuardQuit();
                DeactivateTimer();
                return false;
            }
            return true;
        }
    
        public void StopGuard() {
            if (_isTimerActive) {
                DeactivateTimer();
            }
        }
    
        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext) {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardScheduledEval();}
            // Timer callback is automatically removed when triggering
            _isTimerActive = false;
            _quitable.GuardQuit();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardScheduledEval();}
        }
    
        public void Accept(EventGuardVisitor visitor) {
            visitor.VisitGuard(20, _scheduleSlot);
        }
    
        private void DeactivateTimer() {
            if (_scheduleHandle != null) {
                _quitable.Context.PatternContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
            }
            _scheduleHandle = null;
            _isTimerActive = false;
        }
    }
}
