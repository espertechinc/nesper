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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    /// <summary>
    ///     2 Stream query strategy/execution tree
    ///     (stream 0)         Lookup in stream 1
    ///     (stream 1)         Lookup in stream 0
    ///     <para>
    ///     ------ Example 1   a 3 table join
    ///     </para>
    ///     <para>
    ///         " where streamA.id = streamB.id " +
    ///         "   and streamB.id = streamC.id";
    ///     </para>
    ///     <para>
    ///         =&gt; Index propery names for each stream
    ///         for stream 0 to 4 = "id"
    ///     </para>
    ///     <para>
    ///         =&gt; join order, ie.
    ///         for stream 0 = {1, 2}
    ///         for stream 1 = {factor [0,2]}
    ///         for stream 2 = {1, 0}
    ///     </para>
    ///     <para>
    ///         =&gt; IndexKeyGen optionalIndexKeyGen, created by nested query plan nodes
    ///     </para>
    ///     <para>
    ///     </para>
    ///     <para>
    ///         3 Stream query strategy
    ///         (stream 0)          Nested iteration
    ///         Lookup in stream 1        Lookup in stream 2
    ///     </para>
    ///     <para>
    ///         (stream 1)         Factor
    ///         Lookup in stream 0        Lookup in stream 2
    ///     </para>
    ///     <para>
    ///         (stream 2)         Nested iteration
    ///         Lookup in stream 1        Lookup in stream 0
    ///     </para>
    ///     <para>
    ///     </para>
    ///     <para>
    ///         ------ Example 2  a 4 table join
    ///     </para>
    ///     <para>
    ///         " where streamA.id = streamB.id " +
    ///         "   and streamB.id = streamC.id";
    ///         "   and streamC.id = streamD.id";
    ///     </para>
    ///     <para>
    ///         =&gt; join order, ie.
    ///         for stream 0 = {1, 2, 3}
    ///         for stream 1 = {factor [0,2], use 2 for 3}
    ///         for stream 2 = {factor [1,3], use 1 for 0}
    ///         for stream 3 = {2, 1, 0}
    ///     </para>
    ///     <para>
    ///     </para>
    ///     <para>
    ///         params concepts[] nested iteration, inner loop
    ///     </para>
    ///     <para>
    ///         select * from s1, s2, s3, s4 where s1.id=s2.id and s2.id=s3.id and s3.id=s4.id
    ///     </para>
    ///     <para>
    ///     </para>
    ///     <para>
    ///         (stream 0)              Nested iteration
    ///         Lookup in stream 1        Lookup in stream 2        Lookup in stream 3
    ///     </para>
    ///     <para>
    ///         (stream 1)              Factor
    ///         lookup in stream 0                 Nested iteration
    ///         Lookup in stream 2        Lookup in stream 3
    ///     </para>
    ///     <para>
    ///         (stream 2)              Factor
    ///         lookup in stream 3                 Nested iteration
    ///         Lookup in stream 1        Lookup in stream 0
    ///     </para>
    ///     <para>
    ///         (stream 3)              Nested iteration
    ///         Lookup in stream 2        Lookup in stream 1        Lookup in stream 0
    ///     </para>
    ///     <para>
    ///         ------ Example 4  a 4 table join, orphan table
    ///     </para>
    ///     <para>
    ///         " where streamA.id = streamB.id " +
    ///         "   and streamB.id = streamC.id"; (no table D join criteria)
    ///     </para>
    ///     <para>
    ///         ------ Example 5  a 3 table join with 2 indexes for stream B
    ///     </para>
    ///     <para>
    ///         " where streamA.A1 = streamB.B1 " +
    ///         "   and streamB.B2 = streamC.C1"; (no table D join criteria)
    ///     </para>
    /// </summary>
    /// <summary>
    ///     Builds a query plan for 3 or more streams in a join.
    /// </summary>
    public class NStreamQueryPlanBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static QueryPlanForgeDesc Build(
            QueryGraphForge queryGraph,
            EventType[] typesPerStream,
            HistoricalViewableDesc historicalViewableDesc,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            bool hasForceNestedIter,
            string[][][] indexedStreamsUniqueProps,
            TableMetaData[] tablesPerStream,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".build filterQueryGraph=" + queryGraph);
            }

            var numStreams = queryGraph.NumStreams;
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            var indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(
                queryGraph,
                typesPerStream,
                indexedStreamsUniqueProps);
            if (Log.IsDebugEnabled) {
                Log.Debug(".build Index build completed, indexes=" + QueryPlanIndexForge.Print(indexSpecs));
            }

            // any historical streams don't get indexes, the lookup strategy accounts for cached indexes
            if (historicalViewableDesc.IsHistorical) {
                for (var i = 0; i < historicalViewableDesc.Historical.Length; i++) {
                    if (historicalViewableDesc.Historical[i]) {
                        indexSpecs[i] = null;
                    }
                }
            }

            var planNodeSpecs = new QueryPlanNodeForge[numStreams];
            var worstDepth = int.MaxValue;
            for (var streamNo = 0; streamNo < numStreams; streamNo++) {
                // no plan for historical streams that are dependent upon other streams
                if (historicalViewableDesc.Historical[streamNo] && dependencyGraph.HasDependency(streamNo)) {
                    planNodeSpecs[streamNo] = new QueryPlanNodeNoOpForge();
                    continue;
                }

                var bestChainResult = ComputeBestPath(streamNo, queryGraph, dependencyGraph);
                var bestChain = bestChainResult.Chain;
                if (Log.IsDebugEnabled) {
                    Log.Debug(".build For stream " + streamNo + " bestChain=" + bestChain.RenderAny());
                }

                if (bestChainResult.Depth < worstDepth) {
                    worstDepth = bestChainResult.Depth;
                }

                var planDesc = CreateStreamPlan(
                    streamNo,
                    bestChain,
                    queryGraph,
                    indexSpecs,
                    typesPerStream,
                    historicalViewableDesc.Historical,
                    historicalStreamIndexLists,
                    tablesPerStream,
                    streamJoinAnalysisResult,
                    raw,
                    serdeResolver);

                planNodeSpecs[streamNo] = planDesc.Forge;
                additionalForgeables.AddAll(planDesc.AdditionalForgeables);

                if (Log.IsDebugEnabled) {
                    Log.Debug(".build spec=" + planNodeSpecs[streamNo]);
                }
            }

            // We use the merge/nested (outer) join algorithm instead.
            if (worstDepth < numStreams - 1 && !hasForceNestedIter) {
                return null;
            }

            // build historical index and lookup strategies
            for (var i = 0; i < numStreams; i++) {
                var plan = planNodeSpecs[i];
                QueryPlanNodeForgeVisitor visitor = new ProxyQueryPlanNodeForgeVisitor {
                    ProcVisit = node => {
                        if (node is HistoricalDataPlanNodeForge historical) {
                            var desc = historicalStreamIndexLists[historical.StreamNum]
                                .GetStrategy(
                                    historical.LookupStreamNum,
                                    raw,
                                    serdeResolver);
                            historical.PollResultIndexingStrategy = desc.IndexingForge;
                            historical.HistoricalIndexLookupStrategy = desc.LookupForge;
                            additionalForgeables.AddAll(desc.AdditionalForgeables);
                        }
                    }
                };
                plan.Accept(visitor);
            }

            var forge = new QueryPlanForge(indexSpecs, planNodeSpecs);
            return new QueryPlanForgeDesc(forge, additionalForgeables);
        }

        /// <summary>
        ///     Walks the chain of lookups and constructs lookup strategy and plan specification based
        ///     on the index specifications.
        /// </summary>
        /// <param name="lookupStream">the stream to construct the query plan for</param>
        /// <param name="bestChain">the chain that the lookup follows to make best use of indexes</param>
        /// <param name="queryGraph">the repository for key properties to indexes</param>
        /// <param name="indexSpecsPerStream">specifications of indexes</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="isHistorical">indicator for each stream if it is a historical streams or not</param>
        /// <param name="historicalStreamIndexLists">index management, populated for the query plan</param>
        /// <param name="tablesPerStream">tables</param>
        /// <param name="streamJoinAnalysisResult">stream join analysis</param>
        /// <param name="raw">raw statement information</param>
        /// <param name="serdeResolver">serde resolver</param>
        /// <returns>NestedIterationNode with lookups attached underneath</returns>
        public static QueryPlanNodeForgeDesc CreateStreamPlan(
            int lookupStream,
            int[] bestChain,
            QueryGraphForge queryGraph,
            QueryPlanIndexForge[] indexSpecsPerStream,
            EventType[] typesPerStream,
            bool[] isHistorical,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            TableMetaData[] tablesPerStream,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            var nestedIterNode = new NestedIterationNodeForge(bestChain);
            var currentLookupStream = lookupStream;
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // Walk through each successive lookup
            for (var i = 0; i < bestChain.Length; i++) {
                var indexedStream = bestChain[i];

                QueryPlanNodeForge node;
                if (isHistorical[indexedStream]) {
                    if (historicalStreamIndexLists[indexedStream] == null) {
                        historicalStreamIndexLists[indexedStream] = new HistoricalStreamIndexListForge(
                            indexedStream,
                            typesPerStream,
                            queryGraph);
                    }

                    historicalStreamIndexLists[indexedStream].AddIndex(currentLookupStream);
                    node = new HistoricalDataPlanNodeForge(
                        indexedStream,
                        lookupStream,
                        currentLookupStream,
                        typesPerStream.Length,
                        null);
                }
                else {
                    var tableLookupPlan = CreateLookupPlan(
                        queryGraph,
                        currentLookupStream,
                        indexedStream,
                        streamJoinAnalysisResult.IsVirtualDW(indexedStream),
                        indexSpecsPerStream[indexedStream],
                        typesPerStream,
                        tablesPerStream[indexedStream],
                        raw,
                        serdeResolver);
                    node = new TableLookupNodeForge(tableLookupPlan.Forge);
                    additionalForgeables.AddAll(tableLookupPlan.AdditionalForgeables);
                }

                nestedIterNode.AddChildNode(node);

                currentLookupStream = bestChain[i];
            }

            return new QueryPlanNodeForgeDesc(nestedIterNode, additionalForgeables);
        }

        /// <summary>
        ///     Create the table lookup plan for a from-stream to look up in an indexed stream
        ///     using the columns supplied in the query graph and looking at the actual indexes available
        ///     and their index number.
        /// </summary>
        /// <param name="queryGraph">contains properties joining the 2 streams</param>
        /// <param name="currentLookupStream">stream to use key values from</param>
        /// <param name="indexedStream">stream to look up in</param>
        /// <param name="indexSpecs">index specification defining indexes to be created for stream</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="indexedStreamTableMeta">table info</param>
        /// <param name="indexedStreamIsVDW">vdw indicators</param>
        /// <param name="raw">raw statement information</param>
        /// <param name="serdeResolver">serde resolver</param>
        /// <returns>plan for performing a lookup in a given table using one of the indexes supplied</returns>
        public static TableLookupPlanDesc CreateLookupPlan(
            QueryGraphForge queryGraph,
            int currentLookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            QueryPlanIndexForge indexSpecs,
            EventType[] typesPerStream,
            TableMetaData indexedStreamTableMeta,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            var queryGraphValue = queryGraph.GetGraphValue(currentLookupStream, indexedStream);
            var hashKeyProps = queryGraphValue.HashKeyProps;
            var hashPropsKeys = hashKeyProps.Keys;
            var hashIndexProps = hashKeyProps.Indexed;

            var rangeProps = queryGraphValue.RangeProps;
            var rangePropsKeys = rangeProps.Keys;
            var rangeIndexProps = rangeProps.Indexed;

            var pairIndexHashRewrite =
                indexSpecs.GetIndexNum(hashIndexProps, rangeIndexProps);
            var indexNum = pairIndexHashRewrite?.First;

            // handle index redirection towards unique index
            if (pairIndexHashRewrite != null && pairIndexHashRewrite.Second != null) {
                var indexes = pairIndexHashRewrite.Second;
                var newHashIndexProps = new string[indexes.Length];
                IList<QueryGraphValueEntryHashKeyedForge> newHashKeys = new List<QueryGraphValueEntryHashKeyedForge>();
                for (var i = 0; i < indexes.Length; i++) {
                    newHashIndexProps[i] = hashIndexProps[indexes[i]];
                    newHashKeys.Add(hashPropsKeys[indexes[i]]);
                }

                hashIndexProps = newHashIndexProps;
                hashPropsKeys = newHashKeys;
                rangeIndexProps = Array.Empty<string>();
                rangePropsKeys = Collections.GetEmptyList<QueryGraphValueEntryRangeForge>();
            }

            // no direct hash or range lookups
            if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0) {
                // handle single-direction 'in' keyword
                var singles = queryGraphValue.InKeywordSingles;
                if (!singles.Key.IsEmpty()) {
                    QueryGraphValueEntryInKeywordSingleIdxForge single = null;
                    indexNum = null;
                    if (indexedStreamTableMeta != null) {
                        var indexes = singles.Indexed;
                        var count = 0;
                        foreach (var index in indexes) {
                            var indexPairFound =
                                EventTableIndexUtil.FindIndexBestAvailable(
                                    indexedStreamTableMeta.IndexMetadata.Indexes,
                                    Collections.SingletonSet(index),
                                    Collections.GetEmptySet<string>(),
                                    null);
                            if (indexPairFound != null) {
                                indexNum = new TableLookupIndexReqKey(
                                    indexPairFound.Second.OptionalIndexName,
                                    indexPairFound.Second.OptionalIndexModuleName,
                                    indexedStreamTableMeta.TableName);
                                single = singles.Key[count];
                            }

                            count++;
                        }
                    }
                    else {
                        single = singles.Key[0];
                        var pairIndex = indexSpecs.GetIndexNum(
                            new[] { singles.Indexed[0] },
                            Array.Empty<string>());
                        indexNum = pairIndex.First;
                    }

                    if (indexNum != null) {
                        var forge = new InKeywordTableLookupPlanSingleIdxForge(
                            currentLookupStream,
                            indexedStream,
                            indexedStreamIsVDW,
                            typesPerStream,
                            indexNum,
                            single.KeyExprs);
                        return new TableLookupPlanDesc(forge, EmptyList<StmtClassForgeableFactory>.Instance);
                    }
                }

                // handle multi-direction 'in' keyword
                var multis = queryGraphValue.InKeywordMulti;
                if (!multis.IsEmpty()) {
                    if (indexedStreamTableMeta != null) {
                        return GetFullTableScanTable(
                            currentLookupStream,
                            indexedStream,
                            indexedStreamIsVDW,
                            typesPerStream,
                            indexedStreamTableMeta);
                    }

                    var multi = multis[0];
                    var indexNameArray = new TableLookupIndexReqKey[multi.Indexed.Length];
                    var foundAll = true;
                    for (var i = 0; i < multi.Indexed.Length; i++) {
                        var identNode = (ExprIdentNode)multi.Indexed[i];
                        var pairIndex = indexSpecs.GetIndexNum(
                            new[] { identNode.ResolvedPropertyName },
                            Array.Empty<string>());
                        if (pairIndex == null) {
                            foundAll = false;
                        }
                        else {
                            indexNameArray[i] = pairIndex.First;
                        }
                    }

                    if (foundAll) {
                        var forge = new InKeywordTableLookupPlanMultiIdxForge(
                            currentLookupStream,
                            indexedStream,
                            indexedStreamIsVDW,
                            typesPerStream,
                            indexNameArray,
                            multi.Key.KeyExpr);
                        return new TableLookupPlanDesc(forge, EmptyList<StmtClassForgeableFactory>.Instance);
                    }
                }

                // We don't use a keyed index but use the full stream set as the stream does not have any indexes

                // If no such full set index exists yet, add to specs
                if (indexedStreamTableMeta != null) {
                    return GetFullTableScanTable(
                        currentLookupStream,
                        indexedStream,
                        indexedStreamIsVDW,
                        typesPerStream,
                        indexedStreamTableMeta);
                }

                if (indexNum == null) {
                    indexNum = new TableLookupIndexReqKey(
                        indexSpecs.AddIndex(Array.Empty<string>(), Type.EmptyTypes, typesPerStream[indexedStream]),
                        null);
                }

                var forgeX = new FullTableScanLookupPlanForge(
                    currentLookupStream,
                    indexedStream,
                    indexedStreamIsVDW,
                    typesPerStream,
                    indexNum);
                return new TableLookupPlanDesc(forgeX, EmptyList<StmtClassForgeableFactory>.Instance);
            }

            if (indexNum == null) {
                throw new IllegalStateException(
                    "Failed to query plan as index for " +
                    hashIndexProps.RenderAny() +
                    " and " +
                    rangeIndexProps.RenderAny() +
                    " in the index specification");
            }

            if (indexedStreamTableMeta != null) {
                var indexPairFound =
                    EventTableIndexUtil.FindIndexBestAvailable(
                        indexedStreamTableMeta.IndexMetadata.Indexes,
                        ToSet(hashIndexProps),
                        ToSet(rangeIndexProps),
                        null);
                if (indexPairFound != null) {
                    var indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(
                        indexPairFound.First,
                        hashIndexProps,
                        GetHashKeyFuncsAsSubProp(hashPropsKeys),
                        rangeIndexProps,
                        GetRangeFuncsAsSubProp(rangePropsKeys));
                    if (indexKeyInfo.OrderedKeyCoercionTypes.IsCoerce ||
                        indexKeyInfo.OrderedRangeCoercionTypes.IsCoerce) {
                        return GetFullTableScanTable(
                            currentLookupStream,
                            indexedStream,
                            indexedStreamIsVDW,
                            typesPerStream,
                            indexedStreamTableMeta);
                    }

                    hashPropsKeys = ToHashKeyFuncs(indexKeyInfo.OrderedHashDesc);
                    hashIndexProps = IndexedPropDesc.GetIndexProperties(indexPairFound.First.HashIndexedProps);
                    rangePropsKeys = ToRangeKeyFuncs(indexKeyInfo.OrderedRangeDesc);
                    rangeIndexProps = IndexedPropDesc.GetIndexProperties(indexPairFound.First.RangeIndexedProps);
                    indexNum = new TableLookupIndexReqKey(
                        indexPairFound.Second.OptionalIndexName,
                        indexPairFound.Second.OptionalIndexModuleName,
                        indexedStreamTableMeta.TableName);
                    // the plan will be created below
                    if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0) {
                        return GetFullTableScanTable(
                            currentLookupStream,
                            indexedStream,
                            indexedStreamIsVDW,
                            typesPerStream,
                            indexedStreamTableMeta);
                    }
                }
                else {
                    return GetFullTableScanTable(
                        currentLookupStream,
                        indexedStream,
                        indexedStreamIsVDW,
                        typesPerStream,
                        indexedStreamTableMeta);
                }
            }

            // straight keyed-index lookup
            if (hashIndexProps.Length > 0 && rangeIndexProps.Length == 0) {
                // Determine coercion required
                var coercionTypes = CoercionUtil.GetCoercionTypesHash(
                    typesPerStream,
                    currentLookupStream,
                    indexedStream,
                    hashPropsKeys,
                    hashIndexProps);
                if (coercionTypes.IsCoerce) {
                    // check if there already are coercion types for this index
                    var existCoercionTypes = indexSpecs.GetCoercionTypes(hashIndexProps);
                    if (existCoercionTypes != null) {
                        for (var i = 0; i < existCoercionTypes.Length; i++) {
                            coercionTypes.CoercionTypes[i] = existCoercionTypes[i]
                                .GetCompareToCoercionType(coercionTypes.CoercionTypes[i]);
                        }
                    }

                    if (!indexSpecs.Items.IsEmpty()) {
                        indexSpecs.SetCoercionTypes(hashIndexProps, coercionTypes.CoercionTypes);
                    }
                }

                var coercionTypesArray = coercionTypes.CoercionTypes;
                MultiKeyClassRef tableLookupMultiKey = null;
                IList<StmtClassForgeableFactory> additionalForgeables = EmptyList<StmtClassForgeableFactory>.Instance;
                if (indexNum.TableName != null) {
                    var tableMultiKeyPlan = MultiKeyPlanner.PlanMultiKey(coercionTypesArray, true, raw, serdeResolver);
                    tableLookupMultiKey = tableMultiKeyPlan.ClassRef;
                    additionalForgeables = tableMultiKeyPlan.MultiKeyForgeables;
                }

                var forge = new IndexedTableLookupPlanHashedOnlyForge(
                    currentLookupStream,
                    indexedStream,
                    indexedStreamIsVDW,
                    typesPerStream,
                    indexNum,
                    hashPropsKeys.ToArray(),
                    indexSpecs,
                    coercionTypesArray,
                    tableLookupMultiKey);
                return new TableLookupPlanDesc(forge, additionalForgeables);
            }

            // sorted index lookup
            var coercionTypesRange = CoercionUtil.GetCoercionTypesRange(
                typesPerStream,
                indexedStream,
                rangeIndexProps,
                rangePropsKeys);
            var coercionTypesHash = CoercionUtil.GetCoercionTypesHash(
                typesPerStream,
                currentLookupStream,
                indexedStream,
                hashPropsKeys,
                hashIndexProps);
            if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 1) {
                var range = rangePropsKeys[0];
                Type coercionType = null;
                if (coercionTypesRange.IsCoerce) {
                    coercionType = coercionTypesRange.CoercionTypes[0];
                }

                var forge = new SortedTableLookupPlanForge(
                    currentLookupStream,
                    indexedStream,
                    indexedStreamIsVDW,
                    typesPerStream,
                    indexNum,
                    range,
                    coercionType);
                return new TableLookupPlanDesc(forge, EmptyList<StmtClassForgeableFactory>.Instance);
            }
            else {
                MultiKeyClassRef tableLookupMultiKey = null;
                IList<StmtClassForgeableFactory> additionalForgeables = EmptyList<StmtClassForgeableFactory>.Instance;
                if (indexNum.TableName != null) {
                    var tableMultiKeyPlan = MultiKeyPlanner.PlanMultiKey(
                        coercionTypesHash.CoercionTypes,
                        true,
                        raw,
                        serdeResolver);
                    tableLookupMultiKey = tableMultiKeyPlan.ClassRef;
                    additionalForgeables = tableMultiKeyPlan.MultiKeyForgeables;
                }

                // composite range and index lookup
                var forge = new CompositeTableLookupPlanForge(
                    currentLookupStream,
                    indexedStream,
                    indexedStreamIsVDW,
                    typesPerStream,
                    indexNum,
                    hashPropsKeys,
                    coercionTypesHash.CoercionTypes,
                    rangePropsKeys,
                    coercionTypesRange.CoercionTypes,
                    indexSpecs,
                    tableLookupMultiKey);
                return new TableLookupPlanDesc(forge, additionalForgeables);
            }
        }

        /// <summary>
        ///     Compute a best chain or path for lookups to take for the lookup stream passed in and the query
        ///     property relationships.
        ///     The method runs through all possible permutations of lookup path <seealso cref="NumberSetPermutationEnumeration" />
        ///     until a path is found in which all streams can be accessed via an index.
        ///     If not such path is found, the method returns the path with the greatest depth, ie. where
        ///     the first one or more streams are index accesses.
        ///     If no depth other then zero is found, returns the default nesting order.
        /// </summary>
        /// <param name="lookupStream">stream to start look up</param>
        /// <param name="queryGraph">navigability between streams</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <returns>chain and chain depth</returns>
        public static BestChainResult ComputeBestPath(
            int lookupStream,
            QueryGraphForge queryGraph,
            DependencyGraph dependencyGraph)
        {
            var defNestingorder = BuildDefaultNestingOrder(queryGraph.NumStreams, lookupStream);
            IEnumerable<int[]> streamEnum;
            if (defNestingorder.Length < 6) {
                streamEnum = NumberSetPermutationEnumeration.New(defNestingorder);
            }
            else {
                streamEnum = NumberSetShiftGroupEnumeration.New(defNestingorder);
            }

            int[] bestPermutation = null;
            var bestDepth = -1;

            foreach (var permutation in streamEnum) {
                // Only if the permutation satisfies all dependencies is the permutation considered
                if (dependencyGraph != null) {
                    var pass = IsDependencySatisfied(lookupStream, permutation, dependencyGraph);
                    if (!pass) {
                        continue;
                    }
                }

                var permutationDepth = ComputeNavigableDepth(lookupStream, permutation, queryGraph);

                if (permutationDepth > bestDepth) {
                    bestPermutation = permutation;
                    bestDepth = permutationDepth;
                }

                // Stop when the permutation yielding the full depth (lenght of stream chain) was hit
                if (permutationDepth == queryGraph.NumStreams - 1) {
                    break;
                }
            }

            return new BestChainResult(bestDepth, bestPermutation);
        }

        /// <summary>
        ///     Determine if the proposed permutation of lookups passes dependencies
        /// </summary>
        /// <param name="lookupStream">stream to initiate</param>
        /// <param name="permutation">permutation of lookups</param>
        /// <param name="dependencyGraph">dependencies</param>
        /// <returns>pass or fail indication</returns>
        public static bool IsDependencySatisfied(
            int lookupStream,
            int[] permutation,
            DependencyGraph dependencyGraph)
        {
            foreach (var entry in dependencyGraph.Dependencies) {
                var target = entry.Key;
                var positionTarget = PositionOf(target, lookupStream, permutation);
                if (positionTarget == -1) {
                    throw new ArgumentException(
                        "Target dependency not found in permutation for target " +
                        target +
                        " and permutation " +
                        permutation.RenderAny() +
                        " and lookup stream " +
                        lookupStream);
                }

                // check the position of each dependency, it must be higher
                foreach (var dependency in entry.Value) {
                    var positonDep = PositionOf(dependency, lookupStream, permutation);
                    if (positonDep == -1) {
                        throw new ArgumentException(
                            "Dependency not found in permutation for dependency " +
                            dependency +
                            " and permutation " +
                            permutation.RenderAny() +
                            " and lookup stream " +
                            lookupStream);
                    }

                    if (positonDep > positionTarget) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int PositionOf(
            int stream,
            int lookupStream,
            int[] permutation)
        {
            if (stream == lookupStream) {
                return 0;
            }

            for (var i = 0; i < permutation.Length; i++) {
                if (permutation[i] == stream) {
                    return i + 1;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Given a chain of streams to look up and indexing information, compute the index within the
        ///     chain of the first non-index lookup.
        /// </summary>
        /// <param name="lookupStream">stream to start lookup for</param>
        /// <param name="nextStreams">list of stream numbers next in lookup</param>
        /// <param name="queryGraph">indexing information</param>
        /// <returns>value between 0 and (nextStreams.lenght - 1)</returns>
        public static int ComputeNavigableDepth(
            int lookupStream,
            int[] nextStreams,
            QueryGraphForge queryGraph)
        {
            var currentStream = lookupStream;
            var currentDepth = 0;

            for (var i = 0; i < nextStreams.Length; i++) {
                var nextStream = nextStreams[i];
                var navigable = queryGraph.IsNavigableAtAll(currentStream, nextStream);
                if (!navigable) {
                    break;
                }

                currentStream = nextStream;
                currentDepth++;
            }

            return currentDepth;
        }

        /// <summary>
        ///     Returns default nesting order for a given number of streams for a certain stream.
        ///     Example: numStreams = 5, forStream = 2, result = {0, 1, 3, 4}
        ///     The resulting array has all streams except the forStream, in ascdending order.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="forStream">stream to generate a nesting order for</param>
        /// <returns>
        ///     int array with all stream numbers starting at 0 to (numStreams - 1) leaving theforStream out
        /// </returns>
        public static int[] BuildDefaultNestingOrder(
            int numStreams,
            int forStream)
        {
            var nestingOrder = new int[numStreams - 1];

            var count = 0;
            for (var i = 0; i < numStreams; i++) {
                if (i == forStream) {
                    continue;
                }

                nestingOrder[count++] = i;
            }

            return nestingOrder;
        }

        private static IList<QueryGraphValueEntryRangeForge> ToRangeKeyFuncs(
            IList<SubordPropRangeKeyForge> orderedRangeDesc)
        {
            IList<QueryGraphValueEntryRangeForge> result = new List<QueryGraphValueEntryRangeForge>();
            foreach (var key in orderedRangeDesc) {
                result.Add(key.RangeInfo);
            }

            return result;
        }

        private static IList<QueryGraphValueEntryHashKeyedForge> ToHashKeyFuncs(
            IList<SubordPropHashKeyForge> orderedHashProperties)
        {
            IList<QueryGraphValueEntryHashKeyedForge> result = new List<QueryGraphValueEntryHashKeyedForge>();
            foreach (var key in orderedHashProperties) {
                result.Add(key.HashKey);
            }

            return result;
        }

        private static TableLookupPlanDesc GetFullTableScanTable(
            int lookupStream,
            int indexedStream,
            bool indexedStreamIsVDW,
            EventType[] typesPerStream,
            TableMetaData indexedStreamTableMeta)
        {
            var indexName = new TableLookupIndexReqKey(
                indexedStreamTableMeta.TableName,
                indexedStreamTableMeta.TableModuleName,
                indexedStreamTableMeta.TableName);
            var forge = new FullTableScanUniquePerKeyLookupPlanForge(
                lookupStream,
                indexedStream,
                indexedStreamIsVDW,
                typesPerStream,
                indexName);
            return new TableLookupPlanDesc(forge, EmptyList<StmtClassForgeableFactory>.Instance);
        }

        private static SubordPropRangeKeyForge[] GetRangeFuncsAsSubProp(IList<QueryGraphValueEntryRangeForge> funcs)
        {
            var keys = new SubordPropRangeKeyForge[funcs.Count];
            for (var i = 0; i < funcs.Count; i++) {
                var func = funcs[i];
                keys[i] = new SubordPropRangeKeyForge(func, func.Expressions[0].Forge.EvaluationType);
            }

            return keys;
        }

        private static SubordPropHashKeyForge[] GetHashKeyFuncsAsSubProp(
            IList<QueryGraphValueEntryHashKeyedForge> funcs)
        {
            var keys = new SubordPropHashKeyForge[funcs.Count];
            for (var i = 0; i < funcs.Count; i++) {
                keys[i] = new SubordPropHashKeyForge(funcs[i], null, null);
            }

            return keys;
        }

        private static ISet<string> ToSet(string[] strings)
        {
            return new LinkedHashSet<string>(strings);
        }

        /// <summary>
        ///     Encapsulates the chain information.
        /// </summary>
        public class BestChainResult
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="depth">depth this chain resolves into a indexed lookup</param>
            /// <param name="chain">chain for nested lookup</param>
            public BestChainResult(
                int depth,
                int[] chain)
            {
                Depth = depth;
                Chain = chain;
            }

            /// <summary>
            ///     Returns depth of lookups via index in chain.
            /// </summary>
            /// <returns>depth</returns>
            public int Depth { get; }

            /// <summary>
            ///     Returns chain of stream numbers.
            /// </summary>
            /// <returns>array of stream numbers</returns>
            public int[] Chain { get; }

            public override string ToString()
            {
                return "depth=" + Depth + " chain=" + Chain.RenderAny();
            }
        }
    }
} // end of namespace