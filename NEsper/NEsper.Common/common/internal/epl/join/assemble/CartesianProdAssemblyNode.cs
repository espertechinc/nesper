///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a branch with a two or more child nodes (required and optional) below it.
    /// </summary>
    public class CartesianProdAssemblyNode : BaseAssemblyNode
    {
        private readonly bool _allSubStreamsOptional;
        private readonly int[] _childStreamIndex; // maintain mapping of stream number to index in array
        private int[][] _combinedSubStreams; // for any cartesian product past 2 streams

        // For tracking when we have multiple events for this stream
        private IDictionary<EventBean, CartesianProdAssemblyNode.ChildStreamResults> _completedEvents;
        private bool _haveChildResults;

        // keep a reference to results for processing optional child nodes not generating results
        private IList<Node> _resultsForStream;

        // For tracking when we only have a single event for this stream as a result
        private Node _singleResultNode;
        private EventBean _singleResultParentEvent;
        private IList<EventBean[]>[] _singleResultRowsPerStream;

        // maintain for each child the list of stream number descending that child
        private int[][] _subStreamsNumsPerChild;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="allSubStreamsOptional">
        ///     true if all child nodes to this node are optional, or false ifone or more child nodes are required for a result.
        /// </param>
        /// <param name="childStreamIndex">indexes for child streams</param>
        public CartesianProdAssemblyNode(
            int streamNum,
            int numStreams,
            bool allSubStreamsOptional,
            int[] childStreamIndex)
            : base(
                streamNum,
                numStreams)
        {
            this._childStreamIndex = childStreamIndex;
            this._allSubStreamsOptional = allSubStreamsOptional;
        }

        public override void Init(IList<Node>[] result)
        {
            _resultsForStream = result[streamNum];
            _singleResultNode = null;
            _singleResultParentEvent = null;
            _singleResultRowsPerStream = null;
            _haveChildResults = false;

            if (_subStreamsNumsPerChild == null) {
                if (childNodes.Count < 2) {
                    throw new IllegalStateException("Expecting at least 2 child nodes");
                }

                _subStreamsNumsPerChild = new int[childNodes.Count][];
                for (var i = 0; i < childNodes.Count; i++) {
                    _subStreamsNumsPerChild[i] = childNodes[i].Substreams;
                }

                _combinedSubStreams = RootCartProdAssemblyNode.ComputeCombined(_subStreamsNumsPerChild);
            }

            if (_resultsForStream != null) {
                var numNodes = _resultsForStream.Count;
                if (numNodes == 1) {
                    var node = _resultsForStream[0];
                    var nodeEvents = node.Events;

                    // If there is a single result event (typical case)
                    if (nodeEvents.Count == 1) {
                        _singleResultNode = node;
                        _singleResultParentEvent = nodeEvents.First();
                        _singleResultRowsPerStream = new IList<EventBean[]>[childNodes.Count];
                    }
                }

                if (_singleResultNode == null) {
                    _completedEvents = new Dictionary<EventBean, ChildStreamResults>();
                }
            }
            else {
                _completedEvents = new Dictionary<EventBean, ChildStreamResults>();
            }
        }

        public override void Process(
            IList<Node>[] result,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            // there cannot be child nodes to compute a cartesian product if this node had no results
            if (_resultsForStream == null) {
                return;
            }

            // If this node's result set consisted of a single event
            if (_singleResultNode != null) {
                // If no child has posted any rows
                if (!_haveChildResults) {
                    // And all substreams are optional, generate a row
                    if (_allSubStreamsOptional) {
                        var row = new EventBean[numStreams];
                        row[streamNum] = _singleResultParentEvent;
                        parentNode.Result(
                            row,
                            streamNum,
                            _singleResultNode.ParentEvent,
                            _singleResultNode,
                            resultFinalRows,
                            resultRootEvent);
                    }

                    return;
                }

                // Compute the cartesian product
                PostCartesian(_singleResultRowsPerStream, _singleResultNode, resultFinalRows, resultRootEvent);
                return;
            }

            // We have multiple events for this node, generate an event row for each event not yet received from
            // event rows generated by the child node.
            foreach (var node in _resultsForStream) {
                foreach (var theEvent in node.Events) {
                    var results = _completedEvents.Get(theEvent);

                    // If there were no results for the event posted by any child nodes
                    if (results == null) {
                        if (_allSubStreamsOptional) {
                            var row = new EventBean[numStreams];
                            row[streamNum] = theEvent;
                            parentNode.Result(
                                row,
                                streamNum,
                                node.ParentEvent,
                                node.Parent,
                                resultFinalRows,
                                resultRootEvent);
                        }

                        continue;
                    }

                    // Compute the cartesian product
                    PostCartesian(results.RowsPerStream, node, resultFinalRows, resultRootEvent);
                }
            }
        }

        private void PostCartesian(
            IList<EventBean[]>[] rowsPerStream,
            Node node,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            IList<EventBean[]> result = new List<EventBean[]>();
            CartesianUtil.ComputeCartesian(
                rowsPerStream[0],
                _subStreamsNumsPerChild[0],
                rowsPerStream[1],
                _subStreamsNumsPerChild[1],
                result);

            if (rowsPerStream.Length > 2) {
                for (var i = 0; i < _subStreamsNumsPerChild.Length - 2; i++) {
                    IList<EventBean[]> product = new List<EventBean[]>();
                    CartesianUtil.ComputeCartesian(
                        result,
                        _combinedSubStreams[i],
                        rowsPerStream[i + 2],
                        _subStreamsNumsPerChild[i + 2],
                        product);
                    result = product;
                }
            }

            foreach (var row in result) {
                parentNode.Result(row, streamNum, node.ParentEvent, node.Parent, resultFinalRows, resultRootEvent);
            }
        }

        public override void Result(
            EventBean[] row,
            int fromStreamNum,
            EventBean myEvent,
            Node myNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            // fill event in
            row[streamNum] = myEvent;
            var childStreamArrIndex = _childStreamIndex[fromStreamNum];

            // treat single-event result for this stream
            if (_singleResultNode != null) {
                // record the fact that an event that was generated by a child
                _haveChildResults = true;

                if (_singleResultRowsPerStream == null) {
                    _singleResultRowsPerStream = new List<EventBean[]>[childNodes.Count];
                }

                var streamRows = _singleResultRowsPerStream[childStreamArrIndex];
                if (streamRows == null) {
                    streamRows = new List<EventBean[]>();
                    _singleResultRowsPerStream[childStreamArrIndex] = streamRows;
                }

                streamRows.Add(row);
                return;
            }

            var childStreamResults = _completedEvents.Get(myEvent);
            if (childStreamResults == null) {
                childStreamResults = new ChildStreamResults(childNodes.Count);
                _completedEvents.Put(myEvent, childStreamResults);
            }

            childStreamResults.Add(childStreamArrIndex, row);
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("CartesianProdAssemblyNode streamNum=" + streamNum);
        }

        /// <summary>
        ///     Structure to represent a list of event result rows per stream.
        /// </summary>
        public class ChildStreamResults
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="size">number of streams</param>
            public ChildStreamResults(int size)
            {
                RowsPerStream = new List<EventBean[]>[size];
            }

            /// <summary>
            ///     Returns rows per stream.
            /// </summary>
            /// <returns>rows per stream</returns>
            public IList<EventBean[]>[] RowsPerStream { get; }

            /// <summary>
            ///     Add result from stream.
            /// </summary>
            /// <param name="fromStreamIndex">from stream</param>
            /// <param name="row">row to add</param>
            public void Add(
                int fromStreamIndex,
                EventBean[] row)
            {
                var rows = RowsPerStream[fromStreamIndex];
                if (rows == null) {
                    rows = new List<EventBean[]>();
                    RowsPerStream[fromStreamIndex] = rows;
                }

                rows.Add(row);
            }
        }
    }
} // end of namespace