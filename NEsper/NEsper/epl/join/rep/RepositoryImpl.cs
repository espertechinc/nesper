///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.rep
{
    /// <summary>
    /// Implements a repository for join events and lookup results.
    /// </summary>
    public class RepositoryImpl : Repository
    {
        private readonly int _numStreams;
        private readonly EventBean _rootEvent;
        private readonly int _rootStream;

        private IList<Node>[] _nodesPerStream;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the root event</param>
        /// <param name="rootEvent">is the root event</param>
        /// <param name="numStreams">is the number of streams</param>
        public RepositoryImpl(int rootStream,
                              EventBean rootEvent,
                              int numStreams)
        {
            _rootStream = rootStream;
            _rootEvent = rootEvent;
            _numStreams = numStreams;
        }

        /// <summary>
        /// Returns a list of nodes that are the lookup results per stream.
        /// </summary>
        /// <value>
        /// 	result nodes per stream
        /// </value>
        public IList<Node>[] NodesPerStream
        {
            get { return _nodesPerStream; }
        }

        #region Repository Members

        public IEnumerator<Cursor> GetCursors(int lookupFromStream)
        {
            if (lookupFromStream == _rootStream)
            {
                yield return new Cursor(_rootEvent, _rootStream, null);
            }
            else
            {
                IList<Node> nodeList = _nodesPerStream[lookupFromStream];
                if (nodeList != null)
                {
                    int nodeListCount = nodeList.Count;
                    for (int ii = 0; ii < nodeListCount; ii++)
                    {
                        Node node = nodeList[ii];
                        ICollection<EventBean> eventCollection = node.Events;
                        foreach (EventBean currEvent in eventCollection)
                        {
                            yield return new Cursor(currEvent, lookupFromStream, node);
                        }
                    }
                }
            }
        }

        public void AddResult(Cursor cursor,
                              ICollection<EventBean> lookupResults,
                              int resultStream)
        {
            if (lookupResults.IsEmpty())
            {
                throw new ArgumentException("Attempting to add zero results");
            }

            Node parentNode = cursor.Node;
            if (parentNode == null)
            {
                var leafNodeInner = new Node(resultStream);
                leafNodeInner.Events = lookupResults;

                if (_nodesPerStream == null)
                {
                    _nodesPerStream = new List<Node>[_numStreams];
                }

                IList<Node> nodesInner = _nodesPerStream[resultStream];
                if (nodesInner == null)
                {
                    nodesInner = new List<Node>();
                    _nodesPerStream[resultStream] = nodesInner;
                }
                leafNodeInner.ParentEvent = _rootEvent;

                nodesInner.Add(leafNodeInner);
                return;
            }

            var leafNode = new Node(resultStream);
            leafNode.Events = lookupResults;
            leafNode.Parent = cursor.Node;
            leafNode.ParentEvent = cursor.Event;

            IList<Node> nodes = _nodesPerStream[resultStream];
            if (nodes == null)
            {
                nodes = new List<Node>();
                _nodesPerStream[resultStream] = nodes;
            }

            nodes.Add(leafNode);
        }

        #endregion
    }
}