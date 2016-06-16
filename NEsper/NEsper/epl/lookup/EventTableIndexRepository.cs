///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.lookup
{
	/// <summary>
	/// A repository of index tables for use with anything that
	/// may use the indexes to correlate triggering events with indexed events.
	/// <para />Maintains index tables and keeps a reference count for user. Allows reuse of indexes for multiple
	/// deleting statements.
	/// </summary>
	public class EventTableIndexRepository
	{
	    private readonly IList<EventTable> _tables;
	    private readonly IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry> _tableIndexesRefCount;
	    private readonly IDictionary<string, EventTable> _explicitIndexes;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public EventTableIndexRepository()
	    {
	        _tables = new List<EventTable>();
	        _tableIndexesRefCount = new Dictionary<IndexMultiKey, EventTableIndexRepositoryEntry>();
	        _explicitIndexes = new Dictionary<string, EventTable>();
	    }

	    public Pair<IndexMultiKey, EventTableAndNamePair> AddExplicitIndexOrReuse(
	        bool unique,
	        IList<IndexedPropDesc> hashProps,
	        IList<IndexedPropDesc> btreeProps,
	        IEnumerable<EventBean> prefilledEvents,
	        EventType indexedType,
	        string indexName,
	        AgentInstanceContext agentInstanceContext,
	        object optionalSerde)
	    {
	        if (hashProps.IsEmpty() && btreeProps.IsEmpty()) {
	            throw new ArgumentException("Invalid zero element list for hash and btree columns");
	        }

	        // Get an existing table, if any, matching the exact requirement
	        var indexPropKeyMatch = EventTableIndexUtil.FindExactMatchNameAndType(_tableIndexesRefCount.Keys, unique, hashProps, btreeProps);
	        if (indexPropKeyMatch != null) {
	            var refTablePair = _tableIndexesRefCount.Get(indexPropKeyMatch);
	            return new Pair<IndexMultiKey, EventTableAndNamePair>(indexPropKeyMatch, new EventTableAndNamePair(refTablePair.Table, refTablePair.OptionalIndexName));
	        }

	        return AddIndex(unique, hashProps, btreeProps, prefilledEvents, indexedType, indexName, false, agentInstanceContext, optionalSerde);
	    }

	    public void AddIndex(IndexMultiKey indexMultiKey, EventTableIndexRepositoryEntry entry) {
	        _tableIndexesRefCount.Put(indexMultiKey, entry);
	        _tables.Add(entry.Table);
	    }

	    /// <summary>
	    /// Returns a list of current index tables in the repository.
	    /// </summary>
	    /// <returns>index tables</returns>
	    public IList<EventTable> GetTables()
	    {
	        return _tables;
	    }

	    /// <summary>
	    /// Dispose indexes.
	    /// </summary>
	    public void Destroy()
	    {
	        foreach (var table in _tables) {
	            table.Destroy();
	        }
	        _tables.Clear();
	        _tableIndexesRefCount.Clear();
	    }

	    public Pair<IndexMultiKey, EventTableAndNamePair> FindTable(ISet<string> keyPropertyNames, ISet<string> rangePropertyNames, IList<IndexHintInstruction> optionalIndexHintInstructions) {
	        var pair = EventTableIndexUtil.FindIndexBestAvailable(_tableIndexesRefCount, keyPropertyNames, rangePropertyNames, optionalIndexHintInstructions);
	        if (pair == null) {
	            return null;
	        }
	        var tableFound = ((EventTableIndexRepositoryEntry) pair.Second).Table;
	        return new Pair<IndexMultiKey, EventTableAndNamePair>(pair.First, new EventTableAndNamePair(tableFound, pair.Second.OptionalIndexName));
	    }

	    public IndexMultiKey[] GetIndexDescriptors()
	    {
	        return _tableIndexesRefCount.Keys.ToArray();
	    }

	    public void ValidateAddExplicitIndex(bool unique, string indexName, IList<CreateIndexItem> columns, EventType eventType, IEnumerable<EventBean> dataWindowContents, AgentInstanceContext agentInstanceContext, bool allowIndexExists, object optionalSerde)
	    {
	        if (_explicitIndexes.ContainsKey(indexName)) {
	            if (allowIndexExists) {
	                return;
	            }
	            throw new ExprValidationException("Index by name '" + indexName + "' already exists");
	        }

	        var desc = EventTableIndexUtil.ValidateCompileExplicitIndex(unique, columns, eventType);
	        AddExplicitIndex(indexName, desc, eventType, dataWindowContents, agentInstanceContext, optionalSerde);
	    }

        public void AddExplicitIndex(string indexName, EventTableCreateIndexDesc desc, EventType eventType, IEnumerable<EventBean> dataWindowContents, AgentInstanceContext agentInstanceContext, object optionalSerde)
        {
	        var pair = AddExplicitIndexOrReuse(desc.IsUnique, desc.HashProps, desc.BtreeProps, dataWindowContents, eventType, indexName, agentInstanceContext, optionalSerde);
	        _explicitIndexes.Put(indexName, pair.Second.EventTable);
	    }

	    public EventTable GetExplicitIndexByName(string indexName)
        {
	        return _explicitIndexes.Get(indexName);
	    }

	    public EventTable GetIndexByDesc(IndexMultiKey indexKey)
        {
	        var entry = _tableIndexesRefCount.Get(indexKey);
	        if (entry == null) {
	            return null;
	        }
	        return entry.Table;
	    }

        private Pair<IndexMultiKey, EventTableAndNamePair> AddIndex(bool unique, IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps, IEnumerable<EventBean> prefilledEvents, EventType indexedType, string indexName, bool mustCoerce, AgentInstanceContext agentInstanceContext, object optionalSerde)
        {
	        // not resolved as full match and not resolved as unique index match, allocate
	        var indexPropKey = new IndexMultiKey(unique, hashProps, btreeProps);

	        var indexedPropDescs = hashProps.ToArray();
	        var indexProps = IndexedPropDesc.GetIndexProperties(indexedPropDescs);
	        var indexCoercionTypes = IndexedPropDesc.GetCoercionTypes(indexedPropDescs);
	        if (!mustCoerce) {
	            indexCoercionTypes = null;
	        }

	        var rangePropDescs = btreeProps.ToArray();
	        var rangeProps = IndexedPropDesc.GetIndexProperties(rangePropDescs);
	        var rangeCoercionTypes = IndexedPropDesc.GetCoercionTypes(rangePropDescs);

	        var indexItem = new QueryPlanIndexItem(indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes, false);
	        var table = EventTableUtil.BuildIndex(agentInstanceContext, 0, indexItem, indexedType, true, unique, indexName, optionalSerde, false);

	        // fill table since its new
	        var events = new EventBean[1];
	        foreach (var prefilledEvent in prefilledEvents)
	        {
	            events[0] = prefilledEvent;
	            table.Add(events);
	        }

	        // add table
	        _tables.Add(table);

	        // add index, reference counted
	        _tableIndexesRefCount.Put(indexPropKey, new EventTableIndexRepositoryEntry(indexName, table));

	        return new Pair<IndexMultiKey, EventTableAndNamePair>(indexPropKey, new EventTableAndNamePair(table, indexName));
	    }

	    public string[] GetExplicitIndexNames()
        {
	        return _explicitIndexes.Keys.ToArray();
	    }

	    public void RemoveIndex(IndexMultiKey index)
        {
	        var entry = _tableIndexesRefCount.Pluck(index);
	        if (entry != null) {
	            _tables.Remove(entry.Table);
	            if (entry.OptionalIndexName != null) {
	                _explicitIndexes.Remove(entry.OptionalIndexName);
	            }
	            entry.Table.Destroy();
	        }
	    }

	    public IndexMultiKey GetIndexByName(string indexName)
        {
	        foreach (var entry in _tableIndexesRefCount)
            {
	            if (entry.Value.OptionalIndexName == indexName)
                {
	                return entry.Key;
	            }
	        }
	        return null;
	    }

	    public void RemoveExplicitIndex(string indexName)
        {
            var eventTable = _explicitIndexes.Pluck(indexName);
	        if (eventTable != null) {
	            eventTable.Destroy();
	        }
	    }
	}
} // end of namespace
