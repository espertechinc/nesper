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
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.@join.hint;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class EventTableIndexUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IndexComparatorShortestPath INDEX_COMPARATOR_INSTANCE =
            new IndexComparatorShortestPath();

        public static QueryPlanIndexItemForge ValidateCompileExplicitIndex(
            string indexName,
            bool unique,
            IList<CreateIndexItem> columns,
            EventType eventType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IList<IndexedPropDesc> hashProps = new List<IndexedPropDesc>();
            IList<IndexedPropDesc> btreeProps = new List<IndexedPropDesc>();
            ISet<string> indexedColumns = new HashSet<string>();
            EventAdvancedIndexProvisionCompileTime advancedIndexProvisionDesc = null;

            foreach (var columnDesc in columns) {
                string indexType = columnDesc.IndexType.Trim();
                if (indexType.Equals(CreateIndexType.HASH.GetName(), StringComparison.InvariantCultureIgnoreCase) ||
                    indexType.Equals(CreateIndexType.BTREE.GetName(), StringComparison.InvariantCultureIgnoreCase)) {
                    ValidateBuiltin(columnDesc, eventType, hashProps, btreeProps, indexedColumns);
                }
                else {
                    if (advancedIndexProvisionDesc != null) {
                        throw new ExprValidationException("Nested advanced-type indexes are not supported");
                    }

                    advancedIndexProvisionDesc = ValidateAdvanced(
                        indexName, indexType, columnDesc, eventType, statementRawInfo, services);
                }
            }

            if (unique && !btreeProps.IsEmpty()) {
                throw new ExprValidationException("Combination of unique index with btree (range) is not supported");
            }

            if ((!btreeProps.IsEmpty() || !hashProps.IsEmpty()) && advancedIndexProvisionDesc != null) {
                throw new ExprValidationException(
                    "Combination of hash/btree columns an advanced-type indexes is not supported");
            }

            return new QueryPlanIndexItemForge(hashProps, btreeProps, unique, advancedIndexProvisionDesc, eventType);
        }

        private static EventAdvancedIndexProvisionCompileTime ValidateAdvanced(
            string indexName,
            string indexType,
            CreateIndexItem columnDesc,
            EventType eventType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // validate index expressions: valid and plain expressions
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(eventType, null, false);
            var validationContextColumns =
                new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services)
                    .WithDisablePropertyExpressionEventCollCache(true).Build();
            var columns = columnDesc.Expressions.ToArray();
            ExprNodeUtilityValidate.GetValidatedSubtree(
                ExprNodeOrigin.CREATEINDEXCOLUMN, columns, validationContextColumns);
            ExprNodeUtilityValidate.ValidatePlainExpression(ExprNodeOrigin.CREATEINDEXCOLUMN, columns);

            // validate parameters, may not depend on props
            ExprNode[] parameters = null;
            if (columnDesc.Parameters != null && !columnDesc.Parameters.IsEmpty()) {
                parameters = columnDesc.Parameters.ToArray();
                ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.CREATEINDEXPARAMETER, parameters, validationContextColumns);
                ExprNodeUtilityValidate.ValidatePlainExpression(ExprNodeOrigin.CREATEINDEXPARAMETER, parameters);

                // validate no stream dependency of parameters
                var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
                foreach (var param in columnDesc.Parameters) {
                    param.Accept(visitor);
                    if (!visitor.Refs.IsEmpty()) {
                        throw new ExprValidationException("Index parameters may not refer to event properties");
                    }
                }
            }

            // obtain provider
            AdvancedIndexFactoryProvider provider;
            try {
                provider = services.ImportServiceCompileTime.ResolveAdvancedIndexProvider(indexType);
            }
            catch (ImportException ex) {
                throw new ExprValidationException(ex.Message, ex);
            }

            return provider.ValidateEventIndex(indexName, indexType, columns, parameters);
        }

        private static void ValidateBuiltin(
            CreateIndexItem columnDesc,
            EventType eventType,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            ISet<string> indexedColumns)
        {
            if (columnDesc.Expressions.IsEmpty()) {
                throw new ExprValidationException("Invalid empty list of index expressions");
            }

            if (columnDesc.Expressions.Count > 1) {
                throw new ExprValidationException(
                    "Invalid multiple index expressions for index type '" + columnDesc.IndexType + "'");
            }

            ExprNode expression = columnDesc.Expressions[0];
            if (!(expression is ExprIdentNode)) {
                throw new ExprValidationException(
                    "Invalid index expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression) + "'");
            }

            var identNode = (ExprIdentNode) expression;
            if (identNode.FullUnresolvedName.Contains(".")) {
                throw new ExprValidationException(
                    "Invalid index expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression) + "'");
            }

            var columnName = identNode.FullUnresolvedName;
            Type type = Boxing.GetBoxedType(eventType.GetPropertyType(columnName));
            if (type == null) {
                throw new ExprValidationException("Property named '" + columnName + "' not found");
            }

            if (!indexedColumns.Add(columnName)) {
                throw new ExprValidationException(
                    "Property named '" + columnName + "' has been declared more then once");
            }

            var desc = new IndexedPropDesc(columnName, type);
            string indexType = columnDesc.IndexType;
            if (indexType.Equals(CreateIndexType.HASH.GetName(), StringComparison.InvariantCultureIgnoreCase)) {
                hashProps.Add(desc);
            }
            else {
                btreeProps.Add(desc);
            }
        }

        public static IndexMultiKey FindIndexConsiderTyping(
            IDictionary<IndexMultiKey, EventTableIndexMetadataEntry> tableIndexesRefCount,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            if (hashProps.IsEmpty() && btreeProps.IsEmpty()) {
                throw new ArgumentException("Invalid zero element list for hash and btree columns");
            }

            var indexCandidates =
                (IDictionary<IndexMultiKey, EventTableIndexRepositoryEntry>) FindCandidates(
                    tableIndexesRefCount, hashProps, btreeProps);

            // if there are hints, follow these
            if (optionalIndexHintInstructions != null) {
                var found = FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
                if (found != null) {
                    return found;
                }
            }

            // Get an existing table, if any, matching the exact requirement, prefer unique
            var indexPropKeyMatch = FindExactMatchNameAndType(
                tableIndexesRefCount.Keys, true, hashProps, btreeProps);
            if (indexPropKeyMatch == null) {
                indexPropKeyMatch = FindExactMatchNameAndType(
                    tableIndexesRefCount.Keys, false, hashProps, btreeProps);
            }

            if (indexPropKeyMatch != null) {
                return indexPropKeyMatch;
            }

            if (indexCandidates.IsEmpty()) {
                return null;
            }

            return GetBestCandidate(
                (IDictionary<IndexMultiKey, EventTableIndexEntryBase>) (IDictionary<object, object>) indexCandidates).First;
        }

        public static Pair<IndexMultiKey, EventTableIndexEntryBase> FindIndexBestAvailable<T>(
            IDictionary<IndexMultiKey, T> tablesAvailable,
            ISet<string> keyPropertyNames,
            ISet<string> rangePropertyNames,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
            where T : EventTableIndexEntryBase
        {
            if (keyPropertyNames.IsEmpty() && rangePropertyNames.IsEmpty()) {
                return null;
            }

            // determine candidates
            IList<IndexedPropDesc> hashProps = new List<IndexedPropDesc>();
            foreach (var keyPropertyName in keyPropertyNames) {
                hashProps.Add(new IndexedPropDesc(keyPropertyName, null));
            }

            IList<IndexedPropDesc> rangeProps = new List<IndexedPropDesc>();
            foreach (var rangePropertyName in rangePropertyNames) {
                rangeProps.Add(new IndexedPropDesc(rangePropertyName, null));
            }

            var indexCandidates =
                (IDictionary<IndexMultiKey, EventTableIndexEntryBase>) FindCandidates(
                    tablesAvailable, hashProps, rangeProps);

            // handle hint
            if (optionalIndexHintInstructions != null) {
                var found = FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
                if (found != null) {
                    return GetPair(tablesAvailable, found);
                }
            }

            // no candidates
            if (indexCandidates == null || indexCandidates.IsEmpty()) {
                if (Log.IsDebugEnabled) {
                    Log.Debug("No index found.");
                }

                return null;
            }

            return GetBestCandidate(indexCandidates);
        }

        private static Pair<IndexMultiKey, EventTableIndexEntryBase> GetBestCandidate(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexCandidates)
        {
            // take the table that has a unique index
            IList<IndexMultiKey> indexes = new List<IndexMultiKey>();
            foreach (var entry in indexCandidates) {
                if (entry.Key.IsUnique) {
                    indexes.Add(entry.Key);
                }
            }

            if (!indexes.IsEmpty()) {
                Collections.SortInPlace(indexes, INDEX_COMPARATOR_INSTANCE);
                return GetPair(indexCandidates, indexes[0]);
            }

            // take the best available table
            indexes.Clear();
            indexes.AddAll(indexCandidates.Keys);
            if (indexes.Count > 1) {
                Collections.SortInPlace(indexes, INDEX_COMPARATOR_INSTANCE);
            }

            return GetPair(indexCandidates, indexes[0]);
        }

        public static IndexMultiKey FindByIndexHint<T>(
            IDictionary<IndexMultiKey, T> indexCandidates,
            IList<IndexHintInstruction> instructions)
            where T : EventTableIndexEntryBase
        {
            foreach (var instruction in instructions) {
                if (instruction is IndexHintInstructionIndexName) {
                    var indexName = ((IndexHintInstructionIndexName) instruction).IndexName;
                    var found = FindExplicitIndexByName(indexCandidates, indexName);
                    if (found != null) {
                        return found;
                    }
                }

                if (instruction is IndexHintInstructionExplicit) {
                    var found = FindExplicitIndexAnyName(indexCandidates);
                    if (found != null) {
                        return found;
                    }
                }

                if (instruction is IndexHintInstructionBust) {
                    throw new EPException("Failed to plan index access, index hint busted out");
                }
            }

            return null;
        }

        public static IndexMultiKey FindExactMatchNameAndType(
            ICollection<IndexMultiKey> indexMultiKeys,
            IndexMultiKey proposed)
        {
            foreach (var existing in indexMultiKeys) {
                if (existing.Equals(proposed)) {
                    return existing;
                }
            }

            return null;
        }

        public static IndexMultiKey FindExactMatchNameAndType(
            ICollection<IndexMultiKey> indexMultiKeys,
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps)
        {
            foreach (var existing in indexMultiKeys) {
                if (IsExactMatch(existing, unique, hashProps, btreeProps)) {
                    return existing;
                }
            }

            return null;
        }

        private static IDictionary<IndexMultiKey, T> FindCandidates<T>(
            IDictionary<IndexMultiKey, T> indexes,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps)
            where T : EventTableIndexEntryBase
        {
            IDictionary<IndexMultiKey, T> indexCandidates =
                new Dictionary<IndexMultiKey, T>();
            foreach (var entry in indexes) {
                if (entry.Key.AdvancedIndexDesc != null) {
                    continue;
                }

                var matches = IndexMatchesProvided(entry.Key, hashProps, btreeProps);
                if (matches) {
                    indexCandidates.Put(entry.Key, entry.Value);
                }
            }

            return indexCandidates;
        }

        private static IndexMultiKey FindExplicitIndexByName<T>(
            IDictionary<IndexMultiKey, T> indexCandidates,
            string name)
            where T : EventTableIndexEntryBase
        {
            foreach (var entry in indexCandidates) {
                if (entry.Value.OptionalIndexName != null && entry.Value.OptionalIndexName.Equals(name)) {
                    return entry.Key;
                }
            }

            return null;
        }

        private static IndexMultiKey FindExplicitIndexAnyName<T>(IDictionary<IndexMultiKey, T> indexCandidates)
            where T : EventTableIndexEntryBase
        {
            foreach (var entry in indexCandidates) {
                if (entry.Value.OptionalIndexName != null) {
                    return entry.Key;
                }
            }

            return null;
        }

        private static bool IndexHashIsProvided(
            IndexedPropDesc hashPropIndexed,
            IList<IndexedPropDesc> hashPropsProvided)
        {
            foreach (var hashPropProvided in hashPropsProvided) {
                var nameMatch = hashPropProvided.IndexPropName.Equals(hashPropIndexed.IndexPropName);
                var typeMatch = true;
                if (hashPropProvided.CoercionType != null && !TypeHelper.IsSubclassOrImplementsInterface(
                        hashPropProvided.CoercionType.GetBoxedType(),
                        hashPropIndexed.CoercionType.GetBoxedType())) {
                    typeMatch = false;
                }

                if (nameMatch && typeMatch) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExactMatch(
            IndexMultiKey existing,
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps)
        {
            if (existing.IsUnique != unique) {
                return false;
            }

            if (!IndexedPropDesc.Compare(existing.HashIndexedProps, hashProps)) {
                return false;
            }

            if (!IndexedPropDesc.Compare(existing.RangeIndexedProps, btreeProps)) {
                return false;
            }

            return true;
        }

        private static bool IndexMatchesProvided(
            IndexMultiKey indexDesc,
            IList<IndexedPropDesc> hashPropsProvided,
            IList<IndexedPropDesc> rangePropsProvided)
        {
            var hashPropIndexedList = indexDesc.HashIndexedProps;
            foreach (var hashPropIndexed in hashPropIndexedList) {
                var foundHashProp = IndexHashIsProvided(hashPropIndexed, hashPropsProvided);
                if (!foundHashProp) {
                    return false;
                }
            }

            var rangePropIndexedList = indexDesc.RangeIndexedProps;
            foreach (var rangePropIndexed in rangePropIndexedList) {
                var foundRangeProp = IndexHashIsProvided(rangePropIndexed, rangePropsProvided);
                if (!foundRangeProp) {
                    return false;
                }
            }

            return true;
        }

        private static Pair<IndexMultiKey, EventTableIndexEntryBase> GetPair<T>(
            IDictionary<IndexMultiKey, T> tableIndexesRefCount,
            IndexMultiKey indexMultiKey)
            where T : EventTableIndexEntryBase
        {
            EventTableIndexEntryBase indexFound = tableIndexesRefCount.Get(indexMultiKey);
            return new Pair<IndexMultiKey, EventTableIndexEntryBase>(indexMultiKey, indexFound);
        }

        [Serializable]
        private class IndexComparatorShortestPath : IComparer<IndexMultiKey>
        {
            public int Compare(
                IndexMultiKey o1,
                IndexMultiKey o2)
            {
                var indexedProps1 = IndexedPropDesc.GetIndexProperties(o1.HashIndexedProps);
                var indexedProps2 = IndexedPropDesc.GetIndexProperties(o2.HashIndexedProps);
                if (indexedProps1.Length > indexedProps2.Length) {
                    return 1; // sort desc by count columns
                }

                if (indexedProps1.Length == indexedProps2.Length) {
                    return 0;
                }

                return -1;
            }
        }
    }
} // end of namespace