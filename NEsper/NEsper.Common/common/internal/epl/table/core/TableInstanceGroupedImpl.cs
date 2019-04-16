///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableInstanceGroupedImpl : TableInstanceGroupedBase,
        TableInstanceGrouped
    {
        private readonly IDictionary<object, ObjectArrayBackedEventBean> rows;

        public TableInstanceGroupedImpl(
            Table table,
            AgentInstanceContext agentInstanceContext)
            : base(table, agentInstanceContext)
        {
            var eventTable =
                (PropertyHashedEventTableUnique) table.PrimaryIndexFactory.MakeEventTables(agentInstanceContext, null)
                    [0];
            rows = CompatExtensions.UnwrapDictionary<object, ObjectArrayBackedEventBean>(eventTable.PropertyIndex);
            indexRepository.AddIndex(
                table.MetaData.KeyIndexMultiKey,
                new EventTableIndexRepositoryEntry(
                    table.MetaData.TableName, table.MetaData.TableModuleName, eventTable));
        }

        public override ObjectArrayBackedEventBean GetRowForGroupKey(object groupKey)
        {
            return rows.Get(groupKey);
        }

        public override ICollection<EventBean> EventCollection => rows.Values.Unwrap<EventBean>();

        public override IEnumerable<EventBean> IterableTableScan => new PrimaryIndexIterable(rows);

        public override void DeleteEvent(EventBean matchingEvent)
        {
            agentInstanceContext.InstrumentationProvider.QTableDeleteEvent(matchingEvent);
            foreach (var table in indexRepository.Tables) {
                table.Remove(matchingEvent, agentInstanceContext);
            }

            agentInstanceContext.InstrumentationProvider.ATableDeleteEvent();
        }

        public override void AddExplicitIndex(
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            bool isRecoveringResilient)
        {
            indexRepository.ValidateAddExplicitIndex(
                indexName, indexModuleName, explicitIndexDesc, table.MetaData.InternalEventType,
                new PrimaryIndexIterable(rows), AgentInstanceContext, isRecoveringResilient, null);
        }

        public override void RemoveExplicitIndex(string indexName)
        {
            indexRepository.RemoveExplicitIndex(indexName);
        }

        public override EventTable GetIndex(string indexName)
        {
            if (indexName.Equals(table.MetaData.TableName)) {
                return indexRepository.GetIndexByDesc(table.MetaData.KeyIndexMultiKey);
            }

            return indexRepository.GetExplicitIndexByName(indexName);
        }

        public override void ClearInstance()
        {
            foreach (var table in indexRepository.Tables) {
                table.Destroy();
            }
        }

        public override void Destroy()
        {
            ClearInstance();
        }

        public override ObjectArrayBackedEventBean GetCreateRowIntoTable(
            object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = rows.Get(groupByKey);
            if (bean != null) {
                return bean;
            }

            return CreateRowIntoTable(groupByKey);
        }

        public override ICollection<object> GroupKeys => rows.Keys;

        public override void HandleRowUpdated(ObjectArrayBackedEventBean updatedEvent)
        {
            if (agentInstanceContext.InstrumentationProvider.Activated()) {
                agentInstanceContext.InstrumentationProvider.QTableUpdatedEvent(updatedEvent);
                agentInstanceContext.InstrumentationProvider.ATableUpdatedEvent();
            }

            // no action
        }

        public override void HandleRowUpdateKeyBeforeUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
            agentInstanceContext.InstrumentationProvider.QaTableUpdatedEventWKeyBefore(updatedEvent);
        }

        public override void HandleRowUpdateKeyAfterUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
            agentInstanceContext.InstrumentationProvider.QaTableUpdatedEventWKeyAfter(updatedEvent);
        }

        internal class PrimaryIndexIterable : IEnumerable<EventBean>
        {
            private readonly IDictionary<object, ObjectArrayBackedEventBean> rows;

            internal PrimaryIndexIterable(IDictionary<object, ObjectArrayBackedEventBean> rows)
            {
                this.rows = rows;
            }

            public IEnumerator<EventBean> GetEnumerator()
            {
                return rows.Values.Unwrap<EventBean>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
} // end of namespace