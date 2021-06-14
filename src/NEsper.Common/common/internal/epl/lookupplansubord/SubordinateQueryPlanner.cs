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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.lookupsubord;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
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
            ISet<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var allStreamsZeroIndexed = new EventType[] {eventTypeIndexed, filterEventType};
            var outerStreams = new EventType[] {filterEventType};
            var joinedPropPlan = QueryPlanIndexBuilder.GetJoinProps(
                joinExpr,
                1,
                allStreamsZeroIndexed,
                excludePlanHint);

            // No join expression means all
            if (joinExpr == null && !isVirtualDataWindow) {
                var forgeX = new SubordinateWMatchExprQueryPlanForge(new SubordWMatchExprLookupStrategyAllUnfilteredForge(), null);
                return new SubordinateWMatchExprQueryPlanResult(forgeX, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            var queryPlan = PlanSubquery(
                outerStreams,
                joinedPropPlan,
                true,
                false,
                optionalIndexHint,
                isIndexShare,
                subqueryNumber,
                isVirtualDataWindow,
                indexMetadata,
                optionalUniqueKeyProps,
                onlyUseExistingIndexes,
                eventTypeIndexed,
                statementRawInfo,
                compileTimeServices);

            if (queryPlan == null) {
                var forgeX = new SubordinateWMatchExprQueryPlanForge(new SubordWMatchExprLookupStrategyAllFilteredForge(joinExpr), null);
                return new SubordinateWMatchExprQueryPlanResult(forgeX, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            var queryPlanDesc = queryPlan.Forge;
            SubordinateWMatchExprQueryPlanForge forge;
            if (joinExpr == null) {   // it can be null when using virtual data window
                forge = new SubordinateWMatchExprQueryPlanForge(
                    new SubordWMatchExprLookupStrategyIndexedUnfilteredForge(queryPlanDesc.LookupStrategyFactory), queryPlanDesc.IndexDescs);
            } else {
                var filteredForge =
                    new SubordWMatchExprLookupStrategyIndexedFilteredForge(joinExpr.Forge, queryPlanDesc.LookupStrategyFactory);
                forge = new SubordinateWMatchExprQueryPlanForge(filteredForge, queryPlanDesc.IndexDescs);
            }

            return new SubordinateWMatchExprQueryPlanResult(forge, queryPlan.AdditionalForgeables);
        }

        public static SubordinateQueryPlan PlanSubquery(
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
                    statementRawInfo.StatementName,
                    statementRawInfo.Annotations,
                    outerStreams,
                    indexProps.HashJoinedProps,
                    new CoercionDesc(false, indexProps.HashIndexCoercionType),
                    indexProps.RangeJoinedProps,
                    new CoercionDesc(false, indexProps.RangeIndexCoercionType),
                    isNWOnTrigger,
                    joinDesc,
                    forceTableScan,
                    indexProps.ListPair);
                var forge = new SubordinateQueryPlanDescForge(lookupStrategyFactoryX, null);
                return new SubordinateQueryPlan(forge, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            if (joinDesc.CustomIndexOps != null && !joinDesc.CustomIndexOps.IsEmpty()) {
                foreach (var op
                    in joinDesc.CustomIndexOps) {
                    foreach (var index in indexMetadata.Indexes) {
                        if (IsCustomIndexMatch(index, op)) {
                            var provisionDesc =
                                index.Value.OptionalQueryPlanIndexItem.AdvancedIndexProvisionDesc;
                            var lookupStrategyFactoryX =
                                provisionDesc.Factory.Forge.GetSubordinateLookupStrategy(
                                    op.Key.OperationName,
                                    op.Value.PositionalExpressions,
                                    isNWOnTrigger,
                                    outerStreams.Length);
                            var provisionCompileTime =
                                provisionDesc.ToCompileTime(eventTypeIndexed, statementRawInfo, services);
                            var indexItemForge = new QueryPlanIndexItemForge(
                                new string[0],
                                new Type[0],
                                new string[0],
                                new Type[0],
                                false,
                                provisionCompileTime,
                                eventTypeIndexed);
                            var indexDesc = new SubordinateQueryIndexDescForge(
                                null,
                                index.Value.OptionalIndexName,
                                index.Value.OptionalIndexModuleName,
                                index.Key,
                                indexItemForge);
                            var forge = new SubordinateQueryPlanDescForge(lookupStrategyFactoryX, new SubordinateQueryIndexDescForge[]{indexDesc});
                            return new SubordinateQueryPlan(forge, EmptyList<StmtClassForgeableFactory>.Instance);
                        }
                    }
                }
            }

            var hashKeys = Collections.GetEmptyList<SubordPropHashKeyForge>();
            CoercionDesc hashKeyCoercionTypes = null;
            var rangeKeys = Collections.GetEmptyList<SubordPropRangeKeyForge>();
            CoercionDesc rangeKeyCoercionTypes = null;
            IList<ExprNode> inKeywordSingleIdxKeys = null;
            ExprNode inKeywordMultiIdxKey = null;

            SubordinateQueryIndexDescForge[] indexDescs;
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            MultiKeyClassRef multiKeyClasses = null;
            if (joinDesc.InKeywordSingleIndex != null) {
                var single = joinDesc.InKeywordSingleIndex;
                var keyInfo = new SubordPropHashKeyForge(
                    new QueryGraphValueEntryHashKeyedForgeExpr(single.Expressions[0], false),
                    null,
                    single.CoercionType);
                var index = FindOrSuggestIndex(
                    Collections.SingletonMap(single.IndexedProp, keyInfo),
                    EmptyDictionary<string, SubordPropRangeKeyForge>.Instance,
                    optionalIndexHint,
                    indexShare,
                    subqueryNumber,
                    indexMetadata,
                    optionalUniqueKeyProps,
                    onlyUseExistingIndexes,
                    eventTypeIndexed,
                    statementRawInfo,
                    services);
                
                if (index == null) {
                    return null;
                }

                var indexDesc = index.Forge;
                var desc = new SubordinateQueryIndexDescForge(
                    indexDesc.OptionalIndexKeyInfo,
                    indexDesc.IndexName,
                    indexDesc.IndexModuleName,
                    indexDesc.IndexMultiKey,
                    indexDesc.OptionalQueryPlanIndexItem);
                indexDescs = new[] {desc};
                inKeywordSingleIdxKeys = single.Expressions;
                additionalForgeables.AddAll(index.MultiKeyForgeables);
            }
            else if (joinDesc.InKeywordMultiIndex != null) {
                var multi = joinDesc.InKeywordMultiIndex;

                indexDescs = new SubordinateQueryIndexDescForge[multi.IndexedProp.Length];
                for (var i = 0; i < multi.IndexedProp.Length; i++) {
                    var keyInfo = new SubordPropHashKeyForge(
                        new QueryGraphValueEntryHashKeyedForgeExpr(multi.Expression, false),
                        null,
                        multi.CoercionType);
                    var index = FindOrSuggestIndex(
                        Collections.SingletonMap(multi.IndexedProp[i], keyInfo),
                        EmptyDictionary<string, SubordPropRangeKeyForge>.Instance,
                        optionalIndexHint,
                        indexShare,
                        subqueryNumber,
                        indexMetadata,
                        optionalUniqueKeyProps,
                        onlyUseExistingIndexes,
                        eventTypeIndexed,
                        statementRawInfo,
                        services);
                    var indexDesc = index.Forge;
                    additionalForgeables.AddAll(index.MultiKeyForgeables);
                    if (indexDesc == null) {
                        return null;
                    }

                    indexDescs[i] = indexDesc;
                }

                inKeywordMultiIdxKey = multi.Expression;
            }
            else {
                var index = FindOrSuggestIndex(
                    joinDesc.HashProps,
                    joinDesc.RangeProps,
                    optionalIndexHint,
                    false,
                    subqueryNumber,
                    indexMetadata,
                    optionalUniqueKeyProps,
                    onlyUseExistingIndexes,
                    eventTypeIndexed,
                    statementRawInfo,
                    services);
                if (index == null) {
                    return null;
                }
                var indexDesc = index.Forge;
                additionalForgeables.AddRange(index.MultiKeyForgeables);
                var indexKeyInfo = indexDesc.OptionalIndexKeyInfo;

                hashKeys = indexKeyInfo.OrderedHashDesc;
                hashKeyCoercionTypes = indexKeyInfo.OrderedKeyCoercionTypes;
                rangeKeys = indexKeyInfo.OrderedRangeDesc;
                rangeKeyCoercionTypes = indexKeyInfo.OrderedRangeCoercionTypes;
                var desc = new SubordinateQueryIndexDescForge(
                    indexDesc.OptionalIndexKeyInfo,
                    indexDesc.IndexName,
                    indexDesc.IndexModuleName,
                    indexDesc.IndexMultiKey,
                    indexDesc.OptionalQueryPlanIndexItem);
                indexDescs = new[] {desc};
                
                if (indexDesc.OptionalQueryPlanIndexItem == null) {
                    var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(hashKeyCoercionTypes.CoercionTypes, true, statementRawInfo, services.SerdeResolver);
                    multiKeyClasses = multiKeyPlan.ClassRef;
                    additionalForgeables.AddRange(multiKeyPlan.MultiKeyForgeables);
                } else {
                    multiKeyClasses = indexDesc.OptionalQueryPlanIndexItem.HashMultiKeyClasses;
                }
            }

            if (forceTableScan) {
                return null;
            }

            var lookupStrategyFactory = SubordinateTableLookupStrategyUtil.GetLookupStrategy(
                outerStreams,
                hashKeys,
                hashKeyCoercionTypes,
                multiKeyClasses,
                rangeKeys,
                rangeKeyCoercionTypes,
                inKeywordSingleIdxKeys,
                inKeywordMultiIdxKey,
                isNWOnTrigger);
            var forgeX = new SubordinateQueryPlanDescForge(lookupStrategyFactory, indexDescs);
            return new SubordinateQueryPlan(forgeX, additionalForgeables);
        }

        private static bool IsCustomIndexMatch(
            KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> index,
            KeyValuePair<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> op)
        {
            if (index.Value.ExplicitIndexNameIfExplicit == null || index.Value.OptionalQueryPlanIndexItem == null) {
                return false;
            }

            var provision = index.Key.AdvancedIndexDesc;
            if (provision == null) {
                return false;
            }

            var provisionDesc =
                index.Value.OptionalQueryPlanIndexItem.AdvancedIndexProvisionDesc;
            if (!provisionDesc.Factory.Forge.ProvidesIndexForOperation(op.Key.OperationName)) {
                return false;
            }

            var opExpressions = op.Key.ExprNodes;
            var opProperties = GetPropertiesPerExpressionExpectSingle(opExpressions);
            var indexProperties = index.Key.AdvancedIndexDesc.IndexedProperties;
            return CompatExtensions.AreEqual(indexProperties, opProperties);
        }

        private static SubordinateQueryIndexSuggest FindOrSuggestIndex(
            IDictionary<string, SubordPropHashKeyForge> hashProps,
            IDictionary<string, SubordPropRangeKeyForge> rangeProps,
            IndexHint optionalIndexHint,
            bool isIndexShare,
            int subqueryNumber,
            EventTableIndexMetadata indexMetadata,
            ISet<string> optionalUniqueKeyProps,
            bool onlyUseExistingIndexes,
            EventType eventTypeIndexed,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
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
            SubordinateQueryIndexPlan planned = null;

            // consider index hints
            IList<IndexHintInstruction> optionalIndexHintInstructions = null;
            if (optionalIndexHint != null) {
                optionalIndexHintInstructions = optionalIndexHint.GetInstructionsSubquery(subqueryNumber);
            }

            var indexFoundPair = EventTableIndexUtil.FindIndexConsiderTyping(
                indexMetadata.Indexes,
                hashedAndBtreeProps.HashedProps,
                hashedAndBtreeProps.BtreeProps,
                optionalIndexHintInstructions);
            if (indexFoundPair != null) {
                var hintIndex = indexMetadata.Indexes.Get(indexFoundPair);
                existing = new Pair<IndexMultiKey, NameAndModule>(
                    indexFoundPair,
                    new NameAndModule(hintIndex.OptionalIndexName, hintIndex.OptionalIndexModuleName));
            }

            // nothing found: plan one
            if (existing == null && !onlyUseExistingIndexes) {
                // not found, see if the item is declared unique
                var proposedHashedProps = hashedAndBtreeProps.HashedProps;
                var proposedBtreeProps = hashedAndBtreeProps.BtreeProps;

                // match against unique-key properties when suggesting an index
                var unique = false;
                var coerce = !isIndexShare;
                if (optionalUniqueKeyProps != null && !optionalUniqueKeyProps.IsEmpty()) {
                    IList<IndexedPropDesc> newHashProps = new List<IndexedPropDesc>();
                    foreach (var uniqueKey in optionalUniqueKeyProps) {
                        var found = false;
                        foreach (var hashProp in hashedAndBtreeProps.HashedProps) {
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

                planned = PlanIndex(unique, proposedHashedProps, proposedBtreeProps, eventTypeIndexed, raw, services);
            }

            // compile index information
            if (existing == null && planned == null) {
                return null;
            }

            // handle existing
            IList<StmtClassForgeableFactory> multiKeyForgeables;
            if (existing != null) {
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(
                    existing.First,
                    indexProps.HashIndexPropsProvided,
                    indexProps.HashJoinedProps,
                    indexProps.RangeIndexPropsProvided,
                    indexProps.RangeJoinedProps);
                indexName = existing.Second.Name;
                indexModuleName = existing.Second.ModuleName;
                indexMultiKey = existing.First;
                multiKeyForgeables = EmptyList<StmtClassForgeableFactory>.Instance;
            }
            else {
                // handle planned
                indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(
                    planned.IndexPropKey,
                    indexProps.HashIndexPropsProvided,
                    indexProps.HashJoinedProps,
                    indexProps.RangeIndexPropsProvided,
                    indexProps.RangeJoinedProps);
                indexMultiKey = planned.IndexPropKey;
                planIndexItem = planned.IndexItem;
                multiKeyForgeables = planned.MultiKeyForgeables;
            }

            var forge = new SubordinateQueryIndexDescForge(indexKeyInfo, indexName, indexModuleName, indexMultiKey, planIndexItem);
            return new SubordinateQueryIndexSuggest(forge, multiKeyForgeables);
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
            foreach (var entry in hashProps) {
                hashIndexPropsProvided[count] = entry.Key;
                hashIndexCoercionType[count] = entry.Value.CoercionType;
                hashJoinedProps[count++] = entry.Value;
            }

            // range property names and types
            var rangeIndexPropsProvided = new string[rangeProps.Count];
            var rangeIndexCoercionType = new Type[rangeProps.Count];
            var rangeJoinedProps = new SubordPropRangeKeyForge[rangeProps.Count];
            count = 0;
            foreach (var entry in rangeProps) {
                rangeIndexPropsProvided[count] = entry.Key;
                rangeIndexCoercionType[count] = entry.Value.CoercionType;
                rangeJoinedProps[count++] = entry.Value;
            }

            // Add all joined fields to an array for sorting
            var listPair = SubordinateQueryPlannerUtil.ToListOfHashedAndBtreeProps(
                hashIndexPropsProvided,
                hashIndexCoercionType,
                rangeIndexPropsProvided,
                rangeIndexCoercionType);
            return new SubordinateQueryPlannerIndexPropDesc(
                hashIndexPropsProvided,
                hashIndexCoercionType,
                rangeIndexPropsProvided,
                rangeIndexCoercionType,
                listPair,
                hashJoinedProps,
                rangeJoinedProps);
        }

        private static SubordinateQueryIndexPlan PlanIndex(
            bool unique,
            IList<IndexedPropDesc> hashProps,
            IList<IndexedPropDesc> btreeProps,
            EventType eventTypeIndexed,
            StatementRawInfo raw,
            StatementCompileTimeServices services)
        {
            // not resolved as full match and not resolved as unique index match, allocate
            var indexPropKey = new IndexMultiKey(unique, hashProps, btreeProps, null);

            var indexedPropDescs = hashProps.ToArray();
            var indexProps = IndexedPropDesc.GetIndexProperties(indexedPropDescs);
            var indexCoercionTypes = IndexedPropDesc.GetCoercionTypes(indexedPropDescs);

            var rangePropDescs = btreeProps.ToArray();
            var rangeProps = IndexedPropDesc.GetIndexProperties(rangePropDescs);
            var rangeCoercionTypes = IndexedPropDesc.GetCoercionTypes(rangePropDescs);

            var indexItem = new QueryPlanIndexItemForge(
                indexProps,
                indexCoercionTypes,
                rangeProps,
                rangeCoercionTypes,
                unique,
                null,
                eventTypeIndexed);
            
            
            var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(indexCoercionTypes, true, raw, services.SerdeResolver);
            indexItem.HashMultiKeyClasses = multiKeyPlan.ClassRef;

            var rangeSerdes = new DataInputOutputSerdeForge[rangeCoercionTypes.Length];
            for (var i = 0; i < rangeCoercionTypes.Length; i++) {
                rangeSerdes[i] = services.SerdeResolver.SerdeForIndexBtree(rangeCoercionTypes[i], raw);
            }
            indexItem.RangeSerdes = rangeSerdes;

            return new SubordinateQueryIndexPlan(indexItem, indexPropKey, multiKeyPlan.MultiKeyForgeables);
        }
    }
} // end of namespace