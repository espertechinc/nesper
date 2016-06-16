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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition that is satisfied at the end of every time interval of a given length.
    /// </summary>
    public sealed class OutputConditionTime
        : OutputConditionBase
        , OutputCondition
    {
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = true;

        private readonly AgentInstanceContext _context;
        private readonly OutputConditionTimeFactory _parent;

        private readonly ScheduleSlot _scheduleSlot;
        private long? _currentReferencePoint;
        private bool _isCallbackScheduled;
        private EPStatementHandleCallback _handle;
        private long _currentScheduledTime;

        public OutputConditionTime(
            OutputCallback outputCallback,
            AgentInstanceContext context,
            OutputConditionTimeFactory outputConditionTimeFactory,
            bool isStartConditionOnCreation)
            : base(outputCallback)
        {
            _context = context;
            _parent = outputConditionTimeFactory;
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

            // If we pull the interval from a variable, then we may need to reschedule
            if (_parent.TimePeriod.HasVariable)
            {
                var now = _context.StatementContext.SchedulingService.Time;
                var delta = _parent.TimePeriod.NonconstEvaluator().DeltaMillisecondsAddWReference(
                    now, _currentReferencePoint.Value, null, true, _context);
                if (delta.Delta != _currentScheduledTime)
                {
                    if (_isCallbackScheduled)
                    {
                        // reschedule
                        _context.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                        ScheduleCallback();
                    }
                }
            }

            // Schedule the next callback if there is none currently scheduled
            if (!_isCallbackScheduled)
            {
                ScheduleCallback();
            }
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        private void ScheduleCallback()
        {
            _isCallbackScheduled = true;
            long current = _context.StatementContext.SchedulingService.Time;
            ExprTimePeriodEvalDeltaResult delta = _parent.TimePeriod.NonconstEvaluator().DeltaMillisecondsAddWReference(
                current, _currentReferencePoint.Value, null, true, _context);
            long afterMSec = delta.Delta;
            _currentReferencePoint = delta.LastReference;
            _currentScheduledTime = afterMSec;

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".scheduleCallback Scheduled new callback for " +
                    " afterMsec=" + afterMSec +
                    " now=" + current +
                    " currentReferencePoint=" + _currentReferencePoint);
            }

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
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

            _handle = new EPStatementHandleCallback(_context.EpStatementAgentInstanceHandle, callback);
            _context.StatementContext.SchedulingService.Add(afterMSec, _handle, _scheduleSlot);
            _context.AddTerminationCallback(new ProxyStopCallback(Stop));
        }

        public override void Stop()
        {
            if (_handle != null)
            {
                _context.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
