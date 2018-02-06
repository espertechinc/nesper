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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.hint;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Build query index plans.
    /// </summary>
    public class QueryPlanIndexBuilder
    {
        /// <summary>
        /// Build index specification from navigability INFO.
        /// <para/>
        /// Looks at each stream and determines which properties in the stream must be indexed
        /// in order for other streams to look up into the stream. Determines the unique set of
        /// properties to avoid building duplicate indexes on the same set of properties.
        /// </summary>
        /// <param name="queryGraph">navigability INFO</param>
        /// <param name="typePerStream">The type per stream.</param>
        /// <param name="indexedStreamsUniqueProps">The indexed streams unique props.</param>
        /// <returns>query index specs for each stream</returns>
        public static QueryPlanIndex[] BuildIndexSpec(QueryGraph queryGraph, EventType[] typePerStream, String[][][] indexedStreamsUniqueProps)
        {
            var numStreams = queryGraph.NumStreams;
            var indexSpecs = new QueryPlanIndex[numStreams];
    
            // For each stream compile a list of index property sets.
            for (int streamIndexed = 0; streamIndexed < numStreams; streamIndexed++)
            {
                var indexesSet = new List<QueryPlanIndexItem>();
    
                // Look at the index from the viewpoint of the stream looking up in the index
                for (int streamLookup = 0; streamLookup < numStreams; streamLookup++)
                {
                    if (streamIndexed == streamLookup)
                    {
                        continue;
                    }
    
                    var value = queryGraph.GetGraphValue(streamLookup, streamIndexed);
                    var hashKeyAndIndexProps = value.HashKeyProps;
    
                    // Sort index properties, but use the sorted properties only to eliminate duplicates
                    var hashIndexProps = hashKeyAndIndexProps.Indexed;
                    var hashKeyProps = hashKeyAndIndexProps.Keys;
                    var indexCoercionTypes = CoercionUtil.GetCoercionTypesHash(typePerStream, streamLookup, streamIndexed, hashKeyProps, hashIndexProps);
                    var hashCoercionTypeArr = indexCoercionTypes.CoercionTypes;

                    var rangeAndIndexProps = value.RangeProps;
                    var rangeIndexProps = rangeAndIndexProps.Indexed;
                    var rangeKeyProps = rangeAndIndexProps.Keys;
                    var rangeCoercionTypes = CoercionUtil.GetCoercionTypesRange(typePerStream, streamIndexed, rangeIndexProps, rangeKeyProps);
                    var rangeCoercionTypeArr = rangeCoercionTypes.CoercionTypes;

                    if (hashIndexProps.Count == 0 && rangeIndexProps.Count == 0)
                    {
                        QueryGraphValuePairInKWSingleIdx singles = value.InKeywordSingles;
                        if (!singles.Key.IsEmpty()) {
                            String indexedProp = singles.Indexed[0];
                            QueryPlanIndexItem indexItem = new QueryPlanIndexItem(new String[] {indexedProp}, null, null, null, false, null);
                            CheckDuplicateOrAdd(indexItem, indexesSet);
                        }

                        IList<QueryGraphValuePairInKWMultiIdx> multis = value.InKeywordMulti;
                        if (!multis.IsEmpty()) {
                            QueryGraphValuePairInKWMultiIdx multi = multis[0];
                            foreach (ExprNode propIndexed in multi.Indexed) {
                                ExprIdentNode identNode = (ExprIdentNode) propIndexed;
                                QueryPlanIndexItem indexItem = new QueryPlanIndexItem(new String[] {identNode.ResolvedPropertyName}, null, null, null, false, null);
                                CheckDuplicateOrAdd(indexItem, indexesSet);
                            }
                        }

                        continue;
                    }

                    // reduce to any unique index if applicable
                    var unique = false;
                    var reduced = QueryPlanIndexUniqueHelper.ReduceToUniqueIfPossible(hashIndexProps, hashCoercionTypeArr, hashKeyProps, indexedStreamsUniqueProps[streamIndexed]);
                    if (reduced != null)
                    {
                        hashIndexProps = reduced.PropertyNames;
                        hashCoercionTypeArr = reduced.CoercionTypes;
                        unique = true;
                        rangeIndexProps = new String[0];
                        rangeCoercionTypeArr = new Type[0];
                    }

                    var proposed = new QueryPlanIndexItem(
                        hashIndexProps, 
                        hashCoercionTypeArr, 
                        rangeIndexProps, 
                        rangeCoercionTypeArr, 
                        unique, null);
                    CheckDuplicateOrAdd(proposed, indexesSet);
                }
    
                // create full-table-scan
                if (indexesSet.IsEmpty()) {
                    indexesSet.Add(new QueryPlanIndexItem(null, null, null, null, false, null));
                }
    
                indexSpecs[streamIndexed] = QueryPlanIndex.MakeIndex(indexesSet);
            }
    
            return indexSpecs;
        }

        public static SubordPropPlan GetJoinProps(ExprNode filterExpr, int outsideStreamCount, EventType[] allStreamTypesZeroIndexed, ExcludePlanHint excludePlanHint)
        {
            // No filter expression means full table scan
            if (filterExpr == null)
            {
                return new SubordPropPlan();
            }
    
            // analyze query graph
            var queryGraph = new QueryGraph(outsideStreamCount + 1, excludePlanHint, true);
            FilterExprAnalyzer.Analyze(filterExpr, queryGraph, false);
    
            // Build a list of streams and indexes
            var joinProps = new LinkedHashMap<String, SubordPropHashKey>();
            var rangeProps = new LinkedHashMap<String, SubordPropRangeKey>();
            var customIndexOps = Collections.GetEmptyMap<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation>();
            
            for (int stream = 0; stream <  outsideStreamCount; stream++)
            {
                int lookupStream = stream + 1;
    
                QueryGraphValue queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);
                QueryGraphValuePairHashKeyIndex hashKeysAndIndexes = queryGraphValue.HashKeyProps;

                // determine application functions
                foreach (QueryGraphValueDesc item in queryGraphValue.Items)
                {
                    if (item.Entry is QueryGraphValueEntryCustom)
                    {
                        if (customIndexOps.IsEmpty())
                        {
                            customIndexOps = new Dictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation>();
                        }
                        QueryGraphValueEntryCustom custom = (QueryGraphValueEntryCustom) item.Entry;
                        custom.MergeInto(customIndexOps);
                    }
                }

                // handle key-lookups
                var keyPropertiesJoin = hashKeysAndIndexes.Keys;
                var indexPropertiesJoin = hashKeysAndIndexes.Indexed;
                if (keyPropertiesJoin.IsNotEmpty())
                {
                    if (keyPropertiesJoin.Count != indexPropertiesJoin.Count)
                    {
                        throw new IllegalStateException("Invalid query key and index property collection for stream " + stream);
                    }
    
                    for (int i = 0; i < keyPropertiesJoin.Count; i++)
                    {
                        QueryGraphValueEntryHashKeyed keyDesc = keyPropertiesJoin[i];
                        ExprNode compareNode = keyDesc.KeyExpr;
    
                        var keyPropType = compareNode.ExprEvaluator.ReturnType.GetBoxedType();
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(indexPropertiesJoin[i]).GetBoxedType();
                        var coercionType = indexedPropType;
                        if (keyPropType != indexedPropType)
                        {
                            coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                        }
    
                        SubordPropHashKey desc;
                        if (keyPropertiesJoin[i] is QueryGraphValueEntryHashKeyedExpr) {
                            var keyExpr = (QueryGraphValueEntryHashKeyedExpr) keyPropertiesJoin[i];
                            var keyStreamNum = keyExpr.IsRequiresKey ? stream : (int?) null;
                            desc = new SubordPropHashKey(keyDesc, keyStreamNum, coercionType);
                        }
                        else {
                            var prop = (QueryGraphValueEntryHashKeyedProp) keyDesc;
                            desc = new SubordPropHashKey(prop, stream, coercionType);
                        }
                        joinProps.Put(indexPropertiesJoin[i], desc);
                    }
                }
    
                // handle range lookups
                QueryGraphValuePairRangeIndex rangeKeysAndIndexes = queryGraphValue.RangeProps;
                var rangeIndexes = rangeKeysAndIndexes.Indexed;
                var rangeDescs = rangeKeysAndIndexes.Keys;
                if (rangeDescs.IsEmpty()) {
                    continue;
                }
    
                // get all ranges lookups
                int count = -1;
                foreach (QueryGraphValueEntryRange rangeDesc in rangeDescs) {
                    count++;
                    String rangeIndexProp = rangeIndexes[count];
    
                    SubordPropRangeKey subqRangeDesc = rangeProps.Get(rangeIndexProp);
    
                    // other streams may specify the start or end endpoint of a range, therefore this operation can be additive
                    if (subqRangeDesc != null) {
                        if (subqRangeDesc.RangeInfo.RangeType.IsRange()) {
                            continue;
                        }
    
                        // see if we can make this additive by using a range
                        var relOpOther = (QueryGraphValueEntryRangeRelOp) subqRangeDesc.RangeInfo;
                        var relOpThis = (QueryGraphValueEntryRangeRelOp) rangeDesc;
    
                        QueryGraphRangeConsolidateDesc opsDesc = QueryGraphRangeUtil.GetCanConsolidate(
                            relOpThis.RangeType, 
                            relOpOther.RangeType);
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
                            var rangeIn = new QueryGraphValueEntryRangeIn(opsDesc.RangeType, start, end, allowRangeReversal);
    
                            var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                            var coercionType = indexedPropType;
                            var proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                            if (proposedType != null && proposedType != indexedPropType)
                            {
                                coercionType = proposedType;
                            }
    
                            subqRangeDesc = new SubordPropRangeKey(rangeIn, coercionType);
                            rangeProps.Put(rangeIndexProp, subqRangeDesc);
                        }
                        // ignore
                        continue;
                    }
    
                    // an existing entry has not been found
                    if (rangeDesc.RangeType.IsRange()) {
                        var rangeIn = (QueryGraphValueEntryRangeIn) rangeDesc;
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                        var coercionType = indexedPropType;
                        var proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
                        if (proposedType != null && proposedType != indexedPropType)
                        {
                            coercionType = proposedType;
                        }
                        subqRangeDesc = new SubordPropRangeKey(rangeDesc, coercionType);
                    }
                    else {
                        var relOp = (QueryGraphValueEntryRangeRelOp) rangeDesc;
                        var keyPropType = relOp.Expression.ExprEvaluator.ReturnType;
                        var indexedPropType = allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp).GetBoxedType();
                        var coercionType = indexedPropType;
                        if (keyPropType != indexedPropType)
                        {
                            coercionType = keyPropType.GetCompareToCoercionType(indexedPropType);
                        }
                        subqRangeDesc = new SubordPropRangeKey(rangeDesc, coercionType);
                    }
                    rangeProps.Put(rangeIndexProp, subqRangeDesc);
                }
            }

            SubordPropInKeywordSingleIndex inKeywordSingleIdxProp = null;
            SubordPropInKeywordMultiIndex inKeywordMultiIdxProp = null;
            if (joinProps.IsEmpty() && rangeProps.IsEmpty()) {
                for (int stream = 0; stream <  outsideStreamCount; stream++) {
                    int lookupStream = stream + 1;
                    QueryGraphValue queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);

                    QueryGraphValuePairInKWSingleIdx inkwSingles = queryGraphValue.InKeywordSingles;
                    if (inkwSingles.Indexed.Length != 0) {
                        var keys = inkwSingles.Key[0].KeyExprs;
                        var key = inkwSingles.Indexed[0];
                        if (inKeywordSingleIdxProp != null) {
                            continue;
                        }
                        var coercionType = keys[0].ExprEvaluator.ReturnType;  // for in-comparison the same type is required
                        inKeywordSingleIdxProp = new SubordPropInKeywordSingleIndex(key, coercionType, keys);
                    }

                    IList<QueryGraphValuePairInKWMultiIdx> inkwMultis = queryGraphValue.InKeywordMulti;
                    if (!inkwMultis.IsEmpty()) {
                        QueryGraphValuePairInKWMultiIdx multi = inkwMultis[0];
                        inKeywordMultiIdxProp = new SubordPropInKeywordMultiIndex(
                            ExprNodeUtility.GetIdentResolvedPropertyNames(multi.Indexed), 
                            multi.Indexed[0].ExprEvaluator.ReturnType, 
                            multi.Key.KeyExpr);
                    }

                    if (inKeywordSingleIdxProp != null && inKeywordMultiIdxProp != null) {
                        inKeywordMultiIdxProp = null;
                    }
                }
            }

            return new SubordPropPlan(joinProps, rangeProps, inKeywordSingleIdxProp, inKeywordMultiIdxProp, customIndexOps);
        }

        private static void CheckDuplicateOrAdd(QueryPlanIndexItem proposed, IList<QueryPlanIndexItem> indexesSet)
        {
            var found = indexesSet.Any(proposed.EqualsCompareSortedProps);
            if (!found)
            {
                indexesSet.Add(proposed);
            }
        }
    }
}
