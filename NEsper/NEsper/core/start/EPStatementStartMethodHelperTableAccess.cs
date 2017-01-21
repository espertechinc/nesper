///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodHelperTableAccess
    {
        public static IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> AttachTableAccess(
            EPServicesContext services,
            AgentInstanceContext agentInstanceContext,
            ExprTableAccessNode[] tableNodes)
        {
            if (tableNodes == null || tableNodes.Length == 0)
            {
                return Collections.GetEmptyMap<ExprTableAccessNode, ExprTableAccessEvalStrategy>();
            }

            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> strategies =
                new Dictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy>();
            foreach (var tableNode in tableNodes)
            {
                var writesToTables = agentInstanceContext.StatementContext.IsWritesToTables;
                TableAndLockProvider provider = services.TableService.GetStateProvider(tableNode.TableName, agentInstanceContext.AgentInstanceId, writesToTables);
                TableMetadata tableMetadata = services.TableService.GetTableMetadata(tableNode.TableName);
                ExprTableAccessEvalStrategy strategy = ExprTableEvalStrategyFactory.GetTableAccessEvalStrategy(tableNode, provider, tableMetadata);
                strategies.Put(tableNode, strategy);
            }

            return strategies;
        }
    }
}
