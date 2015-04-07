///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnUpdateViewFactory : TableOnViewFactory, TableUpdateStrategyReceiver
    {
        public TableOnUpdateViewFactory(StatementResultService statementResultService, TableMetadata tableMetadata, EventBeanUpdateHelper updateHelper, TableUpdateStrategy tableUpdateStrategy)
        {
            StatementResultService = statementResultService;
            TableMetadata = tableMetadata;
            UpdateHelper = updateHelper;
            TableUpdateStrategy = tableUpdateStrategy;
        }
    
        public TableOnViewBase Make(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance tableState, AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor)
        {
            return new TableOnUpdateView(lookupStrategy, tableState, agentInstanceContext, TableMetadata, this);
        }

        public StatementResultService StatementResultService { get; private set; }

        public TableMetadata TableMetadata { get; private set; }

        public EventBeanUpdateHelper UpdateHelper { get; private set; }

        public TableUpdateStrategy TableUpdateStrategy { get; private set; }

        public void Update(TableUpdateStrategy updateStrategy) {
            TableUpdateStrategy = updateStrategy;
        }
    }
}
