///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a root with a one required child node below it.
    /// </summary>
    public class RootRequiredAssemblyNode : BaseAssemblyNode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        public RootRequiredAssemblyNode(
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
            parentNode.Result(row, streamNum, null, null, resultFinalRows, resultRootEvent);
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("RootRequiredAssemblyNode streamNum=" + streamNum);
        }
    }
} // end of namespace