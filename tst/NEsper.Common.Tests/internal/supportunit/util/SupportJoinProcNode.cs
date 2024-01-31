///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.assemble;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportJoinProcNode : BaseAssemblyNode
    {
        public SupportJoinProcNode(
            int streamNum,
            int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public IList<EventBean[]> RowsList { get; } = new List<EventBean[]>();

        public IList<int> StreamNumList { get; } = new List<int>();

        public IList<EventBean> MyEventList { get; } = new List<EventBean>();

        public IList<Node> MyNodeList { get; } = new List<Node>();

        public override void Init(IList<Node>[] result)
        {
        }

        public override void Process(
            IList<Node>[] result,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
        }

        public override void Result(
            EventBean[] row,
            int streamNum,
            EventBean myEvent,
            Node myNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent)
        {
            RowsList.Add(row);
            StreamNumList.Add(streamNum);
            MyEventList.Add(myEvent);
            MyNodeList.Add(myNode);
        }

        public override void Print(IndentWriter indentWriter)
        {
            throw new UnsupportedOperationException("unsupported");
        }
    }
} // end of namespace
