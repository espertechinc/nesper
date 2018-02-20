///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.mgmt
{
	public class TableStateInstanceGroupedImpl 
        : TableStateInstance 
        , TableStateInstanceGrouped
	{
	    private readonly IDictionary<object, ObjectArrayBackedEventBean> _rows =
	        new Dictionary<object, ObjectArrayBackedEventBean>().WithNullSupport();
	    private readonly IndexMultiKey _primaryIndexKey;

	    public TableStateInstanceGroupedImpl(
	        TableMetadata tableMetadata, 
	        AgentInstanceContext agentInstanceContext,
	        IReaderWriterLockManager rwLockManager)
            : base(tableMetadata, agentInstanceContext, rwLockManager)
        {
	        IList<EventPropertyGetter> indexGetters = new List<EventPropertyGetter>();
	        IList<string> keyNames = new List<string>();
	        foreach (var entry in tableMetadata.TableColumns) {
	            if (entry.Value.IsKey) {
	                keyNames.Add(entry.Key);
	                indexGetters.Add(tableMetadata.InternalEventType.GetGetter(entry.Key));
	            }
	        }

	        var tableName = "primary-" + tableMetadata.TableName;
	        var organization = new EventTableOrganization(tableName, true, false, 0, Collections.GetEmptyList<string>(), EventTableOrganizationType.HASH);

	        EventTable table;
	        if (indexGetters.Count == 1)
	        {
	            var tableMap = new TransformDictionary<object, EventBean, object, ObjectArrayBackedEventBean>(
	                _rows, k => k, v => v, k => k, v => v as ObjectArrayBackedEventBean);
                table = new PropertyIndexedEventTableSingleUnique(indexGetters[0], organization, tableMap);
	        }
	        else
            {
	            var getters = indexGetters.ToArrayOrNull();
                var tableMap = new TransformDictionary<MultiKeyUntyped, EventBean, object, ObjectArrayBackedEventBean>(
                    _rows, k => k as MultiKeyUntyped, v => v, k => k, v => v as ObjectArrayBackedEventBean);
	            table = new PropertyIndexedEventTableUnique(getters, organization, tableMap);
	        }

	        var pair = TableServiceUtil.GetIndexMultikeyForKeys(tableMetadata.TableColumns, tableMetadata.InternalEventType);
	        _primaryIndexKey = pair.Second;
	        _indexRepository.AddIndex(_primaryIndexKey, new EventTableIndexRepositoryEntry(tableName, table));
	    }

	    public override EventTable GetIndex(string indexName)
        {
	        if (indexName == _tableMetadata.TableName) {
	            return _indexRepository.GetIndexByDesc(_primaryIndexKey);
	        }
	        return _indexRepository.GetExplicitIndexByName(indexName);
	    }

	    public IDictionary<object, ObjectArrayBackedEventBean> Rows
	    {
	        get { return _rows; }
	    }

	    public override void AddEvent(EventBean theEvent)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableAddEvent(theEvent); }
	        try
            {
                foreach (var table in _indexRepository.Tables)
                {
	                table.Add(theEvent, AgentInstanceContext);
	            }
	        }
	        catch (EPException)
            {
	            foreach (var table in _indexRepository.Tables)
                {
	                table.Remove(theEvent, AgentInstanceContext);
	            }
	            throw;
	        }
	        finally
            {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableAddEvent(); }
	        }
	    }

	    public override void DeleteEvent(EventBean matchingEvent)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableDeleteEvent(matchingEvent); }
	        foreach (var table in _indexRepository.Tables) {
	            table.Remove(matchingEvent, AgentInstanceContext);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableDeleteEvent(); }
	    }

	    public override IEnumerable<EventBean> IterableTableScan
	    {
	        get { return new PrimaryIndexIterable(_rows); }
	    }

        public override void AddExplicitIndex(string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, bool isRecoveringResilient, bool allowIndexExists)
        {
            _indexRepository.ValidateAddExplicitIndex(
                explicitIndexName, 
                explicitIndexDesc, 
                _tableMetadata.InternalEventType, 
                new PrimaryIndexIterable(_rows), 
                AgentInstanceContext, 
                isRecoveringResilient || allowIndexExists, null);
        }

        public override string[] SecondaryIndexes
	    {
	        get { return _indexRepository.ExplicitIndexNames; }
	    }

	    public override EventTableIndexRepository IndexRepository
	    {
	        get { return _indexRepository; }
	    }

	    public override ICollection<EventBean> EventCollection
	    {
	        get { return _rows.Values.Unwrap<EventBean>(); }
	    }

	    public ObjectArrayBackedEventBean GetRowForGroupKey(object groupKey)
        {
	        return _rows.Get(groupKey);
	    }

	    public ICollection<object> GroupKeys
	    {
	        get { return _rows.Keys; }
	    }

	    public void Clear()
        {
	        ClearInstance();
	    }

	    public override void ClearInstance()
        {
	        _rows.Clear();
	        foreach (EventTable table in _indexRepository.Tables) {
	            table.Destroy();
	        }
	    }

	    public override void DestroyInstance() {
	        ClearInstance();
	    }

	    public override ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
	        var bean = _rows.Get(groupByKey);
	        if (bean != null) {
	            return bean;
	        }
	        var row = _tableMetadata.RowFactory.MakeOA(exprEvaluatorContext.AgentInstanceId, groupByKey, null, AggregationServicePassThru);
	        AddEvent(row);
	        return row;
	    }

	    public override int RowCount
	    {
	        get { return _rows.Count; }
	    }

	    public override AggregationServicePassThru AggregationServicePassThru
	    {
	        get { return null; }
	    }

	    internal class PrimaryIndexIterable : IEnumerable<EventBean>
        {
	        internal readonly IDictionary<object, ObjectArrayBackedEventBean> rows;

	        internal PrimaryIndexIterable(IDictionary<object, ObjectArrayBackedEventBean> rows)
            {
	            this.rows = rows;
	        }

	        public IEnumerator<EventBean> GetEnumerator()
	        {
	            return rows.Values.UnwrapEnumerable<EventBean>().GetEnumerator();
	        }

	        IEnumerator IEnumerable.GetEnumerator()
	        {
	            return GetEnumerator();
	        }
        }
	}
} // end of namespace
