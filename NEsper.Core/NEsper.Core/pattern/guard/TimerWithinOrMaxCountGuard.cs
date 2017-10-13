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
    /// and also keeps a count of the number of matches so far, checking both count and timer,
    /// letting all <seealso cref="com.espertech.esper.pattern.MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class TimerWithinOrMaxCountGuard : Guard, ScheduleHandleCallback {
        private readonly long deltaTime;
        private readonly int numCountTo;
        private readonly Quitable quitable;
        private readonly long scheduleSlot;
    
        private int counter;
        private bool isTimerActive;
        private EPStatementHandleCallback scheduleHandle;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="deltaTime">- number of millisecond to guard expiration</param>
        /// <param name="numCountTo">- max number of counts</param>
        /// <param name="quitable">- to use to indicate that the gaurd quitted</param>
        public TimerWithinOrMaxCountGuard(long deltaTime, int numCountTo, Quitable quitable) {
            this.deltaTime = deltaTime;
            this.numCountTo = numCountTo;
            this.quitable = quitable;
            this.scheduleSlot = quitable.Context.PatternContext.ScheduleBucket.AllocateSlot();
        }
    
        public void StartGuard() {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }
    
            scheduleHandle = new EPStatementHandleCallback(quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            quitable.Context.PatternContext.SchedulingService.Add(deltaTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
            counter = 0;
        }
    
        public bool Inspect(MatchedEventMap matchEvent) {
            counter++;
            if (counter > numCountTo) {
                quitable.GuardQuit();
                DeactivateTimer();
                return false;
            }
            return true;
        }
    
        public void StopGuard() {
            if (isTimerActive) {
                DeactivateTimer();
            }
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
            visitor.VisitGuard(20, scheduleSlot);
        }
    
        private void DeactivateTimer() {
            if (scheduleHandle != null) {
                quitable.Context.PatternContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }
            scheduleHandle = null;
            isTimerActive = false;
        }
    }
} // end of namespace
