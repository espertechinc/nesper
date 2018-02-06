///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.mgmt
{
    public interface TableService
    {
        string[] Tables { get; }
        TableExprEvaluatorContext TableExprEvaluatorContext { get; }
        TableMetadata GetTableMetadata(string tableName);
        TableStateInstance GetState(string name, int agentInstanceId);
        void RemoveTableIfFound(string tableName);
        ExprTableIdentNode GetTableIdentNode(StreamTypeService streamTypeService, string unresolvedPropertyName, string streamOrPropertyName) ;
        TableMetadata GetTableMetadataFromEventType(EventType type);
        Pair<ExprNode,IList<ExprChainedSpec>> GetTableNodeChainable(StreamTypeService streamTypeService, IList<ExprChainedSpec> chainSpec, EngineImportService engineImportService) ;
        ICollection<int> GetAgentInstanceIds(string tableName);
        TableUpdateStrategy GetTableUpdateStrategy(TableMetadata tableMetadata, EventBeanUpdateHelper updateHelper, bool isOnMerge) ;
        void AddTableUpdateStrategyReceiver(TableMetadata tableMetadata, string statementName, TableUpdateStrategyReceiver receiver, EventBeanUpdateHelper updateHelper, bool isOnMerge);
        void RemoveTableUpdateStrategyReceivers(TableMetadata tableMetadata, string statementName);
        void ValidateAddIndex(string createIndexStatementName, TableMetadata tableMetadata, string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, IndexMultiKey imk);
        void RemoveIndexReferencesStmtMayRemoveIndex(string statementName, TableMetadata tableMetadata);
        TableMetadata AddTable(string tableName, string eplExpression, string statementName, Type[] keyTypes, IDictionary<String, TableMetadataColumn> tableColumns, TableStateRowFactory tableStateRowFactory, int numberMethodAggregations, StatementContext statementContext, ObjectArrayEventType internalEventType, ObjectArrayEventType publicEventType, TableMetadataInternalEventToPublic eventToPublic, bool queryPlanLogging) ;
        TableAndLockProvider GetStateProvider(String tableName, int agentInstanceId, bool writesToTables);
    }

    public class TableServiceConstants
    {
        public const string INTERNAL_RESERVED_PROPERTY = "internal-reserved";
    }
}
