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
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.@join.hint;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.queryplanbuild;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.lookupsubord;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlanner
    {
        public static SubordinateWMatchExprQueryPlanForge PlanOnExpression(
            ExprNode joinExpr,
            EventType filterEventType,
            IndexHint optionalIndexHint,
            bool isIndexShare,
            int subqueryNumber,
            ExcludePlanHint excludePlanHint,
            bool isVirtualDataWindow,
            EventTableIndexMetadata indexMetadata,
            EventType eventTypeIndexed,
            ISet<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var allStreamsZeroIndexed = new EventType[] {eventTypeIndexed, filterEventType};
            var outerStreams = new EventType[] {filterEventType};
            SubordPropPlan joinedPropPlan = QueryPlanIndexBuilder.GetJoinProps(
                joinExpr, 1, allStreamsZeroIndexed, excludePlanHint);

            // No join expression means all
            if (joinExpr == null && !isVirtualDataWindow) {
                return new SubordinateWMatchExprQueryPlanForge(
                    new SubordWMatchExprLookupStrategyAllUnfilteredForge(), null);
            }

            var queryPlanDesc = PlanSubquery(
                outerStreams, joinedPropPlan, true, false, optionalIndexHint, isIndexShare, subqueryNumber,
                isVirtualDataWindow, indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes, eventTypeIndexed,
                statementRawInfo, compileTimeServices);

            if (queryPlanDesc == null) {
                return new SubordinateWMatchExprQueryPlanForge(
                    new SubordWMatchExprLookupStrategyAllFilteredForge(joinExpr), null);
            }

            if (joinExpr == null) { // it can be null when using virtual data window
                return new SubordinateWMatchExprQueryPlanForge(
                    new SubordWMatchExprLookupStrategyIndexedUnfilteredForge(queryPlanDesc.LookupStrategyFactory),
                    queryPlanDesc.IndexDescs);
            }

            var forge = new SubordWMatchExprLookupStrategyIndexedFilteredForge(
                joinExpr.Forge, queryPlanDesc.LookupStrategyFactory);
            return new SubordinateWMatchExprQueryPlanForge(forge, queryPlanDesc.IndexDescs);
        }

        public static SubordinateQueryPlanDescForge PlanSubquery(
            EventType[] outerStreams,
            SubordPropPlan joinDesc,
            bool isNWOnTrigger,
            bool forceTableScan,
            IndexHint optionalIndexHint,
            bool indexShare,
            int subqueryNumber,
            bool isVirtualDataWindow,
            EventTableIndexMetadata indexMetadata,
            ISet<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            EventType eventTypeIndexed,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (isVirtualDataWindow) {
                var indexProps = GetIndexPropDesc(joinDesc.HashProps, joinDesc.RangeProps);
                var lookupStrategyFactoryX = new SubordTableLookupStrategyFactoryForgeVDW(
                    statementRawInfo.StatementName, statementRawInfo.Annotations,
                    outerStreams,
                    indexProps.HashJoinedProps,
                    new CoercionDesc(false, indexProps.HashIndexCoercionType),
                    indexProps.RangeJoinedProps,
                    new CoercionDesc(false, indexProps.RangeIndexCoercionType),
                    isNWOnTrigger,
                    joinDesc, forceTableScan, indexProps.ListPair);
                return new SubordinateQueryPlanDescForge(lookupStrategyFactoryX, null);
            }

            if (joinDesc.CustomIndexOps != null && !joinDesc.CustomIndexOps.IsEmpty()) {
                foreach (KeyValuePair<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> op
                    in joinDesc.CustomIndexOps) {
                    foreach (KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> index in indexMetadata.Indexes) {
                        if (IsCustomIndexMatch(index, op)) {
                            EventAdvancedIndexProvisionRuntime provisionDesc =
                                index.Value.OptionalQueryPlanIndexItem.AdvancedIndexProvisionDesc;
                            SubordTableLookupStrategyFactoryQuadTreeForge lookupStrategyFactoryX =
                                provisionDesc.Factory.Forge.GetSubordinateLookupStrategy(
                                    op.Key.OperationName, op.Value.PositionalExpressions, isNWOnTrigger,
                                    outerStreams.Length);
                            EventAdvancedIndexProvisionCompileTime provisionCompileTime =
                                provisionDesc.ToCompileTime(eventTypeIndexed, statementRawInfo, services);
                            var indexItemForge = new QueryPlanIndexItemForge(
                                new string[0], new Type[0], new string[0], new Type[0], false, provisionCompileTime,
                                eventTypeIndexed);
                            var indexDesc = new SubordinateQueryIndexDescForge(
                                null, index.Value.OptionalIndexName, index.Value.OptionalIndexModuleName, index.Key,
                                indexItemForge);
                            return new SubordinateQueryPlanDescForge(lookupStrategyFactoryX, new[] {indexDesc});
                        }
                    }
                }
            }

            IList<SubordPropHashKeyForge> hashKeys = Collections.GetEmptyList<SubordPropHashKeyForge>();
            CoercionDesc hashKeyCoercionTypes = null;
            IList<SubordPropRangeKeyForge> rangeKeys = Collections.GetEmptyList<SubordPropRangeKeyForge>();
            CoercionDesc rangeKeyCoercionTypes = null;
            ExprNode[] inKeywordSingleIdxKeys = null;
            ExprNode inKeywordMultiIdxKey = null;

            SubordinateQueryIndexDescForge[] indexDescs;
            if (joinDesc.InKeywordSingleIndex != null) {
                SubordPropInKeywordSingleIndex single = joinDesc.InKeywordSingleIndex;
                var keyInfo = new SubordPropHashKeyForge(
                    new QueryGraphValueEntryHashKeyedForgeExpr(single.Expressions[0], false), null,
                    single.CoercionType);
                var indexDesc = FindOrSuggestIndex(
                    Collections.SingletonMap(single.IndexedProp, keyInfo),
                    new EmptyDictionary<string, SubordPropRangeKeyForge>(),
                    optionalIndexHint, indexShare, subqueryNumber,
                    indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes, eventTypeIndexed);
                if (indexDesc == null) {
                    return null;
                }

                var desc = new SubordinateQueryIndexDescForge(
                    indexDesc.OptionalIndexKeyInfo, indexDesc.IndexName, indexDesc.IndexModuleName,
                    indexDesc.IndexMultiKey, indexDesc.OptionalQueryPlanIndexItem);
                indexDescs = new[] {desc};
                inKeywordSingleIdxKeys = single.Expressions;
            }
            else if (joinDesc.InKeywordMultiIndex != null) {
                SubordPropInKeywordMultiIndex multi = joinDesc.InKeywordMultiIndex;

                indexDescs = new SubordinateQueryIndexDescForge[multi.IndexedProp.Length];
                for (var i = 0; i < multi.IndexedProp.Length; i++) {
                    var keyInfo = new SubordPropHashKeyForge(
                        new QueryGraphValueEntryHashKeyedForgeExpr(multi.Expression, false), null, multi.CoercionType);
                    var indexDesc = FindOrSuggestIndex(
                        Collections.SingletonMap(multi.IndexedProp[i], keyInfo),
                        new EmptyDictionary<string, SubordPropRangeKeyForge>(),
                        optionalIndexHint, indexShare, subqueryNumber,
                        indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes, eventTypeIndexed);
                    if (indexDesc == null) {
                        return null;
                    }

                    indexDescs[i] = indexDesc;
                }

                inKeywordMultiIdxKey = multi.Expression;
            }
            else {
                var indexDesc = FindOrSuggestIndex(
                    joinDesc.HashProps,
                    joinDesc.RangeProps, optionalIndexHint, false, subqueryNumber,
                    indexMetadata, optionalUniqueKeyProps, onlyUseExistingIndexes, eventTypeIndexed);
                if (indexDesc == null) {
                    return null;
                }

                var indexKeyInfo = indexDesc.OptionalIndexKeyInfo;
                hashKeys = indexKeyInfo.OrderedHashDesc;
                hashKeyCoercionTypes = indexKeyInfo.OrderedKeyCoercionTypes;
                rangeKeys = indexKeyInfo.OrderedRangeDesc;
                rangeKeyCoercionTypes = indexKeyInfo.OrderedRangeCoercionTypes;
                var desc = new SubordinateQueryIndexDescForge(
                    indexDesc.OptionalIndexKeyInfo, indexDesc.IndexName, indexDesc.IndexModuleName,
                    indexDesc.IndexMultiKey, indexDesc.OptionalQueryPlanIndexItem);
                indexDescs = new[] {desc};
            }

            if (forceTableScan) {
                return null;
            }

            SubordTableLookupStrategyFactoryForge lookupStrategyFactory = SubordinateTableLookupStrategyUtil.GetLookupStrategy(
                outerStreams,
                hashKeys, hashKeyCoercionTypes, rangeKeys, rangeKeyCoercionTypes, inKeywordSingleIdxKeys,
                inKeywordMultiIdxKey, isNWOnTrigger);
            return new SubordinateQueryPlanDescForge(lookupStrategyFactory, indexDescs);
        }

        private static bool IsCustomIndexMatch(
            KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> index,
            KeyValuePair<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> op)
        {
            if (index.Value.ExplicitIndexNameIfExplicit == null || index.Value.OptionalQueryPlanIndexItem == null) {
                return false;
            }

            AdvancedIndexIndexMultiKeyPart provision = index.Key.AdvancedIndexDesc;
            if (provision == null) {
                return false;
            }

            EventAdvancedIndexProvisionRuntime provisionDesc =
                index.Value.OptionalQueryPlanIndexItem.AdvancedIndexProvisionDesc;
            if (!provisionDesc.Factory.Forge.ProvidesIndexForOperation(op.Key.OperationName)) {
                return false;
            }

            var opExpressions = op.Key.ExprNodes;
            var opProperties = GetPropertiesPerExpressionExpectSingle(opExpressions);
            string[] indexProperties = index.Key.AdvancedIndexDesc.IndexedProperties;
            return CompatExtensions.AreEqual(indexProperties, opProperties);
        }

        private static SubordinateQueryIndexDescForge FindOrSuggestIndex(
            IDictionary<string, SubordPropHashKeyForge> hashProps,
            IDictionary<string, SubordPropRangeKeyForge> rangeProps,
            IndexHint optionalIndexHint,
            bool isIndexShare,
            int subqueryNumber,
            EventTableIndexMetadata indexMetadata,
            ISet<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            EventType eventTypeIndexed)
        {
            var indexProps = GetIndexPropDesc(hashProps, rangeProps);
            var hashedAndBtreeProps = indexProps.ListPair;

            // Get or create the table for this index (exact match or property names, type of index and coercion type is expected)
            IndexKeyInfo indexKeyInfo; // how needs all of IndexKeyInfo+QueryPlanIndexItem+IndexMultiKey
            IndexMultiKey indexMultiKey;
            string indexName = null;
            string indexModuleName = null;
            QueryPlanIndexItemForge planIndexItem = null;

            if (hashedAndBtreeProps.HashedProps.IsEmpty() && hashedAndBtreeProps.BtreeProps.IsEmpty()) {
                return null;
            }

            Pair<IndexMultiKey, NameAndModule> existing = null;
            Pair<QueryPlanIndexItemForge, IndexMultiKey> planned = null;

            // consider index hints
            IList<IndexHintInstruction> optionalIndexHintInstructions = null;
            if (optionalIndexHint != null) {
                optionalIndexHintInstructions = optionalIndexHint.GetInstructionsSubquery(subqueryNumber);
            }

            IndexMultiKey indexFoundPair = EventTableIndexUtil.FindIndexConsiderTyping(
                indexMetadata.Indexes, hashedAndBtreeProps.HashedProps, hashedAndBtreeProps.BtreeProps,
                optionalIndexHintInstructions);
            if (indexFoundPair != null) {
                var hintIndex = indexMetadata.Indexes.Get(indexFoundPair);
                existing = new Pair<IndexMultiKey, NameAndModule>(
                    indexFoundPair, new NameAndModule(hintIndex.OptionalIndexName, hintIndex.OptionalIndexModuleName));
            }

            // nothing found: plan one
            if (existing == null && !onlyUseExistingIndexes) {
                // not found, see if the item is declared unique
                IList<IndexedPropDesc> proposedHashedProps = hashedAndBtreeProps.HashedProps;
                IList<IndexedPropDesc> proposedBtreeProps = hashedAndBtreeProps.BtreeProps;

                // match against unique-key properties when suggesting an index
                var unique = false;
                var coerce = !isIndexShare;
                if (optionalUniqueKeyProps != null && !optionalUniqueKeyProps.IsEmpty()) {
                    IList<IndexedPropDesc> newHashProps = new List<IndexedPropDesc>();
                    foreach (var uniqueKey in optionalUniqueKeyProps) {
                        var found = false;
                        foreach (IndexedPropDesc hashProp in hashedAndBtreeProps.HashedProps) {
                            if (hashProp.IndexPropName.Equals(uniqueKey)) {
                                newHashProps.Add(hashProp);
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            newHashProps = null;
                            break;
                        }
                    }

                    if (newHashProps != null) {
                        proposedHashedProps = newHashProps;
                        proposedBtreeProps = new EmptyList<IndexedPropDesc>();
                        unique = true;
                        coerce = false;
                    }
                }

                planned = PlanIndex(unique, proposedHashedProps, proposedBtreeProps, coerce, eventTypeIndexed);
            }

            // compile index information
            if (existing == null && planned == null) {
                return null;
            }

            // handle existing
            if (existing != null) {
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(
                    existing.First,
                    indexProps.HashIndexPropsProvided, indexProps.HashJoinedProps,
                    indexProps.RangeIndexPropsProvided, indexProps.RangeJoinedProps);
                indexName = existing.Second.Name;
                indexModuleName = existing.Second.ModuleName;
                indexMultiKey = existing.First;
            }
            else {
                // handle planned
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(
                    planned.Second,
                    indexProps.HashIndexPropsProvided, indexProps.HashJoinedProps,
                    indexProps.RangeIndexPropsProvided, indexProps.RangeJoinedProps);
                indexMultiKey = planned.Second;
                planIndexItem = planned.First;
            }

            return new SubordinateQueryIndexDescForge(
                indexKeyInfo, indexName, indexModuleName, indexMultiKey, planIndexItem);
        }

        private static SubordinateQueryPlannerIndexPropDesc GetIndexPropDesc(
            IDictionary<string, SubordPropHashKeyForge> hashProps,
            IDictionary<string, SubordPropRangeKeyForge> rangeProps)
        {
            // hash property names and types
            var hashIndexPropsProvided = new string[hashProps.Count];
            var hashIndexCoercionType = new Type[hashProps.Count];
            var hashJoinedProps = new SubordPropHashKeyForge[hashProps.Count];
            var count = 0;
            foreach (KeyValuePair<string, SubordPropHashKeyForge> entry in hashProps) {
                hashIndexPropsProvided[count] = entry.Key;
                hashIndexCoercionType[count] = entry.Value.CoercionType;
                hashJoinedProps[count++] = entry.Value;
            }

            // range property names and types
            var rangeIndexPropsProvided = new string[rangeProps.Count];
            var rangeIndexCoercionType = new Type[rangeProps.Count];
            var rangeJoinedProps = new SubordPropRangeKeyForge[rangeProps.Count];
            count = 0;
            foreach (KeyValuePair<string, SubordPropRangeKeyForge> entry in rangeProps) {
                rangeIndexPropsProvided[count] = entry.Key;
                rangeIndexCoercionType[count] = entry.Value.CoercionType;
                rangeJoinedProps[count++] = entry.Value;
            }

            // Add all joined fields to an array for sorting
            var listPair = SubordinateQueryPlannerUtil.ToListOfHashedAndBtreeProps(
                hashIndexPropsProvided,
                hashIndexCoercionType, rangeIndexPropsProvided, rangeIndexCoercionType);
            return new SubordinateQueryPlannerIndexPropDesc(
                hashIndexPropsProvided, hashIndexCoercionType,
                rangeIndexPropsProvided, rangeIndexCoercionType, listPair,
                hashJoinedProps, rangeJoinedProps);
        }

        private static Pair<QueryPlanIndexItemForge, IndexMultiKey> PlanIndex(
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            bool mustCoerce,
            EventType eventTypeIndexed)
        {
            // not resolved as full match and not resolved as unique index match, allocate
            var indexPropKey = new IndexMultiKey(unique, hashProps, btreeProps, null);

            IndexedPropDesc[] indexedPropDescs = hashProps.ToArray();
            string[] indexProps = IndexedPropDesc.GetIndexProperties(indexedPropDescs);
            Type[] indexCoercionTypes = IndexedPropDesc.GetCoercionTypes(indexedPropDescs);

            IndexedPropDesc[] rangePropDescs = btreeProps.ToArray();
            string[] rangeProps = IndexedPropDesc.GetIndexProperties(rangePropDescs);
            Type[] rangeCoercionTypes = IndexedPropDesc.GetCoercionTypes(rangePropDescs);

            var indexItem = new QueryPlanIndexItemForge(
                indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes, unique, null, eventTypeIndexed);
            return new Pair<QueryPlanIndexItemForge, IndexMultiKey>(indexItem, indexPropKey);
        }
    }
} // end of namespace