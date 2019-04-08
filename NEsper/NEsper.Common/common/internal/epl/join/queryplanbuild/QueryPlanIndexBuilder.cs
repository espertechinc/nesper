///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.@join.hint;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    /// <summary>
    ///     Build query index plans.
    /// </summary>
    public class QueryPlanIndexBuilder
    {
        /// <summary>
        ///     Build index specification from navigability info.
        ///     <para />
        ///     Looks at each stream and determines which properties in the stream must be indexed
        ///     in order for other streams to look up into the stream. Determines the unique set of properties
        ///     to avoid building duplicate indexes on the same set of properties.
        /// </summary>
        /// <param name="queryGraph">navigability info</param>
        /// <param name="typePerStream">type info</param>
        /// <param name="indexedStreamsUniqueProps">per-stream unique props</param>
        /// <returns>query index specs for each stream</returns>
        public static QueryPlanIndexForge[] BuildIndexSpec(
            QueryGraphForge queryGraph,
            EventType[] typePerStream,
            string[][][] indexedStreamsUniqueProps)
        {
            var numStreams = queryGraph.NumStreams;
            var indexSpecs = new QueryPlanIndexForge[numStreams];

            // For each stream compile a list of index property sets.
            for (var streamIndexed = 0; streamIndexed < numStreams; streamIndexed++) {
                IList<QueryPlanIndexItemForge> indexesSet = new List<QueryPlanIndexItemForge>();

                // Look at the index from the viewpoint of the stream looking up in the index
                for (var streamLookup = 0; streamLookup < numStreams; streamLookup++) {
                    if (streamIndexed == streamLookup) {
                        continue;
                    }

                    var value = queryGraph.GetGraphValue(streamLookup, streamIndexed);
                    var hashKeyAndIndexProps = value.HashKeyProps;

                    // Sort index properties, but use the sorted properties only to eliminate duplicates
                    var hashIndexProps = hashKeyAndIndexProps.Indexed;
                    var hashKeyProps = hashKeyAndIndexProps.Keys;
                    var indexCoercionTypes = CoercionUtil.GetCoercionTypesHash(
                        typePerStream, streamLookup, streamIndexed, hashKeyProps, hashIndexProps);
                    var hashCoercionTypeArr = indexCoercionTypes.CoercionTypes;

                    var rangeAndIndexProps = value.RangeProps;
                    var rangeIndexProps = rangeAndIndexProps.Indexed;
                    var rangeKeyProps = rangeAndIndexProps.Keys;
                    var rangeCoercionTypes = CoercionUtil.GetCoercionTypesRange(typePerStream, streamIndexed, rangeIndexProps, rangeKeyProps);
                    var rangeCoercionTypeArr = rangeCoercionTypes.CoercionTypes;

                    if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0) {
                        var singles = value.InKeywordSingles;
                        if (!singles.Key.IsEmpty()) {
                            var indexedProp = singles.Indexed[0];
                            var indexedType = typePerStream[streamIndexed].GetPropertyType(indexedProp);
                            var indexItem = new QueryPlanIndexItemForge(
                                new[] {indexedProp}, new[] {indexedType}, new string[0], new Type[0], false, null, typePerStream[streamIndexed]);
                            CheckDuplicateOrAdd(indexItem, indexesSet);
                        }

                        var multis = value.InKeywordMulti;
                        if (!multis.IsEmpty()) {
                            QueryGraphValuePairInKWMultiIdx multi = multis[0];
                            foreach (var propIndexed in multi.Indexed) {
                                var identNode = (ExprIdentNode) propIndexed;
                                var type = identNode.Forge.EvaluationType;
                                var indexItem = new QueryPlanIndexItemForge(
                                    new[] {identNode.ResolvedPropertyName}, new[] {type}, new string[0], new Type[0], false, null,
                                    typePerStream[streamIndexed]);
                                CheckDuplicateOrAdd(indexItem, indexesSet);
                            }
                        }

                        continue;
                    }

                    // reduce to any unique index if applicable
                    var unique = false;
                    var reduced = QueryPlanIndexUniqueHelper.ReduceToUniqueIfPossible(
                        hashIndexProps, hashCoercionTypeArr, hashKeyProps, indexedStreamsUniqueProps[streamIndexed]);
                    if (reduced != null) {
                        hashIndexProps = reduced.PropertyNames;
                        hashCoercionTypeArr = reduced.CoercionTypes;
                        unique = true;
                        rangeIndexProps = new string[0];
                        rangeCoercionTypeArr = new Type[0];
                    }

                    var proposed = new QueryPlanIndexItemForge(
                        hashIndexProps, hashCoercionTypeArr,
                        rangeIndexProps, rangeCoercionTypeArr, unique, null, typePerStream[streamIndexed]);
                    CheckDuplicateOrAdd(proposed, indexesSet);
                }

                // create full-table-scan
                if (indexesSet.IsEmpty()) {
                    indexesSet.Add(
                        new QueryPlanIndexItemForge(
                            new string[0], new Type[0], new string[0], new Type[0], false, null, typePerStream[streamIndexed]));
                }

                indexSpecs[streamIndexed] = QueryPlanIndexForge.MakeIndex(indexesSet);
            }

            return indexSpecs;
        }

        public static SubordPropPlan GetJoinProps(
            ExprNode filterExpr,
            int outsideStreamCount,
            EventType[] allStreamTypesZeroIndexed,
            ExcludePlanHint excludePlanHint)
        {
            // No filter expression means full table scan
            if (filterExpr == null) {
                return new SubordPropPlan();
            }

            // analyze query graph
            var queryGraph = new QueryGraphForge(outsideStreamCount + 1, excludePlanHint, true);
            FilterExprAnalyzer.Analyze(filterExpr, queryGraph, false);

            // Build a list of streams and indexes
            var joinProps = new LinkedHashMap<string, SubordPropHashKeyForge>();
            var rangeProps = new LinkedHashMap<string, SubordPropRangeKeyForge>();
            IDictionary<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> customIndexOps =
                new EmptyDictionary<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge>();

            for (var stream = 0; stream < outsideStreamCount; stream++) {
                var lookupStream = stream + 1;

                var queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);
                var hashKeysAndIndexes = queryGraphValue.HashKeyProps;

                // determine application functions
                foreach (var item in queryGraphValue.Items) {
                    if (item.Entry is QueryGraphValueEntryCustomForge) {
                        if (customIndexOps.IsEmpty()) {
                            customIndexOps = new Dictionary<>();
                        }

                        var custom = (QueryGraphValueEntryCustomForge) item.Entry;
                        custom.MergeInto(customIndexOps);
                    }
                }

                // handle key-lookups
                var keyPropertiesJoin = hashKeysAndIndexes.Keys;
                var indexPropertiesJoin = hashKeysAndIndexes.Indexed;
                if (!keyPropertiesJoin.IsEmpty()) {
                    if (keyPropertiesJoin.Count != indexPropertiesJoin.Length) {
                        throw new IllegalStateException("Invalid query key and index property collection for stream " + stream);
                    }

                    for (var i = 0; i < keyPropertiesJoin.Count; i++) {
                        QueryGraphValueEntryHashKeyedForge keyDesc = keyPropertiesJoin[i];
                        var compareNode = keyDesc.KeyExpr;

                        var keyPropType = compareNode.Forge.EvaluationType.GetBoxedType();
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(indexPropertiesJoin[i]).GetBoxedType();
                        var coercionType = indexedPropType;
                        if (keyPropType != indexedPropType) {
                            coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                        }

                        SubordPropHashKeyForge desc;
                        if (keyPropertiesJoin[i] is QueryGraphValueEntryHashKeyedForgeExpr) {
                            var keyExpr = (QueryGraphValueEntryHashKeyedForgeExpr) keyPropertiesJoin[i];
                            var keyStreamNum = keyExpr.IsRequiresKey ? stream : null;
                            desc = new SubordPropHashKeyForge(keyDesc, keyStreamNum, coercionType);
                        }
                        else {
                            var prop = (QueryGraphValueEntryHashKeyedForgeProp) keyDesc;
                            desc = new SubordPropHashKeyForge(prop, stream, coercionType);
                        }

                        joinProps.Put(indexPropertiesJoin[i], desc);
                    }
                }

                // handle range lookups
                var rangeKeysAndIndexes = queryGraphValue.RangeProps;
                var rangeIndexes = rangeKeysAndIndexes.Indexed;
                var rangeDescs = rangeKeysAndIndexes.Keys;
                if (rangeDescs.IsEmpty()) {
                    continue;
                }

                // get all ranges lookups
                var count = -1;
                foreach (var rangeDesc in rangeDescs) {
                    count++;
                    var rangeIndexProp = rangeIndexes[count];

                    var subqRangeDesc = rangeProps.Get(rangeIndexProp);

                    // other streams may specify the start or end endpoint of a range, therefore this operation can be additive
                    if (subqRangeDesc != null) {
                        if (subqRangeDesc.RangeInfo.Type.IsRange) {
                            continue;
                        }

                        // see if we can make this additive by using a range
                        var relOpOther = (QueryGraphValueEntryRangeRelOpForge) subqRangeDesc.RangeInfo;
                        var relOpThis = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;

                        var opsDesc = QueryGraphRangeUtil.GetCanConsolidate(relOpThis.Type, relOpOther.Type);
                        if (opsDesc != null) {
                            ExprNode start;
                            ExprNode end;
                            if (!opsDesc.IsReverse) {
                                start = relOpOther.Expression;
                                end = relOpThis.Expression;
                            }
                            else {
                                start = relOpThis.Expression;
                                end = relOpOther.Expression;
                            }

                            var allowRangeReversal = relOpOther.IsBetweenPart && relOpThis.IsBetweenPart;
                            var rangeIn = new QueryGraphValueEntryRangeInForge(opsDesc.Type, start, end, allowRangeReversal);

                            var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                            var coercionType = indexedPropType;
                            var proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                            if (proposedType != null && proposedType != indexedPropType) {
                                coercionType = proposedType;
                            }

                            subqRangeDesc = new SubordPropRangeKeyForge(rangeIn, coercionType);
                            rangeProps.Put(rangeIndexProp, subqRangeDesc);
                        }

                        // ignore
                        continue;
                    }

                    // an existing entry has not been found
                    if (rangeDesc.Type.IsRange) {
                        var rangeIn = (QueryGraphValueEntryRangeInForge) rangeDesc;
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                        var coercionType = indexedPropType;
                        var proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                        if (proposedType != null && proposedType != indexedPropType) {
                            coercionType = proposedType;
                        }

                        subqRangeDesc = new SubordPropRangeKeyForge(rangeDesc, coercionType);
                    }
                    else {
                        var relOp = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;
                        var keyPropType = relOp.Expression.Forge.EvaluationType;
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                        var coercionType = indexedPropType;
                        if (keyPropType != indexedPropType) {
                            coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                        }

                        subqRangeDesc = new SubordPropRangeKeyForge(rangeDesc, coercionType);
                    }

                    rangeProps.Put(rangeIndexProp, subqRangeDesc);
                }
            }

            SubordPropInKeywordSingleIndex inKeywordSingleIdxProp = null;
            SubordPropInKeywordMultiIndex inKeywordMultiIdxProp = null;
            if (joinProps.IsEmpty() && rangeProps.IsEmpty()) {
                for (var stream = 0; stream < outsideStreamCount; stream++) {
                    var lookupStream = stream + 1;
                    var queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);

                    var inkwSingles = queryGraphValue.InKeywordSingles;
                    if (inkwSingles.Indexed.Length != 0) {
                        ExprNode[] keys = inkwSingles.Key[0].KeyExprs;
                        var key = inkwSingles.Indexed[0];
                        if (inKeywordSingleIdxProp != null) {
                            continue;
                        }

                        var coercionType = keys[0].Forge.EvaluationType; // for in-comparison the same type is required
                        inKeywordSingleIdxProp = new SubordPropInKeywordSingleIndex(key, coercionType, keys);
                    }

                    var inkwMultis = queryGraphValue.InKeywordMulti;
                    if (!inkwMultis.IsEmpty()) {
                        QueryGraphValuePairInKWMultiIdx multi = inkwMultis[0];
                        inKeywordMultiIdxProp = new SubordPropInKeywordMultiIndex(
                            ExprNodeUtilityQuery.GetIdentResolvedPropertyNames(multi.Indexed), multi.Indexed[0].Forge.EvaluationType,
                            multi.Key.KeyExpr);
                    }

                    if (inKeywordSingleIdxProp != null && inKeywordMultiIdxProp != null) {
                        inKeywordMultiIdxProp = null;
                    }
                }
            }

            return new SubordPropPlan(joinProps, rangeProps, inKeywordSingleIdxProp, inKeywordMultiIdxProp, customIndexOps);
        }

        private static void CheckDuplicateOrAdd(
            QueryPlanIndexItemForge proposed,
            IList<QueryPlanIndexItemForge> indexesSet)
        {
            var found = false;
            foreach (var index in indexesSet) {
                if (proposed.EqualsCompareSortedProps(index)) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                indexesSet.Add(proposed);
            }
        }
    }
} // end of namespace