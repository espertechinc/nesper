///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnSelectViewFactory : TableOnViewFactory
    {
        private readonly TableMetadata _tableMetadata;
        private readonly bool _deleteAndSelect;
    
        public TableOnSelectViewFactory(TableMetadata tableMetadata, InternalEventRouter internalEventRouter, EPStatementHandle statementHandle, EventBeanReader eventBeanReader, bool distinct, StatementResultService statementResultService, InternalEventRouteDest internalEventRouteDest, bool deleteAndSelect)
        {
            _tableMetadata = tableMetadata;
            InternalEventRouter = internalEventRouter;
            StatementHandle = statementHandle;
            EventBeanReader = eventBeanReader;
            IsDistinct = distinct;
            StatementResultService = statementResultService;
            InternalEventRouteDest = internalEventRouteDest;
            _deleteAndSelect = deleteAndSelect;
        }
    
        public TableOnViewBase Make(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance tableState, AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor)
        {
            bool audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.StatementContext.Annotations) != null;
            return new TableOnSelectView(lookupStrategy, tableState, agentInstanceContext, _tableMetadata, this, resultSetProcessor, audit, _deleteAndSelect);
        }

        public InternalEventRouter InternalEventRouter { get; private set; }

        public EPStatementHandle StatementHandle { get; private set; }

        public EventBeanReader EventBeanReader { get; private set; }

        public bool IsDistinct { get; private set; }

        public StatementResultService StatementResultService { get; private set; }

        public InternalEventRouteDest InternalEventRouteDest { get; private set; }
    }
}
