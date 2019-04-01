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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
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
	    private readonly Type[] _keyTypes;
	    private readonly IDictionary<string, TableMetadataColumn> _tableColumns;
	    private readonly TableStateRowFactory _rowFactory;
	    private readonly int _numberMethodAggregations;
	    private readonly StatementContext _statementContextCreateTable;
	    private readonly ObjectArrayEventType _internalEventType;
	    private readonly ObjectArrayEventType _publicEventType;
	    private readonly TableMetadataInternalEventToPublic _eventToPublic;
	    private readonly bool _queryPlanLogging;

	    private readonly IDictionary<string, IList<TableUpdateStrategyReceiverDesc>> _stmtNameToUpdateStrategyReceivers = new Dictionary<string, IList<TableUpdateStrategyReceiverDesc>>();
	    private readonly EventTableIndexMetadata _eventTableIndexMetadataRepo = new EventTableIndexMetadata();

	    private TableStateFactory _tableStateFactory;
	    private TableMetadataContext _tableMetadataContext;
	    private readonly TableRowKeyFactory _tableRowKeyFactory;

	    public TableMetadata(
	        string tableName,
	        string eplExpression,
	        string statementName,
	        Type[] keyTypes,
	        IDictionary<string, TableMetadataColumn> tableColumns,
	        TableStateRowFactory rowFactory,
	        int numberMethodAggregations,
	        StatementContext createTableStatementContext,
	        ObjectArrayEventType internalEventType,
	        ObjectArrayEventType publicEventType,
	        TableMetadataInternalEventToPublic eventToPublic,
	        bool queryPlanLogging)
	    {
	        _tableName = tableName;
	        _eplExpression = eplExpression;
	        _statementName = statementName;
	        _keyTypes = keyTypes;
	        _tableColumns = tableColumns;
	        _rowFactory = rowFactory;
	        _numberMethodAggregations = numberMethodAggregations;
	        _statementContextCreateTable = createTableStatementContext;
	        _internalEventType = internalEventType;
	        _publicEventType = publicEventType;
	        _eventToPublic = eventToPublic;
	        _queryPlanLogging = queryPlanLogging;

	        if (keyTypes.Length > 0)
            {
	            var pair = TableServiceUtil.GetIndexMultikeyForKeys(tableColumns, internalEventType);
                var queryPlanIndexItem = QueryPlanIndexItem.FromIndexMultikeyTablePrimaryKey(pair.Second);
                _eventTableIndexMetadataRepo.AddIndexExplicit(true, pair.Second, tableName, queryPlanIndexItem, createTableStatementContext.StatementName);
	            _tableRowKeyFactory = new TableRowKeyFactory(pair.First);
	        }
	    }

	    public Type[] KeyTypes
	    {
	        get { return _keyTypes; }
	    }

	    public TableStateFactory TableStateFactory
	    {
	        get { return _tableStateFactory; }
	        set { _tableStateFactory = value; }
	    }

	    public IDictionary<string, TableMetadataColumn> TableColumns
	    {
	        get { return _tableColumns; }
	    }

	    public TableStateRowFactory RowFactory
	    {
	        get { return _rowFactory; }
	    }

	    public int NumberMethodAggregations
	    {
	        get { return _numberMethodAggregations; }
	    }

	    public string ContextName
	    {
	        get { return _statementContextCreateTable.ContextName; }
	    }

	    public ObjectArrayEventType InternalEventType
	    {
	        get { return _internalEventType; }
	    }

	    public bool IsQueryPlanLogging
	    {
	        get { return _queryPlanLogging; }
	    }

	    public ISet<string> UniqueKeyProps
	    {
	        get
	        {
	            ISet<string> keys = new LinkedHashSet<string>();
	            foreach (var entry in _tableColumns)
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

	    public EventBean GetPublicEventBean(EventBean @event, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return _eventToPublic.Convert(@event, new EvaluateParams(eventsPerStream, isNewData, context));
	    }

	    public EventType PublicEventType
	    {
	        get { return _publicEventType; }
	    }

	    public TableMetadataInternalEventToPublic EventToPublic
	    {
	        get { return _eventToPublic; }
	    }

        public void ValidateAddIndexAssignUpdateStrategies(
            string createIndexStatementName, 
            IndexMultiKey imk, 
            string explicitIndexName, 
            QueryPlanIndexItem explicitIndexDesc)
        {
            // add index - for now
            _eventTableIndexMetadataRepo.AddIndexExplicit(false, imk, explicitIndexName, explicitIndexDesc, createIndexStatementName);

	        // validate strategies, rollback if required
	        foreach (var stmtEntry in _stmtNameToUpdateStrategyReceivers)
	        {
	            foreach (var strategyReceiver in stmtEntry.Value)
	            {
	                try
	                {
	                    TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(
	                        this, strategyReceiver.UpdateHelper, strategyReceiver.IsOnMerge);
	                }
	                catch (ExprValidationException ex)
	                {
	                    _eventTableIndexMetadataRepo.RemoveIndex(imk);
	                    throw new ExprValidationException(
	                        "Failed to validate statement '" + stmtEntry.Key + "' as a recipient of the proposed index: " +
	                        ex.Message);
	                }
	            }
	        }

	        // assign new strategies
	        foreach (var stmtEntry in _stmtNameToUpdateStrategyReceivers)
	        {
	            foreach (var strategyReceiver in stmtEntry.Value)
	            {
	                var strategy = TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(
	                    this, strategyReceiver.UpdateHelper, strategyReceiver.IsOnMerge);
	                strategyReceiver.Receiver.Update(strategy);
	            }
	        }
	    }

	    public void AddTableUpdateStrategyReceiver(string statementName, TableUpdateStrategyReceiver receiver, EventBeanUpdateHelper updateHelper, bool onMerge)
        {
	        var receivers = _stmtNameToUpdateStrategyReceivers.Get(statementName);
	        if (receivers == null) {
	            receivers = new List<TableUpdateStrategyReceiverDesc>();
	            _stmtNameToUpdateStrategyReceivers.Put(statementName, receivers);
	        }
	        receivers.Add(new TableUpdateStrategyReceiverDesc(receiver, updateHelper, onMerge));
	    }

	    public void RemoveTableUpdateStrategyReceivers(string statementName)
        {
	        _stmtNameToUpdateStrategyReceivers.Remove(statementName);
	    }

	    public void AddIndexReference(string indexName, string statementName)
        {
	        _eventTableIndexMetadataRepo.AddIndexReference(indexName, statementName);
	    }

	    public void RemoveIndexReferencesStatement(string statementName)
        {
	        var indexesDereferenced = _eventTableIndexMetadataRepo.GetRemoveRefIndexesDereferenced(statementName);
	        foreach (var indexDereferenced in indexesDereferenced)
            {
	            // remove tables
	            foreach (int agentInstanceId in AgentInstanceIds)
                {
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
	        var createTableResources = _statementContextCreateTable.StatementExtensionServicesContext.StmtResources;

	        StatementResourceHolder holder = null;
	        if (_statementContextCreateTable.ContextName == null)
            {
	            holder = createTableResources.ResourcesUnpartitioned;
	        }
	        else
            {
	            if (createTableResources.ResourcesPartitioned != null)
                {
	                holder = createTableResources.ResourcesPartitioned.Get(agentInstanceId);
	            }
	        }
	        if (holder == null)
            {
	            return null;
	        }

	        var aggsvc = (AggregationServiceTable) holder.AggregationService;
	        return aggsvc.TableState;
	    }

	    public ICollection<int> AgentInstanceIds
	    {
	        get
	        {
	            var createTableResources = _statementContextCreateTable.StatementExtensionServicesContext.StmtResources;

	            if (_statementContextCreateTable.ContextName == null)
	            {
	                return Collections.SingletonList(-1);
	            }
	            if (createTableResources.ResourcesPartitioned != null)
	            {
	                return createTableResources.ResourcesPartitioned.Keys;
	            }
	            return Collections.SingletonList(-1);
	        }
	    }

	    public string[][] UniqueIndexes
	    {
	        get { return _eventTableIndexMetadataRepo.UniqueIndexProps; }
	    }

	    public TableMetadataContext TableMetadataContext
	    {
	        set { _tableMetadataContext = value; }
	        get { return _tableMetadataContext; }
	    }

	    public TableRowKeyFactory TableRowKeyFactory
	    {
	        get { return _tableRowKeyFactory; }
	    }

	    public void ClearTableInstances()
        {
	        foreach (int agentInstanceId in AgentInstanceIds)
            {
	            var state = GetState(agentInstanceId);
	            if (state != null)
                {
	                state.DestroyInstance();
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

	    public StatementContext StatementContextCreateTable
	    {
	        get { return _statementContextCreateTable; }
	    }
    }
} // end of namespace
