///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableMetadata
    {
        private readonly string _tableName;
        private readonly string _eplExpression;
        private readonly string _statementName;
        private readonly StatementResourceService _createTableResources;
        private readonly ObjectArrayEventType _publicEventType;
        private readonly TableMetadataInternalEventToPublic _eventToPublic;
        private readonly bool _queryPlanLogging;
    
        private readonly IDictionary<String, IList<TableUpdateStrategyReceiverDesc>> _stmtNameToUpdateStrategyReceivers = new Dictionary<string, IList<TableUpdateStrategyReceiverDesc>>();
        private readonly EventTableIndexMetadata _eventTableIndexMetadataRepo = new EventTableIndexMetadata();

        private TableMetadataContext _tableMetadataContext;
        private readonly TableRowKeyFactory _tableRowKeyFactory;
    
        public TableMetadata(string tableName, string eplExpression, string statementName, Type[] keyTypes, IDictionary<String, TableMetadataColumn> tableColumns, TableStateRowFactory rowFactory, int numberMethodAggregations, StatementResourceService createTableResources, string contextName, ObjectArrayEventType internalEventType, ObjectArrayEventType publicEventType, TableMetadataInternalEventToPublic eventToPublic, bool queryPlanLogging, string createTableStatementName)
        {
            _tableName = tableName;
            _eplExpression = eplExpression;
            _statementName = statementName;
            KeyTypes = keyTypes;
            TableColumns = tableColumns;
            RowFactory = rowFactory;
            NumberMethodAggregations = numberMethodAggregations;
            _createTableResources = createTableResources;
            ContextName = contextName;
            InternalEventType = internalEventType;
            _publicEventType = publicEventType;
            _eventToPublic = eventToPublic;
            _queryPlanLogging = queryPlanLogging;
    
            if (keyTypes.Length > 0) {
                var pair = TableServiceUtil.GetIndexMultikeyForKeys(tableColumns, internalEventType);
                _eventTableIndexMetadataRepo.AddIndex(true, pair.Second, tableName, createTableStatementName, true);
                _tableRowKeyFactory = new TableRowKeyFactory(pair.First);
            }
        }

        public Type[] KeyTypes { get; private set; }

        public TableStateFactory TableStateFactory { get; set; }

        public IDictionary<string, TableMetadataColumn> TableColumns { get; private set; }

        public TableStateRowFactory RowFactory { get; private set; }

        public int NumberMethodAggregations { get; private set; }

        public string ContextName { get; private set; }

        public ObjectArrayEventType InternalEventType { get; private set; }

        public bool IsQueryPlanLogging
        {
            get { return _queryPlanLogging; }
        }

        public ISet<string> UniqueKeyProps
        {
            get
            {
                ISet<string> keys = new LinkedHashSet<string>();
                foreach (var entry in TableColumns)
                {
                    if (entry.Value.IsKey)
                    {
                        keys.Add(entry.Key);
                    }
                }
                return keys;
            }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public EventTableIndexMetadata EventTableIndexMetadataRepo
        {
            get { return _eventTableIndexMetadataRepo; }
        }

        public EventBean GetPublicEventBean(EventBean @event, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return _eventToPublic.Convert(@event, eventsPerStream, isNewData, context);
        }

        public EventType PublicEventType
        {
            get { return _publicEventType; }
        }

        public TableMetadataInternalEventToPublic EventToPublic
        {
            get { return _eventToPublic; }
        }

        public void ValidateAddIndexAssignUpdateStrategies(string createIndexStatementName, IndexMultiKey imk, string indexName)
        {
            // add index - for now
            _eventTableIndexMetadataRepo.AddIndex(false, imk, indexName, createIndexStatementName, true);
    
            // validate strategies, rollback if required
            foreach (var stmtEntry in _stmtNameToUpdateStrategyReceivers) {
                foreach (var strategyReceiver in stmtEntry.Value) {
                    try {
                        TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(this, strategyReceiver.UpdateHelper, strategyReceiver.IsOnMerge);
                    }
                    catch (ExprValidationException ex) {
                        _eventTableIndexMetadataRepo.RemoveIndex(imk);
                        throw new ExprValidationException("Failed to validate statement '" + stmtEntry.Key + "' as a recipient of the proposed index: " + ex.Message);
                    }
                }
            }
    
            // assign new strategies
            foreach (var stmtEntry in _stmtNameToUpdateStrategyReceivers) {
                foreach (var strategyReceiver in stmtEntry.Value) {
                    var strategy = TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(this, strategyReceiver.UpdateHelper, strategyReceiver.IsOnMerge);
                    strategyReceiver.Receiver.Update(strategy);
                }
            }
        }
    
        public void AddTableUpdateStrategyReceiver(string statementName, TableUpdateStrategyReceiver receiver, EventBeanUpdateHelper updateHelper, bool onMerge) {
            var receivers = _stmtNameToUpdateStrategyReceivers.Get(statementName);
            if (receivers == null) {
                receivers = new List<TableUpdateStrategyReceiverDesc>(2);
                _stmtNameToUpdateStrategyReceivers.Put(statementName, receivers);
            }
            receivers.Add(new TableUpdateStrategyReceiverDesc(receiver, updateHelper, onMerge));
        }
    
        public void RemoveTableUpdateStrategyReceivers(string statementName) {
            _stmtNameToUpdateStrategyReceivers.Remove(statementName);
        }
    
        public void AddIndexReference(string indexName, string statementName) {
            _eventTableIndexMetadataRepo.AddIndexReference(indexName, statementName);
        }
    
        public void RemoveIndexReferencesStatement(string statementName) {
            var indexesDereferenced = _eventTableIndexMetadataRepo.GetRemoveRefIndexesDereferenced(statementName);
            foreach (var indexDereferenced in indexesDereferenced) {
                // remove tables
                foreach (var agentInstanceId in AgentInstanceIds) {
                    var state = GetState(agentInstanceId);
                    if (state != null) {
                        var mk = state.IndexRepository.GetIndexByName(indexDereferenced);
                        if (mk != null) {
                            state.IndexRepository.RemoveIndex(mk);
                        }
                    }
                }
            }
        }
    
        public TableStateInstance GetState(int agentInstanceId)
        {
            StatementResourceHolder holder = null;
            if (ContextName == null || agentInstanceId == 0) {
                holder = _createTableResources.ResourcesZero;
            }
            else {
                if (_createTableResources.ResourcesNonZero != null) {
                    holder = _createTableResources.ResourcesNonZero.Get(agentInstanceId);
                }
            }
            if (holder == null) {
                return null;
            }
    
            var aggsvc = (AggregationServiceTable) holder.AggegationService;
            return aggsvc.TableState;
        }

        public ICollection<int> AgentInstanceIds
        {
            get
            {
                if (ContextName == null)
                {
                    return Collections.SingletonList<int>(-1);
                }
                if (_createTableResources.ResourcesNonZero != null)
                {
                    return _createTableResources.ResourcesNonZero.Keys;
                }
                return Collections.SingletonList(-1);
            }
        }

        public string[][] UniqueIndexes
        {
            get { return _eventTableIndexMetadataRepo.UniqueIndexProps; }
        }

        public void SetTableMetadataContext(TableMetadataContext tableMetadataContext)
        {
            _tableMetadataContext = tableMetadataContext;
        }

        public TableMetadataContext TableMetadataContext
        {
            get { return _tableMetadataContext; }
        }

        public TableRowKeyFactory TableRowKeyFactory
        {
            get { return _tableRowKeyFactory; }
        }

        public void ClearTableInstances()
        {
            foreach (var agentInstanceId in AgentInstanceIds)
            {
                var state = GetState(agentInstanceId);
                if (state != null) {
                    state.ClearEvents();
                }
            }
        }

        public string EplExpression
        {
            get { return _eplExpression; }
        }

        public string StatementName
        {
            get { return _statementName; }
        }
    }
}
