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
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.lookup
{
    public class EventTableIndexUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IndexComparatorShortestPath INDEX_COMPARATOR_INSTANCE = new IndexComparatorShortestPath();
    
        public static QueryPlanIndexItem ValidateCompileExplicitIndex(
            string indexName, 
            bool unique, 
            IList<CreateIndexItem> columns, 
            EventType eventType, 
            StatementContext statementContext)
        {
            var hashProps = new List<IndexedPropDesc>();
            var btreeProps = new List<IndexedPropDesc>();
            var indexedColumns = new HashSet<string>();
            EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc = null;
    
            foreach (CreateIndexItem columnDesc in columns) {
                string indexType = columnDesc.IndexType.ToLowerInvariant();
                if ((indexType == CreateIndexType.HASH.GetNameInvariant()) ||
                    (indexType == CreateIndexType.BTREE.GetNameInvariant())) {
                    ValidateBuiltin(columnDesc, eventType, hashProps, btreeProps, indexedColumns);
                } else {
                    if (advancedIndexProvisionDesc != null) {
                        throw new ExprValidationException("Nested advanced-type indexes are not supported");
                    }
                    advancedIndexProvisionDesc = ValidateAdvanced(indexName, indexType, columnDesc, eventType, statementContext);
                }
            }
    
            if (unique && !btreeProps.IsEmpty()) {
                throw new ExprValidationException("Combination of unique index with btree (range) is not supported");
            }
            if ((!btreeProps.IsEmpty() || !hashProps.IsEmpty()) && advancedIndexProvisionDesc != null) {
                throw new ExprValidationException("Combination of hash/btree columns an advanced-type indexes is not supported");
            }
            return new QueryPlanIndexItem(hashProps, btreeProps, unique, advancedIndexProvisionDesc);
        }
    
        private static EventAdvancedIndexProvisionDesc ValidateAdvanced(
            string indexName, 
            string indexType, 
            CreateIndexItem columnDesc, 
            EventType eventType, 
            StatementContext statementContext)
        {
            // validate index expressions: valid and plain expressions
            ExprValidationContext validationContextColumns = GetValidationContext(eventType, statementContext);
            ExprNode[] columns = columnDesc.Expressions.ToArray();
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CREATEINDEXCOLUMN, columns, validationContextColumns);
            ExprNodeUtility.ValidatePlainExpression(ExprNodeOrigin.CREATEINDEXCOLUMN, columns);
    
            // validate parameters, may not depend on props
            ExprNode[] parameters = null;
            if (columnDesc.Parameters != null && !columnDesc.Parameters.IsEmpty()) {
                parameters = columnDesc.Parameters.ToArray();
                ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CREATEINDEXPARAMETER, parameters, validationContextColumns);
                ExprNodeUtility.ValidatePlainExpression(ExprNodeOrigin.CREATEINDEXPARAMETER, parameters);
    
                // validate no stream dependency of parameters
                var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
                foreach (ExprNode param in columnDesc.Parameters) {
                    param.Accept(visitor);
                    if (!visitor.GetRefs().IsEmpty()) {
                        throw new ExprValidationException("Index parameters may not refer to event properties");
                    }
                }
            }
    
            // obtain provider
            AdvancedIndexFactoryProvider provider;
            try {
                provider = statementContext.EngineImportService.ResolveAdvancedIndexProvider(indexType);
            } catch (EngineImportException ex) {
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
                throw new ExprValidationException("Invalid multiple index expressions for index type '" + columnDesc.IndexType + "'");
            }
            ExprNode expression = columnDesc.Expressions[0];
            if (!(expression is ExprIdentNode)) {
                throw new ExprValidationException("Invalid index expression '" + expression.ToExpressionStringMinPrecedenceSafe() + "'");
            }
            ExprIdentNode identNode = (ExprIdentNode) expression;
            if (identNode.FullUnresolvedName.Contains(".")) {
                throw new ExprValidationException("Invalid index expression '" + expression.ToExpressionStringMinPrecedenceSafe() + "'");
            }
    
            string columnName = identNode.FullUnresolvedName;
            Type type = eventType.GetPropertyType(columnName).GetBoxedType();
            if (type == null) {
                throw new ExprValidationException("Property named '" + columnName + "' not found");
            }
            if (!indexedColumns.Add(columnName)) {
                throw new ExprValidationException("Property named '" + columnName + "' has been declared more then once");
            }
    
            var desc = new IndexedPropDesc(columnName, type);
            string indexType = columnDesc.IndexType;
            if (string.Equals(indexType, CreateIndexType.HASH.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                hashProps.Add(desc);
            } else {
                btreeProps.Add(desc);
            }
        }
    
        public static IndexMultiKey FindIndexConsiderTyping(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> tableIndexesRefCount,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            if (hashProps.IsEmpty() && btreeProps.IsEmpty()) {
                throw new ArgumentException("Invalid zero element list for hash and btree columns");
            }
    
            var indexCandidates = FindCandidates(tableIndexesRefCount, hashProps, btreeProps);
    
            // if there are hints, follow these
            if (optionalIndexHintInstructions != null) {
                IndexMultiKey found = FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
                if (found != null) {
                    return found;
                }
            }
    
            // Get an existing table, if any, matching the exact requirement, prefer unique
            IndexMultiKey indexPropKeyMatch = FindExactMatchNameAndType(tableIndexesRefCount.Keys, true, hashProps, btreeProps, null);
            if (indexPropKeyMatch == null) {
                indexPropKeyMatch = FindExactMatchNameAndType(tableIndexesRefCount.Keys, false, hashProps, btreeProps, null);
            }
            if (indexPropKeyMatch != null) {
                return indexPropKeyMatch;
            }
    
            if (indexCandidates.IsEmpty()) {
                return null;
            }
    
            return GetBestCandidate(indexCandidates).First;
        }
    
        public static Pair<IndexMultiKey, EventTableIndexEntryBase> FindIndexBestAvailable(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> tablesAvailable,
            ISet<string> keyPropertyNames,
            ISet<string> rangePropertyNames,
            IList<IndexHintInstruction> optionalIndexHintInstructions)
        {
            if (keyPropertyNames.IsEmpty() && rangePropertyNames.IsEmpty()) {
                return null;
            }
    
            // determine candidates
            var hashProps = new List<IndexedPropDesc>();
            foreach (string keyPropertyName in keyPropertyNames) {
                hashProps.Add(new IndexedPropDesc(keyPropertyName, null));
            }
            var rangeProps = new List<IndexedPropDesc>();
            foreach (string rangePropertyName in rangePropertyNames) {
                rangeProps.Add(new IndexedPropDesc(rangePropertyName, null));
            }
            var indexCandidates = FindCandidates(tablesAvailable, hashProps, rangeProps);
    
            // handle hint
            if (optionalIndexHintInstructions != null) {
                IndexMultiKey found = FindByIndexHint(indexCandidates, optionalIndexHintInstructions);
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
            var indexes = new List<IndexMultiKey>();
            foreach (var entry in indexCandidates) {
                if (entry.Key.IsUnique) {
                    indexes.Add(entry.Key);
                }
            }
            if (!indexes.IsEmpty()) {
                indexes.Sort(INDEX_COMPARATOR_INSTANCE);
                return GetPair(indexCandidates, indexes[0]);
            }
    
            // take the best available table
            indexes.Clear();
            indexes.AddAll(indexCandidates.Keys);
            if (indexes.Count > 1) {
                indexes.Sort(INDEX_COMPARATOR_INSTANCE);
            }
            return GetPair(indexCandidates, indexes[0]);
        }
    
        public static IndexMultiKey FindByIndexHint(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexCandidates, 
            IList<IndexHintInstruction> instructions)
        {
            foreach (IndexHintInstruction instruction in instructions) {
                if (instruction is IndexHintInstructionIndexName name) {
                    string indexName = name.IndexName;
                    IndexMultiKey found = FindExplicitIndexByName(indexCandidates, indexName);
                    if (found != null) {
                        return found;
                    }
                }
                if (instruction is IndexHintInstructionExplicit) {
                    IndexMultiKey found = FindExplicitIndexAnyName(indexCandidates);
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
            bool unique, 
            IList<IndexedPropDesc> hashProps, 
            IList<IndexedPropDesc> btreeProps, 
            AdvancedIndexDesc advancedIndexDesc)
        {
            foreach (IndexMultiKey existing in indexMultiKeys) {
                if (IsExactMatch(existing, unique, hashProps, btreeProps, advancedIndexDesc)) {
                    return existing;
                }
            }
            return null;
        }
    
        private static IDictionary<IndexMultiKey, EventTableIndexEntryBase> FindCandidates(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexes, 
            IList<IndexedPropDesc> hashProps, 
            IList<IndexedPropDesc> btreeProps)
        {
            var indexCandidates = new Dictionary<IndexMultiKey, EventTableIndexEntryBase>();
            foreach (var entry in indexes) {
                if (entry.Key.AdvancedIndexDesc != null) {
                    continue;
                }
                bool matches = IndexMatchesProvided(entry.Key, hashProps, btreeProps);
                if (matches) {
                    indexCandidates.Put(entry.Key, entry.Value);
                }
            }
            return indexCandidates;
        }
    
        private static IndexMultiKey FindExplicitIndexByName(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexCandidates, string name)
        {
            foreach (var entry in indexCandidates) {
                if (entry.Value.OptionalIndexName != null && entry.Value.OptionalIndexName.Equals(name)) {
                    return entry.Key;
                }
            }
            return null;
        }
    
        private static IndexMultiKey FindExplicitIndexAnyName(IDictionary<IndexMultiKey, EventTableIndexEntryBase> indexCandidates)
        {
            foreach (var entry in indexCandidates) {
                if (entry.Value.OptionalIndexName != null) {
                    return entry.Key;
                }
            }
            return null;
        }
    
        private static bool IndexHashIsProvided(IndexedPropDesc hashPropIndexed, IList<IndexedPropDesc> hashPropsProvided)
        {
            foreach (IndexedPropDesc hashPropProvided in hashPropsProvided) {
                bool nameMatch = hashPropProvided.IndexPropName.Equals(hashPropIndexed.IndexPropName);
                bool typeMatch = true;
                if ((hashPropProvided.CoercionType != null) &&
                    (!TypeHelper.IsSubclassOrImplementsInterface(hashPropProvided.CoercionType.GetBoxedType(), hashPropIndexed.CoercionType.GetBoxedType()))) {
                    typeMatch = false;
                }
                if (nameMatch && typeMatch) {
                    return true;
                }
            }
            return false;
        }
    
        private static bool IsExactMatch(
            IndexMultiKey existing, bool unique,
            IList<IndexedPropDesc> hashProps, 
            IList<IndexedPropDesc> btreeProps, 
            AdvancedIndexDesc advancedIndexDesc)
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
            if (existing.AdvancedIndexDesc == null) {
                return advancedIndexDesc == null;
            }
            return advancedIndexDesc != null && existing.AdvancedIndexDesc.EqualsAdvancedIndex(advancedIndexDesc);
        }
    
        private static bool IndexMatchesProvided(
            IndexMultiKey indexDesc, 
            IList<IndexedPropDesc> hashPropsProvided, 
            IList<IndexedPropDesc> rangePropsProvided)
        {
            IndexedPropDesc[] hashPropIndexedList = indexDesc.HashIndexedProps;
            foreach (IndexedPropDesc hashPropIndexed in hashPropIndexedList) {
                bool foundHashProp = IndexHashIsProvided(hashPropIndexed, hashPropsProvided);
                if (!foundHashProp) {
                    return false;
                }
            }
    
            IndexedPropDesc[] rangePropIndexedList = indexDesc.RangeIndexedProps;
            foreach (IndexedPropDesc rangePropIndexed in rangePropIndexedList) {
                bool foundRangeProp = IndexHashIsProvided(rangePropIndexed, rangePropsProvided);
                if (!foundRangeProp) {
                    return false;
                }
            }
    
            return true;
        }
    
        private static Pair<IndexMultiKey, EventTableIndexEntryBase> GetPair(
            IDictionary<IndexMultiKey, EventTableIndexEntryBase> tableIndexesRefCount, 
            IndexMultiKey indexMultiKey)
        {
            EventTableIndexEntryBase indexFound = tableIndexesRefCount.Get(indexMultiKey);
            return new Pair<IndexMultiKey, EventTableIndexEntryBase>(indexMultiKey, indexFound);
        }
    
        private static ExprValidationContext GetValidationContext(EventType eventType, StatementContext statementContext)
        {
            var streamTypeService = new StreamTypeServiceImpl(eventType, null, false, statementContext.EngineURI);
            return new ExprValidationContext(
                statementContext.Container,
                streamTypeService,
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null, 
                statementContext.TimeProvider, 
                statementContext.VariableService, 
                statementContext.TableService, 
                new ExprEvaluatorContextStatement(statementContext, false),
                statementContext.EventAdapterService,
                statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations, 
                statementContext.ContextDescriptor,
                statementContext.ScriptingService,
                true, false, false, false, null, false);
        }
    
        [Serializable]
        private class IndexComparatorShortestPath : IComparer<IndexMultiKey>
        {
            public int Compare(IndexMultiKey o1, IndexMultiKey o2) {
                string[] indexedProps1 = IndexedPropDesc.GetIndexProperties(o1.HashIndexedProps);
                string[] indexedProps2 = IndexedPropDesc.GetIndexProperties(o2.HashIndexedProps);
                if (indexedProps1.Length > indexedProps2.Length) {
                    return 1;  // sort desc by count columns
                }
                if (indexedProps1.Length == indexedProps2.Length) {
                    return 0;
                }
                return -1;
            }
        }
    }
} // end of namespace
