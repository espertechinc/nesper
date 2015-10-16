///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerConditionCrontab : ContextControllerCondition
    {
        private readonly StatementContext _statementContext;
        private readonly ScheduleSlot _scheduleSlot;
        private readonly ContextDetailConditionCrontab _spec;
        private readonly ContextControllerConditionCallback _callback;
        private readonly ContextInternalFilterAddendum _filterAddendum;

        private EPStatementHandleCallback _scheduleHandle;

        public ContextControllerConditionCrontab(StatementContext statementContext, ScheduleSlot scheduleSlot, ContextDetailConditionCrontab spec, ContextControllerConditionCallback callback, ContextInternalFilterAddendum filterAddendum)
        {
            _statementContext = statementContext;
            _scheduleSlot = scheduleSlot;
            _spec = spec;
            _callback = callback;
            _filterAddendum = filterAddendum;
        }

        public void Activate(EventBean optionalTriggerEvent, MatchedEventMap priorMatches, long timeOffset, bool isRecoveringResilient)
        {
            StartContextCallback();
        }

        public void Deactivate()
        {
            EndContextCallback();
        }

        public bool IsRunning
        {
            get { return _scheduleHandle != null; }
        }

        private void StartContextCallback()
        {
            ScheduleHandleCallback scheduleCallback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QContextScheduledEval(_statementContext.ContextDescriptor),
                    i => i.AContextScheduledEval(),
                    () =>
                    {
                        _scheduleHandle = null; // terminates automatically unless scheduled again
                        _callback.RangeNotification(
                            Collections.GetEmptyMap<String, Object>(), this, null, null, _filterAddendum);
                    })
            };

            var agentHandle = new EPStatementAgentInstanceHandle(_statementContext.EpStatementHandle, _statementContext.DefaultAgentInstanceLock, -1, new StatementAgentInstanceFilterVersion());
            _scheduleHandle = new EPStatementHandleCallback(agentHandle, scheduleCallback);
            var schedulingService = _statementContext.SchedulingService;
            var nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(_spec.Schedule, schedulingService.Time, _statementContext.MethodResolutionService.EngineImportService.TimeZone);
            _statementContext.SchedulingService.Add(nextScheduledTime, _scheduleHandle, _scheduleSlot);
        }

        private void EndContextCallback()
        {
            if (_scheduleHandle != null)
            {
                _statementContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
            }
            _scheduleHandle = null;
        }

        public long? ExpectedEndTime
        {
            get { return ScheduleComputeHelper.ComputeNextOccurance(_spec.Schedule, _statementContext.TimeProvider.Time, _statementContext.MethodResolutionService.EngineImportService.TimeZone); }
        }

        public bool IsImmediate
        {
            get { return _spec.IsImmediate; }
        }
    }
}
