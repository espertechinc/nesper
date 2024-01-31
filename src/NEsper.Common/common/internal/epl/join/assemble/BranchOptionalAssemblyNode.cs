///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a branch with a single optional child node below it.
    /// </summary>
    public class BranchOptionalAssemblyNode : BaseAssemblyNode
    {
        // For tracking when we have multiple events for this stream
        private ISet<EventBean> _completedEvents;
        private bool _haveChildResults;
        private IList<Node> _resultsForStream;
        private EventBean _singleResultEvent;

        // For tracking when we only have a single event for this stream as a result
        private Node _singleResultNode;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        public BranchOptionalAssemblyNode(
            int streamNum,
            int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public override void Init(IList<Node>[] result)
        {
            _resultsForStream = result[streamNum];
            _singleResultNode = null;
            _singleResultEvent = null;
            _haveChildResults = false;

            if (_resultsForStream != null) {
                var numNodes = _resultsForStream.Count;
                if (numNodes == 1) {
                    var node = _resultsForStream[0];
                    var nodeEvents = node.Events;

                    // If there is a single result event (typical case)
                    if (nodeEvents.Count == 1) {
                        _singleResultNode = node;
                        _singleResultEvent = nodeEvents.First();
                    }
                }

                if (_singleResultNode == null) {
                    _completedEvents = new HashSet<EventBean>();
                }
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
                // If there are no child results, post a row
                if (!_haveChildResults) {
                    var row = new EventBean[numStreams];
                    row[streamNum] = _singleResultEvent;
                    parentNode.Result(
                        row,
                        streamNum,
                        _singleResultNode.ParentEvent,
                        _singleResultNode,
                        resultFinalRows,
                        resultRootEvent);
                }

                // if there were child results we are done since they have already been posted to the parent
                return;
            }

            // We have multiple events for this node, generate an event row for each event not yet received from
            // event rows generated by the child node.
            foreach (var node in _resultsForStream) {
                foreach (var theEvent in node.Events) {
                    if (_completedEvents.Contains(theEvent)) {
                        continue;
                    }

                    ProcessEvent(theEvent, node, resultFinalRows, resultRootEvent);
                }
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
            row[streamNum] = myEvent;
            var parentResultNode = myNode.Parent;
            parentNode.Result(row, streamNum, myNode.ParentEvent, parentResultNode, resultFinalRows, resultRootEvent);

            // record the fact that an event that was generated by a child
            _haveChildResults = true;

            // If we had more then on result event for this stream, we need to track all the different events
            // generated by the child node
            if (_singleResultNode == null) {
                _completedEvents.Add(myEvent);
            }
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("BranchOptionalAssemblyNode streamNum=" + streamNum);
        }

        private void ProcessEvent(
            EventBean theEvent,
            Node currentNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            var row = new EventBean[numStreams];
            row[streamNum] = theEvent;
            parentNode.Result(
                row,
                streamNum,
                currentNode.ParentEvent,
                currentNode.Parent,
                resultFinalRows,
                resultRootEvent);
        }
    }
} // end of namespace