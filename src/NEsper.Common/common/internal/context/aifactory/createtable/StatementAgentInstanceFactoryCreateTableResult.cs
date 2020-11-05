///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createtable
{
    public class StatementAgentInstanceFactoryCreateTableResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactoryCreateTableResult(
            Viewable finalView,
            AgentInstanceMgmtCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            TableInstance tableInstance)
            : base(
                finalView,
                stopCallback,
                agentInstanceContext,
                null,
                Collections.GetEmptyMap<int, SubSelectFactoryResult>(),
                null,
                null,
                null,
                Collections.GetEmptyMap<int, ExprTableEvalStrategy>(),
                Collections.GetEmptyList<StatementAgentInstancePreload>(),
                null)
        {
            TableInstance = tableInstance;
        }

        public TableInstance TableInstance { get; }
    }
} // end of namespace