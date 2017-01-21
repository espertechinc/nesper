///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// An output strategy that handles routing (insert-into) and stream selection.
    /// </summary>
    public class OutputStrategyPostProcessFactory
    {
        public OutputStrategyPostProcessFactory(
            bool route,
            SelectClauseStreamSelectorEnum? insertIntoStreamSelector,
            SelectClauseStreamSelectorEnum selectStreamDirEnum,
            InternalEventRouter internalEventRouter,
            EPStatementHandle epStatementHandle,
            bool addToFront,
            TableService tableService,
            string tableName)
        {
            IsRoute = route;
            InsertIntoStreamSelector = insertIntoStreamSelector;
            SelectStreamDirEnum = selectStreamDirEnum;
            InternalEventRouter = internalEventRouter;
            EpStatementHandle = epStatementHandle;
            IsAddToFront = addToFront;
            TableService = tableService;
            TableName = tableName;
        }

        public bool IsRoute { get; private set; }

        public SelectClauseStreamSelectorEnum? InsertIntoStreamSelector { get; private set; }

        public SelectClauseStreamSelectorEnum SelectStreamDirEnum { get; private set; }

        public InternalEventRouter InternalEventRouter { get; private set; }

        public EPStatementHandle EpStatementHandle { get; private set; }

        public bool IsAddToFront { get; private set; }

        public TableService TableService { get; private set; }

        public string TableName { get; private set; }

        public OutputStrategyPostProcess Make(AgentInstanceContext agentInstanceContext)
        {
            TableStateInstance tableStateInstance = null;
            if (TableName != null)
            {
                tableStateInstance = TableService.GetState(TableName, agentInstanceContext.AgentInstanceId);
            }
            return new OutputStrategyPostProcess(this, agentInstanceContext, tableStateInstance);
        }
    }
}