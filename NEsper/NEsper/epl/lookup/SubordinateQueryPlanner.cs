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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class SubordinateQueryPlanner
    {
        public static SubordinateWMatchExprQueryPlanResult PlanOnExpression(
            ExprNode joinExpr,
            EventType filterEventType,
            IndexHint optionalIndexHint,
            bool isIndexShare,
            int subqueryNumber,
            ExcludePlanHint excludePlanHint,
            bool isVirtualDataWindow,
            EventTableIndexMetadata indexMetadata,
            EventType eventTypeIndexed,
            ICollection<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            string statementName,
            int statementId,
            Attribute[] annotations)
        {
            var allStreamsZeroIndexed = new EventType[] { eventTypeIndexed, filterEventType };
            var outerStreams = new EventType[] { filterEventType };
            var joinedPropPlan = QueryPlanIndexBuilder.GetJoinProps(joinExpr, 1, allStreamsZeroIndexed, excludePlanHint);

            // No join expression means all
            if (joinExpr == null && !isVirtualDataWindow)
            {
                return new SubordinateWMatchExprQueryPlanResult(new SubordWMatchExprLookupStrategyFactoryAllUnfiltered(), null);
            }

            var queryPlanDesc = PlanSubquery(outerStreams, joinedPropPlan, true, false, optionalIndexHint, isIndexShare, subqueryNumber,
                    isVirtualDataWindow, indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes, statementName, statementId, annotations);

            if (queryPlanDesc == null)
            {
                return new SubordinateWMatchExprQueryPlanResult(new SubordWMatchExprLookupStrategyFactoryAllFiltered(joinExpr.ExprEvaluator), null);
            }

            if (joinExpr == null)
            {   // it can be null when using virtual data window
                return new SubordinateWMatchExprQueryPlanResult(
                        new SubordWMatchExprLookupStrategyFactoryIndexedUnfiltered(queryPlanDesc.LookupStrategyFactory), queryPlanDesc.IndexDescs);
            }
            else
            {
                return new SubordinateWMatchExprQueryPlanResult(
                        new SubordWMatchExprLookupStrategyFactoryIndexedFiltered(joinExpr.ExprEvaluator, queryPlanDesc.LookupStrategyFactory), queryPlanDesc.IndexDescs);
            }
        }

        public static SubordinateQueryPlanDesc PlanSubquery(
            EventType[] outerStreams,
            SubordPropPlan joinDesc,
            bool isNWOnTrigger,
            bool forceTableScan,
            IndexHint optionalIndexHint,
            bool indexShare,
            int subqueryNumber,
            bool isVirtualDataWindow,
            EventTableIndexMetadata indexMetadata,
            ICollection<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            string statementName,
            int statementId,
            Attribute[] annotations)
        {
            if (isVirtualDataWindow)
            {
                var indexProps = GetIndexPropDesc(joinDesc.HashProps, joinDesc.RangeProps);
                var lookupStrategyFactoryVdw = new SubordTableLookupStrategyFactoryVDW(statementName, statementId, annotations,
                        outerStreams,
                        indexProps.HashJoinedProps,
                        new CoercionDesc(false, indexProps.HashIndexCoercionType),
                        indexProps.RangeJoinedProps,
                        new CoercionDesc(false, indexProps.RangeIndexCoercionType),
                        isNWOnTrigger,
                        joinDesc, forceTableScan, indexProps.ListPair);
                return new SubordinateQueryPlanDesc(lookupStrategyFactoryVdw, null);
            }

            if ((joinDesc.CustomIndexOps != null) && (!joinDesc.CustomIndexOps.IsEmpty()))
            {
                foreach (var op in joinDesc.CustomIndexOps)
                {
                    foreach (var index in indexMetadata.Indexes)
                    {
                        if (IsCustomIndexMatch(index, op))
                        {
                            var provisionDesc = index.Value.QueryPlanIndexItem.AdvancedIndexProvisionDesc;
                            var lookupStrategyFactoryX = provisionDesc.Factory.GetSubordinateLookupStrategy(
                                op.Key.OperationName, op.Value.PositionalExpressions, isNWOnTrigger, outerStreams.Length);
                            var indexDesc = new SubordinateQueryIndexDesc(null, index.Value.OptionalIndexName, index.Key, null);
                            return new SubordinateQueryPlanDesc(lookupStrategyFactoryX, new SubordinateQueryIndexDesc[] { indexDesc });
                        }
                    }
                }
            }

            var hashKeys = Collections.GetEmptyList<SubordPropHashKey>();
            CoercionDesc hashKeyCoercionTypes = null;
            var rangeKeys = Collections.GetEmptyList<SubordPropRangeKey>();
            CoercionDesc rangeKeyCoercionTypes = null;
            IList<ExprNode> inKeywordSingleIdxKeys = null;
            ExprNode inKeywordMultiIdxKey = null;

            SubordinateQueryIndexDesc[] indexDescs;
            if (joinDesc.InKeywordSingleIndex != null)
            {
                var single = joinDesc.InKeywordSingleIndex;
                var keyInfo = new SubordPropHashKey(new QueryGraphValueEntryHashKeyedExpr(single.Expressions[0], false), null, single.CoercionType);
                var indexDesc = FindOrSuggestIndex(
                        Collections.SingletonMap(single.IndexedProp, keyInfo),
                        Collections.GetEmptyMap<string, SubordPropRangeKey>(), optionalIndexHint, indexShare, subqueryNumber,
                        indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes);
                if (indexDesc == null)
                {
                    return null;
                }
                var desc = new SubordinateQueryIndexDesc(indexDesc.OptionalIndexKeyInfo, indexDesc.IndexName, indexDesc.IndexMultiKey, indexDesc.QueryPlanIndexItem);
                indexDescs = new SubordinateQueryIndexDesc[] { desc };
                inKeywordSingleIdxKeys = single.Expressions;
            }
            else if (joinDesc.InKeywordMultiIndex != null)
            {
                var multi = joinDesc.InKeywordMultiIndex;

                indexDescs = new SubordinateQueryIndexDesc[multi.IndexedProp.Length];
                for (var i = 0; i < multi.IndexedProp.Length; i++)
                {
                    var keyInfo = new SubordPropHashKey(new QueryGraphValueEntryHashKeyedExpr(multi.Expression, false), null, multi.CoercionType);
                    var indexDesc = FindOrSuggestIndex(
                            Collections.SingletonMap(multi.IndexedProp[i], keyInfo),
                            Collections.GetEmptyMap<string, SubordPropRangeKey>(), optionalIndexHint, indexShare, subqueryNumber,
                            indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes);
                    if (indexDesc == null)
                    {
                        return null;
                    }
                    indexDescs[i] = indexDesc;
                }
                inKeywordMultiIdxKey = multi.Expression;
            }
            else
            {
                var indexDesc = FindOrSuggestIndex(joinDesc.HashProps,
                        joinDesc.RangeProps, optionalIndexHint, false, subqueryNumber,
                        indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes);
                if (indexDesc == null)
                {
                    return null;
                }
                var indexKeyInfo = indexDesc.OptionalIndexKeyInfo;
                hashKeys = indexKeyInfo.OrderedHashDesc;
                hashKeyCoercionTypes = indexKeyInfo.OrderedKeyCoercionTypes;
                rangeKeys = indexKeyInfo.OrderedRangeDesc;
                rangeKeyCoercionTypes = indexKeyInfo.OrderedRangeCoercionTypes;
                var desc = new SubordinateQueryIndexDesc(indexDesc.OptionalIndexKeyInfo, indexDesc.IndexName, indexDesc.IndexMultiKey, indexDesc.QueryPlanIndexItem);
                indexDescs = new SubordinateQueryIndexDesc[] { desc };
            }

            if (forceTableScan)
            {
                return null;
            }

            var lookupStrategyFactory = SubordinateTableLookupStrategyUtil.GetLookupStrategy(
                outerStreams, 
                hashKeys, hashKeyCoercionTypes, 
                rangeKeys, rangeKeyCoercionTypes, 
                inKeywordSingleIdxKeys, 
                inKeywordMultiIdxKey, 
                isNWOnTrigger);
            return new SubordinateQueryPlanDesc(lookupStrategyFactory, indexDescs);
        }

        private static bool IsCustomIndexMatch(
            KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> index,
            KeyValuePair<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> op)
        {
            if (index.Value.ExplicitIndexNameIfExplicit == null || 
                index.Value.QueryPlanIndexItem == null)
            {
                return false;
            }

            var provision = index.Value.QueryPlanIndexItem.AdvancedIndexProvisionDesc;
            if (provision == null)
            {
                return false;
            }
            if (!provision.Factory.ProvidesIndexForOperation(op.Key.OperationName, op.Value.PositionalExpressions))
            {
                return false;
            }
            return ExprNodeUtility.DeepEquals(index.Key.AdvancedIndexDesc.IndexedExpressions, op.Key.ExprNodes, true);
        }

        private static SubordinateQueryIndexDesc FindOrSuggestIndex(
            IDictionary<String, SubordPropHashKey> hashProps,
            IDictionary<String, SubordPropRangeKey> rangeProps,
            IndexHint optionalIndexHint,
            bool isIndexShare,
            int subqueryNumber,
            EventTableIndexMetadata indexMetadata,
            ICollection<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes)
        {
            var indexProps = GetIndexPropDesc(hashProps, rangeProps);
            var hashedAndBtreeProps = indexProps.ListPair;

            // Get or create the table for this index (exact match or property names, type of index and coercion type is expected)
            IndexKeyInfo indexKeyInfo;   // how needs all of IndexKeyInfo+QueryPlanIndexItem+IndexMultiKey
            IndexMultiKey indexMultiKey;
            string indexName = null;
            QueryPlanIndexItem planIndexItem = null;

            if (hashedAndBtreeProps.HashedProps.IsEmpty() && hashedAndBtreeProps.BtreeProps.IsEmpty())
            {
                return null;
            }

            Pair<IndexMultiKey, string> existing = null;
            Pair<QueryPlanIndexItem, IndexMultiKey> planned = null;

            // consider index hints
            IList<IndexHintInstruction> optionalIndexHintInstructions = null;
            if (optionalIndexHint != null)
            {
                optionalIndexHintInstructions = optionalIndexHint.GetInstructionsSubquery(subqueryNumber);
            }

            var indexFoundPair = EventTableIndexUtil.FindIndexConsiderTyping(
                indexMetadata.IndexesAsBase, 
                hashedAndBtreeProps.HashedProps, 
                hashedAndBtreeProps.BtreeProps, 
                optionalIndexHintInstructions);
            if (indexFoundPair != null)
            {
                var hintIndex = indexMetadata.Indexes.Get(indexFoundPair);
                existing = new Pair<IndexMultiKey, string>(indexFoundPair, hintIndex.OptionalIndexName);
            }

            // nothing found: plan one
            if (existing == null && !onlyUseExistingIndexes)
            {
                // not found, see if the item is declared unique
                var proposedHashedProps = hashedAndBtreeProps.HashedProps;
                var proposedBtreeProps = hashedAndBtreeProps.BtreeProps;

                // match against unique-key properties when suggesting an index
                var unique = false;
                var coerce = !isIndexShare;
                if (optionalUniqueKeyProps != null && !optionalUniqueKeyProps.IsEmpty())
                {
                    IList<IndexedPropDesc> newHashProps = new List<IndexedPropDesc>();
                    foreach (var uniqueKey in optionalUniqueKeyProps)
                    {
                        var found = false;
                        foreach (var hashProp in hashedAndBtreeProps.HashedProps)
                        {
                            if (hashProp.IndexPropName.Equals(uniqueKey))
                            {
                                newHashProps.Add(hashProp);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            newHashProps = null;
                            break;
                        }
                    }
                    if (newHashProps != null)
                    {
                        proposedHashedProps = newHashProps;
                        proposedBtreeProps = Collections.GetEmptyList<IndexedPropDesc>();
                        unique = true;
                        coerce = false;
                    }
                }

                planned = PlanIndex(unique, proposedHashedProps, proposedBtreeProps, coerce);
            }

            // compile index information
            if (existing == null && planned == null)
            {
                return null;
            }
            // handle existing
            if (existing != null)
            {
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(existing.First,
                        indexProps.HashIndexPropsProvided, indexProps.HashJoinedProps,
                        indexProps.RangeIndexPropsProvided, indexProps.RangeJoinedProps);
                indexName = existing.Second;
                indexMultiKey = existing.First;
            }
            // handle planned
            else
            {
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(planned.Second,
                        indexProps.HashIndexPropsProvided, indexProps.HashJoinedProps,
                        indexProps.RangeIndexPropsProvided, indexProps.RangeJoinedProps);
                indexMultiKey = planned.Second;
                planIndexItem = planned.First;
            }

            return new SubordinateQueryIndexDesc(indexKeyInfo, indexName, indexMultiKey, planIndexItem);
        }

        private static SubordinateQueryPlannerIndexPropDesc GetIndexPropDesc(IDictionary<String, SubordPropHashKey> hashProps, IDictionary<String, SubordPropRangeKey> rangeProps)
        {
            // hash property names and types
            var hashIndexPropsProvided = new string[hashProps.Count];
            var hashIndexCoercionType = new Type[hashProps.Count];
            var hashJoinedProps = new SubordPropHashKey[hashProps.Count];
            var count = 0;
            foreach (var entry in hashProps)
            {
                hashIndexPropsProvided[count] = entry.Key;
                hashIndexCoercionType[count] = entry.Value.CoercionType;
                hashJoinedProps[count++] = entry.Value;
            }

            // range property names and types
            var rangeIndexPropsProvided = new string[rangeProps.Count];
            var rangeIndexCoercionType = new Type[rangeProps.Count];
            var rangeJoinedProps = new SubordPropRangeKey[rangeProps.Count];
            count = 0;
            foreach (var entry in rangeProps)
            {
                rangeIndexPropsProvided[count] = entry.Key;
                rangeIndexCoercionType[count] = entry.Value.CoercionType;
                rangeJoinedProps[count++] = entry.Value;
            }

            // Add all joined fields to an array for sorting
            var listPair = SubordinateQueryPlannerUtil.ToListOfHashedAndBtreeProps(hashIndexPropsProvided,
                    hashIndexCoercionType, rangeIndexPropsProvided, rangeIndexCoercionType);
            return new SubordinateQueryPlannerIndexPropDesc(hashIndexPropsProvided, hashIndexCoercionType,
                    rangeIndexPropsProvided, rangeIndexCoercionType, listPair,
                    hashJoinedProps, rangeJoinedProps);
        }

        private static Pair<QueryPlanIndexItem, IndexMultiKey> PlanIndex(
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            bool mustCoerce)
        {
            // not resolved as full match and not resolved as unique index match, allocate
            var indexPropKey = new IndexMultiKey(unique, hashProps, btreeProps, null);

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

            var indexItem = new QueryPlanIndexItem(indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes, unique, null);
            return new Pair<QueryPlanIndexItem, IndexMultiKey>(indexItem, indexPropKey);
        }
    }
}
