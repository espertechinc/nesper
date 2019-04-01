///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalHelperStart
    {
        public static IDictionary<int, ExprTableEvalStrategy> StartTableAccess(
            IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses, AgentInstanceContext agentInstanceContext)
        {
            if (tableAccesses == null || tableAccesses.IsEmpty())
            {
                return Collections.EmptyMap();
            }

            bool writesToTables = agentInstanceContext.StatementContext.StatementInformationals.IsWritesToTables;
            IDictionary<int, ExprTableEvalStrategy> evals = new Dictionary<>(tableAccesses.Count, 1f);
            foreach (KeyValuePair<int, ExprTableEvalStrategyFactory> entry in tableAccesses)
            {
                Table table = entry.Value.Table;
                var provider = table.GetStateProvider(agentInstanceContext.AgentInstanceId, writesToTables);
                ExprTableEvalStrategy strategy = entry.Value.MakeStrategy(provider);
                evals.Put(entry.Key, strategy);
            }

            return evals;
        }
    }
} // end of namespace