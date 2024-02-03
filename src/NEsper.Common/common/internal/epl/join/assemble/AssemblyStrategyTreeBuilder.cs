///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    /// Builds a tree of assembly nodes given a strategy for how to join streams.
    /// </summary>
    public class AssemblyStrategyTreeBuilder
    {
        /// <summary>
        /// Builds a tree of <seealso cref="BaseAssemblyNode" /> from join strategy information.
        /// </summary>
        /// <param name="rootStream">the root stream supplying the event to evaluate</param>
        /// <param name="streamsJoinedPerStream">a map in which the key is the stream number to supply an event,and the value is an array of streams to find events in for the given event
        /// </param>
        /// <param name="isRequiredPerStream">indicates which streams are required join streams versus optional streams</param>
        /// <returns>root assembly node</returns>
        public static BaseAssemblyNodeFactory Build(
            int rootStream,
            IDictionary<int, int[]> streamsJoinedPerStream,
            bool[] isRequiredPerStream)
        {
            if (streamsJoinedPerStream.Count < 3) {
                throw new ArgumentException("Not a 3-way join");
            }

            if (rootStream < 0 || rootStream >= streamsJoinedPerStream.Count) {
                throw new ArgumentException("Invalid root stream");
            }

            if (isRequiredPerStream.Length != streamsJoinedPerStream.Count) {
                throw new ArgumentException("Arrays not matching up");
            }

            NStreamOuterQueryPlanBuilder.VerifyJoinedPerStream(rootStream, streamsJoinedPerStream);

            if (Log.IsDebugEnabled) {
                Log.Debug(
                    ".build Building node for root stream " +
                    rootStream +
                    " streamsJoinedPerStream=" +
                    NStreamOuterQueryPlanBuilder.Print(streamsJoinedPerStream) +
                    " isRequiredPerStream=" +
                    isRequiredPerStream.Render());
            }

            var topNode = CreateNode(
                true,
                rootStream,
                streamsJoinedPerStream.Count,
                streamsJoinedPerStream.Get(rootStream),
                isRequiredPerStream);

            RecursiveBuild(rootStream, topNode, streamsJoinedPerStream, isRequiredPerStream);

            if (Log.IsDebugEnabled) {
                var buf = new StringWriter();
                var indentWriter = new IndentWriter(buf, 0, 2);
                topNode.PrintDescendends(indentWriter);

                Log.Debug(".build Dumping root node for stream " + rootStream + ": \n" + buf.ToString());
            }

            return topNode;
        }

        private static void RecursiveBuild(
            int parentStreamNum,
            BaseAssemblyNodeFactory parentNode,
            IDictionary<int, int[]> streamsJoinedPerStream,
            bool[] isRequiredPerStream)
        {
            var numStreams = streamsJoinedPerStream.Count;

            for (var i = 0; i < streamsJoinedPerStream.Get(parentStreamNum).Length; i++) {
                var streamJoined = streamsJoinedPerStream.Get(parentStreamNum)[i];
                var childNode = CreateNode(
                    false,
                    streamJoined,
                    numStreams,
                    streamsJoinedPerStream.Get(streamJoined),
                    isRequiredPerStream);
                parentNode.AddChild(childNode);

                if (streamsJoinedPerStream.Get(streamJoined).Length > 0) {
                    RecursiveBuild(streamJoined, childNode, streamsJoinedPerStream, isRequiredPerStream);
                }
            }
        }

        private static BaseAssemblyNodeFactory CreateNode(
            bool isRoot,
            int streamNum,
            int numStreams,
            int[] joinedStreams,
            bool[] isRequiredPerStream)
        {
            if (joinedStreams.Length == 0) {
                return new LeafAssemblyNodeFactory(streamNum, numStreams);
            }

            if (joinedStreams.Length == 1) {
                var joinedStream = joinedStreams[0];
                var isRequired = isRequiredPerStream[joinedStream];
                if (isRequired) {
                    if (isRoot) {
                        return new RootRequiredAssemblyNodeFactory(streamNum, numStreams);
                    }
                    else {
                        return new BranchRequiredAssemblyNodeFactory(streamNum, numStreams);
                    }
                }
                else {
                    if (isRoot) {
                        return new RootOptionalAssemblyNodeFactory(streamNum, numStreams);
                    }
                    else {
                        return new BranchOptionalAssemblyNodeFactory(streamNum, numStreams);
                    }
                }
            }

            // Determine if all substream are outer (optional) joins
            var allSubStreamsOptional = true;
            for (var i = 0; i < joinedStreams.Length; i++) {
                var stream = joinedStreams[i];
                if (isRequiredPerStream[stream]) {
                    allSubStreamsOptional = false;
                }
            }

            // Make node for building a cartesian product
            if (isRoot) {
                return new RootCartProdAssemblyNodeFactory(streamNum, numStreams, allSubStreamsOptional);
            }
            else {
                return new CartesianProdAssemblyNodeFactory(streamNum, numStreams, allSubStreamsOptional);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace