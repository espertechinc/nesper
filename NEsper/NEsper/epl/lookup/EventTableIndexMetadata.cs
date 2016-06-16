///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class EventTableIndexMetadata
    {
        private readonly IDictionary<IndexMultiKey, EventTableIndexMetadataEntry> indexes = new Dictionary<IndexMultiKey, EventTableIndexMetadataEntry>();
    
        public EventTableIndexMetadata()
        {
        }

        public void AddIndex(bool isPrimary, IndexMultiKey indexMultiKey, string explicitIndexName, string statementName, bool failIfExists, QueryPlanIndexItem optionalQueryPlanIndexItem)
        {
            if (GetIndexByName(explicitIndexName) != null) {
                throw new ExprValidationException("An index by name '" + explicitIndexName + "' already exists");
            }
            if (indexes.ContainsKey(indexMultiKey)) {
                if (failIfExists) {
                    throw new ExprValidationException("An index for the same columns already exists");
                }
                return;
            }
            EventTableIndexMetadataEntry entry = new EventTableIndexMetadataEntry(explicitIndexName, isPrimary, optionalQueryPlanIndexItem);
            entry.AddReferringStatement(statementName);
            indexes.Put(indexMultiKey, entry);
        }

        public IDictionary<IndexMultiKey, EventTableIndexMetadataEntry> Indexes
        {
            get { return indexes; }
        }

        public void RemoveIndex(IndexMultiKey imk)
        {
            indexes.Remove(imk);
        }
    
        public bool RemoveIndexReference(IndexMultiKey index, string referringStatementName)
        {
            var entry = indexes.Get(index);
            if (entry == null) {
                return false;
            }
            return entry.RemoveReferringStatement(referringStatementName);
        }
    
        public void AddIndexReference(string indexName, string statementName)
        {
            var entry = FindIndex(indexName);
            if (entry == null) {
                return;
            }
            entry.Value.Value.AddReferringStatement(statementName);
        }
    
        public void AddIndexReference(IndexMultiKey indexMultiKey, string statementName)
        {
            EventTableIndexMetadataEntry entry = indexes.Get(indexMultiKey);
            if (entry == null) {
                return;
            }
            entry.AddReferringStatement(statementName);
        }
    
        public IndexMultiKey GetIndexByName(string indexName)
        {
            var entry = FindIndex(indexName);
            if (entry == null) {
                return null;
            }
            return entry.Value.Key;
        }
    
        public ICollection<string> GetRemoveRefIndexesDereferenced(string statementName)
        {
            ICollection<string> indexNamesDerrefd = null;
            foreach (var entry in indexes) {
                bool last = entry.Value.RemoveReferringStatement(statementName);
                if (last) {
                    if (indexNamesDerrefd == null) {
                        indexNamesDerrefd = new ArrayDeque<string>(2);
                    }
                    indexNamesDerrefd.Add(entry.Value.OptionalIndexName);
                }
            }
            if (indexNamesDerrefd == null) {
                return Collections.GetEmptyList<string>();
            }
            foreach (string name in indexNamesDerrefd) {
                RemoveIndex(GetIndexByName(name));
            }
            return indexNamesDerrefd;
        }
    
        private KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry>? FindIndex(string indexName)
        {
            foreach (var entry in indexes)
            {
                if (entry.Value.OptionalIndexName != null && entry.Value.OptionalIndexName == indexName)
                {
                    return entry;
                }
            }
            return null;
        }

        public string[][] UniqueIndexProps
        {
            get
            {
                var uniques = new ArrayDeque<string[]>(2);
                foreach (var entry in indexes)
                {
                    if (entry.Key.IsUnique)
                    {
                        var props = new string[entry.Key.HashIndexedProps.Length];
                        for (int i = 0; i < entry.Key.HashIndexedProps.Length; i++)
                        {
                            props[i] = entry.Key.HashIndexedProps[i].IndexPropName;
                        }
                        uniques.Add(props);
                    }
                }
                return uniques.ToArray();
            }
        }
    }
}
