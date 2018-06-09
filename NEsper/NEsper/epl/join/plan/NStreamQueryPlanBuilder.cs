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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /*
     *
     2 Stream query strategy/execution tree
     (stream 0)         Lookup in stream 1
     (stream 1)         Lookup in stream 0

     * ------ Example 1   a 3 table join
     *
     *          " where streamA.id = streamB.id " +
                "   and streamB.id = streamC.id";

     => Index propery names for each stream
        for stream 0 to 4 = "id"

     => join order, ie.
        for stream 0 = {1, 2}
        for stream 1 = {factor [0,2]}
        for stream 2 = {1, 0}

     => IndexKeyGen optionalIndexKeyGen, created by nested query plan nodes


     3 Stream query strategy
     (stream 0)          Nested iteration
        Lookup in stream 1        Lookup in stream 2

     (stream 1)         Factor
        Lookup in stream 0        Lookup in stream 2

     (stream 2)         Nested iteration
        Lookup in stream 1        Lookup in stream 0


     * ------ Example 2  a 4 table join
     *
     *          " where streamA.id = streamB.id " +
                "   and streamB.id = streamC.id";
                "   and streamC.id = streamD.id";

     => join order, ie.
        for stream 0 = {1, 2, 3}
        for stream 1 = {factor [0,2], use 2 for 3}
        for stream 2 = {factor [1,3], use 1 for 0}
        for stream 3 = {2, 1, 0}


     concepts... nested iteration, inner loop

     select * from s1, s2, s3, s4 where s1.id=s2.id and s2.id=s3.id and s3.id=s4.id


     (stream 0)              Nested iteration
        Lookup in stream 1        Lookup in stream 2        Lookup in stream 3

     (stream 1)              Factor
     lookup in stream 0                 Nested iteration
                              Lookup in stream 2        Lookup in stream 3

     (stream 2)              Factor
     lookup in stream 3                 Nested iteration
                              Lookup in stream 1        Lookup in stream 0

     (stream 3)              Nested iteration
        Lookup in stream 2        Lookup in stream 1        Lookup in stream 0

     * ------ Example 4  a 4 table join, orphan table
     *
     *          " where streamA.id = streamB.id " +
                "   and streamB.id = streamC.id"; (no table D join criteria)

     * ------ Example 5  a 3 table join with 2 indexes for stream B
     *
     *          " where streamA.A1 = streamB.B1 " +
                "   and streamB.B2 = streamC.C1"; (no table D join criteria)
     */

    /// <summary>
    /// Builds a query plan for 3 or more streams in a join.
    /// </summary>
    public class NStreamQueryPlanBuilder
    {
        /// <summary>
        /// Build a query plan based on the stream property relationships indicated in queryGraph.
        /// </summary>
        /// <param name="queryGraph">navigation info between streams</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="historicalViewableDesc">The historical viewable desc.</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <param name="historicalStreamIndexLists">index management, populated for the query plan</param>
        /// <param name="hasForceNestedIter">if set to <c>true</c> [has force nested iter].</param>
        /// <param name="indexedStreamsUniqueProps">The indexed streams unique props.</param>
        /// <param name="tablesPerStream">The tables per stream.</param>
        /// <returns>
        /// query plan
        /// </returns>
        internal static QueryPlan Build(
            QueryGraph queryGraph,
            EventType[] typesPerStream,
            HistoricalViewableDesc historicalViewableDesc,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            bool hasForceNestedIter,
            string[][][] indexedStreamsUniqueProps,
            TableMetadata[] tablesPerStream)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".build queryGraph=" + queryGraph);
            }

            var numStreams = queryGraph.NumStreams;
            var indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, typesPerStream, indexedStreamsUniqueProps);
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".build Index build completed, indexes=" + QueryPlanIndex.Print(indexSpecs));
            }

            // any historical streams don't get indexes, the lookup strategy accounts for cached indexes
            if (historicalViewableDesc.HasHistorical)
            {
                for (var i = 0; i < historicalViewableDesc.Historical.Length; i++)
                {
                    if (historicalViewableDesc.Historical[i])
                    {
                        indexSpecs[i] = null;
                    }
                }
            }

            var planNodeSpecs = new QueryPlanNode[numStreams];
            int worstDepth = int.MaxValue;
            for (var streamNo = 0; streamNo < numStreams; streamNo++)
            {
                // no plan for historical streams that are dependent upon other streams
                if ((historicalViewableDesc.Historical[streamNo]) && (dependencyGraph.HasDependency(streamNo)))
                {
                    planNodeSpecs[streamNo] = new QueryPlanNodeNoOp();
                    continue;
                }

                var bestChainResult = ComputeBestPath(streamNo, queryGraph, dependencyGraph);
                var bestChain = bestChainResult.Chain;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".build For stream " + streamNo + " bestChain=" + bestChain.Render());
                }

                if (bestChainResult.Depth < worstDepth)
                {
                    worstDepth = bestChainResult.Depth;
                }

                planNodeSpecs[streamNo] = CreateStreamPlan(streamNo, bestChain, queryGraph, indexSpecs, typesPerStream, historicalViewableDesc.Historical, historicalStreamIndexLists, tablesPerStream);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".build spec=" + planNodeSpecs[streamNo]);
                }
            }

            // We use the merge/nested (outer) join algorithm instead.
            if ((worstDepth < numStreams - 1) && (!hasForceNestedIter))
            {
                return null;
            }
            return new QueryPlan(indexSpecs, planNodeSpecs);
        }

        /// <summary>
        /// Walks the chain of lookups and constructs lookup strategy and plan specification based
        /// on the index specifications.
        /// </summary>
        /// <param name="lookupStream">the stream to construct the query plan for</param>
        /// <param name="bestChain">the chain that the lookup follows to make best use of indexes</param>
        /// <param name="queryGraph">the repository for key properties to indexes</param>
        /// <param name="indexSpecsPerStream">specifications of indexes</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="isHistorical">indicator for each stream if it is a historical streams or not</param>
        /// <param name="historicalStreamIndexLists">index management, populated for the query plan</param>
        /// <param name="tablesPerStream">The tables per stream.</param>
        /// <returns>
        /// NestedIterationNode with lookups attached underneath
        /// </returns>
        internal static QueryPlanNode CreateStreamPlan(
            int lookupStream,
            int[] bestChain,
            QueryGraph queryGraph,
            QueryPlanIndex[] indexSpecsPerStream,
            EventType[] typesPerStream,
            bool[] isHistorical,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            TableMetadata[] tablesPerStream)
        {
            var nestedIterNode = new NestedIterationNode(bestChain);
            var currentLookupStream = lookupStream;

            // Walk through each successive lookup
            for (var i = 0; i < bestChain.Length; i++)
            {
                var indexedStream = bestChain[i];

                QueryPlanNode node;
                if (isHistorical[indexedStream])
                {
                    if (historicalStreamIndexLists[indexedStream] == null)
                    {
                        historicalStreamIndexLists[indexedStream] = new HistoricalStreamIndexList(indexedStream, typesPerStream, queryGraph);
                    }
                    historicalStreamIndexLists[indexedStream].AddIndex(currentLookupStream);
                    node = new HistoricalDataPlanNode(indexedStream, lookupStream, currentLookupStream, typesPerStream.Length, null);
                }
                else
                {
                    var tableLookupPlan = CreateLookupPlan(queryGraph, currentLookupStream, indexedStream, indexSpecsPerStream[indexedStream], typesPerStream, tablesPerStream[indexedStream]);
                    node = new TableLookupNode(tableLookupPlan);
                }
                nestedIterNode.AddChildNode(node);

                currentLookupStream = bestChain[i];
            }

            return nestedIterNode;
        }

        /// <summary>
        /// Create the table lookup plan for a from-stream to look up in an indexed stream
        /// using the columns supplied in the query graph and looking at the actual indexes available
        /// and their index number.
        /// </summary>
        /// <param name="queryGraph">contains properties joining the 2 streams</param>
        /// <param name="currentLookupStream">stream to use key values from</param>
        /// <param name="indexedStream">stream to look up in</param>
        /// <param name="indexSpecs">index specification defining indexes to be created for stream</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="indexedStreamTableMeta">The indexed stream table meta.</param>
        /// <returns>
        /// plan for performing a lookup in a given table using one of the indexes supplied
        /// </returns>
        /// <exception cref="IllegalStateException">Failed to query plan as index for " + hashIndexProps.Render() + " and " + rangeIndexProps.Render() + " in the index specification</exception>
        public static TableLookupPlan CreateLookupPlan(
            QueryGraph queryGraph, int currentLookupStream, int indexedStream,
            QueryPlanIndex indexSpecs, EventType[] typesPerStream,
            TableMetadata indexedStreamTableMeta)
        {
            var queryGraphValue = queryGraph.GetGraphValue(currentLookupStream, indexedStream);
            var hashKeyProps = queryGraphValue.HashKeyProps;
            var hashPropsKeys = hashKeyProps.Keys;
            var hashIndexProps = hashKeyProps.Indexed.ToArray();

            var rangeProps = queryGraphValue.RangeProps;
            var rangePropsKeys = rangeProps.Keys;
            var rangeIndexProps = rangeProps.Indexed.ToArray();

            var pairIndexHashRewrite = indexSpecs.GetIndexNum(hashIndexProps, rangeIndexProps);
            var indexNum = pairIndexHashRewrite == null ? null : pairIndexHashRewrite.First;

            // handle index redirection towards unique index
            if (pairIndexHashRewrite != null && pairIndexHashRewrite.Second != null)
            {
                var indexes = pairIndexHashRewrite.Second;
                var newHashIndexProps = new string[indexes.Length];
                IList<QueryGraphValueEntryHashKeyed> newHashKeys = new List<QueryGraphValueEntryHashKeyed>();
                for (var i = 0; i < indexes.Length; i++)
                {
                    newHashIndexProps[i] = hashIndexProps[indexes[i]];
                    newHashKeys.Add(hashPropsKeys[indexes[i]]);
                }
                hashIndexProps = newHashIndexProps;
                hashPropsKeys = newHashKeys;
                rangeIndexProps = new string[0];
                rangePropsKeys = Collections.GetEmptyList<QueryGraphValueEntryRange>();
            }

            // no direct hash or range lookups
            if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0)
            {

                // handle single-direction 'in' keyword
                var singles = queryGraphValue.InKeywordSingles;
                if (!singles.Key.IsEmpty())
                {

                    QueryGraphValueEntryInKeywordSingleIdx single = null;
                    indexNum = null;
                    if (indexedStreamTableMeta != null)
                    {
                        var indexes = singles.Indexed;
                        var count = 0;
                        foreach (var index in indexes)
                        {
                            Pair<IndexMultiKey, EventTableIndexEntryBase> indexPairFound =
                                EventTableIndexUtil.FindIndexBestAvailable(
                                    indexedStreamTableMeta.EventTableIndexMetadataRepo.IndexesAsBase,
                                    Collections.SingletonSet(index),
                                    Collections.GetEmptySet<string>(), null);
                            if (indexPairFound != null)
                            {
                                indexNum = new TableLookupIndexReqKey(indexPairFound.Second.OptionalIndexName, indexedStreamTableMeta.TableName);
                                single = singles.Key[count];
                            }
                            count++;
                        }
                    }
                    else
                    {
                        single = singles.Key[0];
                        var pairIndex = indexSpecs.GetIndexNum(new string[] { singles.Indexed[0] }, null);
                        indexNum = pairIndex.First;
                    }

                    if (indexNum != null)
                    {
                        return new InKeywordTableLookupPlanSingleIdx(currentLookupStream, indexedStream, indexNum, single.KeyExprs);
                    }
                }

                // handle multi-direction 'in' keyword
                var multis = queryGraphValue.InKeywordMulti;
                if (!multis.IsEmpty())
                {
                    if (indexedStreamTableMeta != null)
                    {
                        return GetFullTableScanTable(currentLookupStream, indexedStream, indexedStreamTableMeta);
                    }
                    QueryGraphValuePairInKWMultiIdx multi = multis[0];
                    var indexNameArray = new TableLookupIndexReqKey[multi.Indexed.Count];
                    var foundAll = true;
                    for (var i = 0; i < multi.Indexed.Count; i++)
                    {
                        var identNode = (ExprIdentNode)multi.Indexed[i];
                        var pairIndex = indexSpecs.GetIndexNum(new string[] { identNode.ResolvedPropertyName }, null);
                        if (pairIndex == null)
                        {
                            foundAll = false;
                        }
                        else
                        {
                            indexNameArray[i] = pairIndex.First;
                        }
                    }
                    if (foundAll)
                    {
                        return new InKeywordTableLookupPlanMultiIdx(currentLookupStream, indexedStream, indexNameArray, multi.Key.KeyExpr);
                    }
                }

                // We don't use a keyed index but use the full stream set as the stream does not have any indexes

                // If no such full set index exists yet, add to specs
                if (indexedStreamTableMeta != null)
                {
                    return GetFullTableScanTable(currentLookupStream, indexedStream, indexedStreamTableMeta);
                }
                if (indexNum == null)
                {
                    indexNum = new TableLookupIndexReqKey(indexSpecs.AddIndex(null, null));
                }
                return new FullTableScanLookupPlan(currentLookupStream, indexedStream, indexNum);
            }

            if (indexNum == null)
            {
                throw new IllegalStateException("Failed to query plan as index for " + hashIndexProps.Render() + " and " + rangeIndexProps.Render() + " in the index specification");
            }

            if (indexedStreamTableMeta != null)
            {
                var indexPairFound = EventTableIndexUtil.FindIndexBestAvailable(
                    indexedStreamTableMeta.EventTableIndexMetadataRepo.IndexesAsBase, 
                    ToSet(hashIndexProps), 
                    ToSet(rangeIndexProps), 
                    null);
                if (indexPairFound != null)
                {
                    var indexKeyInfo = SubordinateQueryPlannerUtil.CompileIndexKeyInfo(indexPairFound.First, hashIndexProps, GetHashKeyFuncsAsSubProp(hashPropsKeys), rangeIndexProps, GetRangeFuncsAsSubProp(rangePropsKeys));
                    if (indexKeyInfo.OrderedKeyCoercionTypes.IsCoerce || indexKeyInfo.OrderedRangeCoercionTypes.IsCoerce)
                    {
                        return GetFullTableScanTable(currentLookupStream, indexedStream, indexedStreamTableMeta);
                    }
                    hashPropsKeys = ToHashKeyFuncs(indexKeyInfo.OrderedHashDesc);
                    hashIndexProps = IndexedPropDesc.GetIndexProperties(indexPairFound.First.HashIndexedProps);
                    rangePropsKeys = ToRangeKeyFuncs(indexKeyInfo.OrderedRangeDesc);
                    rangeIndexProps = IndexedPropDesc.GetIndexProperties(indexPairFound.First.RangeIndexedProps);
                    indexNum = new TableLookupIndexReqKey(indexPairFound.Second.OptionalIndexName, indexedStreamTableMeta.TableName);
                    // the plan will be created below
                    if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 0)
                    {
                        return GetFullTableScanTable(currentLookupStream, indexedStream, indexedStreamTableMeta);
                    }
                }
                else
                {
                    return GetFullTableScanTable(currentLookupStream, indexedStream, indexedStreamTableMeta);
                }
            }

            // straight keyed-index lookup
            if (hashIndexProps.Length > 0 && rangeIndexProps.Length == 0)
            {
                TableLookupPlan tableLookupPlan;
                if (hashPropsKeys.Count == 1)
                {
                    tableLookupPlan = new IndexedTableLookupPlanSingle(currentLookupStream, indexedStream, indexNum, hashPropsKeys[0]);
                }
                else
                {
                    tableLookupPlan = new IndexedTableLookupPlanMulti(currentLookupStream, indexedStream, indexNum, hashPropsKeys);
                }

                // Determine coercion required
                var coercionTypes = CoercionUtil.GetCoercionTypesHash(typesPerStream, currentLookupStream, indexedStream, hashPropsKeys, hashIndexProps);
                if (coercionTypes.IsCoerce)
                {
                    // check if there already are coercion types for this index
                    var existCoercionTypes = indexSpecs.GetCoercionTypes(hashIndexProps);
                    if (existCoercionTypes != null)
                    {
                        for (var i = 0; i < existCoercionTypes.Length; i++)
                        {
                            coercionTypes.CoercionTypes[i] = TypeHelper.GetCompareToCoercionType(existCoercionTypes[i], coercionTypes.CoercionTypes[i]);
                        }
                    }
                    indexSpecs.SetCoercionTypes(hashIndexProps, coercionTypes.CoercionTypes);
                }

                return tableLookupPlan;
            }

            // sorted index lookup
            if (hashIndexProps.Length == 0 && rangeIndexProps.Length == 1)
            {
                QueryGraphValueEntryRange range = rangePropsKeys[0];
                return new SortedTableLookupPlan(currentLookupStream, indexedStream, indexNum, range);
            }
            // composite range and index lookup
            else
            {
                return new CompositeTableLookupPlan(currentLookupStream, indexedStream, indexNum, hashPropsKeys, rangePropsKeys);
            }
        }

        /// <summary>
        /// Compute a best chain or path for lookups to take for the lookup stream passed in and the query
        /// property relationships.
        /// The method runs through all possible permutations of lookup path <seealso cref="NumberSetPermutationEnumeration" />until a path is found in which all streams can be accessed via an index.
        /// If not such path is found, the method returns the path with the greatest depth, ie. where
        /// the first one or more streams are index accesses.
        /// If no depth other then zero is found, returns the default nesting order.
        /// </summary>
        /// <param name="lookupStream">stream to start look up</param>
        /// <param name="queryGraph">navigability between streams</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <returns>chain and chain depth</returns>
        internal static BestChainResult ComputeBestPath(int lookupStream, QueryGraph queryGraph, DependencyGraph dependencyGraph)
        {
            var defNestingorder = BuildDefaultNestingOrder(queryGraph.NumStreams, lookupStream);
            IEnumerator<int[]> streamEnum;
            if (defNestingorder.Length < 6)
            {
                streamEnum = NumberSetPermutationEnumeration.New(defNestingorder).GetEnumerator();
            }
            else
            {
                streamEnum = NumberSetShiftGroupEnumeration.New(defNestingorder).GetEnumerator();
            }
            int[] bestPermutation = null;
            var bestDepth = -1;

            while (streamEnum.MoveNext())
            {
                int[] permutation = streamEnum.Current;

                // Only if the permutation satisfies all dependencies is the permutation considered
                if (dependencyGraph != null)
                {
                    var pass = IsDependencySatisfied(lookupStream, permutation, dependencyGraph);
                    if (!pass)
                    {
                        continue;
                    }
                }

                var permutationDepth = ComputeNavigableDepth(lookupStream, permutation, queryGraph);

                if (permutationDepth > bestDepth)
                {
                    bestPermutation = permutation;
                    bestDepth = permutationDepth;
                }

                // Stop when the permutation yielding the full depth (lenght of stream chain) was hit
                if (permutationDepth == queryGraph.NumStreams - 1)
                {
                    break;
                }
            }

            return new BestChainResult(bestDepth, bestPermutation);
        }

        /// <summary>
        /// Determine if the proposed permutation of lookups passes dependencies
        /// </summary>
        /// <param name="lookupStream">stream to initiate</param>
        /// <param name="permutation">permutation of lookups</param>
        /// <param name="dependencyGraph">dependencies</param>
        /// <returns>pass or fail indication</returns>
        internal static bool IsDependencySatisfied(int lookupStream, int[] permutation, DependencyGraph dependencyGraph)
        {
            foreach (var entry in dependencyGraph.Dependencies)
            {
                var target = entry.Key;
                var positionTarget = PositionOf(target, lookupStream, permutation);
                if (positionTarget == -1)
                {
                    throw new ArgumentException("Target dependency not found in permutation for target " + target + " and permutation " + permutation.Render() + " and lookup stream " + lookupStream);
                }

                // check the position of each dependency, it must be higher
                foreach (int dependency in entry.Value)
                {
                    var positonDep = PositionOf(dependency, lookupStream, permutation);
                    if (positonDep == -1)
                    {
                        throw new ArgumentException("Dependency not found in permutation for dependency " + dependency + " and permutation " + permutation.Render() + " and lookup stream " + lookupStream);
                    }

                    if (positonDep > positionTarget)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static int PositionOf(int stream, int lookupStream, int[] permutation)
        {
            if (stream == lookupStream)
            {
                return 0;
            }
            for (var i = 0; i < permutation.Length; i++)
            {
                if (permutation[i] == stream)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Given a chain of streams to look up and indexing information, compute the index within the
        /// chain of the first non-index lookup.
        /// </summary>
        /// <param name="lookupStream">stream to start lookup for</param>
        /// <param name="nextStreams">list of stream numbers next in lookup</param>
        /// <param name="queryGraph">indexing information</param>
        /// <returns>value between 0 and (nextStreams.lenght - 1)</returns>
        internal static int ComputeNavigableDepth(int lookupStream, int[] nextStreams, QueryGraph queryGraph)
        {
            var currentStream = lookupStream;
            var currentDepth = 0;

            for (var i = 0; i < nextStreams.Length; i++)
            {
                var nextStream = nextStreams[i];
                var navigable = queryGraph.IsNavigableAtAll(currentStream, nextStream);
                if (!navigable)
                {
                    break;
                }
                currentStream = nextStream;
                currentDepth++;
            }

            return currentDepth;
        }

        /// <summary>
        /// Returns default nesting order for a given number of streams for a certain stream.
        /// Example: numStreams = 5, forStream = 2, result = {0, 1, 3, 4}
        /// The resulting array has all streams except the forStream, in ascdending order.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="forStream">stream to generate a nesting order for</param>
        /// <returns>int array with all stream numbers starting at 0 to (numStreams - 1) leaving theforStream out
        /// </returns>
        internal static int[] BuildDefaultNestingOrder(int numStreams, int forStream)
        {
            var nestingOrder = new int[numStreams - 1];

            var count = 0;
            for (var i = 0; i < numStreams; i++)
            {
                if (i == forStream)
                {
                    continue;
                }
                nestingOrder[count++] = i;
            }

            return nestingOrder;
        }

        private static IList<QueryGraphValueEntryRange> ToRangeKeyFuncs(IEnumerable<SubordPropRangeKey> orderedRangeDesc)
        {
            return orderedRangeDesc.Select(key => key.RangeInfo).ToList();
        }

        private static IList<QueryGraphValueEntryHashKeyed> ToHashKeyFuncs(IEnumerable<SubordPropHashKey> orderedHashProperties)
        {
            return orderedHashProperties.Select(key => key.HashKey).ToList();
        }

        private static TableLookupPlan GetFullTableScanTable(int lookupStream, int indexedStream, TableMetadata indexedStreamTableMeta)
        {
            var indexName = new TableLookupIndexReqKey(indexedStreamTableMeta.TableName, indexedStreamTableMeta.TableName);
            return new FullTableScanUniquePerKeyLookupPlan(lookupStream, indexedStream, indexName);
        }

        private static ISet<string> ToSet(IEnumerable<string> strings)
        {
            return new LinkedHashSet<string>(strings);
        }

        private static SubordPropRangeKey[] GetRangeFuncsAsSubProp(IList<QueryGraphValueEntryRange> funcs)
        {
            var keys = new SubordPropRangeKey[funcs.Count];
            for (var i = 0; i < funcs.Count; i++)
            {
                var func = funcs[i];
                keys[i] = new SubordPropRangeKey(func, func.Expressions[0].ExprEvaluator.ReturnType);
            }
            return keys;
        }

        private static SubordPropHashKey[] GetHashKeyFuncsAsSubProp(IList<QueryGraphValueEntryHashKeyed> funcs)
        {
            var keys = new SubordPropHashKey[funcs.Count];
            for (var i = 0; i < funcs.Count; i++)
            {
                keys[i] = new SubordPropHashKey(funcs[i], null, null);
            }
            return keys;
        }

        /// <summary>
        /// Encapsulates the chain information.
        /// </summary>
        public class BestChainResult
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="depth">depth this chain resolves into a indexed lookup</param>
            /// <param name="chain">chain for nested lookup</param>
            public BestChainResult(int depth, int[] chain)
            {
                Depth = depth;
                Chain = chain;
            }

            /// <summary>
            /// Returns depth of lookups via index in chain.
            /// </summary>
            /// <value>depth</value>
            public int Depth { get; private set; }

            /// <summary>
            /// Returns chain of stream numbers.
            /// </summary>
            /// <value>array of stream numbers</value>
            public int[] Chain { get; private set; }

            public override string ToString()
            {
                return "depth=" + Depth + " chain=" + Chain.Render();
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
