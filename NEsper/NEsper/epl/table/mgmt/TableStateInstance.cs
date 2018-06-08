///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.mgmt
{
	public abstract class TableStateInstance
    {
	    protected readonly TableMetadata _tableMetadata;
	    private readonly AgentInstanceContext _agentInstanceContext;

        private readonly IReaderWriterLock _tableLevelRWLock;
	    protected readonly EventTableIndexRepository _indexRepository;

	    public abstract IEnumerable<EventBean> IterableTableScan { get; }
	    public abstract void AddEvent(EventBean theEvent);
	    public abstract void DeleteEvent(EventBean matchingEvent);
	    public abstract void ClearInstance();
	    public abstract void DestroyInstance();
        public abstract void AddExplicitIndex(string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, bool isRecoveringResilient, bool allowIndexExists);

        public abstract string[] SecondaryIndexes { get; }
	    public abstract EventTable GetIndex(string indexName);
	    public abstract ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract ICollection<EventBean> EventCollection { get; }
	    public abstract int RowCount { get; }
	    public abstract AggregationServicePassThru AggregationServicePassThru { get; }

	    public void HandleRowUpdated(ObjectArrayBackedEventBean row)
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QaTableUpdatedEvent(row);
	        }
	    }

	    public void AddEventUnadorned(EventBean @event)
        {
	        ObjectArrayBackedEventBean oa = (ObjectArrayBackedEventBean) @event;
	        AggregationRowPair aggs = _tableMetadata.RowFactory.MakeAggs(_agentInstanceContext.AgentInstanceId, null, null, AggregationServicePassThru);
	        oa.Properties[0] = aggs;
	        AddEvent(oa);
	    }

	    protected TableStateInstance(
	        TableMetadata tableMetadata, 
	        AgentInstanceContext agentInstanceContext,
	        IReaderWriterLockManager rwLockManager)
        {
            this._tableLevelRWLock = rwLockManager.CreateLock(GetType());
            this._tableMetadata = tableMetadata;
	        this._agentInstanceContext = agentInstanceContext;
            this._indexRepository = new EventTableIndexRepository(tableMetadata.EventTableIndexMetadataRepo);
        }

        public virtual TableMetadata TableMetadata
	    {
	        get { return _tableMetadata; }
	    }

	    public virtual AgentInstanceContext AgentInstanceContext
	    {
	        get { return _agentInstanceContext; }
	    }

	    public virtual IReaderWriterLock TableLevelRWLock
	    {
	        get { return _tableLevelRWLock; }
	    }

	    public virtual EventTableIndexRepository IndexRepository
	    {
	        get { return _indexRepository; }
	    }

        public virtual void HandleRowUpdateKeyBeforeUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QaTableUpdatedEventWKeyBefore(updatedEvent);
	        }
	        // no action
	    }

        public virtual void HandleRowUpdateKeyAfterUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QaTableUpdatedEventWKeyAfter(updatedEvent);
	        }
	        // no action
	    }

        public virtual void RemoveExplicitIndex(string indexName)
        {
	        _indexRepository.RemoveExplicitIndex(indexName);
	    }
	}
} // end of namespace
