///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.lookup
{
    public class EventTableIndexUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IndexComparatorShortestPath INDEX_COMPARATOR_INSTANCE = new IndexComparatorShortestPath();

        public static EventTableCreateIndexDesc ValidateCompileExplicitIndex(bool unique, IList<CreateIndexItem> columns, EventType eventType)
        {
            IList<IndexedPropDesc> hashProps = new List<IndexedPropDesc>();
            IList<IndexedPropDesc> btreeProps = new List<IndexedPropDesc>();

            ISet<string> indexed = new HashSet<string>();
            foreach (var columnDesc in columns)
            {
                var columnName = columnDesc.Name;

                var type = eventType.GetPropertyType(columnName).GetBoxedType();
                if (type == null)
                {
                    throw new ExprValidationException("Property named '" + columnName + "' not found");
                }
                if (!indexed.Add(columnName))
                {
                    throw new ExprValidationException("Property named '" + columnName + "' has been declared more then once");
                }

                var desc = new IndexedPropDesc(columnName, type);
                if (columnDesc.Type == CreateIndexType.HASH)
                {
                    hashProps.Add(desc);
                }
                else
                {
                    btreeProps.Add(desc);
                }
            }

            if (unique && !btreeProps.IsEmpty())
            {
                throw new ExprValidationException("Combination of unique index with btree (range) is not supported");
            }
            return new EventTableCreateIndexDesc(hashProps, btreeProps, unique);
        }

        public static IndexMultiKey FindIndexConsiderTyping(IDictionary<IndexMultiKey, EventTableIndexMetadataEntry> tableIndexesRefCount,
                                                            IList<IndexedPropDesc> hashProps,
                                                            IList<IndexedPropDesc> btreeProps,
                                                            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            if (hashProps.IsEmpty() && btreeProps.IsEmpty())
            {
                throw new ArgumentException("Invalid zero element list for hash and btree columns");
            }

            var indexCandidates = //(IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry>) 
                EventTableIndexUtil.FindCandidates(tableIndexesRefCount, hashProps, btreeProps);

            // if there are hints, follow these
            if (optionalIndexHintInstructions != null)
            {
                var found = EventTableIndexUtil.FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
                if (found != null)
                {
                    return found;
                }
            }

            // Get an existing table, if any, matching the exact requirement, prefer unique
            var indexPropKeyMatch = EventTableIndexUtil.FindExactMatchNameAndType(tableIndexesRefCount.Keys, true, hashProps, btreeProps);
            if (indexPropKeyMatch == null)
            {
                indexPropKeyMatch = EventTableIndexUtil.FindExactMatchNameAndType(tableIndexesRefCount.Keys, false, hashProps, btreeProps);
            }
            if (indexPropKeyMatch != null)
            {
                return indexPropKeyMatch;
            }

            if (indexCandidates.IsEmpty())
            {
                return null;
            }

            var transIndexCandidates = indexCandidates.Transform<IndexMultiKey, EventTableIndexEntryBase, IndexMultiKey, EventTableIndexMetadataEntry>(
                k => k, v => v,
                k => k, v => (EventTableIndexMetadataEntry)v);

            return GetBestCandidate(transIndexCandidates).First;
        }

        public static Pair<IndexMultiKey, EventTableIndexEntryBase> FindIndexBestAvailable<T>(
            IDictionary<IndexMultiKey, T> tablesAvailable,
            ISet<string> keyPropertyNames,
            ISet<string> rangePropertyNames,
            IList<IndexHintInstruction> optionalIndexHintInstructions) where T : EventTableIndexEntryBase
        {
            if (keyPropertyNames.IsEmpty() && rangePropertyNames.IsEmpty())
            {
                return null;
            }

            // determine candidates
            IList<IndexedPropDesc> hashProps = new List<IndexedPropDesc>();
            foreach (var keyPropertyName in keyPropertyNames)
            {
                hashProps.Add(new IndexedPropDesc(keyPropertyName, null));
            }
            IList<IndexedPropDesc> rangeProps = new List<IndexedPropDesc>();
            foreach (var rangePropertyName in rangePropertyNames)
            {
                rangeProps.Add(new IndexedPropDesc(rangePropertyName, null));
            }

            var indexCandidates = EventTableIndexUtil
                .FindCandidates(tablesAvailable, hashProps, rangeProps)
                .Transform<IndexMultiKey, EventTableIndexEntryBase, IndexMultiKey, T>(
                    k => k, v => v,
                    k => k, v => v as T);

            // handle hint
            if (optionalIndexHintInstructions != null)
            {
                var found = EventTableIndexUtil.FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
                if (found != null)
                {
                    return GetPair(tablesAvailable, found);
                }
            }

            // no candidates
            if (indexCandidates == null || indexCandidates.IsEmpty())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("No index found.");
                }
                return null;
            }

            return GetBestCandidate(indexCandidates);
        }

        private static Pair<IndexMultiKey, EventTableIndexEntryBase> GetBestCandidate(IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexCandidates)
        {
            // take the table that has a unique index
            IList<IndexMultiKey> indexes = new List<IndexMultiKey>();
            foreach (var entry in indexCandidates)
            {
                if (entry.Key.IsUnique)
                {
                    indexes.Add(entry.Key);
                }
            }
            if (!indexes.IsEmpty())
            {
                indexes.SortInPlace(INDEX_COMPARATOR_INSTANCE);
                return GetPair(indexCandidates, indexes[0]);
            }

            // take the best available table
            indexes.Clear();
            indexes.AddAll(indexCandidates.Keys);
            if (indexes.Count > 1)
            {
                indexes.SortInPlace(INDEX_COMPARATOR_INSTANCE);
            }
            return GetPair(indexCandidates, indexes[0]);
        }

        public static IndexMultiKey FindByIndexHint<T>(IDictionary<IndexMultiKey, T> indexCandidates, IList<IndexHintInstruction> instructions)
            where T : EventTableIndexEntryBase
        {
            foreach (var instruction in instructions)
            {
                if (instruction is IndexHintInstructionIndexName)
                {
                    var indexName = ((IndexHintInstructionIndexName)instruction).IndexName;
                    var found = FindExplicitIndexByName(indexCandidates, indexName);
                    if (found != null)
                    {
                        return found;
                    }
                }
                if (instruction is IndexHintInstructionExplicit)
                {
                    var found = FindExplicitIndexAnyName(indexCandidates);
                    if (found != null)
                    {
                        return found;
                    }
                }
                if (instruction is IndexHintInstructionBust)
                {
                    throw new EPException("Failed to plan index access, index hint busted out");
                }
            }
            return null;
        }

        public static IndexMultiKey FindExactMatchNameAndType(IEnumerable<IndexMultiKey> indexMultiKeys, bool unique, IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps)
        {
            foreach (var existing in indexMultiKeys)
            {
                if (IsExactMatch(existing, unique, hashProps, btreeProps))
                {
                    return existing;
                }
            }
            return null;
        }

        private static IDictionary<IndexMultiKey, T> FindCandidates<T>(IDictionary<IndexMultiKey, T> indexes, IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps)
            where T : EventTableIndexEntryBase
        {
            IDictionary<IndexMultiKey, T> indexCandidates = new Dictionary<IndexMultiKey, T>();
            foreach (var entry in indexes)
            {
                var matches = IndexMatchesProvided(entry.Key, hashProps, btreeProps);
                if (matches)
                {
                    indexCandidates.Put(entry.Key, entry.Value);
                }
            }
            return indexCandidates;
        }

        private static IndexMultiKey FindExplicitIndexByName<T>(IDictionary<IndexMultiKey, T> indexCandidates, string name)
            where T : EventTableIndexEntryBase
        {
            foreach (var entry in indexCandidates)
            {
                if (entry.Value.OptionalIndexName != null && entry.Value.OptionalIndexName.Equals(name))
                {
                    return entry.Key;
                }
            }
            return null;
        }

        private static IndexMultiKey FindExplicitIndexAnyName<T>(IDictionary<IndexMultiKey, T> indexCandidates)
            where T : EventTableIndexEntryBase
        {
            foreach (var entry in indexCandidates)
            {
                if (entry.Value.OptionalIndexName != null)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        private static bool IndexHashIsProvided(IndexedPropDesc hashPropIndexed, IList<IndexedPropDesc> hashPropsProvided)
        {
            foreach (var hashPropProvided in hashPropsProvided)
            {
                var nameMatch = hashPropProvided.IndexPropName.Equals(hashPropIndexed.IndexPropName);
                var typeMatch = true;
                if (hashPropProvided.CoercionType != null && !TypeHelper.IsSubclassOrImplementsInterface(hashPropProvided.CoercionType.GetBoxedType(), hashPropIndexed.CoercionType.GetBoxedType()))
                {
                    typeMatch = false;
                }
                if (nameMatch && typeMatch)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsExactMatch(IndexMultiKey existing, bool unique, IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps)
        {
            if (existing.IsUnique != unique)
            {
                return false;
            }
            var keyPropCompare = IndexedPropDesc.Compare(existing.HashIndexedProps, hashProps);
            return keyPropCompare && IndexedPropDesc.Compare(existing.RangeIndexedProps, btreeProps);
        }

        private static bool IndexMatchesProvided(IndexMultiKey indexDesc, IList<IndexedPropDesc> hashPropsProvided, IList<IndexedPropDesc> rangePropsProvided)
        {
            var hashPropIndexedList = indexDesc.HashIndexedProps;
            foreach (var hashPropIndexed in hashPropIndexedList)
            {
                var foundHashProp = IndexHashIsProvided(hashPropIndexed, hashPropsProvided);
                if (!foundHashProp)
                {
                    return false;
                }
            }

            var rangePropIndexedList = indexDesc.RangeIndexedProps;
            foreach (var rangePropIndexed in rangePropIndexedList)
            {
                var foundRangeProp = IndexHashIsProvided(rangePropIndexed, rangePropsProvided);
                if (!foundRangeProp)
                {
                    return false;
                }
            }

            return true;
        }

        private static Pair<IndexMultiKey, EventTableIndexEntryBase> GetPair<T>(IDictionary<IndexMultiKey, T> tableIndexesRefCount, IndexMultiKey indexMultiKey)
            where T : EventTableIndexEntryBase
        {
            EventTableIndexEntryBase indexFound = tableIndexesRefCount.Get(indexMultiKey);
            return new Pair<IndexMultiKey, EventTableIndexEntryBase>(indexMultiKey, indexFound);
        }

        [Serializable]
        internal class IndexComparatorShortestPath : IComparer<IndexMultiKey>
        {
            public int Compare(IndexMultiKey o1, IndexMultiKey o2)
            {
                var indexedProps1 = IndexedPropDesc.GetIndexProperties(o1.HashIndexedProps);
                var indexedProps2 = IndexedPropDesc.GetIndexProperties(o2.HashIndexedProps);
                if (indexedProps1.Length > indexedProps2.Length)
                {
                    return 1;  // sort desc by count columns
                }
                if (indexedProps1.Length == indexedProps2.Length)
                {
                    return 0;
                }
                return -1;
            }
        }
    }
}
