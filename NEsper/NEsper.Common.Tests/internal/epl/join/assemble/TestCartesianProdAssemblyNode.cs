///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestCartesianProdAssemblyNode : CommonTest
    {
        [SetUp]
        public void SetUp()
        {
            optCartNode = new CartesianProdAssemblyNode(1, 4, true, new[] { 0, 0, 0, 1 });

            parentNode = new SupportJoinProcNode(-1, 4);
            parentNode.AddChild(optCartNode);

            // add child nodes to indicate what sub-streams to build the cartesian product from
            optCartNode.AddChild(new SupportJoinProcNode(2, 4));
            optCartNode.AddChild(new SupportJoinProcNode(3, 4));

            resultMultipleEvents = supportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 2, 1); // 2 nodes 1 event each for (1)
            resultSingleEvent = supportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 1, 1); // 1 nodes 1 event each for (1)
        }

        private SupportJoinProcNode parentNode;
        private CartesianProdAssemblyNode optCartNode;
        private IList<Node>[] resultMultipleEvents;
        private IList<Node>[] resultSingleEvent;

        [Test]
        public void TestFlow()
        {
            optCartNode.Init(resultMultipleEvents);

            var stream2Events = supportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            var stream3Events = supportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product

            Node nodeOne = resultMultipleEvents[1][0];
            EventBean eventOneStreamOne = nodeOne.Events.First();
            Node nodeTwo = resultMultipleEvents[1][1];
            EventBean eventTwoStreamOne = nodeTwo.Events.First();

            // generate an event row originating from child 1
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            var childRow = new EventBean[4]; // new rows for each result
            childRow[2] = stream2Events[0];
            optCartNode.Result(childRow, 2, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 2
            childRow = new EventBean[4];
            childRow[2] = stream2Events[1];
            optCartNode.Result(childRow, 2, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 2

            // generate an event row originating from child 2
            childRow = new EventBean[4];
            childRow[3] = stream3Events[0];
            optCartNode.Result(childRow, 3, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 3
            childRow = new EventBean[4];
            childRow[3] = stream3Events[1];
            optCartNode.Result(childRow, 3, eventOneStreamOne, nodeOne, resultFinalRows, null); // child is stream 3

            // process posted rows (child rows were stored and are compared to find other rows to generate)
            optCartNode.Process(resultMultipleEvents, resultFinalRows, null);

            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(5, parentNode.RowsList.Count);

            var rowArr = supportJoinResultNodeFactory.ConvertTo2DimArr(parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {null, eventOneStreamOne, stream2Events[0], stream3Events[0]},
                    new[] {null, eventOneStreamOne, stream2Events[0], stream3Events[1]},
                    new[] {null, eventOneStreamOne, stream2Events[1], stream3Events[0]},
                    new[] {null, eventOneStreamOne, stream2Events[1], stream3Events[1]},
                    new[] {null, eventTwoStreamOne, null, null}
                },
                rowArr);
        }

        [Test]
        public void TestProcessSingleEvent()
        {
            optCartNode.Init(resultSingleEvent);

            // test that the node indeed manufactures event rows for any event not received from a child
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            optCartNode.Process(resultSingleEvent, resultFinalRows, null);

            // check generated row
            Assert.AreEqual(1, parentNode.RowsList.Count);
            EventBean[] row = parentNode.RowsList[0];
            Assert.AreEqual(4, row.Length);
            Node node = resultSingleEvent[1][0];
            Assert.AreEqual(node.Events.First(), row[1]);
        }
    }
} // end of namespace