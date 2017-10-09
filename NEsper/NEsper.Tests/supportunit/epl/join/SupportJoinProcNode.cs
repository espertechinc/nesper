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
using com.espertech.esper.epl.join.assemble;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.util;

namespace com.espertech.esper.supportunit.epl.join
{
    public class SupportJoinProcNode : BaseAssemblyNode
    {
        private readonly IList<EventBean[]> _rowsList = new List<EventBean[]>();
        private readonly IList<int?> _streamNumList = new List<int?>();
        private readonly IList<EventBean> _myEventList = new List<EventBean>();
        private readonly IList<Node> _myNodeList = new List<Node>();
    
        public SupportJoinProcNode(int streamNum, int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public override void Init(IList<Node>[] result)
        {

        }

        public override void Process(IList<Node>[] result, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {

        }

        public override void Result(EventBean[] row, int streamNum, EventBean myEvent, Node myNode, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {    
            _rowsList.Add(row);
            _streamNumList.Add(streamNum);
            _myEventList.Add(myEvent);
            _myNodeList.Add(myNode);
        }
    
        public override void Print(IndentWriter indentWriter)
        {
            throw new NotSupportedException("unsupported");
        }

        public IList<EventBean[]> RowsList
        {
            get { return _rowsList; }
        }

        public IList<int?> StreamNumList
        {
            get { return _streamNumList; }
        }

        public IList<EventBean> MyEventList
        {
            get { return _myEventList; }
        }

        public IList<Node> MyNodeList
        {
            get { return _myNodeList; }
        }
    }
}
