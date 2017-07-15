///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.assemble;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>Builds a query plan for 3 or more streams in a outer join.</summary>
    public class NStreamOuterQueryPlanBuilder {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Build a query plan based on the stream property relationships indicated in queryGraph.
        /// </summary>
        /// <param name="queryGraph">- navigation INFO between streams</param>
        /// <param name="streamNames">- stream names</param>
        /// <param name="outerJoinDescList">- descriptors for all outer joins</param>
        /// <param name="typesPerStream">- event types for each stream</param>
        /// <param name="dependencyGraph">- dependencies between historical streams</param>
        /// <param name="historicalStreamIndexLists">- index management, populated for the query plan</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <param name="historicalViewableDesc">historicals</param>
        /// <param name="indexedStreamsUniqueProps">unique props</param>
        /// <param name="tablesPerStream">tables</param>
        /// <exception cref="ExprValidationException">if the query planning failed</exception>
        /// <returns>query plan</returns>
        internal static QueryPlan Build(QueryGraph queryGraph,
                                         OuterJoinDesc[] outerJoinDescList,
                                         string[] streamNames,
                                         EventType[] typesPerStream,
                                         HistoricalViewableDesc historicalViewableDesc,
                                         DependencyGraph dependencyGraph,
                                         HistoricalStreamIndexList[] historicalStreamIndexLists,
                                         ExprEvaluatorContext exprEvaluatorContext,
                                         string[][][] indexedStreamsUniqueProps,
                                         TableMetadata[] tablesPerStream)
                {
            if (Log.IsDebugEnabled) {
                Log.Debug(".build queryGraph=" + queryGraph);
            }
    
            int numStreams = queryGraph.NumStreams;
            var planNodeSpecs = new QueryPlanNode[numStreams];
    
            // Build index specifications
            QueryPlanIndex[] indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, typesPerStream, indexedStreamsUniqueProps);
            if (Log.IsDebugEnabled) {
                Log.Debug(".build Index build completed, indexes=" + QueryPlanIndex.Print(indexSpecs));
            }
    
            // any historical streams don't get indexes, the lookup strategy accounts for cached indexes
            if (historicalViewableDesc.IsHasHistorical) {
                for (int i = 0; i < historicalViewableDesc.Historical.Length; i++) {
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
            } else {
                // all inner joins - thereby no (or empty) directional graph
                outerInnerGraph = new OuterInnerDirectionalGraph(numStreams);
                innerJoinGraph = new InnerJoinGraph(numStreams, true);
            }
            if (Log.IsDebugEnabled) {
                Log.Debug(".build directional graph=" + outerInnerGraph.Print());
            }
    
            // For each stream determine the query plan
            for (int streamNo = 0; streamNo < numStreams; streamNo++) {
                // no plan for historical streams that are dependent upon other streams
                if ((historicalViewableDesc.Historical[streamNo]) && (dependencyGraph.HasDependency(streamNo))) {
                    planNodeSpecs[streamNo] = new QueryPlanNodeNoOp();
                    continue;
                }
    
                QueryPlanNode queryPlanNode = BuildPlanNode(numStreams, streamNo, streamNames, queryGraph, outerInnerGraph, outerJoinDescList, innerJoinGraph, indexSpecs, typesPerStream, historicalViewableDesc.Historical, dependencyGraph, historicalStreamIndexLists, exprEvaluatorContext, tablesPerStream);
    
                if (Log.IsDebugEnabled) {
                    Log.Debug(".build spec for stream '" + streamNames[streamNo] +
                            "' number " + streamNo + " is " + queryPlanNode);
                }
    
                planNodeSpecs[streamNo] = queryPlanNode;
            }
    
            var queryPlan = new QueryPlan(indexSpecs, planNodeSpecs);
            if (Log.IsDebugEnabled) {
                Log.Debug(".build query plan=" + queryPlan.ToString());
            }
    
            return queryPlan;
        }
    
        private static QueryPlanNode BuildPlanNode(int numStreams,
                                                   int streamNo,
                                                   string[] streamNames,
                                                   QueryGraph queryGraph,
                                                   OuterInnerDirectionalGraph outerInnerGraph,
                                                   OuterJoinDesc[] outerJoinDescList,
                                                   InnerJoinGraph innerJoinGraph,
                                                   QueryPlanIndex[] indexSpecs,
                                                   EventType[] typesPerStream,
                                                   bool[] ishistorical,
                                                   DependencyGraph dependencyGraph,
                                                   HistoricalStreamIndexList[] historicalStreamIndexLists,
                                                   ExprEvaluatorContext exprEvaluatorContext,
                                                   TableMetadata[] tablesPerStream)
                {
            // For each stream build an array of substreams, considering required streams (inner joins) first
            // The order is relevant therefore preserving order via a LinkedHashMap.
            var substreamsPerStream = new LinkedHashMap<int?, int[]>();
            var requiredPerStream = new bool[numStreams];
    
            // Recursive populating the required (outer) and optional (inner) relationships
            // of this stream and the substream
            var completedStreams = new HashSet<int?>();
            // keep track of tree path as only those stream events are always available to historical streams
            var streamCallStack = new Stack<int?>();
            streamCallStack.Push(streamNo);
    
            // For all inner-joins, the algorithm is slightly different
            if (innerJoinGraph.IsAllInnerJoin) {
                Arrays.Fill(requiredPerStream, true);
                RecursiveBuildInnerJoin(streamNo, streamCallStack, queryGraph, completedStreams, substreamsPerStream, dependencyGraph);
    
                // compute a best chain to see if all streams are handled and add the remaining
                NStreamQueryPlanBuilder.BestChainResult bestChain = NStreamQueryPlanBuilder.ComputeBestPath(streamNo, queryGraph, dependencyGraph);
                AddNotYetNavigated(streamNo, numStreams, substreamsPerStream, bestChain);
            } else {
                RecursiveBuild(streamNo, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph, completedStreams, substreamsPerStream, requiredPerStream, dependencyGraph);
            }
    
            // verify the substreamsPerStream, all streams must exists and be linked
            VerifyJoinedPerStream(streamNo, substreamsPerStream);
    
            // build list of instructions for lookup
            List<LookupInstructionPlan> lookupInstructions = BuildLookupInstructions(streamNo, substreamsPerStream, requiredPerStream,
                    streamNames, queryGraph, indexSpecs, typesPerStream, outerJoinDescList, ishistorical, historicalStreamIndexLists, exprEvaluatorContext, tablesPerStream);
    
            // build strategy tree for putting the result back together
            BaseAssemblyNodeFactory assemblyTopNodeFactory = AssemblyStrategyTreeBuilder.Build(streamNo, substreamsPerStream, requiredPerStream);
            List<BaseAssemblyNodeFactory> assemblyInstructionFactories = BaseAssemblyNodeFactory.GetDescendentNodesBottomUp(assemblyTopNodeFactory);
    
            return new LookupInstructionQueryPlanNode(streamNo, streamNames[streamNo], numStreams, requiredPerStream,
                    lookupInstructions, assemblyInstructionFactories);
        }
    
        private static void AddNotYetNavigated(int streamNo, int numStreams, LinkedHashMap<int?, int[]> substreamsPerStream, NStreamQueryPlanBuilder.BestChainResult bestChain) {
            // sum up all substreams (the query plan for each stream: nested iteration or cardinal)
            var streams = new HashSet<int?>();
            streams.Add(streamNo);
            RecursiveAdd(streamNo, streamNo, substreamsPerStream, streams, false);
    
            // we are done, all have navigated
            if (streams.Count == numStreams) {
                return;
            }
    
            int previous = streamNo;
            foreach (int stream in bestChain.Chain) {
    
                if (streams.Contains(stream)) {
                    previous = stream;
                    continue;
                }
    
                // add node as a nested join to the previous stream
                int[] substreams = substreamsPerStream.Get(previous);
                if (substreams == null) {
                    substreams = new int[0];
                }
                int[] added = CollectionUtil.AddValue(substreams, stream);
                substreamsPerStream.Put(previous, added);
    
                if (!substreamsPerStream.ContainsKey(stream)) {
                    substreamsPerStream.Put(stream, new int[0]);
                }
    
                previous = stream;
            }
        }
    
        private static List<LookupInstructionPlan> BuildLookupInstructions(
                int rootStreamNum,
                LinkedHashMap<int?, int[]> substreamsPerStream,
                bool[] requiredPerStream,
                string[] streamNames,
                QueryGraph queryGraph,
                QueryPlanIndex[] indexSpecs,
                EventType[] typesPerStream,
                OuterJoinDesc[] outerJoinDescList,
                bool[] isHistorical,
                HistoricalStreamIndexList[] historicalStreamIndexLists,
                ExprEvaluatorContext exprEvaluatorContext,
                TableMetadata[] tablesPerStream) {
            var result = new LinkedList<LookupInstructionPlan>();
    
            foreach (int fromStream in substreamsPerStream.KeySet()) {
                int[] substreams = substreamsPerStream.Get(fromStream);
    
                // for streams with no substreams we don't need to look up
                if (substreams.Length == 0) {
                    continue;
                }
    
                var plans = new TableLookupPlan[substreams.Length];
                var historicalPlans = new HistoricalDataPlanNode[substreams.Length];
    
                for (int i = 0; i < substreams.Length; i++) {
                    int toStream = substreams[i];
    
                    if (isHistorical[toStream]) {
                        // There may not be an outer-join descriptor, use if provided to build the associated expression
                        ExprNode outerJoinExpr = null;
                        if (outerJoinDescList.Length > 0) {
                            OuterJoinDesc outerJoinDesc;
                            if (toStream == 0) {
                                outerJoinDesc = outerJoinDescList[0];
                            } else {
                                outerJoinDesc = outerJoinDescList[toStream - 1];
                            }
                            outerJoinExpr = outerJoinDesc.MakeExprNode(exprEvaluatorContext);
                        }
    
                        if (historicalStreamIndexLists[toStream] == null) {
                            historicalStreamIndexLists[toStream] = new HistoricalStreamIndexList(toStream, typesPerStream, queryGraph);
                        }
                        historicalStreamIndexLists[toStream].AddIndex(fromStream);
                        historicalPlans[i] = new HistoricalDataPlanNode(toStream, rootStreamNum, fromStream, typesPerStream.Length, outerJoinExpr);
                    } else {
                        plans[i] = NStreamQueryPlanBuilder.CreateLookupPlan(queryGraph, fromStream, toStream, indexSpecs[toStream], typesPerStream, tablesPerStream[toStream]);
                    }
                }
    
                string fromStreamName = streamNames[fromStream];
                var instruction = new LookupInstructionPlan(fromStream, fromStreamName, substreams, plans, historicalPlans, requiredPerStream);
                result.Add(instruction);
            }
    
            return result;
        }
    
        /// <summary>
        /// Recusivly builds a substream-per-stream ordered tree graph using the
        /// join information supplied for outer joins and from the query graph (where clause).
        /// <para>
        /// Required streams are considered first and their lookup is placed first in the list
        /// to gain performance.
        /// </para>
        /// </summary>
        /// <param name="streamNum">is the root stream number that supplies the incoming event to build the tree for</param>
        /// <param name="queryGraph">contains where-clause stream relationship INFO</param>
        /// <param name="outerInnerGraph">contains the outer join stream relationship INFO</param>
        /// <param name="completedStreams">is a temporary holder for streams already considered</param>
        /// <param name="substreamsPerStream">is the ordered, tree-like structure to be filled</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional</param>
        /// <param name="streamCallStack">the query plan call stack of streams available via cursor</param>
        /// <param name="dependencyGraph">- dependencies between historical streams</param>
        /// <param name="innerJoinGraph">inner join graph</param>
        /// <exception cref="ExprValidationException">if the query planning failed</exception>
        internal static void RecursiveBuild(int streamNum,
                                             Stack<int?> streamCallStack,
                                             QueryGraph queryGraph,
                                             OuterInnerDirectionalGraph outerInnerGraph,
                                             InnerJoinGraph innerJoinGraph,
                                             ISet<int?> completedStreams,
                                             LinkedHashMap<int?, int[]> substreamsPerStream,
                                             bool[] requiredPerStream,
                                             DependencyGraph dependencyGraph
        )
                {
            // add this stream to the set of completed streams
            completedStreams.Add(streamNum);
    
            // check if the dependencies have been satisfied
            if (dependencyGraph.HasDependency(streamNum)) {
                ISet<int?> dependencies = dependencyGraph.GetDependenciesForStream(streamNum);
                for (int? dependentStream : dependencies) {
                    if (!streamCallStack.Contains(dependentStream)) {
                        throw new ExprValidationException("Historical stream " + streamNum + " parameter dependency originating in stream " + dependentStream + " cannot or may not be satisfied by the join");
                    }
                }
            }
    
            // Determine the streams we can navigate to from this stream
            ISet<int?> navigableStreams = queryGraph.GetNavigableStreams(streamNum);
    
            // add unqualified navigable streams (since on-expressions in outer joins are optional)
            ISet<int?> unqualifiedNavigable = outerInnerGraph.UnqualifiedNavigableStreams.Get(streamNum);
            if (unqualifiedNavigable != null) {
                navigableStreams.AddAll(unqualifiedNavigable);
            }
    
            // remove those already done
            navigableStreams.RemoveAll(completedStreams);
    
            // Which streams are inner streams to this stream (optional), which ones are outer to the stream (required)
            ISet<int?> requiredStreams = GetOuterStreams(streamNum, navigableStreams, outerInnerGraph);
    
            // Add inner joins, if any, unless already completed for this stream
            innerJoinGraph.AddRequiredStreams(streamNum, requiredStreams, completedStreams);
    
            ISet<int?> optionalStreams = GetInnerStreams(streamNum, navigableStreams, outerInnerGraph, innerJoinGraph, completedStreams);
    
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
            int count = 0;
            foreach (int stream in requiredStreams) {
                substreams[count++] = stream;
                requiredPerStream[stream] = true;
            }
            foreach (int stream in optionalStreams) {
                substreams[count++] = stream;
            }
    
            // next we look at all the required streams and add their dependent streams
            foreach (int stream in requiredStreams) {
                completedStreams.Add(stream);
            }
    
            foreach (int stream in requiredStreams) {
                streamCallStack.Push(stream);
                RecursiveBuild(stream, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph,
                        completedStreams, substreamsPerStream, requiredPerStream, dependencyGraph);
                streamCallStack.Pop();
            }
            // look at all the optional streams and add their dependent streams
            foreach (int stream in optionalStreams) {
                streamCallStack.Push(stream);
                RecursiveBuild(stream, streamCallStack, queryGraph, outerInnerGraph, innerJoinGraph,
                        completedStreams, substreamsPerStream, requiredPerStream, dependencyGraph);
                streamCallStack.Pop();
            }
        }
    
        /// <summary>
        /// Recusivly builds a substream-per-stream ordered tree graph using the
        /// join information supplied for outer joins and from the query graph (where clause).
        /// <para>
        /// Required streams are considered first and their lookup is placed first in the list
        /// to gain performance.
        /// </para>
        /// </summary>
        /// <param name="streamNum">is the root stream number that supplies the incoming event to build the tree for</param>
        /// <param name="queryGraph">contains where-clause stream relationship INFO</param>
        /// <param name="completedStreams">is a temporary holder for streams already considered</param>
        /// <param name="substreamsPerStream">is the ordered, tree-like structure to be filled</param>
        /// <param name="streamCallStack">the query plan call stack of streams available via cursor</param>
        /// <param name="dependencyGraph">- dependencies between historical streams</param>
        /// <exception cref="ExprValidationException">if the query planning failed</exception>
        internal static void RecursiveBuildInnerJoin(int streamNum,
                                                      Stack<int?> streamCallStack,
                                                      QueryGraph queryGraph,
                                                      ISet<int?> completedStreams,
                                                      LinkedHashMap<int?, int[]> substreamsPerStream,
                                                      DependencyGraph dependencyGraph)
                {
            // add this stream to the set of completed streams
            completedStreams.Add(streamNum);
    
            // check if the dependencies have been satisfied
            if (dependencyGraph.HasDependency(streamNum)) {
                ISet<int?> dependencies = dependencyGraph.GetDependenciesForStream(streamNum);
                for (int? dependentStream : dependencies) {
                    if (!streamCallStack.Contains(dependentStream)) {
                        throw new ExprValidationException("Historical stream " + streamNum + " parameter dependency originating in stream " + dependentStream + " cannot or may not be satisfied by the join");
                    }
                }
            }
    
            // Determine the streams we can navigate to from this stream
            ISet<int?> navigableStreams = queryGraph.GetNavigableStreams(streamNum);
    
            // remove streams with a dependency on other streams not yet processed
            int?[] navigableStreamArr = navigableStreams.ToArray(new int?[navigableStreams.Count]);
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
            int count = 0;
            foreach (int stream in navigableStreams) {
                substreams[count++] = stream;
                completedStreams.Add(stream);
            }
    
            foreach (int stream in navigableStreams) {
                streamCallStack.Push(stream);
                RecursiveBuildInnerJoin(stream, streamCallStack, queryGraph, completedStreams, substreamsPerStream, dependencyGraph);
                streamCallStack.Pop();
            }
        }
    
        private static ISet<int?> GetInnerStreams(int fromStream, ISet<int?> toStreams, OuterInnerDirectionalGraph outerInnerGraph,
                                                    InnerJoinGraph innerJoinGraph,
                                                    ISet<int?> completedStreams) {
            var innerStreams = new HashSet<int?>();
            foreach (int toStream in toStreams) {
                if (outerInnerGraph.IsInner(fromStream, toStream)) {
                    // if the to-stream, recursively, has an inner join itself, it becomes a required stream and not optional
                    bool hasInnerJoin = false;
                    if (!innerJoinGraph.IsEmpty()) {
                        var doNotUseStreams = new HashSet<int?>(completedStreams);
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
    
        private static bool RecursiveHasInnerJoin(int toStream, OuterInnerDirectionalGraph outerInnerGraph, InnerJoinGraph innerJoinGraph, ISet<int?> completedStreams) {
            // Check if the to-stream is in any of the inner joins
            bool hasInnerJoin = innerJoinGraph.HasInnerJoin(toStream);
    
            if (hasInnerJoin) {
                return true;
            }
    
            ISet<int?> innerToToStream = outerInnerGraph.GetInner(toStream);
            if (innerToToStream != null) {
                foreach (int nextStream in innerToToStream) {
                    if (completedStreams.Contains(nextStream)) {
                        continue;
                    }
    
                    var notConsider = new HashSet<int?>(completedStreams);
                    notConsider.Add(toStream);
                    bool result = RecursiveHasInnerJoin(nextStream, outerInnerGraph, innerJoinGraph, notConsider);
    
                    if (result) {
                        return true;
                    }
                }
            }
    
            ISet<int?> outerToToStream = outerInnerGraph.GetOuter(toStream);
            if (outerToToStream != null) {
                foreach (int nextStream in outerToToStream) {
                    if (completedStreams.Contains(nextStream)) {
                        continue;
                    }
    
                    var notConsider = new HashSet<int?>(completedStreams);
                    notConsider.Add(toStream);
                    bool result = RecursiveHasInnerJoin(nextStream, outerInnerGraph, innerJoinGraph, notConsider);
    
                    if (result) {
                        return true;
                    }
                }
            }
    
            return false;
        }
    
        // which streams are to this table an outer stream
        private static ISet<int?> GetOuterStreams(int fromStream, ISet<int?> toStreams, OuterInnerDirectionalGraph outerInnerGraph) {
            var outerStreams = new HashSet<int?>();
            foreach (int toStream in toStreams) {
                if (outerInnerGraph.IsOuter(toStream, fromStream)) {
                    outerStreams.Add(toStream);
                }
            }
            return outerStreams;
        }
    
        /// <summary>
        /// Builds a graph of outer joins given the outer join information from the statement.
        /// Eliminates right and left joins and full joins by placing the information in a graph object.
        /// </summary>
        /// <param name="numStreams">- is the number of streams</param>
        /// <param name="outerJoinDescList">- list of outer join stream numbers and property names</param>
        /// <returns>graph object</returns>
        internal static OuterInnerDirectionalGraph GraphOuterJoins(int numStreams, OuterJoinDesc[] outerJoinDescList) {
            if ((outerJoinDescList.Length + 1) != numStreams) {
                throw new ArgumentException("Number of outer join descriptors and number of streams not matching up");
            }
    
            var graph = new OuterInnerDirectionalGraph(numStreams);
    
            for (int i = 0; i < outerJoinDescList.Length; i++) {
                OuterJoinDesc desc = outerJoinDescList[i];
                int streamMax = i + 1;       // the outer join must references streams less then streamMax
    
                // Check outer join on-expression, if provided
                int streamOne;
                int streamTwo;
                int lowerStream;
                int higherStream;
                if (desc.OptLeftNode != null) {
                    streamOne = desc.OptLeftNode.StreamId;
                    streamTwo = desc.OptRightNode.StreamId;
    
                    if ((streamOne > streamMax) || (streamTwo > streamMax) ||
                            (streamOne == streamTwo)) {
                        throw new ArgumentException("Outer join descriptors reference future streams, or same streams");
                    }
    
                    // Determine who is the first stream in the streams listed
                    lowerStream = streamOne;
                    higherStream = streamTwo;
                    if (streamOne > streamTwo) {
                        lowerStream = streamTwo;
                        higherStream = streamOne;
                    }
                } else {
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
                } else if (desc.OuterJoinType == OuterJoinType.LEFT) {
                    graph.Add(lowerStream, higherStream);
                } else if (desc.OuterJoinType == OuterJoinType.RIGHT) {
                    graph.Add(higherStream, lowerStream);
                } else if (desc.OuterJoinType == OuterJoinType.INNER) {
                    // no navigability for inner joins
                } else {
                    throw new ArgumentException("Outer join descriptors join type not handled, type=" + desc.OuterJoinType);
                }
            }
    
            return graph;
        }
    
        /// <summary>
        /// Verifies that the tree-like structure representing which streams join (lookup) into which sub-streams
        /// is correct, ie. all streams are included and none are listed twice.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the incoming event</param>
        /// <param name="streamsJoinedPerStream">
        /// is keyed by the from-stream number and contains as values all
        /// stream numbers of lookup into to-streams.
        /// </param>
        public static void VerifyJoinedPerStream(int rootStream, IDictionary<int?, int[]> streamsJoinedPerStream) {
            var streams = new HashSet<int?>();
            streams.Add(rootStream);
    
            RecursiveAdd(rootStream, rootStream, streamsJoinedPerStream, streams, true);
    
            if (streams.Count != streamsJoinedPerStream.Count) {
                throw new ArgumentException("Not all streams found, streamsJoinedPerStream=" +
                        Print(streamsJoinedPerStream));
            }
        }
    
        private static void RecursiveAdd(int validatedStream, int currentStream, IDictionary<int?, int[]> streamsJoinedPerStream, ISet<int?> streams, bool verify) {
            if (currentStream >= streamsJoinedPerStream.Count && verify) {
                throw new ArgumentException("Error in stream " + currentStream + " streamsJoinedPerStream=" +
                        Print(streamsJoinedPerStream));
            }
            int[] joinedStreams = streamsJoinedPerStream.Get(currentStream);
            for (int i = 0; i < joinedStreams.Length; i++) {
                int addStream = joinedStreams[i];
                if (streams.Contains(addStream)) {
                    throw new ArgumentException("Stream " + addStream + " found twice when validating " + validatedStream);
                }
                streams.Add(addStream);
                RecursiveAdd(validatedStream, addStream, streamsJoinedPerStream, streams, verify);
            }
        }
    
        /// <summary>
        /// Returns textual presentation of stream-substream relationships.
        /// </summary>
        /// <param name="streamsJoinedPerStream">is the tree-like structure of stream-substream</param>
        /// <returns>textual presentation</returns>
        public static string Print(IDictionary<int?, int[]> streamsJoinedPerStream) {
            var buf = new StringWriter();
            var printer = new PrintWriter(buf);
    
            foreach (int stream in streamsJoinedPerStream.KeySet()) {
                int[] substreams = streamsJoinedPerStream.Get(stream);
                printer.Println("stream " + stream + " : " + Arrays.ToString(substreams));
            }
    
            return Buf.ToString();
        }
    }
} // end of namespace
