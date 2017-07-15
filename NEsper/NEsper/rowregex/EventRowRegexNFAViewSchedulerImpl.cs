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
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.rowregex
{
    public class EventRowRegexNFAViewSchedulerImpl : EventRowRegexNFAViewScheduler {
        private AgentInstanceContext agentInstanceContext;
        private long scheduleSlot;
        private EPStatementHandleCallback handle;
    
        public void SetScheduleCallback(AgentInstanceContext agentInstanceContext, EventRowRegexNFAViewScheduleCallback scheduleCallback) {
            this.agentInstanceContext = agentInstanceContext;
            this.scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            var callback = new ProxyScheduleHandleCallback() {
                ProcScheduledTrigger = (extensionServicesContext) => {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().QRegExScheduledEval();
                    }
                    scheduleCallback.Triggered();
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().ARegExScheduledEval();
                    }
                };
            };
            this.handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
        }
    
        public void AddSchedule(long timeDelta) {
            agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, handle, scheduleSlot);
        }
    
        public void ChangeSchedule(long timeDelta) {
            agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
            agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, handle, scheduleSlot);
        }
    
        public void RemoveSchedule() {
            agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
        }
    }
} // end of namespace
