///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionCrontab : OutputConditionBase,
        OutputCondition
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "crontab";

        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = true;

        private readonly AgentInstanceContext _context;

        private readonly long _scheduleSlot;
        private readonly ScheduleSpec _scheduleSpec;
        private long? _currentReferencePoint;
        private bool _isCallbackScheduled;

        public OutputConditionCrontab(
            OutputCallback outputCallback,
            AgentInstanceContext context,
            bool isStartConditionOnCreation,
            ScheduleSpec scheduleSpec)
            : base(outputCallback)
        {
            _context = context;
            _scheduleSpec = scheduleSpec;
            _scheduleSlot = context.StatementContext.ScheduleBucket.AllocateSlot();
            if (isStartConditionOnCreation) {
                UpdateOutputCondition(0, 0);
            }
        }

        public override void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            if (_currentReferencePoint == null) {
                _currentReferencePoint = _context.StatementContext.SchedulingService.Time;
            }

            // Schedule the next callback if there is none currently scheduled
            if (!_isCallbackScheduled) {
                ScheduleCallback();
            }
        }

        public override void Terminated()
        {
            outputCallback.Invoke(true, true);
        }

        public override void StopOutputCondition()
        {
            // no action required
        }

        public override string ToString()
        {
            return GetType().Name +
                   " spec=" +
                   _scheduleSpec;
        }

        private void ScheduleCallback()
        {
            _isCallbackScheduled = true;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    _context.InstrumentationProvider.QOutputRateConditionScheduledEval();
                    _context.AuditProvider.ScheduleFire(
                        _context,
                        ScheduleObjectType.outputratelimiting,
                        NAME_AUDITPROVIDER_SCHEDULE);
                    _isCallbackScheduled = false;
                    outputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
                    ScheduleCallback();
                    _context.InstrumentationProvider.AOutputRateConditionScheduledEval();
                }
            };
            var handle = new EPStatementHandleCallbackSchedule(_context.EpStatementAgentInstanceHandle, callback);
            var schedulingService = _context.StatementContext.SchedulingService;
            var importService = _context.StatementContext.ImportServiceRuntime;
            var nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                _scheduleSpec,
                schedulingService.Time,
                importService.TimeZone,
                importService.TimeAbacus);
            _context.AuditProvider.ScheduleAdd(
                nextScheduledTime,
                _context,
                handle,
                ScheduleObjectType.outputratelimiting,
                NAME_AUDITPROVIDER_SCHEDULE);
            schedulingService.Add(nextScheduledTime, handle, _scheduleSlot);
        }
    }
} // end of namespace