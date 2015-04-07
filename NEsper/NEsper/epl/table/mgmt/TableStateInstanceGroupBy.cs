///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableStateInstanceGroupBy : TableStateInstance
    {
        private readonly IDictionary<Object, ObjectArrayBackedEventBean> _rows = 
            new Dictionary<object, ObjectArrayBackedEventBean>().WithNullSupport();
        private readonly IndexMultiKey _primaryIndexKey;
    
        public TableStateInstanceGroupBy(TableMetadata tableMetadata, AgentInstanceContext agentInstanceContext)
            : base(tableMetadata, agentInstanceContext)
        {
            var indexGetters = new List<EventPropertyGetter>();
            var keyNames = new List<string>();
            foreach (var entry in tableMetadata.TableColumns) {
                if (entry.Value.IsKey) {
                    keyNames.Add(entry.Key);
                    indexGetters.Add(tableMetadata.InternalEventType.GetGetter(entry.Key));
                }
            }
    
            var tableName = "primary-" + tableMetadata.TableName;
            var organization = new EventTableOrganization(tableName, true, false, 0, CollectionUtil.ToArray(keyNames), EventTableOrganization.EventTableOrganizationType.HASH);
    
            EventTable table;
            if (indexGetters.Count == 1) {
                var tableMap = new TransformDictionary<object, EventBean, object, ObjectArrayBackedEventBean>(
                    _rows, k => k, v => v, k => k, v => v as ObjectArrayBackedEventBean);
                table = new PropertyIndexedEventTableSingleUnique(indexGetters[0], organization, tableMap);
            }
            else {
                var getters = indexGetters.ToArray();
                var tableMap = new TransformDictionary<MultiKeyUntyped, EventBean, object, ObjectArrayBackedEventBean>(
                    _rows, k => k as MultiKeyUntyped, v => v, k => k, v => v as ObjectArrayBackedEventBean);
                table = new PropertyIndexedEventTableUnique(getters, organization, tableMap);
            }
    
            var pair = TableServiceUtil.GetIndexMultikeyForKeys(tableMetadata.TableColumns, tableMetadata.InternalEventType);
            _primaryIndexKey = pair.Second;
            base.IndexRepository.AddIndex(_primaryIndexKey, new EventTableIndexRepositoryEntry(tableName, table));
        }
    
        public override EventTable GetIndex(string indexName)
        {
            if (indexName.Equals(TableMetadata.TableName)) {
                return base.IndexRepository.GetIndexByDesc(_primaryIndexKey);
            }
            return base.IndexRepository.GetExplicitIndexByName(indexName);
        }

        public IDictionary<object, ObjectArrayBackedEventBean> Rows
        {
            get { return _rows; }
        }

        public override void AddEvent(EventBean theEvent)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableAddEvent(theEvent); }

            var indexRepositoryTables = IndexRepository.Tables;

            try {
                for (int ii = 0; ii < indexRepositoryTables.Count; ii++) {
                    indexRepositoryTables[ii].Add(theEvent);
                }
            }
            catch (EPException) {
                for (int ii = 0; ii < indexRepositoryTables.Count; ii++) {
                    indexRepositoryTables[ii].Remove(theEvent);
                }
                throw;
            }
            finally {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableAddEvent(); }
            }
        }
    
        public override void DeleteEvent(EventBean matchingEvent)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableDeleteEvent(matchingEvent); }
            var indexRepositoryTables = base.IndexRepository.Tables;
            for (int ii = 0; ii < indexRepositoryTables.Count; ii++) {
                indexRepositoryTables[ii].Remove(matchingEvent);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableDeleteEvent(); }
        }

        public override IEnumerable<EventBean> IterableTableScan
        {
            get { return new PrimaryIndexIterable(_rows); }
        }

        public override void AddExplicitIndex(CreateIndexDesc spec) {
            IndexRepository.ValidateAddExplicitIndex(spec.IsUnique, spec.IndexName, spec.Columns, TableMetadata.InternalEventType, new PrimaryIndexIterable(_rows));
        }

        public override string[] SecondaryIndexes
        {
            get { return IndexRepository.ExplicitIndexNames; }
        }

        public override ICollection<EventBean> EventCollection
        {
            get
            {
                return new TransformCollection<ObjectArrayBackedEventBean, EventBean>(
                    _rows.Values,
                    value => (ObjectArrayBackedEventBean) value,
                    value => value);
            }
        }

        public override void ClearEvents()
        {
            _rows.Clear();

            var indexRepositoryTables = base.IndexRepository.Tables;
            for (int ii = 0; ii < indexRepositoryTables.Count; ii++)
            {
                indexRepositoryTables[ii].Clear();
            }
        }

        public override ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = _rows.Get(groupByKey);
            if (bean != null)
            {
                return bean;
            }
            var row = TableMetadata.RowFactory.MakeOA(exprEvaluatorContext.AgentInstanceId, groupByKey, null);
            AddEvent(row);
            return row;
        }

        public override int RowCount
        {
            get { return _rows.Count; }
        }

        internal class PrimaryIndexIterable : IEnumerable<EventBean>
        {
            private readonly IDictionary<Object, ObjectArrayBackedEventBean> _rows;

            internal PrimaryIndexIterable(IDictionary<Object, ObjectArrayBackedEventBean> rows)
            {
                _rows = rows;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<EventBean> GetEnumerator()
            {
                return _rows.Values.Select(bean => (EventBean) bean).GetEnumerator();
            }
        }
    }
}
