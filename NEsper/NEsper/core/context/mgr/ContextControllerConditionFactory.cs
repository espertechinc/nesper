///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerConditionFactory
    {

        public static ContextControllerCondition GetEndpoint(
            String contextName,
            EPServicesContext servicesContext,
            AgentInstanceContext agentInstanceContext,
            ContextDetailCondition endpoint,
            ContextControllerConditionCallback callback,
            ContextInternalFilterAddendum filterAddendum,
            bool isStartEndpoint,
            int nestingLevel,
            int pathId,
            int subpathId)
        {
            if (endpoint is ContextDetailConditionCrontab)
            {
                var crontab = (ContextDetailConditionCrontab) endpoint;
                var scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
                return new ContextControllerConditionCrontab(
                    agentInstanceContext.StatementContext, scheduleSlot, crontab, callback, filterAddendum);
            }
            else if (endpoint is ContextDetailConditionFilter)
            {
                var filter = (ContextDetailConditionFilter) endpoint;
                return new ContextControllerConditionFilter(
                    servicesContext, agentInstanceContext, filter, callback, filterAddendum);
            }
            else if (endpoint is ContextDetailConditionPattern)
            {
                var key = new ContextStatePathKey(nestingLevel, pathId, subpathId);
                var pattern = (ContextDetailConditionPattern) endpoint;
                return new ContextControllerConditionPattern(
                    servicesContext, agentInstanceContext, pattern, callback, filterAddendum, isStartEndpoint, key);
            }
            else if (endpoint is ContextDetailConditionTimePeriod)
            {
                var timePeriod = (ContextDetailConditionTimePeriod) endpoint;
                var scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
                return new ContextControllerConditionTimePeriod(
                    contextName, agentInstanceContext, scheduleSlot, timePeriod, callback, filterAddendum);
            }
            else if (endpoint is ContextDetailConditionImmediate)
            {
                return new ContextControllerConditionImmediate();
            }
            throw new IllegalStateException("Unrecognized context range endpoint " + endpoint.GetType());
        }
    }
}
