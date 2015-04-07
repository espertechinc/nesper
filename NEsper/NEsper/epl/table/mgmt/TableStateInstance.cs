///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.mgmt
{
    public abstract class TableStateInstance
    {
        private readonly TableMetadata _tableMetadata;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly IReaderWriterLock _tableLevelRwLock = ReaderWriterLockManager.CreateDefaultLock();
        private readonly EventTableIndexRepository _indexRepository = new EventTableIndexRepository();

        public abstract IEnumerable<EventBean> IterableTableScan { get; }
        public abstract void AddEvent(EventBean theEvent);
        public abstract void DeleteEvent(EventBean matchingEvent);
        public abstract void ClearEvents();
        public abstract void AddExplicitIndex(CreateIndexDesc spec) ;
        public abstract string[] SecondaryIndexes { get; }
        public abstract EventTable GetIndex(string indexName);
        public abstract ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext);
        public abstract ICollection<EventBean> EventCollection { get; }
        public abstract int RowCount { get; }

        public void HandleRowUpdated(ObjectArrayBackedEventBean row)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QaTableUpdatedEvent(row);
            }
        }
    
        public void AddEventUnadorned(EventBean @event)
        {
            var oa = (ObjectArrayBackedEventBean) @event;
            var aggs = _tableMetadata.RowFactory.MakeAggs(_agentInstanceContext.AgentInstanceId, null, null);
            oa.Properties[0] = aggs;
            AddEvent(oa);
        }
    
        protected TableStateInstance(TableMetadata tableMetadata, AgentInstanceContext agentInstanceContext)
        {
            _tableMetadata = tableMetadata;
            _agentInstanceContext = agentInstanceContext;
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
            get { return _tableLevelRwLock; }
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
    }
}
