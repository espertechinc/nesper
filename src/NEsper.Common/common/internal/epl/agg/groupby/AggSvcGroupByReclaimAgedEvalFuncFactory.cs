///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public interface AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        AggSvcGroupByReclaimAgedEvalFunc Make(AgentInstanceContext agentInstanceContext);
    }

    public class ProxyAggSvcGroupByReclaimAgedEvalFuncFactory : AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        public Func<AgentInstanceContext, AggSvcGroupByReclaimAgedEvalFunc> ProcMake;

        public AggSvcGroupByReclaimAgedEvalFunc Make(AgentInstanceContext agentInstanceContext)
            => ProcMake(agentInstanceContext);
    }
} // end of namespace