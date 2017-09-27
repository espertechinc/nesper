///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>
    /// Guard implementation that keeps a timer instance and quits when the timer expired,
    /// letting all <seealso cref="MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class TimerWithinGuard : Guard, ScheduleHandleCallback {
        private readonly long deltaTime;
        private readonly Quitable quitable;
        private readonly long scheduleSlot;
    
        private bool isTimerActive;
        private EPStatementHandleCallback scheduleHandle;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="delta">- number of millisecond to guard expiration</param>
        /// <param name="quitable">- to use to indicate that the gaurd quitted</param>
        public TimerWithinGuard(long delta, Quitable quitable) {
            this.deltaTime = delta;
            this.quitable = quitable;
            this.scheduleSlot = quitable.Context.PatternContext.ScheduleBucket.AllocateSlot();
        }
    
        public void StartGuard() {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }
    
            // Start the stopwatch timer
            scheduleHandle = new EPStatementHandleCallback(quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            quitable.Context.PatternContext.SchedulingService.Add(deltaTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
        }
    
        public void StopGuard() {
            if (isTimerActive) {
                quitable.Context.PatternContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
                scheduleHandle = null;
                isTimerActive = false;
            }
        }
    
        public bool Inspect(MatchedEventMap matchEvent) {
            // no need to test: for timing only, if the timer expired the guardQuit stops any events from coming here
            return true;
        }
    
        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QPatternGuardScheduledEval();
            }
            // Timer callback is automatically removed when triggering
            isTimerActive = false;
            quitable.GuardQuit();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().APatternGuardScheduledEval();
            }
        }
    
        public void Accept(EventGuardVisitor visitor) {
            visitor.VisitGuard(10, scheduleSlot);
        }
    }
} // end of namespace
