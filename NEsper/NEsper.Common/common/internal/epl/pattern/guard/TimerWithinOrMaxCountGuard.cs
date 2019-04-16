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
    ///     and also keeps a count of the number of matches so far, checking both count and timer,
    ///     letting all <seealso cref="MatchedEventMap" /> instances pass until then.
    /// </summary>
    public class TimerWithinOrMaxCountGuard : Guard,
        ScheduleHandleCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "timer-within-max";
        private readonly long deltaTime;
        private readonly int numCountTo;
        private readonly Quitable quitable;
        private readonly long scheduleSlot;

        private int counter;
        private bool isTimerActive;
        private EPStatementHandleCallbackSchedule scheduleHandle;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="deltaTime">number of millisecond to guard expiration</param>
        /// <param name="numCountTo">max number of counts</param>
        /// <param name="quitable">to use to indicate that the gaurd quitted</param>
        public TimerWithinOrMaxCountGuard(
            long deltaTime,
            int numCountTo,
            Quitable quitable)
        {
            this.deltaTime = deltaTime;
            this.numCountTo = numCountTo;
            this.quitable = quitable;
            scheduleSlot = quitable.Context.AgentInstanceContext.ScheduleBucket.AllocateSlot();
        }

        public void StartGuard()
        {
            if (isTimerActive) {
                throw new IllegalStateException("Timer already active");
            }

            scheduleHandle = new EPStatementHandleCallbackSchedule(
                quitable.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            var agentInstanceContext = quitable.Context.AgentInstanceContext;
            agentInstanceContext.AuditProvider.ScheduleAdd(
                deltaTime, agentInstanceContext, scheduleHandle, ScheduleObjectType.pattern,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.SchedulingService.Add(deltaTime, scheduleHandle, scheduleSlot);
            isTimerActive = true;
            counter = 0;
        }

        public bool Inspect(MatchedEventMap matchEvent)
        {
            counter++;
            if (counter > numCountTo) {
                quitable.GuardQuit();
                DeactivateTimer();
                return false;
            }

            return true;
        }

        public void StopGuard()
        {
            if (isTimerActive) {
                DeactivateTimer();
            }
        }

        public void Accept(EventGuardVisitor visitor)
        {
            visitor.VisitGuard(20, scheduleSlot);
        }

        public void ScheduledTrigger()
        {
            var agentInstanceContext = quitable.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardScheduledEval();
            agentInstanceContext.AuditProvider.ScheduleFire(
                agentInstanceContext, ScheduleObjectType.pattern, NAME_AUDITPROVIDER_SCHEDULE);
            // Timer callback is automatically removed when triggering
            isTimerActive = false;
            quitable.GuardQuit();
            agentInstanceContext.InstrumentationProvider.APatternGuardScheduledEval();
        }

        private void DeactivateTimer()
        {
            if (scheduleHandle != null) {
                var agentInstanceContext = quitable.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext, scheduleHandle, ScheduleObjectType.pattern, NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }

            scheduleHandle = null;
            isTimerActive = false;
        }
    }
} // end of namespace