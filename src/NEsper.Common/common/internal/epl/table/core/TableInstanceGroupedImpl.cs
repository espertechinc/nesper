///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.join.queryplan;
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
            AgentInstanceContext agentInstanceContext) : base(table, agentInstanceContext)
        {
            var eventTable =
                (PropertyHashedEventTableUnique)table.PrimaryIndexFactory.MakeEventTables(
                    agentInstanceContext,
                    null)[0];
            rows = eventTable.PropertyIndex.TransformLeft<object, EventBean, ObjectArrayBackedEventBean>();
            indexRepository.AddIndex(
                table.MetaData.KeyIndexMultiKey,
                new EventTableIndexRepositoryEntry(
                    table.MetaData.TableName,
                    table.MetaData.TableModuleName,
                    eventTable));
        }

        public long Count => rows.Count;

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
                indexName,
                indexModuleName,
                explicitIndexDesc,
                table.MetaData.InternalEventType,
                new PrimaryIndexIterable(rows),
                AgentInstanceContext,
                isRecoveringResilient,
                null);
        }

        public override void RemoveExplicitIndex(
            string indexName,
            string indexModuleName)
        {
            indexRepository.RemoveExplicitIndex(indexName, indexModuleName);
        }

        public override EventTable GetIndex(
            string indexName,
            string moduleName)
        {
            if (indexName.Equals(table.MetaData.TableName)) {
                return indexRepository.GetIndexByDesc(table.MetaData.KeyIndexMultiKey);
            }

            return indexRepository.GetExplicitIndexByName(indexName, moduleName);
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

        public override ICollection<object> GroupKeysMayMultiKey => rows.Keys;

        public override ICollection<object> GroupKeys {
            get {
                var keyTypes = table.MetaData.KeyTypes;
                if (keyTypes.Length == 1 && !keyTypes[0].IsArray) {
                    return rows.Keys;
                }

                IList<object> keys = new List<object>(rows.Count);
                if (keyTypes.Length == 1) {
                    var col = table.MetaData.KeyColNums[0];
                    foreach (var bean in rows.Values) {
                        keys.Add(bean.Properties[col]);
                    }
                }
                else {
                    var cols = table.MetaData.KeyColNums;
                    foreach (var bean in rows.Values) {
                        var mk = new object[cols.Length];
                        for (var i = 0; i < cols.Length; i++) {
                            mk[i] = bean.Properties[cols[i]];
                        }

                        keys.Add(mk);
                    }
                }

                return keys;
            }
        }

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