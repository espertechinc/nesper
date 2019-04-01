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
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
	/// <summary>
	/// Build query index plans.
	/// </summary>
	public class QueryPlanIndexBuilder {
	    /// <summary>
	    /// Build index specification from navigability info.
	    /// <para />Looks at each stream and determines which properties in the stream must be indexed
	    /// in order for other streams to look up into the stream. Determines the unique set of properties
	    /// to avoid building duplicate indexes on the same set of properties.
	    /// </summary>
	    /// <param name="queryGraph">navigability info</param>
	    /// <param name="typePerStream">type info</param>
	    /// <param name="indexedStreamsUniqueProps">per-stream unique props</param>
	    /// <returns>query index specs for each stream</returns>
	    public static QueryPlanIndexForge[] BuildIndexSpec(QueryGraphForge queryGraph, EventType[] typePerStream, string[][][] indexedStreamsUniqueProps) {
	        int numStreams = queryGraph.NumStreams;
	        QueryPlanIndexForge[] indexSpecs = new QueryPlanIndexForge[numStreams];

	        // For each stream compile a list of index property sets.
	        for (int streamIndexed = 0; streamIndexed < numStreams; streamIndexed++) {
	            IList<QueryPlanIndexItemForge> indexesSet = new List<QueryPlanIndexItemForge>();

	            // Look at the index from the viewpoint of the stream looking up in the index
	            for (int streamLookup = 0; streamLookup < numStreams; streamLookup++) {
	                if (streamIndexed == streamLookup) {
	                    continue;
	                }

	                QueryGraphValueForge value = queryGraph.GetGraphValue(streamLookup, streamIndexed);
	                QueryGraphValuePairHashKeyIndexForge hashKeyAndIndexProps = value.HashKeyProps;

	                // Sort index properties, but use the sorted properties only to eliminate duplicates
	                string[] hashIndexProps = hashKeyAndIndexProps.Indexed;
	                IList<QueryGraphValueEntryHashKeyedForge> hashKeyProps = hashKeyAndIndexProps.Keys;
	                CoercionDesc indexCoercionTypes = CoercionUtil.GetCoercionTypesHash(typePerStream, streamLookup, streamIndexed, hashKeyProps, hashIndexProps);
	                Type[] hashCoercionTypeArr = indexCoercionTypes.CoercionTypes;

	                QueryGraphValuePairRangeIndexForge rangeAndIndexProps = value.RangeProps;
	                string[] rangeIndexProps = rangeAndIndexProps.Indexed;
	                IList<QueryGraphValueEntryRangeForge> rangeKeyProps = rangeAndIndexProps.Keys;
	                CoercionDesc rangeCoercionTypes = CoercionUtil.GetCoercionTypesRange(typePerStream, streamIndexed, rangeIndexProps, rangeKeyProps);
	                Type[] rangeCoercionTypeArr = rangeCoercionTypes.CoercionTypes;

	                if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0) {
	                    QueryGraphValuePairInKWSingleIdxForge singles = value.InKeywordSingles;
	                    if (!singles.Key.IsEmpty()) {
	                        string indexedProp = singles.Indexed[0];
	                        Type indexedType = typePerStream[streamIndexed].GetPropertyType(indexedProp);
	                        QueryPlanIndexItemForge indexItem = new QueryPlanIndexItemForge(new string[]{indexedProp}, new Type[]{indexedType}, new string[0], new Type[0], false, null, typePerStream[streamIndexed]);
	                        CheckDuplicateOrAdd(indexItem, indexesSet);
	                    }

	                    IList<QueryGraphValuePairInKWMultiIdx> multis = value.InKeywordMulti;
	                    if (!multis.IsEmpty()) {
	                        QueryGraphValuePairInKWMultiIdx multi = multis.Get(0);
	                        foreach (ExprNode propIndexed in multi.Indexed) {
	                            ExprIdentNode identNode = (ExprIdentNode) propIndexed;
	                            Type type = identNode.Forge.EvaluationType;
	                            QueryPlanIndexItemForge indexItem = new QueryPlanIndexItemForge(new string[]{identNode.ResolvedPropertyName}, new Type[]{type}, new string[0], new Type[0], false, null, typePerStream[streamIndexed]);
	                            CheckDuplicateOrAdd(indexItem, indexesSet);
	                        }
	                    }
	                    continue;
	                }

	                // reduce to any unique index if applicable
	                bool unique = false;
	                QueryPlanIndexUniqueHelper.ReducedHashKeys reduced = QueryPlanIndexUniqueHelper.ReduceToUniqueIfPossible(hashIndexProps, hashCoercionTypeArr, hashKeyProps, indexedStreamsUniqueProps[streamIndexed]);
	                if (reduced != null) {
	                    hashIndexProps = reduced.PropertyNames;
	                    hashCoercionTypeArr = reduced.CoercionTypes;
	                    unique = true;
	                    rangeIndexProps = new string[0];
	                    rangeCoercionTypeArr = new Type[0];
	                }

	                QueryPlanIndexItemForge proposed = new QueryPlanIndexItemForge(hashIndexProps, hashCoercionTypeArr,
	                        rangeIndexProps, rangeCoercionTypeArr, unique, null, typePerStream[streamIndexed]);
	                CheckDuplicateOrAdd(proposed, indexesSet);
	            }

	            // create full-table-scan
	            if (indexesSet.IsEmpty()) {
	                indexesSet.Add(new QueryPlanIndexItemForge(new string[0], new Type[0], new string[0], new Type[0], false, null, typePerStream[streamIndexed]));
	            }

	            indexSpecs[streamIndexed] = QueryPlanIndexForge.MakeIndex(indexesSet);
	        }

	        return indexSpecs;
	    }

	    public static SubordPropPlan GetJoinProps(ExprNode filterExpr, int outsideStreamCount, EventType[] allStreamTypesZeroIndexed, ExcludePlanHint excludePlanHint) {
	        // No filter expression means full table scan
	        if (filterExpr == null) {
	            return new SubordPropPlan();
	        }

	        // analyze query graph
	        QueryGraphForge queryGraph = new QueryGraphForge(outsideStreamCount + 1, excludePlanHint, true);
	        FilterExprAnalyzer.Analyze(filterExpr, queryGraph, false);

	        // Build a list of streams and indexes
	        LinkedHashMap<string, SubordPropHashKeyForge> joinProps = new LinkedHashMap<string, SubordPropHashKeyForge>();
	        LinkedHashMap<string, SubordPropRangeKeyForge> rangeProps = new LinkedHashMap<string, SubordPropRangeKeyForge>();
	        IDictionary<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> customIndexOps = Collections.EmptyMap();

	        for (int stream = 0; stream < outsideStreamCount; stream++) {
	            int lookupStream = stream + 1;

	            QueryGraphValueForge queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);
	            QueryGraphValuePairHashKeyIndexForge hashKeysAndIndexes = queryGraphValue.HashKeyProps;

	            // determine application functions
	            foreach (QueryGraphValueDescForge item in queryGraphValue.Items) {
	                if (item.Entry is QueryGraphValueEntryCustomForge) {
	                    if (customIndexOps.IsEmpty()) {
	                        customIndexOps = new Dictionary<>();
	                    }
	                    QueryGraphValueEntryCustomForge custom = (QueryGraphValueEntryCustomForge) item.Entry;
	                    custom.MergeInto(customIndexOps);
	                }
	            }

	            // handle key-lookups
	            IList<QueryGraphValueEntryHashKeyedForge> keyPropertiesJoin = hashKeysAndIndexes.Keys;
	            string[] indexPropertiesJoin = hashKeysAndIndexes.Indexed;
	            if (!keyPropertiesJoin.IsEmpty()) {
	                if (keyPropertiesJoin.Count != indexPropertiesJoin.Length) {
	                    throw new IllegalStateException("Invalid query key and index property collection for stream " + stream);
	                }

	                for (int i = 0; i < keyPropertiesJoin.Count; i++) {
	                    QueryGraphValueEntryHashKeyedForge keyDesc = keyPropertiesJoin.Get(i);
	                    ExprNode compareNode = keyDesc.KeyExpr;

	                    Type keyPropType = Boxing.GetBoxedType(compareNode.Forge.EvaluationType);
	                    Type indexedPropType = Boxing.GetBoxedType(allStreamTypesZeroIndexed[0].GetPropertyType(indexPropertiesJoin[i]));
	                    Type coercionType = indexedPropType;
	                    if (keyPropType != indexedPropType) {
	                        coercionType = TypeHelper.GetCompareToCoercionType(keyPropType, indexedPropType);
	                    }

	                    SubordPropHashKeyForge desc;
	                    if (keyPropertiesJoin.Get(i) is QueryGraphValueEntryHashKeyedForgeExpr) {
	                        QueryGraphValueEntryHashKeyedForgeExpr keyExpr = (QueryGraphValueEntryHashKeyedForgeExpr) keyPropertiesJoin.Get(i);
	                        int? keyStreamNum = keyExpr.IsRequiresKey ? stream : null;
	                        desc = new SubordPropHashKeyForge(keyDesc, keyStreamNum, coercionType);
	                    } else {
	                        QueryGraphValueEntryHashKeyedForgeProp prop = (QueryGraphValueEntryHashKeyedForgeProp) keyDesc;
	                        desc = new SubordPropHashKeyForge(prop, stream, coercionType);
	                    }
	                    joinProps.Put(indexPropertiesJoin[i], desc);
	                }
	            }

	            // handle range lookups
	            QueryGraphValuePairRangeIndexForge rangeKeysAndIndexes = queryGraphValue.RangeProps;
	            string[] rangeIndexes = rangeKeysAndIndexes.Indexed;
	            IList<QueryGraphValueEntryRangeForge> rangeDescs = rangeKeysAndIndexes.Keys;
	            if (rangeDescs.IsEmpty()) {
	                continue;
	            }

	            // get all ranges lookups
	            int count = -1;
	            foreach (QueryGraphValueEntryRangeForge rangeDesc in rangeDescs) {
	                count++;
	                string rangeIndexProp = rangeIndexes[count];

	                SubordPropRangeKeyForge subqRangeDesc = rangeProps.Get(rangeIndexProp);

	                // other streams may specify the start or end endpoint of a range, therefore this operation can be additive
	                if (subqRangeDesc != null) {
	                    if (subqRangeDesc.RangeInfo.Type.IsRange) {
	                        continue;
	                    }

	                    // see if we can make this additive by using a range
	                    QueryGraphValueEntryRangeRelOpForge relOpOther = (QueryGraphValueEntryRangeRelOpForge) subqRangeDesc.RangeInfo;
	                    QueryGraphValueEntryRangeRelOpForge relOpThis = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;

	                    QueryGraphRangeConsolidateDesc opsDesc = QueryGraphRangeUtil.GetCanConsolidate(relOpThis.Type, relOpOther.Type);
	                    if (opsDesc != null) {
	                        ExprNode start;
	                        ExprNode end;
	                        if (!opsDesc.IsReverse) {
	                            start = relOpOther.Expression;
	                            end = relOpThis.Expression;
	                        } else {
	                            start = relOpThis.Expression;
	                            end = relOpOther.Expression;
	                        }
	                        bool allowRangeReversal = relOpOther.IsBetweenPart && relOpThis.IsBetweenPart;
	                        QueryGraphValueEntryRangeInForge rangeIn = new QueryGraphValueEntryRangeInForge(opsDesc.Type, start, end, allowRangeReversal);

	                        Type indexedPropType = Boxing.GetBoxedType(allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp));
	                        Type coercionType = indexedPropType;
	                        Type proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
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
	                    QueryGraphValueEntryRangeInForge rangeIn = (QueryGraphValueEntryRangeInForge) rangeDesc;
	                    Type indexedPropType = Boxing.GetBoxedType(allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp));
	                    Type coercionType = indexedPropType;
	                    Type proposedType = CoercionUtil.GetCoercionTypeRangeIn(indexedPropType, rangeIn.ExprStart, rangeIn.ExprEnd);
	                    if (proposedType != null && proposedType != indexedPropType) {
	                        coercionType = proposedType;
	                    }
	                    subqRangeDesc = new SubordPropRangeKeyForge(rangeDesc, coercionType);
	                } else {
	                    QueryGraphValueEntryRangeRelOpForge relOp = (QueryGraphValueEntryRangeRelOpForge) rangeDesc;
	                    Type keyPropType = relOp.Expression.Forge.EvaluationType;
	                    Type indexedPropType = Boxing.GetBoxedType(allStreamTypesZeroIndexed[0].GetPropertyType(rangeIndexProp));
	                    Type coercionType = indexedPropType;
	                    if (keyPropType != indexedPropType) {
	                        coercionType = TypeHelper.GetCompareToCoercionType(keyPropType, indexedPropType);
	                    }
	                    subqRangeDesc = new SubordPropRangeKeyForge(rangeDesc, coercionType);
	                }
	                rangeProps.Put(rangeIndexProp, subqRangeDesc);
	            }
	        }

	        SubordPropInKeywordSingleIndex inKeywordSingleIdxProp = null;
	        SubordPropInKeywordMultiIndex inKeywordMultiIdxProp = null;
	        if (joinProps.IsEmpty() && rangeProps.IsEmpty()) {
	            for (int stream = 0; stream < outsideStreamCount; stream++) {
	                int lookupStream = stream + 1;
	                QueryGraphValueForge queryGraphValue = queryGraph.GetGraphValue(lookupStream, 0);

	                QueryGraphValuePairInKWSingleIdxForge inkwSingles = queryGraphValue.InKeywordSingles;
	                if (inkwSingles.Indexed.Length != 0) {
	                    ExprNode[] keys = inkwSingles.Key.Get(0).KeyExprs;
	                    string key = inkwSingles.Indexed[0];
	                    if (inKeywordSingleIdxProp != null) {
	                        continue;
	                    }
	                    Type coercionType = keys[0].Forge.EvaluationType;  // for in-comparison the same type is required
	                    inKeywordSingleIdxProp = new SubordPropInKeywordSingleIndex(key, coercionType, keys);
	                }

	                IList<QueryGraphValuePairInKWMultiIdx> inkwMultis = queryGraphValue.InKeywordMulti;
	                if (!inkwMultis.IsEmpty()) {
	                    QueryGraphValuePairInKWMultiIdx multi = inkwMultis.Get(0);
	                    inKeywordMultiIdxProp = new SubordPropInKeywordMultiIndex(ExprNodeUtilityQuery.GetIdentResolvedPropertyNames(multi.Indexed), multi.Indexed[0].Forge.EvaluationType, multi.Key.KeyExpr);
	                }

	                if (inKeywordSingleIdxProp != null && inKeywordMultiIdxProp != null) {
	                    inKeywordMultiIdxProp = null;
	                }
	            }
	        }

	        return new SubordPropPlan(joinProps, rangeProps, inKeywordSingleIdxProp, inKeywordMultiIdxProp, customIndexOps);
	    }

	    private static void CheckDuplicateOrAdd(QueryPlanIndexItemForge proposed, IList<QueryPlanIndexItemForge> indexesSet) {
	        bool found = false;
	        foreach (QueryPlanIndexItemForge index in indexesSet) {
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