///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.rowregex
{
    public class EventRowRegexNFAViewSchedulerImpl : EventRowRegexNFAViewScheduler
    {
        private AgentInstanceContext _agentInstanceContext;
        private long _scheduleSlot;
        private EPStatementHandleCallback _handle;
    
        public void SetScheduleCallback(AgentInstanceContext agentInstanceContext, EventRowRegexNFAViewScheduleCallback scheduleCallback)
        {
            _agentInstanceContext = agentInstanceContext;
            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            var callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext =>
                {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().QRegExScheduledEval();
                    }
                    scheduleCallback.Triggered();
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().ARegExScheduledEval();
                    }
                }
            };
            _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
        }
    
        public void AddSchedule(long timeDelta)
        {
            _agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, _handle, _scheduleSlot);
        }
    
        public void ChangeSchedule(long timeDelta)
        {
            _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            _agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, _handle, _scheduleSlot);
        }
    
        public void RemoveSchedule()
        {
            _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
        }
    }
} // end of namespace
