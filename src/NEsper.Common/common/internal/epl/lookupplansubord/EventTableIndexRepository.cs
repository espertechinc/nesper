///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    /// <summary>
    /// A repository of index tables for use with anything that
    /// may use the indexes to correlate triggering events with indexed events.
    /// <para />Maintains index tables and keeps a reference count for user. Allows reuse of indexes for multiple
    /// deleting statements.
    /// </summary>
    public class EventTableIndexRepository
    {
        private readonly IList<EventTable> tables;
        private readonly IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry> tableIndexesRefCount;
        private readonly IDictionary<string, EventTable> explicitIndexes;
        private readonly EventTableIndexMetadata eventTableIndexMetadata;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTableIndexMetadata">metadata for index</param>
        public EventTableIndexRepository(EventTableIndexMetadata eventTableIndexMetadata)
        {
            tables = new List<EventTable>();
            tableIndexesRefCount = new Dictionary<IndexMultiKey, EventTableIndexRepositoryEntry>();
            explicitIndexes = new Dictionary<string, EventTable>();
            this.eventTableIndexMetadata = eventTableIndexMetadata;
        }

        public EventTableIndexMetadata EventTableIndexMetadata {
            get => eventTableIndexMetadata;
        }

        public Pair<IndexMultiKey, EventTableAndNamePair> AddExplicitIndexOrReuse(
            QueryPlanIndexItem desc,
            IEnumerable<EventBean> prefilledEvents,
            EventType indexedType,
            string indexName,
            string indexModuleName,
            AgentInstanceContext agentInstanceContext,
            DataInputOutputSerde optionalValueSerde)
        {
            IndexMultiKey indexMultiKey = desc.ToIndexMultiKey();
            if (desc.HashPropsAsList.IsEmpty() &&
                desc.BtreePropsAsList.IsEmpty() &&
                desc.AdvancedIndexProvisionDesc == null) {
                throw new ArgumentException("Invalid zero element list for hash and btree columns");
            }

            // Get an existing table, if any, matching the exact requirement
            IndexMultiKey indexPropKeyMatch =
                EventTableIndexUtil.FindExactMatchNameAndType(tableIndexesRefCount.Keys, indexMultiKey);
            if (indexPropKeyMatch != null) {
                EventTableIndexRepositoryEntry refTablePair = tableIndexesRefCount.Get(indexPropKeyMatch);
                return new Pair<IndexMultiKey, EventTableAndNamePair>(
                    indexPropKeyMatch,
                    new EventTableAndNamePair(refTablePair.Table, refTablePair.OptionalIndexName));
            }

            return AddIndex(
                desc,
                prefilledEvents,
                indexedType,
                indexName,
                indexModuleName,
                false,
                agentInstanceContext,
                optionalValueSerde);
        }

        public void AddIndex(
            IndexMultiKey indexMultiKey,
            EventTableIndexRepositoryEntry entry)
        {
            tableIndexesRefCount.Put(indexMultiKey, entry);
            tables.Add(entry.Table);
        }

        /// <summary>
        /// Returns a list of current index tables in the repository.
        /// </summary>
        /// <returns>index tables</returns>
        public IList<EventTable> Tables {
            get => tables;
        }

        /// <summary>
        /// Destroy indexes.
        /// </summary>
        public void Destroy()
        {
            foreach (EventTable table in tables) {
                table.Destroy();
            }

            tables.Clear();
            tableIndexesRefCount.Clear();
        }

        public Pair<IndexMultiKey, EventTableAndNamePair> FindTable(
            ISet<string> keyPropertyNames,
            ISet<string> rangePropertyNames,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            Pair<IndexMultiKey, EventTableIndexEntryBase> pair = EventTableIndexUtil.FindIndexBestAvailable(
                tableIndexesRefCount,
                keyPropertyNames,
                rangePropertyNames,
                optionalIndexHintInstructions);
            if (pair == null) {
                return null;
            }

            EventTable tableFound = ((EventTableIndexRepositoryEntry) pair.Second).Table;
            return new Pair<IndexMultiKey, EventTableAndNamePair>(
                pair.First,
                new EventTableAndNamePair(tableFound, pair.Second.OptionalIndexName));
        }

        public IndexMultiKey[] IndexDescriptors {
            get {
                ICollection<IndexMultiKey> keySet = tableIndexesRefCount.Keys;
                return keySet.ToArray();
            }
        }

        public IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry> TableIndexesRefCount {
            get { return tableIndexesRefCount; }
        }

        public void ValidateAddExplicitIndex(
            string explicitIndexName,
            string explicitIndexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            EventType eventType,
            IEnumerable<EventBean> dataWindowContents,
            AgentInstanceContext agentInstanceContext,
            bool allowIndexExists,
            DataInputOutputSerde optionalValueSerde)
        {
            if (explicitIndexes.ContainsKey(explicitIndexName)) {
                if (allowIndexExists) {
                    return;
                }

                throw new ExprValidationException("Index by name '" + explicitIndexName + "' already exists");
            }

            AddExplicitIndex(
                explicitIndexName,
                explicitIndexModuleName,
                explicitIndexDesc,
                eventType,
                dataWindowContents,
                agentInstanceContext,
                optionalValueSerde);
        }

        public void AddExplicitIndex(
            string explicitIndexName,
            string explicitIndexModuleName,
            QueryPlanIndexItem desc,
            EventType eventType,
            IEnumerable<EventBean> dataWindowContents,
            AgentInstanceContext agentInstanceContext,
            DataInputOutputSerde optionalValueSerde)
        {
            Pair<IndexMultiKey, EventTableAndNamePair> pair = AddExplicitIndexOrReuse(
                desc,
                dataWindowContents,
                eventType,
                explicitIndexName,
                explicitIndexModuleName,
                agentInstanceContext,
                optionalValueSerde);
            explicitIndexes.Put(explicitIndexName, pair.Second.EventTable);
        }

        public EventTable GetExplicitIndexByName(string indexName)
        {
            return explicitIndexes.Get(indexName);
        }

        public EventTable GetIndexByDesc(IndexMultiKey indexKey)
        {
            EventTableIndexRepositoryEntry entry = tableIndexesRefCount.Get(indexKey);

            return entry?.Table;
        }

        private Pair<IndexMultiKey, EventTableAndNamePair> AddIndex(
            QueryPlanIndexItem indexItem,
            IEnumerable<EventBean> prefilledEvents,
            EventType indexedType,
            string indexName,
            string indexModuleName,
            bool mustCoerce,
            AgentInstanceContext agentInstanceContext,
            DataInputOutputSerde optionalValueSerde)
        {
            // not resolved as full match and not resolved as unique index match, allocate
            IndexMultiKey indexPropKey = indexItem.ToIndexMultiKey();

            EventTable table = EventTableUtil.BuildIndex(
                agentInstanceContext,
                0,
                indexItem,
                indexedType,
                indexItem.IsUnique,
                indexName,
                optionalValueSerde,
                false);

            try {
                // fill table since its new
                EventBean[] events = new EventBean[1];
                foreach (EventBean prefilledEvent in prefilledEvents) {
                    events[0] = prefilledEvent;
                    table.Add(events, agentInstanceContext);
                }
            }
            catch (Exception) {
                table.Destroy();
                throw;
            }

            // add table
            tables.Add(table);

            // add index, reference counted
            tableIndexesRefCount.Put(
                indexPropKey,
                new EventTableIndexRepositoryEntry(indexName, indexModuleName, table));

            return new Pair<IndexMultiKey, EventTableAndNamePair>(
                indexPropKey,
                new EventTableAndNamePair(table, indexName));
        }

        public string[] ExplicitIndexNames {
            get {
                var names = explicitIndexes.Keys;
                return names.ToArray();
            }
        }

        public void RemoveIndex(IndexMultiKey index)
        {
            EventTableIndexRepositoryEntry entry = tableIndexesRefCount.Delete(index);
            if (entry != null) {
                tables.Remove(entry.Table);
                if (entry.OptionalIndexName != null) {
                    explicitIndexes.Remove(entry.OptionalIndexName);
                }

                entry.Table.Destroy();
            }
        }

        public IndexMultiKey GetIndexByName(string indexName)
        {
            foreach (KeyValuePair<IndexMultiKey, EventTableIndexRepositoryEntry> entry in tableIndexesRefCount) {
                if (entry.Value.OptionalIndexName.Equals(indexName)) {
                    return entry.Key;
                }
            }

            return null;
        }

        public void RemoveExplicitIndex(string indexName)
        {
            EventTable eventTable = explicitIndexes.Delete(indexName);
            eventTable?.Destroy();
        }
    }
} // end of namespace