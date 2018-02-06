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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events.arr;
using com.espertech.esper.plugin;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableServiceImpl : TableService
    {
        internal static readonly ILog QueryPlanLog = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        private readonly IDictionary<String, TableMetadata> _tables =
            new Dictionary<string, TableMetadata>().WithNullSupport();

        private readonly TableExprEvaluatorContext _tableExprEvaluatorContext;

        private IReaderWriterLockManager _rwLockManager;

        public TableServiceImpl(IContainer container)
            : this(container.Resolve<IReaderWriterLockManager>(), container.Resolve<IThreadLocalManager>())
        {
        }

        public TableServiceImpl(
            IReaderWriterLockManager rwLockManager, 
            IThreadLocalManager threadLocalManager)
        {
            _tableExprEvaluatorContext = new TableExprEvaluatorContext(threadLocalManager);
            _rwLockManager = rwLockManager;
        }

        public void ValidateAddIndex(String createIndexStatementName, TableMetadata tableMetadata, String explicitIndexName, QueryPlanIndexItem explicitIndexDesc, IndexMultiKey imk)
        {
            tableMetadata.ValidateAddIndexAssignUpdateStrategies(createIndexStatementName, imk, explicitIndexName, explicitIndexDesc);
        }

        public TableUpdateStrategy GetTableUpdateStrategy(TableMetadata tableMetadata, EventBeanUpdateHelper updateHelper, bool isOnMerge)
        {
            return TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(tableMetadata, updateHelper, isOnMerge);
        }

        public ICollection<int> GetAgentInstanceIds(string name)
        {
            var metadata = _tables.Get(name);
            if (metadata == null)
            {
                throw new ArgumentException("Failed to find table for name '" + name + "'");
            }
            return metadata.AgentInstanceIds;
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return _tableExprEvaluatorContext; }
        }

        public TableMetadata GetTableMetadata(string tableName)
        {
            return _tables.Get(tableName);
        }

        public TableMetadata AddTable(
            string tableName, 
            string eplExpression, 
            string statementName, 
            Type[] keyTypes, 
            IDictionary<String, TableMetadataColumn> tableColumns,
            TableStateRowFactory tableStateRowFactory,
            int numberMethodAggregations,
            StatementContext statementContext,
            ObjectArrayEventType internalEventType,
            ObjectArrayEventType publicEventType,
            TableMetadataInternalEventToPublic eventToPublic,
            bool queryPlanLogging)
        {
            var metadata = new TableMetadata(
                tableName,
                eplExpression,
                statementName,
                keyTypes,
                tableColumns,
                tableStateRowFactory,
                numberMethodAggregations,
                statementContext,
                internalEventType,
                publicEventType,
                eventToPublic,
                queryPlanLogging);

            // determine table state factory
            TableStateFactory tableStateFactory;
            if (keyTypes.Length == 0)
            { // ungrouped
                tableStateFactory = new ProxyTableStateFactory
                {
                    ProcMakeTableState = agentInstanceContext => new TableStateInstanceUngroupedImpl(
                        metadata, agentInstanceContext, _rwLockManager),
                };
            }
            else
            {
                tableStateFactory = new ProxyTableStateFactory
                {
                    ProcMakeTableState = agentInstanceContext => new TableStateInstanceGroupedImpl(
                        metadata, agentInstanceContext, _rwLockManager),
                };
            }
            metadata.TableStateFactory = tableStateFactory;

            _tables.Put(tableName, metadata);
            return metadata;
        }

        public void RemoveTableIfFound(string tableName)
        {
            var metadata = _tables.Delete(tableName);
            if (metadata != null)
            {
                metadata.ClearTableInstances();
            }
        }

        public TableStateInstance GetState(string name, int agentInstanceId)
        {
            return AssertGetState(name, agentInstanceId);
        }

        private TableStateInstance AssertGetState(string name, int agentInstanceId)
        {
            var metadata = _tables.Get(name);
            if (metadata == null)
            {
                throw new ArgumentException("Failed to find table for name '" + name + "'");
            }
            return metadata.GetState(agentInstanceId);
        }

        public TableMetadata GetTableMetadataFromEventType(EventType type)
        {
            var tableName = TableServiceUtil.GetTableNameFromEventType(type);
            if (tableName == null)
            {
                return null;
            }
            return _tables.Get(tableName);
        }

        public Pair<ExprNode, IList<ExprChainedSpec>> GetTableNodeChainable(
            StreamTypeService streamTypeService,
            IList<ExprChainedSpec> chainSpec,
            EngineImportService engineImportService)
        {
            chainSpec = new List<ExprChainedSpec>(chainSpec);

            var unresolvedPropertyName = chainSpec[0].Name;
            var col = FindTableColumnMayByPrefixed(streamTypeService, unresolvedPropertyName);
            if (col == null)
            {
                return null;
            }
            var pair = col.Pair;
            if (pair.Column is TableMetadataColumnAggregation)
            {
                var agg = (TableMetadataColumnAggregation)pair.Column;

                if (chainSpec.Count > 1)
                {
                    var candidateAccessor = chainSpec[1].Name;
                    var exprNode = (ExprAggregateNodeBase)ASTAggregationHelper.TryResolveAsAggregation(engineImportService, false, candidateAccessor, new LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory>(), streamTypeService.EngineURIQualifier);
                    if (exprNode != null)
                    {
                        var identNode = new ExprTableIdentNodeSubpropAccessor(pair.StreamNum, col.OptionalStreamName, agg, exprNode);
                        exprNode.AddChildNodes(chainSpec[1].Parameters);
                        chainSpec.RemoveAt(0);
                        chainSpec.RemoveAt(0);
                        return new Pair<ExprNode, IList<ExprChainedSpec>>(identNode, chainSpec);
                    }
                }

                var node = new ExprTableIdentNode(null, unresolvedPropertyName);
                var eval = ExprTableEvalStrategyFactory.GetTableAccessEvalStrategy(node, pair.TableMetadata.TableName, pair.StreamNum, agg);
                node.Eval = eval;
                chainSpec.RemoveAt(0);
                return new Pair<ExprNode, IList<ExprChainedSpec>>(node, chainSpec);
            }
            return null;
        }

        public ExprTableIdentNode GetTableIdentNode(StreamTypeService streamTypeService, string unresolvedPropertyName, string streamOrPropertyName)
        {
            var propertyPrefixed = unresolvedPropertyName;
            if (streamOrPropertyName != null)
            {
                propertyPrefixed = streamOrPropertyName + "." + unresolvedPropertyName;
            }
            var col = FindTableColumnMayByPrefixed(streamTypeService, propertyPrefixed);
            if (col == null)
            {
                return null;
            }
            var pair = col.Pair;
            if (pair.Column is TableMetadataColumnAggregation)
            {
                var agg = (TableMetadataColumnAggregation)pair.Column;
                var node = new ExprTableIdentNode(streamOrPropertyName, unresolvedPropertyName);
                var eval = ExprTableEvalStrategyFactory.GetTableAccessEvalStrategy(node, pair.TableMetadata.TableName, pair.StreamNum, agg);
                node.Eval = eval;
                return node;
            }
            return null;
        }

        public void AddTableUpdateStrategyReceiver(TableMetadata tableMetadata, string statementName, TableUpdateStrategyReceiver receiver, EventBeanUpdateHelper updateHelper, bool isOnMerge)
        {
            tableMetadata.AddTableUpdateStrategyReceiver(statementName, receiver, updateHelper, isOnMerge);
        }

        public void RemoveTableUpdateStrategyReceivers(TableMetadata tableMetadata, string statementName)
        {
            tableMetadata.RemoveTableUpdateStrategyReceivers(statementName);
        }

        public string[] Tables
        {
            get { return CollectionUtil.ToArray(_tables.Keys); }
        }

        public TableAndLockProvider GetStateProvider(String tableName, int agentInstanceId, bool writesToTables)
        {
            TableStateInstance instance = AssertGetState(tableName, agentInstanceId);
            ILockable @lock = writesToTables ? instance.TableLevelRWLock.WriteLock : instance.TableLevelRWLock.ReadLock;
            if (instance is TableStateInstanceGrouped)
            {
                return new TableAndLockProviderGroupedImpl(new TableAndLockGrouped(@lock, (TableStateInstanceGrouped)instance));
            }
            else
            {
                return new TableAndLockProviderUngroupedImpl(new TableAndLockUngrouped(@lock, (TableStateInstanceUngrouped)instance));
            }
        }

        private StreamTableColWStreamName FindTableColumnMayByPrefixed(StreamTypeService streamTypeService, string streamAndPropName)
        {
            var indexDot = streamAndPropName.IndexOf('.');
            if (indexDot == -1)
            {
                var pair = FindTableColumnAcrossStreams(streamTypeService, streamAndPropName);
                if (pair != null)
                {
                    return new StreamTableColWStreamName(pair, null);
                }
            }
            else
            {
                var streamName = streamAndPropName.Substring(0, indexDot);
                var colName = streamAndPropName.Substring(indexDot + 1);
                var streamNum = streamTypeService.GetStreamNumForStreamName(streamName);
                if (streamNum == -1)
                {
                    return null;
                }
                var pair = FindTableColumnForType(streamNum, streamTypeService.EventTypes[streamNum], colName);
                if (pair != null)
                {
                    return new StreamTableColWStreamName(pair, streamName);
                }
            }
            return null;
        }

        public void RemoveIndexReferencesStmtMayRemoveIndex(string statementName, TableMetadata tableMetadata)
        {
            tableMetadata.RemoveIndexReferencesStatement(statementName);
        }

        private StreamTableColPair FindTableColumnAcrossStreams(StreamTypeService streamTypeService, string columnName)
        {
            StreamTableColPair found = null;
            for (var i = 0; i < streamTypeService.EventTypes.Length; i++)
            {
                var type = streamTypeService.EventTypes[i];
                var pair = FindTableColumnForType(i, type, columnName);
                if (pair == null)
                {
                    continue;
                }
                if (found != null)
                {
                    if (streamTypeService.IsStreamZeroUnambigous && found.StreamNum == 0)
                    {
                        continue;
                    }
                    throw new ExprValidationException("Ambiguous table column '" + columnName + "' should be prefixed by a stream name");
                }
                found = pair;
            }
            return found;
        }

        private StreamTableColPair FindTableColumnForType(int streamNum, EventType type, string columnName)
        {
            var tableMetadata = GetTableMetadataFromEventType(type);
            if (tableMetadata != null)
            {
                var column = tableMetadata.TableColumns.Get(columnName);
                if (column != null)
                {
                    return new StreamTableColPair(streamNum, column, tableMetadata);
                }
            }
            return null;
        }

        internal class StreamTableColPair
        {
            internal StreamTableColPair(int streamNum, TableMetadataColumn column, TableMetadata tableMetadata)
            {
                StreamNum = streamNum;
                Column = column;
                TableMetadata = tableMetadata;
            }

            public int StreamNum { get; private set; }

            public TableMetadataColumn Column { get; private set; }

            public TableMetadata TableMetadata { get; private set; }
        }

        internal class StreamTableColWStreamName
        {
            internal StreamTableColWStreamName(StreamTableColPair pair, string optionalStreamName)
            {
                Pair = pair;
                OptionalStreamName = optionalStreamName;
            }

            public StreamTableColPair Pair { get; private set; }

            public string OptionalStreamName { get; private set; }
        }
    }
}
