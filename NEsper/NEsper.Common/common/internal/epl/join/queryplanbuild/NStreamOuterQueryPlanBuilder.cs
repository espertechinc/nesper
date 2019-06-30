///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.@join.assemble;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.queryplanouter;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    /// <summary>
    ///     Builds a query plan for 3 or more streams in a outer join.
    /// </summary>
    public class NStreamOuterQueryPlanBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NStreamOuterQueryPlanBuilder));

        protected internal static QueryPlanForge Build(
            QueryGraphForge queryGraph,
            OuterJoinDesc[] outerJoinDescList,
            string[] streamNames,
            EventType[] typesPerStream,
            HistoricalViewableDesc historicalViewableDesc,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            string[][][] indexedStreamsUniqueProps,
            TableMetaData[] tablesPerStream,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".build filterQueryGraph=" + queryGraph);
            }

            var numStreams = queryGraph.NumStreams;
            var planNodeSpecs = new QueryPlanNodeForge[numStreams];

            // Build index specifications
            var indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, typesPerStream, indexedStreamsUniqueProps);

            // any historical streams don't get indexes, the lookup strategy accounts for cached indexes
            if (historicalViewableDesc.IsHistorical) {
                for (var i = 0; i < historicalViewableDesc.Historical.Length; i++) {
                    if (historicalViewableDesc.Historical[i]) {
                        indexSpecs[i] = null;
                    }
                }
            }

            // Build graph of the outer join to inner table relationships.
            // Build a map of inner joins.
            OuterInnerDirectionalGraph outerInnerGraph;
            InnerJoinGraph innerJoinGraph;
            if (outerJoinDescList.Length > 0) {
                outerInnerGraph = GraphOuterJoins(numStreams, outerJoinDescList);
                innerJoinGraph = InnerJoinGraph.GraphInnerJoins(numStreams, outerJoinDescList);
            }
            else {
                // all inner joins - thereby no (or empty) directional graph
                outerInnerGraph = new OuterInnerDirectionalGraph(numStreams);
                innerJoinGraph = new InnerJoinGraph(numStreams, true);
            }

            if (Log.IsDebugEnabled) {
                Log.Debug(".build directional graph=" + outerInnerGraph.Print());
            }

            // For each stream determine the query plan
            for (var streamNo = 0; streamNo < numStreams; streamNo++) {
                // no plan for historical streams that are dependent upon other streams
                if (historicalViewableDesc.Historical[streamNo] && dependencyGraph.HasDependency(streamNo)) {
                    planNodeSpecs[streamNo] = new QueryPlanNodeNoOpForge();
                    continue;
                }

                var queryPlanNode = BuildPlanNode(
                    numStreams, streamNo, streamNames, queryGraph, outerInnerGraph, outerJoinDescList, innerJoinGraph, indexSpecs, typesPerStream,
                    historicalViewableDesc.Historical, dependencyGraph, historicalStreamIndexLists, tablesPerStream, streamJoinAnalysisResult,
                    statementRawInfo, services);

                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        ".build spec for stream '" + streamNames[streamNo] +
                        "' number " + streamNo + " is " + queryPlanNode);
                }

                planNodeSpecs[streamNo] = queryPlanNode;
            }

            var queryPlan = new QueryPlanForge(indexSpecs, planNodeSpecs);
            if (Log.IsDebugEnabled) {
                Log.Debug(".build query plan=" + queryPlan);
            }

            return queryPlan;
        }

        private static QueryPlanNodeForge BuildPlanNode(
            int numStreams,
            int streamNo,
            string[] streamNames,
            QueryGraphForge queryGraph,
            OuterInnerDirectionalGraph outerInnerGraph,
            OuterJoinDesc[] outerJoinDescList,
            InnerJoinGraph innerJoinGraph,
            QueryPlanIndexForge[] indexSpecs,
            EventType[] typesPerStream,
            bool[] isHistorical,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            TableMetaData[] tablesPerStream,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // For each stream build an array of substreams, considering required streams (inner joins) first
            // The order is relevant therefore preserving order via a LinkedHashMap.
            var substreamsPerStream = new LinkedHashMap<int, int[]>();
            var requiredPerStream = new bool[numStreams];

            // Recursive populating the required (outer) and optional (inner) relationships
            // of this stream and the substream
            ISet<int> completedStreams = new HashSet<int>();
            // keep track of tree path as only those stream events are always available to historical streams
            var streamCallStack = new Stack<int>();
            streamCallStack.Push(streamNo);

            // For all inner-joins, the algorithm is slightly different
            if (innerJoinGraph.IsAllInnerJoin) {
                requiredPerStream.Fill(true);
                RecursiveBuildInnerJoin(streamNo, streamCallStack, queryGraph, completedStreams, substreamsPerStream, dependencyGraph);

                // compute a best chain to see if all streams are handled and add the remaining
                var bestChain = NStreamQueryPlanBuilder.ComputeBestPath(streamNo, queryGraph, dependencyGraph);
                AddNotYetNavigated(streamNo, numStreams, substreamsPerStream, bestChain);
            }
            else {
                RecursiveBuild(
                    streamNo, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph, completedStreams, substreamsPerStream, requiredPerStream,
                    dependencyGraph);
            }

            // verify the substreamsPerStream, all streams must exists and be linked
            VerifyJoinedPerStream(streamNo, substreamsPerStream);

            // build list of instructions for lookup
            var lookupInstructions = BuildLookupInstructions(
                streamNo, substreamsPerStream, requiredPerStream,
                streamNames, queryGraph, indexSpecs, typesPerStream, outerJoinDescList, isHistorical, historicalStreamIndexLists, tablesPerStream,
                streamJoinAnalysisResult, statementRawInfo, services);

            // build historical index and lookup strategies
            foreach (var lookups in lookupInstructions) {
                foreach (var historical in lookups.HistoricalPlans) {
                    if (historical == null) {
                        continue;
                    }

                    var pair = historicalStreamIndexLists[historical.StreamNum].GetStrategy(historical.LookupStreamNum);
                    historical.HistoricalIndexLookupStrategy = pair.First;
                    historical.PollResultIndexingStrategy = pair.Second;
                }
            }

            // build strategy tree for putting the result back together
            var assemblyTopNodeFactory = AssemblyStrategyTreeBuilder.Build(streamNo, substreamsPerStream, requiredPerStream);
            var assemblyInstructionFactories = BaseAssemblyNodeFactory.GetDescendentNodesBottomUp(assemblyTopNodeFactory);

            return new LookupInstructionQueryPlanNodeForge(
                streamNo, streamNames[streamNo], numStreams, requiredPerStream,
                lookupInstructions, assemblyInstructionFactories);
        }

        private static void AddNotYetNavigated(
            int streamNo,
            int numStreams,
            IDictionary<int, int[]> substreamsPerStream,
            NStreamQueryPlanBuilder.BestChainResult bestChain)
        {
            // sum up all substreams (the query plan for each stream: nested iteration or cardinal)
            ISet<int> streams = new HashSet<int>();
            streams.Add(streamNo);
            RecursiveAdd(streamNo, streamNo, substreamsPerStream, streams, false);

            // we are done, all have navigated
            if (streams.Count == numStreams) {
                return;
            }

            var previous = streamNo;
            foreach (var stream in bestChain.Chain) {
                if (streams.Contains(stream)) {
                    previous = stream;
                    continue;
                }

                // add node as a nested join to the previous stream
                var substreams = substreamsPerStream.Get(previous);
                if (substreams == null) {
                    substreams = new int[0];
                }

                var added = CollectionUtil.AddValue(substreams, stream);
                substreamsPerStream.Put(previous, added);

                if (!substreamsPerStream.ContainsKey(stream)) {
                    substreamsPerStream.Put(stream, new int[0]);
                }

                previous = stream;
            }
        }

        private static IList<LookupInstructionPlanForge> BuildLookupInstructions(
            int rootStreamNum,
            IDictionary<int, int[]> substreamsPerStream,
            bool[] requiredPerStream,
            string[] streamNames,
            QueryGraphForge queryGraph,
            QueryPlanIndexForge[] indexSpecs,
            EventType[] typesPerStream,
            OuterJoinDesc[] outerJoinDescList,
            bool[] isHistorical,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            TableMetaData[] tablesPerStream,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IList<LookupInstructionPlanForge> result = new List<LookupInstructionPlanForge>();

            foreach (var fromStream in substreamsPerStream.Keys) {
                var substreams = substreamsPerStream.Get(fromStream);

                // for streams with no substreams we don't need to look up
                if (substreams.Length == 0) {
                    continue;
                }

                var plans = new TableLookupPlanForge[substreams.Length];
                var historicalPlans = new HistoricalDataPlanNodeForge[substreams.Length];

                for (var i = 0; i < substreams.Length; i++) {
                    var toStream = substreams[i];

                    if (isHistorical[toStream]) {
                        // There may not be an outer-join descriptor, use if provided to build the associated expression
                        ExprNode outerJoinExpr = null;
                        if (outerJoinDescList.Length > 0) {
                            OuterJoinDesc outerJoinDesc;
                            if (toStream == 0) {
                                outerJoinDesc = outerJoinDescList[0];
                            }
                            else {
                                outerJoinDesc = outerJoinDescList[toStream - 1];
                            }

                            outerJoinExpr = outerJoinDesc.MakeExprNode(statementRawInfo, services);
                        }

                        if (historicalStreamIndexLists[toStream] == null) {
                            historicalStreamIndexLists[toStream] = new HistoricalStreamIndexListForge(toStream, typesPerStream, queryGraph);
                        }

                        historicalStreamIndexLists[toStream].AddIndex(fromStream);
                        historicalPlans[i] = new HistoricalDataPlanNodeForge(
                            toStream, rootStreamNum, fromStream, typesPerStream.Length, outerJoinExpr == null ? null : outerJoinExpr.Forge);
                    }
                    else {
                        plans[i] = NStreamQueryPlanBuilder.CreateLookupPlan(
                            queryGraph, fromStream, toStream, streamJoinAnalysisResult.IsVirtualDW(toStream), indexSpecs[toStream], typesPerStream,
                            tablesPerStream[toStream]);
                    }
                }

                var fromStreamName = streamNames[fromStream];
                var instruction = new LookupInstructionPlanForge(fromStream, fromStreamName, substreams, plans, historicalPlans, requiredPerStream);
                result.Add(instruction);
            }

            return result;
        }

        /// <summary>
        ///     Recusivly builds a substream-per-stream ordered tree graph using the
        ///     join information supplied for outer joins and from the query graph (where clause).
        ///     <para />
        ///     Required streams are considered first and their lookup is placed first in the list
        ///     to gain performance.
        /// </summary>
        /// <param name="streamNum">is the root stream number that supplies the incoming event to build the tree for</param>
        /// <param name="queryGraph">contains where-clause stream relationship info</param>
        /// <param name="outerInnerGraph">contains the outer join stream relationship info</param>
        /// <param name="completedStreams">is a temporary holder for streams already considered</param>
        /// <param name="substreamsPerStream">is the ordered, tree-like structure to be filled</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional</param>
        /// <param name="streamCallStack">the query plan call stack of streams available via cursor</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <param name="innerJoinGraph">inner join graph</param>
        /// <throws>ExprValidationException if the query planning failed</throws>
        public static void RecursiveBuild(
            int streamNum,
            Stack<int> streamCallStack,
            QueryGraphForge queryGraph,
            OuterInnerDirectionalGraph outerInnerGraph,
            InnerJoinGraph innerJoinGraph,
            ISet<int> completedStreams,
            IDictionary<int, int[]> substreamsPerStream,
            bool[] requiredPerStream,
            DependencyGraph dependencyGraph
        )
        {
            // add this stream to the set of completed streams
            completedStreams.Add(streamNum);

            // check if the dependencies have been satisfied
            if (dependencyGraph.HasDependency(streamNum)) {
                var dependencies = dependencyGraph.GetDependenciesForStream(streamNum);
                foreach (var dependentStream in dependencies) {
                    if (!streamCallStack.Contains(dependentStream)) {
                        throw new ExprValidationException(
                            "Historical stream " + streamNum + " parameter dependency originating in stream " + dependentStream +
                            " cannot or may not be satisfied by the join");
                    }
                }
            }

            // Determine the streams we can navigate to from this stream
            var navigableStreams = queryGraph.GetNavigableStreams(streamNum);

            // add unqualified navigable streams (since on-expressions in outer joins are optional)
            var unqualifiedNavigable = outerInnerGraph.UnqualifiedNavigableStreams.Get(streamNum);
            if (unqualifiedNavigable != null) {
                navigableStreams.AddAll(unqualifiedNavigable);
            }

            // remove those already done
            navigableStreams.RemoveAll(completedStreams);

            // Which streams are inner streams to this stream (optional), which ones are outer to the stream (required)
            var requiredStreams = GetOuterStreams(streamNum, navigableStreams, outerInnerGraph);

            // Add inner joins, if any, unless already completed for this stream
            innerJoinGraph.AddRequiredStreams(streamNum, requiredStreams, completedStreams);

            var optionalStreams = GetInnerStreams(streamNum, navigableStreams, outerInnerGraph, innerJoinGraph, completedStreams);

            // Remove from the required streams the optional streams which places 'full' joined streams
            // into the optional stream category
            requiredStreams.RemoveAll(optionalStreams);

            // if we are a leaf node, we are done
            if (navigableStreams.IsEmpty()) {
                substreamsPerStream.Put(streamNum, new int[0]);
                return;
            }

            // First the outer (required) streams to this stream, then the inner (optional) streams
            var substreams = new int[requiredStreams.Count + optionalStreams.Count];
            substreamsPerStream.Put(streamNum, substreams);
            var count = 0;
            foreach (var stream in requiredStreams) {
                substreams[count++] = stream;
                requiredPerStream[stream] = true;
            }

            foreach (var stream in optionalStreams) {
                substreams[count++] = stream;
            }

            // next we look at all the required streams and add their dependent streams
            foreach (var stream in requiredStreams) {
                completedStreams.Add(stream);
            }

            foreach (var stream in requiredStreams) {
                streamCallStack.Push(stream);
                RecursiveBuild(
                    stream, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph,
                    completedStreams, substreamsPerStream, requiredPerStream, dependencyGraph);
                streamCallStack.Pop();
            }

            // look at all the optional streams and add their dependent streams
            foreach (var stream in optionalStreams) {
                streamCallStack.Push(stream);
                RecursiveBuild(
                    stream, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph,
                    completedStreams, substreamsPerStream, requiredPerStream, dependencyGraph);
                streamCallStack.Pop();
            }
        }

        /// <summary>
        ///     Recusivly builds a substream-per-stream ordered tree graph using the
        ///     join information supplied for outer joins and from the query graph (where clause).
        ///     <para />
        ///     Required streams are considered first and their lookup is placed first in the list
        ///     to gain performance.
        /// </summary>
        /// <param name="streamNum">is the root stream number that supplies the incoming event to build the tree for</param>
        /// <param name="queryGraph">contains where-clause stream relationship info</param>
        /// <param name="completedStreams">is a temporary holder for streams already considered</param>
        /// <param name="substreamsPerStream">is the ordered, tree-like structure to be filled</param>
        /// <param name="streamCallStack">the query plan call stack of streams available via cursor</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <throws>ExprValidationException if the query planning failed</throws>
        protected internal static void RecursiveBuildInnerJoin(
            int streamNum,
            Stack<int> streamCallStack,
            QueryGraphForge queryGraph,
            ISet<int> completedStreams,
            IDictionary<int, int[]> substreamsPerStream,
            DependencyGraph dependencyGraph)
        {
            // add this stream to the set of completed streams
            completedStreams.Add(streamNum);

            // check if the dependencies have been satisfied
            if (dependencyGraph.HasDependency(streamNum)) {
                var dependencies = dependencyGraph.GetDependenciesForStream(streamNum);
                foreach (var dependentStream in dependencies) {
                    if (!streamCallStack.Contains(dependentStream)) {
                        throw new ExprValidationException(
                            "Historical stream " + streamNum + " parameter dependency originating in stream " + dependentStream +
                            " cannot or may not be satisfied by the join");
                    }
                }
            }

            // Determine the streams we can navigate to from this stream
            var navigableStreams = queryGraph.GetNavigableStreams(streamNum);

            // remove streams with a dependency on other streams not yet processed
            var navigableStreamArr = navigableStreams.ToArray();
            foreach (int navigableStream in navigableStreamArr) {
                if (dependencyGraph.HasUnsatisfiedDependency(navigableStream, completedStreams)) {
                    navigableStreams.Remove(navigableStream);
                }
            }

            // remove those already done
            navigableStreams.RemoveAll(completedStreams);

            // if we are a leaf node, we are done
            if (navigableStreams.IsEmpty()) {
                substreamsPerStream.Put(streamNum, new int[0]);
                return;
            }

            // First the outer (required) streams to this stream, then the inner (optional) streams
            var substreams = new int[navigableStreams.Count];
            substreamsPerStream.Put(streamNum, substreams);
            var count = 0;
            foreach (var stream in navigableStreams) {
                substreams[count++] = stream;
                completedStreams.Add(stream);
            }

            foreach (var stream in navigableStreams) {
                streamCallStack.Push(stream);
                RecursiveBuildInnerJoin(stream, streamCallStack, queryGraph, completedStreams, substreamsPerStream, dependencyGraph);
                streamCallStack.Pop();
            }
        }

        private static ISet<int> GetInnerStreams(
            int fromStream,
            ISet<int> toStreams,
            OuterInnerDirectionalGraph outerInnerGraph,
            InnerJoinGraph innerJoinGraph,
            ISet<int> completedStreams)
        {
            ISet<int> innerStreams = new HashSet<int>();
            foreach (var toStream in toStreams) {
                if (outerInnerGraph.IsInner(fromStream, toStream)) {
                    // if the to-stream, recursively, has an inner join itself, it becomes a required stream and not optional
                    var hasInnerJoin = false;
                    if (!innerJoinGraph.IsEmpty()) {
                        var doNotUseStreams = new HashSet<int>(completedStreams);
                        completedStreams.Add(fromStream);
                        hasInnerJoin = RecursiveHasInnerJoin(toStream, outerInnerGraph, innerJoinGraph, doNotUseStreams);
                    }

                    if (!hasInnerJoin) {
                        innerStreams.Add(toStream);
                    }
                }
            }

            return innerStreams;
        }

        private static bool RecursiveHasInnerJoin(
            int toStream,
            OuterInnerDirectionalGraph outerInnerGraph,
            InnerJoinGraph innerJoinGraph,
            ISet<int> completedStreams)
        {
            // Check if the to-stream is in any of the inner joins
            var hasInnerJoin = innerJoinGraph.HasInnerJoin(toStream);

            if (hasInnerJoin) {
                return true;
            }

            var innerToToStream = outerInnerGraph.GetInner(toStream);
            if (innerToToStream != null) {
                foreach (var nextStream in innerToToStream) {
                    if (completedStreams.Contains(nextStream)) {
                        continue;
                    }

                    var notConsider = new HashSet<int>(completedStreams);
                    notConsider.Add(toStream);
                    var result = RecursiveHasInnerJoin(nextStream, outerInnerGraph, innerJoinGraph, notConsider);

                    if (result) {
                        return true;
                    }
                }
            }

            var outerToToStream = outerInnerGraph.GetOuter(toStream);
            if (outerToToStream != null) {
                foreach (var nextStream in outerToToStream) {
                    if (completedStreams.Contains(nextStream)) {
                        continue;
                    }

                    var notConsider = new HashSet<int>(completedStreams);
                    notConsider.Add(toStream);
                    var result = RecursiveHasInnerJoin(nextStream, outerInnerGraph, innerJoinGraph, notConsider);

                    if (result) {
                        return true;
                    }
                }
            }

            return false;
        }

        // which streams are to this table an outer stream
        private static ISet<int> GetOuterStreams(
            int fromStream,
            ISet<int> toStreams,
            OuterInnerDirectionalGraph outerInnerGraph)
        {
            ISet<int> outerStreams = new HashSet<int>();
            foreach (var toStream in toStreams) {
                if (outerInnerGraph.IsOuter(toStream, fromStream)) {
                    outerStreams.Add(toStream);
                }
            }

            return outerStreams;
        }

        /// <summary>
        ///     Builds a graph of outer joins given the outer join information from the statement.
        ///     Eliminates right and left joins and full joins by placing the information in a graph object.
        /// </summary>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="outerJoinDescList">list of outer join stream numbers and property names</param>
        /// <returns>graph object</returns>
        public static OuterInnerDirectionalGraph GraphOuterJoins(
            int numStreams,
            OuterJoinDesc[] outerJoinDescList)
        {
            if (outerJoinDescList.Length + 1 != numStreams) {
                throw new ArgumentException("Number of outer join descriptors and number of streams not matching up");
            }

            var graph = new OuterInnerDirectionalGraph(numStreams);

            for (var i = 0; i < outerJoinDescList.Length; i++) {
                var desc = outerJoinDescList[i];
                var streamMax = i + 1; // the outer join must references streams less then streamMax

                // Check outer join on-expression, if provided
                int streamOne;
                int streamTwo;
                int lowerStream;
                int higherStream;
                if (desc.OptLeftNode != null) {
                    streamOne = desc.OptLeftNode.StreamId;
                    streamTwo = desc.OptRightNode.StreamId;

                    if (streamOne > streamMax || streamTwo > streamMax ||
                        streamOne == streamTwo) {
                        throw new ArgumentException("Outer join descriptors reference future streams, or same streams");
                    }

                    // Determine who is the first stream in the streams listed
                    lowerStream = streamOne;
                    higherStream = streamTwo;
                    if (streamOne > streamTwo) {
                        lowerStream = streamTwo;
                        higherStream = streamOne;
                    }
                }
                else {
                    streamOne = i;
                    streamTwo = i + 1;
                    lowerStream = i;
                    higherStream = i + 1;

                    graph.AddUnqualifiedNavigable(streamOne, streamTwo);
                }

                // Add to graph
                if (desc.OuterJoinType == OuterJoinType.FULL) {
                    graph.Add(streamOne, streamTwo);
                    graph.Add(streamTwo, streamOne);
                }
                else if (desc.OuterJoinType == OuterJoinType.LEFT) {
                    graph.Add(lowerStream, higherStream);
                }
                else if (desc.OuterJoinType == OuterJoinType.RIGHT) {
                    graph.Add(higherStream, lowerStream);
                }
                else if (desc.OuterJoinType == OuterJoinType.INNER) {
                    // no navigability for inner joins
                }
                else {
                    throw new ArgumentException("Outer join descriptors join type not handled, type=" + desc.OuterJoinType);
                }
            }

            return graph;
        }

        /// <summary>
        ///     Verifies that the tree-like structure representing which streams join (lookup) into which sub-streams
        ///     is correct, ie. all streams are included and none are listed twice.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the incoming event</param>
        /// <param name="streamsJoinedPerStream">
        ///     is keyed by the from-stream number and contains as values allstream numbers of lookup into to-streams.
        /// </param>
        public static void VerifyJoinedPerStream(
            int rootStream,
            IDictionary<int, int[]> streamsJoinedPerStream)
        {
            ISet<int> streams = new HashSet<int>();
            streams.Add(rootStream);

            RecursiveAdd(rootStream, rootStream, streamsJoinedPerStream, streams, true);

            if (streams.Count != streamsJoinedPerStream.Count) {
                throw new ArgumentException(
                    "Not all streams found, streamsJoinedPerStream=" +
                    Print(streamsJoinedPerStream));
            }
        }

        private static void RecursiveAdd(
            int validatedStream,
            int currentStream,
            IDictionary<int, int[]> streamsJoinedPerStream,
            ISet<int> streams,
            bool verify)
        {
            if (currentStream >= streamsJoinedPerStream.Count && verify) {
                throw new ArgumentException(
                    "Error in stream " + currentStream + " streamsJoinedPerStream=" +
                    Print(streamsJoinedPerStream));
            }

            var joinedStreams = streamsJoinedPerStream.Get(currentStream);
            for (var i = 0; i < joinedStreams.Length; i++) {
                var addStream = joinedStreams[i];
                if (streams.Contains(addStream)) {
                    throw new ArgumentException("Stream " + addStream + " found twice when validating " + validatedStream);
                }

                streams.Add(addStream);
                RecursiveAdd(validatedStream, addStream, streamsJoinedPerStream, streams, verify);
            }
        }

        /// <summary>
        ///     Returns textual presentation of stream-substream relationships.
        /// </summary>
        /// <param name="streamsJoinedPerStream">is the tree-like structure of stream-substream</param>
        /// <returns>textual presentation</returns>
        public static string Print(IDictionary<int, int[]> streamsJoinedPerStream)
        {
            var writer = new StringWriter();

            foreach (var stream in streamsJoinedPerStream.Keys) {
                var substreams = streamsJoinedPerStream.Get(stream);
                writer.WriteLine("stream " + stream + " : " + substreams.RenderAny());
            }

            return writer.ToString();
        }
    }
} // end of namespace