///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.agg.service
{
    public interface AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        AggSvcGroupByReclaimAgedEvalFunc Make(AgentInstanceContext agentInstanceContext);
    }

    public class ProxyAggSvcGroupByReclaimAgedEvalFuncFactory : AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        public Func<AgentInstanceContext, AggSvcGroupByReclaimAgedEvalFunc> ProcMake { get; set; }

        public AggSvcGroupByReclaimAgedEvalFunc Make(AgentInstanceContext agentInstanceContext)
        {
            return ProcMake(agentInstanceContext);
        }
    }
}
