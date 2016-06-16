///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionCrontab
        : OutputConditionBase
        , OutputCondition
    {
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = true;

        private readonly AgentInstanceContext _context;
        private readonly OutputConditionCrontabFactory _factory;

        private readonly ScheduleSlot _scheduleSlot;
        private long? _currentReferencePoint;
        private bool _isCallbackScheduled;

        public OutputConditionCrontab(OutputCallback outputCallback, AgentInstanceContext context, OutputConditionCrontabFactory factory, bool isStartConditionOnCreation)
            : base(outputCallback)
        {
            _context = context;
            _factory = factory;
            _scheduleSlot = context.StatementContext.ScheduleBucket.AllocateSlot();
            if (isStartConditionOnCreation)
            {
                UpdateOutputCondition(0, 0);
            }
        }

        public override void UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".updateOutputCondition, " +
                    "  newEventsCount==" + newEventsCount +
                    "  oldEventsCount==" + oldEventsCount);
            }

            if (_currentReferencePoint == null)
            {
                _currentReferencePoint = _context.StatementContext.SchedulingService.Time;
            }

            // Schedule the next callback if there is none currently scheduled
            if (!_isCallbackScheduled)
            {
                ScheduleCallback();
            }
        }

        public override String ToString()
        {
            return GetType().FullName +
                   " spec=" + _factory.ScheduleSpec;
        }

        private void ScheduleCallback()
        {
            _isCallbackScheduled = true;
            var current = _context.StatementContext.SchedulingService.Time;

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".scheduleCallback Scheduled new callback for " +
                    " now=" + current +
                    " currentReferencePoint=" + _currentReferencePoint +
                    " spec=" + _factory.ScheduleSpec);
            }

            var callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QOutputRateConditionScheduledEval(),
                    i => i.AOutputRateConditionScheduledEval(),
                    () =>
                    {
                        _isCallbackScheduled = false;
                        OutputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
                        ScheduleCallback();
                    })
            };
            var handle = new EPStatementHandleCallback(_context.EpStatementAgentInstanceHandle, callback);
            var schedulingService = _context.StatementContext.SchedulingService;
            var nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                _factory.ScheduleSpec, schedulingService.Time,
                _context.StatementContext.MethodResolutionService.EngineImportService.TimeZone);
            schedulingService.Add(nextScheduledTime, handle, _scheduleSlot);
        }

        public override void Terminated()
        {
            OutputCallback.Invoke(true, true);
        }

        public override void Stop()
        {
            // no action required
        }

        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
