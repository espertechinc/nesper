///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateTable : StatementAgentInstanceFactory
    {
        private readonly TableMetadata _tableMetadata;
    
        public StatementAgentInstanceFactoryCreateTable(TableMetadata tableMetadata)
        {
            this._tableMetadata = tableMetadata;
        }
    
        public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            var tableState = _tableMetadata.TableStateFactory.MakeTableState(agentInstanceContext);
            var aggregationReportingService = new AggregationServiceTable(tableState);
            var finalView = new TableStateViewablePublic(_tableMetadata, tableState);
            return new StatementAgentInstanceFactoryCreateTableResult(finalView, CollectionUtil.STOP_CALLBACK_NONE, agentInstanceContext, aggregationReportingService);
        }

        public void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public void UnassignExpressions()
        {
        }
    }
}
