///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class StatementAgentInstanceFactoryUpdateResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactoryUpdateResult(
            Viewable finalView,
            AgentInstanceStopCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            IDictionary<int, SubSelectFactoryResult> subselectActivations)
            : base(
                finalView,
                stopCallback,
                agentInstanceContext,
                null,
                subselectActivations,
                null,
                null,
                null,
                null,
                Collections.GetEmptyList<StatementAgentInstancePreload>())
        {
        }
    }
} // end of namespace