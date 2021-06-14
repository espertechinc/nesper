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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.rep
{
    /// <summary>
    ///     Implements a repository for join events and lookup results.
    /// </summary>
    public class RepositoryImpl : Repository
    {
        private readonly int numStreams;
        private readonly EventBean rootEvent;
        private readonly int rootStream;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the root event</param>
        /// <param name="rootEvent">is the root event</param>
        /// <param name="numStreams">is the number of streams</param>
        public RepositoryImpl(
            int rootStream,
            EventBean rootEvent,
            int numStreams)
        {
            this.rootStream = rootStream;
            this.rootEvent = rootEvent;
            this.numStreams = numStreams;
        }

        /// <summary>
        ///     Returns a list of nodes that are the lookup results per stream.
        /// </summary>
        /// <returns>result nodes per stream</returns>
        public IList<Node>[] NodesPerStream { get; private set; }

        public IEnumerator<Cursor> GetCursors(int lookupFromStream)
        {
            if (lookupFromStream == rootStream) {
                var cursor = new Cursor(rootEvent, rootStream, null);
                return EnumerationHelper.Singleton(cursor);
            }

            var nodeList = NodesPerStream[lookupFromStream];
            if (nodeList == null) {
                return EnumerationHelper.Empty<Cursor>();
            }

            return GetCursorEnumerator(lookupFromStream, nodeList);
        }

        private IEnumerator<Cursor> GetCursorEnumerator(
            int lookupFromStream,
            IList<Node> nodeList)
        {
            foreach (var currentNode in nodeList) {
                if (currentNode.Events != null) {
                    foreach (var theEvent in currentNode.Events) {
                        yield return new Cursor(theEvent, lookupFromStream, currentNode);
                    }
                }
            }
        }

        public void AddResult(
            Cursor cursor,
            ICollection<EventBean> lookupResults,
            int resultStream)
        {
            if (lookupResults.IsEmpty()) {
                throw new ArgumentException("Attempting to add zero results");
            }

            var parentNode = cursor.Node;
            if (parentNode == null) {
                var leafNodeX = new Node(resultStream);
                leafNodeX.Events = lookupResults;

                if (NodesPerStream == null) {
                    NodesPerStream = new IList<Node>[numStreams];
                }

                var nodesX = NodesPerStream[resultStream];
                if (nodesX == null) {
                    nodesX = new List<Node>();
                    NodesPerStream[resultStream] = nodesX;
                }

                leafNodeX.ParentEvent = rootEvent;

                nodesX.Add(leafNodeX);
                return;
            }

            var leafNode = new Node(resultStream);
            leafNode.Events = lookupResults;
            leafNode.Parent = cursor.Node;
            leafNode.ParentEvent = cursor.TheEvent;

            var nodes = NodesPerStream[resultStream];
            if (nodes == null) {
                nodes = new List<Node>();
                NodesPerStream[resultStream] = nodes;
            }

            nodes.Add(leafNode);
        }
    }
} // end of namespace