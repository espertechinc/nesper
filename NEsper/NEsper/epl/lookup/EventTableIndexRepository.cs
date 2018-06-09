///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// A repository of index tables for use with anything that
    /// may use the indexes to correlate triggering events with indexed events.
    /// <para>
    /// Maintains index tables and keeps a reference count for user. Allows reuse of indexes for multiple
    /// deleting statements.
    /// </para>
    /// </summary>
    public class EventTableIndexRepository
    {
        /// <summary>
        /// Defines the _tables
        /// </summary>
        private readonly IList<EventTable> _tables;

        /// <summary>
        /// Defines the _tableIndexesRefCount
        /// </summary>
        private readonly IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry> _tableIndexesRefCount;

        /// <summary>
        /// Defines the _explicitIndexes
        /// </summary>
        private readonly IDictionary<string, EventTable> _explicitIndexes;

        /// <summary>
        /// Defines the _eventTableIndexMetadata
        /// </summary>
        private readonly EventTableIndexMetadata _eventTableIndexMetadata;

        /// <summary>
        /// The transform dictionary for tableIndexesRefCount.
        /// </summary>
        private readonly IDictionary<IndexMultiKey, EventTableIndexEntryBase> _transIndexesRefCount;

        /// <summary>Ctor.</summary>
        /// <param name="eventTableIndexMetadata">metadata for index</param>
        public EventTableIndexRepository(EventTableIndexMetadata eventTableIndexMetadata)
        {
            _tables = new List<EventTable>();
            _tableIndexesRefCount = new Dictionary<IndexMultiKey, EventTableIndexRepositoryEntry>();
            _explicitIndexes = new Dictionary<string, EventTable>();
            _eventTableIndexMetadata = eventTableIndexMetadata;
            _transIndexesRefCount = new TransformDictionary<
                IndexMultiKey, EventTableIndexEntryBase,
                IndexMultiKey, EventTableIndexRepositoryEntry>(
                _tableIndexesRefCount,
                kOut => kOut,
                kIn => kIn,
                vOut => vOut,
                vIn => (EventTableIndexRepositoryEntry) vIn);
        }

        /// <summary>
        /// Gets the EventTableIndexMetadata
        /// </summary>
        public EventTableIndexMetadata EventTableIndexMetadata => _eventTableIndexMetadata;

        /// <summary>
        /// The AddExplicitIndexOrReuse
        /// </summary>
        /// <param name="unique">The <see cref="bool"/></param>
        /// <param name="hashProps">The <see cref="IList{IndexedPropDesc}"/></param>
        /// <param name="btreeProps">The <see cref="IList{IndexedPropDesc}"/></param>
        /// <param name="advancedIndexProvisionDesc">The <see cref="EventAdvancedIndexProvisionDesc"/></param>
        /// <param name="prefilledEvents">The <see cref="IEnumerable{EventBean}"/></param>
        /// <param name="indexedType">The <see cref="EventType"/></param>
        /// <param name="indexName">The <see cref="string"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="optionalSerde">The <see cref="object"/></param>
        /// <returns>The <see cref="Pair{IndexMultiKey, EventTableAndNamePair}"/></returns>
        public Pair<IndexMultiKey, EventTableAndNamePair> AddExplicitIndexOrReuse(
                bool unique,
                IList<IndexedPropDesc> hashProps,
                IList<IndexedPropDesc> btreeProps,
                EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc,
                IEnumerable<EventBean> prefilledEvents,
                EventType indexedType,
                string indexName,
                AgentInstanceContext agentInstanceContext,
                object optionalSerde)
        {
            if (hashProps.IsEmpty() && btreeProps.IsEmpty() && advancedIndexProvisionDesc == null)
            {
                throw new ArgumentException("Invalid zero element list for hash and btree columns");
            }

            // Get an existing table, if any, matching the exact requirement
            var indexPropKeyMatch = EventTableIndexUtil.FindExactMatchNameAndType(_tableIndexesRefCount.Keys, unique, hashProps, btreeProps, advancedIndexProvisionDesc == null ? null : advancedIndexProvisionDesc.IndexDesc);
            if (indexPropKeyMatch != null)
            {
                EventTableIndexRepositoryEntry refTablePair = _tableIndexesRefCount.Get(indexPropKeyMatch);
                return new Pair<IndexMultiKey, EventTableAndNamePair>(indexPropKeyMatch, new EventTableAndNamePair(refTablePair.Table, refTablePair.OptionalIndexName));
            }

            return AddIndex(unique, hashProps, btreeProps, advancedIndexProvisionDesc, prefilledEvents, indexedType, indexName, false, agentInstanceContext, optionalSerde);
        }

        /// <summary>
        /// The AddIndex
        /// </summary>
        /// <param name="indexMultiKey">The <see cref="IndexMultiKey"/></param>
        /// <param name="entry">The <see cref="EventTableIndexRepositoryEntry"/></param>
        public void AddIndex(IndexMultiKey indexMultiKey, EventTableIndexRepositoryEntry entry)
        {
            _tableIndexesRefCount.Put(indexMultiKey, entry);
            _tables.Add(entry.Table);
        }

        /// <summary>
        /// Returns a list of current index tables in the repository.
        /// </summary>
        /// <returns>index tables</returns>
        public IList<EventTable> Tables => _tables;

        /// <summary>Destroy indexes.</summary>
        public void Destroy()
        {
            foreach (EventTable table in _tables)
            {
                table.Destroy();
            }
            _tables.Clear();
            _tableIndexesRefCount.Clear();
        }

        /// <summary>
        /// The FindTable
        /// </summary>
        /// <param name="keyPropertyNames">The key property names.</param>
        /// <param name="rangePropertyNames">The range property names.</param>
        /// <param name="optionalIndexHintInstructions">The <see cref="IList{IndexHintInstruction}" /></param>
        /// <returns>
        /// The <see cref="Pair{IndexMultiKey, EventTableAndNamePair}" />
        /// </returns>
        public Pair<IndexMultiKey, EventTableAndNamePair> FindTable(
            ISet<string> keyPropertyNames,
            ISet<string> rangePropertyNames,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            var pair = EventTableIndexUtil.FindIndexBestAvailable(
                _transIndexesRefCount, keyPropertyNames, rangePropertyNames, optionalIndexHintInstructions);
            if (pair == null)
            {
                return null;
            }
            EventTable tableFound = ((EventTableIndexRepositoryEntry)pair.Second).Table;
            return new Pair<IndexMultiKey, EventTableAndNamePair>(pair.First, new EventTableAndNamePair(tableFound, pair.Second.OptionalIndexName));
        }

        /// <summary>
        /// The GetIndexDescriptors
        /// </summary>
        /// <returns>The <see cref="IndexMultiKey"/> array</returns>
        public IndexMultiKey[] IndexDescriptors
        {
            get
            {
                var keySet = _tableIndexesRefCount.Keys;
                return keySet.ToArray();
            }
        }

        /// <summary>
        /// Gets the TableIndexesRefCount
        /// </summary>
        public IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry> TableIndexesRefCount => _tableIndexesRefCount;

        /// <summary>
        /// The ValidateAddExplicitIndex
        /// </summary>
        /// <param name="explicitIndexName">The <see cref="string"/></param>
        /// <param name="explicitIndexDesc">The <see cref="QueryPlanIndexItem"/></param>
        /// <param name="eventType">The <see cref="EventType"/></param>
        /// <param name="dataWindowContents">The <see cref="IEnumerable{EventBean}"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="allowIndexExists">The <see cref="bool"/></param>
        /// <param name="optionalSerde">The <see cref="object"/></param>
        public void ValidateAddExplicitIndex(
            string explicitIndexName,
            QueryPlanIndexItem explicitIndexDesc,
            EventType eventType,
            IEnumerable<EventBean> dataWindowContents,
            AgentInstanceContext agentInstanceContext,
            bool allowIndexExists,
            object optionalSerde)
        {
            if (_explicitIndexes.ContainsKey(explicitIndexName))
            {
                if (allowIndexExists)
                {
                    return;
                }
                throw new ExprValidationException("Index by name '" + explicitIndexName + "' already exists");
            }

            AddExplicitIndex(explicitIndexName, explicitIndexDesc, eventType, dataWindowContents, agentInstanceContext, optionalSerde);
        }

        /// <summary>
        /// The AddExplicitIndex
        /// </summary>
        /// <param name="explicitIndexName">The <see cref="string"/></param>
        /// <param name="desc">The <see cref="QueryPlanIndexItem"/></param>
        /// <param name="eventType">The <see cref="EventType"/></param>
        /// <param name="dataWindowContents">The <see cref="IEnumerable{EventBean}"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="optionalSerde">The <see cref="object"/></param>
        public void AddExplicitIndex(
            string explicitIndexName,
            QueryPlanIndexItem desc,
            EventType eventType,
            IEnumerable<EventBean> dataWindowContents,
            AgentInstanceContext agentInstanceContext,
            object optionalSerde)
        {
            Pair<IndexMultiKey, EventTableAndNamePair> pair = AddExplicitIndexOrReuse(desc.IsUnique, desc.HashPropsAsList, desc.BtreePropsAsList, desc.AdvancedIndexProvisionDesc, dataWindowContents, eventType, explicitIndexName, agentInstanceContext, optionalSerde);
            _explicitIndexes.Put(explicitIndexName, pair.Second.EventTable);
        }

        /// <summary>
        /// The GetExplicitIndexByName
        /// </summary>
        /// <param name="indexName">The <see cref="string"/></param>
        /// <returns>The <see cref="EventTable"/></returns>
        public EventTable GetExplicitIndexByName(string indexName)
        {
            return _explicitIndexes.Get(indexName);
        }

        /// <summary>
        /// The GetIndexByDesc
        /// </summary>
        /// <param name="indexKey">The <see cref="IndexMultiKey"/></param>
        /// <returns>The <see cref="EventTable"/></returns>
        public EventTable GetIndexByDesc(IndexMultiKey indexKey)
        {
            EventTableIndexRepositoryEntry entry = _tableIndexesRefCount.Get(indexKey);
            if (entry == null)
            {
                return null;
            }
            return entry.Table;
        }

        /// <summary>
        /// The AddIndex
        /// </summary>
        /// <param name="unique">The <see cref="bool"/></param>
        /// <param name="hashProps">The <see cref="IList{IndexedPropDesc}"/></param>
        /// <param name="btreeProps">The <see cref="IList{IndexedPropDesc}"/></param>
        /// <param name="advancedIndexProvisionDesc">The <see cref="EventAdvancedIndexProvisionDesc"/></param>
        /// <param name="prefilledEvents">The <see cref="IEnumerable{EventBean}"/></param>
        /// <param name="indexedType">The <see cref="EventType"/></param>
        /// <param name="indexName">The <see cref="string"/></param>
        /// <param name="mustCoerce">The <see cref="bool"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="optionalSerde">The <see cref="object"/></param>
        /// <returns>The <see cref="Pair{IndexMultiKey, EventTableAndNamePair}"/></returns>
        private Pair<IndexMultiKey, EventTableAndNamePair> AddIndex(
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc,
            IEnumerable<EventBean> prefilledEvents,
            EventType indexedType,
            string indexName,
            bool mustCoerce,
            AgentInstanceContext agentInstanceContext,
            object optionalSerde)
        {
            // not resolved as full match and not resolved as unique index match, allocate
            var indexPropKey = new IndexMultiKey(unique, hashProps, btreeProps, advancedIndexProvisionDesc == null ? null : advancedIndexProvisionDesc.IndexDesc);

            var indexedPropDescs = hashProps.ToArray();
            var indexProps = IndexedPropDesc.GetIndexProperties(indexedPropDescs);
            var indexCoercionTypes = IndexedPropDesc.GetCoercionTypes(indexedPropDescs);
            if (!mustCoerce)
            {
                indexCoercionTypes = null;
            }

            var rangePropDescs = btreeProps.ToArray();
            var rangeProps = IndexedPropDesc.GetIndexProperties(rangePropDescs);
            var rangeCoercionTypes = IndexedPropDesc.GetCoercionTypes(rangePropDescs);

            var indexItem = new QueryPlanIndexItem(indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes, unique, advancedIndexProvisionDesc);
            var table = EventTableUtil.BuildIndex(agentInstanceContext, 0, indexItem, indexedType, true, unique, indexName, optionalSerde, false);

            // fill table since its new
            var events = new EventBean[1];
            foreach (EventBean prefilledEvent in prefilledEvents)
            {
                events[0] = prefilledEvent;
                table.Add(events, agentInstanceContext);
            }

            // add table
            _tables.Add(table);

            // add index, reference counted
            _tableIndexesRefCount.Put(indexPropKey, new EventTableIndexRepositoryEntry(indexName, table));

            return new Pair<IndexMultiKey, EventTableAndNamePair>(indexPropKey, new EventTableAndNamePair(table, indexName));
        }

        /// <summary>
        /// The GetExplicitIndexNames
        /// </summary>
        /// <returns>The <see cref="string"/> array</returns>
        public string[] ExplicitIndexNames => _explicitIndexes.Keys.ToArray();

        /// <summary>
        /// The RemoveIndex
        /// </summary>
        /// <param name="index">The <see cref="IndexMultiKey"/></param>
        public void RemoveIndex(IndexMultiKey index)
        {
            EventTableIndexRepositoryEntry entry = _tableIndexesRefCount.Delete(index);
            if (entry != null)
            {
                _tables.Remove(entry.Table);
                if (entry.OptionalIndexName != null)
                {
                    _explicitIndexes.Remove(entry.OptionalIndexName);
                }
                entry.Table.Destroy();
            }
        }

        /// <summary>
        /// The GetIndexByName
        /// </summary>
        /// <param name="indexName">The <see cref="string"/></param>
        /// <returns>The <see cref="IndexMultiKey"/></returns>
        public IndexMultiKey GetIndexByName(string indexName)
        {
            foreach (var entry in _tableIndexesRefCount)
            {
                if (entry.Value.OptionalIndexName.Equals(indexName))
                {
                    return entry.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// The RemoveExplicitIndex
        /// </summary>
        /// <param name="indexName">The <see cref="string"/></param>
        public void RemoveExplicitIndex(string indexName)
        {
            EventTable eventTable = _explicitIndexes.Delete(indexName);
            if (eventTable != null)
            {
                eventTable.Destroy();
            }
        }
    }
}
