///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Guard implementation that keeps a timer instance and quits when the timer expired,
    ///     letting all <seealso cref="MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class TimerWithinGuard : Guard,
        ScheduleHandleCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "timer-within";

        private readonly long deltaTime;
        private readonly Quitable quitable;
        private readonly long scheduleSlot;

        private bool isTimerActive;
        private EPStatementHandleCallbackSchedule scheduleHandle;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="delta">number of millisecond to guard expiration</param>
        /// <param name="quitable">to use to indicate that the gaurd quitted</param>
        public TimerWithinGuard(
            long delta,
            Quitable quitable)
        {
            deltaTime = delta;
            this.quitable = quitable;
            scheduleSlot = quitable.Context.AgentInstanceContext.ScheduleBucket.AllocateSlot();
        }

        public void StartGuard()
        {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }

            // Start the stopwatch timer
            scheduleHandle = new EPStatementHandleCallbackSchedule(
                quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            var agentInstanceContext = quitable.Context.AgentInstanceContext;
            agentInstanceContext.AuditProvider.ScheduleAdd(
                deltaTime,
                agentInstanceContext,
                scheduleHandle,
                ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.SchedulingService.Add(deltaTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
        }

        public void StopGuard()
        {
            if (isTimerActive) {
                var agentInstanceContext = quitable.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.pattern,
                    NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
                scheduleHandle = null;
                isTimerActive = false;
            }
        }

        public bool Inspect(MatchedEventMap matchEvent)
        {
            // no need to test: for timing only, if the timer expired the guardQuit stops any events from coming here
            return true;
        }

        public void Accept(EventGuardVisitor visitor)
        {
            visitor.VisitGuard(10, scheduleSlot);
        }

        public void ScheduledTrigger()
        {
            // Timer callback is automatically removed when triggering
            var agentInstanceContext = quitable.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardScheduledEval();
            agentInstanceContext.AuditProvider.ScheduleFire(
                agentInstanceContext,
                ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            isTimerActive = false;
            quitable.GuardQuit();
            agentInstanceContext.InstrumentationProvider.APatternGuardScheduledEval();
        }
    }
} // end of namespace