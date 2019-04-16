///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a leaf with a no child nodes below it.
    /// </summary>
    public class LeafAssemblyNode : BaseAssemblyNode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        public LeafAssemblyNode(
            int streamNum,
            int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public override void Init(IList<Node>[] result)
        {
        }

        public override void Process(
            IList<Node>[] result,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            var nodes = result[streamNum];
            if (nodes == null) {
                return;
            }

            foreach (var node in nodes) {
                var events = node.Events;
                foreach (var theEvent in events) {
                    ProcessEvent(theEvent, node, resultFinalRows, resultRootEvent);
                }
            }
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
                row, streamNum, currentNode.ParentEvent, currentNode.Parent, resultFinalRows, resultRootEvent);
        }

        public override void Result(
            EventBean[] row,
            int streamNum,
            EventBean myEvent,
            Node myNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            throw new UnsupportedOperationException("Leaf node cannot process child results");
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("LeafAssemblyNode streamNum=" + streamNum);
        }
    }
} // end of namespace