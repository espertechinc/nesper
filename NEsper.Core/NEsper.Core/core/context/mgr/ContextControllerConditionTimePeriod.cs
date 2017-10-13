///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerConditionTimePeriod : ContextControllerCondition
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _contextName;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly long _scheduleSlot;
        private readonly ContextDetailConditionTimePeriod _spec;
        private readonly ContextControllerConditionCallback _callback;
        private readonly ContextInternalFilterAddendum _filterAddendum;

        private EPStatementHandleCallback _scheduleHandle;

        public ContextControllerConditionTimePeriod(
            String contextName,
            AgentInstanceContext agentInstanceContext,
            long scheduleSlot,
            ContextDetailConditionTimePeriod spec,
            ContextControllerConditionCallback callback,
            ContextInternalFilterAddendum filterAddendum)
        {
            _contextName = contextName;
            _agentInstanceContext = agentInstanceContext;
            _scheduleSlot = scheduleSlot;
            _spec = spec;
            _callback = callback;
            _filterAddendum = filterAddendum;
        }

        public void Activate(
            EventBean optionalTriggerEvent,
            MatchedEventMap priorMatches,
            long timeOffset,
            bool isRecoveringResilient)
        {
            StartContextCallback(timeOffset);
        }

        public void Deactivate()
        {
            EndContextCallback();
        }

        public bool IsRunning
        {
            get { return _scheduleHandle != null; }
        }

        public bool IsImmediate
        {
            get { return _spec.IsImmediate; }
        }

        private void StartContextCallback(long timeOffset)
        {
            var scheduleCallback = new ProxyScheduleHandleCallback()
            {
                ProcScheduledTrigger = extensionServicesContext =>
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QContextScheduledEval(_agentInstanceContext.StatementContext.ContextDescriptor); }
                    _scheduleHandle = null; // terminates automatically unless scheduled again
                    _callback.RangeNotification(Collections.GetEmptyMap<String, Object>(), this, null, null, _filterAddendum);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AContextScheduledEval(); }
                }
            };
            var agentHandle =
                new EPStatementAgentInstanceHandle(
                    _agentInstanceContext.StatementContext.EpStatementHandle,
                    _agentInstanceContext.StatementContext.DefaultAgentInstanceLock, -1,
                    new StatementAgentInstanceFilterVersion(),
                    _agentInstanceContext.StatementContext.FilterFaultHandlerFactory);
            _scheduleHandle = new EPStatementHandleCallback(agentHandle, scheduleCallback);


            long timeDelta = _spec.TimePeriod.NonconstEvaluator().DeltaUseEngineTime(null, _agentInstanceContext) - timeOffset;
            _agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, _scheduleHandle, _scheduleSlot);
        }

        private void EndContextCallback()
        {
            if (_scheduleHandle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
            }
            _scheduleHandle = null;
        }

        public long? ExpectedEndTime
        {
            get
            {
                return _spec.GetExpectedEndTime(_agentInstanceContext);
            }
        }
    }
}