///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.supportunit.epl.join;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.assemble
{
    [TestFixture]
    public class TestCartesianProdAssemblyNode 
    {
        private SupportJoinProcNode _parentNode;
        private CartesianProdAssemblyNode _optCartNode;
        private IList<Node>[] _resultMultipleEvents;
        private IList<Node>[] _resultSingleEvent;
    
        [SetUp]
        public void SetUp()
        {
            _optCartNode = new CartesianProdAssemblyNode(1, 4, true, new int[] { 0, 0, 0, 1 });
    
            _parentNode = new SupportJoinProcNode(-1, 4);
            _parentNode.AddChild(_optCartNode);
    
            // add child nodes to indicate what sub-streams to build the cartesian product from
            _optCartNode.AddChild(new SupportJoinProcNode(2, 4));
            _optCartNode.AddChild(new SupportJoinProcNode(3, 4));
    
            _resultMultipleEvents = SupportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 2, 1); // 2 nodes 1 event each for (1)
            _resultSingleEvent = SupportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 1, 1); // 1 nodes 1 event each for (1)
        }
    
        [Test]
        public void TestFlow()
        {
            _optCartNode.Init(_resultMultipleEvents);
    
            EventBean[] stream2Events = SupportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            EventBean[] stream3Events = SupportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
    
            Node nodeOne = _resultMultipleEvents[1][0];
            EventBean eventOneStreamOne = nodeOne.Events.First();
            Node nodeTwo = _resultMultipleEvents[1][1];
            EventBean eventTwoStreamOne = nodeTwo.Events.First();
    
            // generate an event row originating from child 1
            List<EventBean[]> resultFinalRows = new List<EventBean[]>();
            EventBean[] childRow = new EventBean[4];        // new rows for each result
            childRow[2] = stream2Events[0];
            _optCartNode.Result(childRow, 2, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 2
            childRow = new EventBean[4];
            childRow[2] = stream2Events[1];
            _optCartNode.Result(childRow, 2, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 2
    
            // generate an event row originating from child 2
            childRow = new EventBean[4];
            childRow[3] = stream3Events[0];
            _optCartNode.Result(childRow, 3, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 3
            childRow = new EventBean[4];
            childRow[3] = stream3Events[1];
            _optCartNode.Result(childRow, 3, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 3
    
            // process posted rows (child rows were stored and are compared to find other rows to generate)
            _optCartNode.Process(_resultMultipleEvents, resultFinalRows, null);
    
            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(5, _parentNode.RowsList.Count);
    
            EventBean[][] rowArr = SupportJoinResultNodeFactory.ConvertTo2DimArr(_parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[]
                {
                    new[] {null, eventOneStreamOne, stream2Events[0], stream3Events[0]},
                    new[] {null, eventOneStreamOne, stream2Events[0], stream3Events[1]},
                    new[] {null, eventOneStreamOne, stream2Events[1], stream3Events[0]},
                    new[] {null, eventOneStreamOne, stream2Events[1], stream3Events[1]},
                    new[] {null, eventTwoStreamOne, null, null},
                }, rowArr);
        }
    
        [Test]
        public void TestProcessSingleEvent()
        {
            _optCartNode.Init(_resultSingleEvent);
    
            // test that the node indeed manufactures event rows for any event not received from a child
            List<EventBean[]> resultFinalRows = new List<EventBean[]>();
            _optCartNode.Process(_resultSingleEvent, resultFinalRows, null);
    
            // check generated row
            Assert.AreEqual(1, _parentNode.RowsList.Count);
            EventBean[] row = _parentNode.RowsList[0];
            Assert.AreEqual(4, row.Length);
            Node node = _resultSingleEvent[1][0];
            Assert.AreEqual(node.Events.First(), row[1]);
        }
    }
}
