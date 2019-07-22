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

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a branch with a single required child node below it.
    /// </summary>
    public class BranchRequiredAssemblyNode : BaseAssemblyNode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        public BranchRequiredAssemblyNode(
            int streamNum,
            int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public override void Init(IList<Node>[] result)
        {
            // need not be concerned with results, all is passed from the child node
        }

        public override void Process(
            IList<Node>[] result,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            // no action here, since we have a required child row
            // The single required child generates all events that may exist
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
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("BranchRequiredAssemblyNode streamNum=" + streamNum);
        }
    }
} // end of namespace