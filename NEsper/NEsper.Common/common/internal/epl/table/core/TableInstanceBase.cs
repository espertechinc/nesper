///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public abstract class TableInstanceBase : TableInstance
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly EventTableIndexRepository indexRepository;
        internal readonly Table table;
        internal readonly IReaderWriterLock tableLevelRWLock = new StandardReaderWriterLock(60000);

        protected TableInstanceBase(Table table, AgentInstanceContext agentInstanceContext)
        {
            this.table = table;
            this.agentInstanceContext = agentInstanceContext;
            indexRepository = new EventTableIndexRepository(table.EventTableIndexMetadata);
        }

        public void AddEventUnadorned(EventBean @event)
        {
            if (@event.EventType != table.MetaData.InternalEventType) {
                throw new IllegalStateException("Unexpected event type for add: " + @event.EventType.Name);
            }

            var oa = (ObjectArrayBackedEventBean) @event;
            var aggs = table.AggregationRowFactory.Make();
            oa.Properties[0] = aggs;
            AddEvent(@event);
        }

        public AgentInstanceContext AgentInstanceContext => agentInstanceContext;

        public Table Table => table;

        public EventTableIndexRepository IndexRepository => indexRepository;

        public IReaderWriterLock TableLevelRWLock => tableLevelRWLock;

        public abstract ICollection<EventBean> EventCollection { get; }
        public abstract IEnumerable<EventBean> IterableTableScan { get; }
        public abstract void AddEvent(EventBean @event);
        public abstract void ClearInstance();
        public abstract void Destroy();
        public abstract void HandleRowUpdated(ObjectArrayBackedEventBean updatedEvent);
        public abstract void DeleteEvent(EventBean matchingEvent);
        public abstract void AddExplicitIndex(
            string indexName, string indexModuleName, QueryPlanIndexItem explicitIndexDesc, bool isRecoveringResilient);

        public abstract void RemoveExplicitIndex(string indexName);
        public abstract EventTable GetIndex(string indexName);
        public abstract void HandleRowUpdateKeyBeforeUpdate(ObjectArrayBackedEventBean updatedEvent);
        public abstract void HandleRowUpdateKeyAfterUpdate(ObjectArrayBackedEventBean updatedEvent);
    }
} // end of namespace