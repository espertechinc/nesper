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
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestSingleOptionalAssemblyNode : AbstractCommonTest
    {
        private SupportJoinProcNode parentNode;
        private BranchOptionalAssemblyNode optAssemblyNode;
        private IList<Node>[] resultMultipleEvents;
        private IList<Node>[] resultSingleEvent;

        [SetUp]
        public void SetUp()
        {
            optAssemblyNode = new BranchOptionalAssemblyNode(1, 4);
            parentNode = new SupportJoinProcNode(-1, 4);
            parentNode.AddChild(optAssemblyNode);

            resultMultipleEvents = supportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 2, 1); // 2 nodes 1 event each for (1)
            resultSingleEvent = supportJoinResultNodeFactory.MakeOneStreamResult(4, 1, 1, 1); // 1 nodes 1 event each for (1)
        }

        [Test]
        public void TestProcessMultipleEvents()
        {
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            optAssemblyNode.Init(resultMultipleEvents);

            // generate an event row originating from a child for 1 of the 2 events in the result
            EventBean[] childRow = new EventBean[4];
            Node nodeOne = resultMultipleEvents[1][0];
            EventBean eventOne = nodeOne.Events.First();
            optAssemblyNode.Result(childRow, 3, eventOne, nodeOne, resultFinalRows, null);

            // test that the node indeed manufactures event rows for any event not received from a child
            parentNode.RowsList.Clear();
            optAssemblyNode.Process(resultMultipleEvents, resultFinalRows, null);

            // check generated row
            Assert.AreEqual(1, parentNode.RowsList.Count);
            EventBean[] row = parentNode.RowsList[0];
            Assert.AreEqual(4, row.Length);
            Node nodeTwo = resultMultipleEvents[1][1];
            Assert.AreEqual(nodeTwo.Events.First(), row[1]);
        }

        [Test]
        public void TestProcessSingleEvent()
        {
            optAssemblyNode.Init(resultSingleEvent);

            // test that the node indeed manufactures event rows for any event not received from a child
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            optAssemblyNode.Process(resultMultipleEvents, resultFinalRows, null);

            // check generated row
            Assert.AreEqual(1, parentNode.RowsList.Count);
            EventBean[] row = parentNode.RowsList[0];
            Assert.AreEqual(4, row.Length);
            Node node = resultSingleEvent[1][0];
            Assert.AreEqual(node.Events.First(), row[1]);
        }

        [Test]
        public void TestChildResult()
        {
            optAssemblyNode.Init(resultMultipleEvents);
            TestChildResult(optAssemblyNode, parentNode);
        }

        public void TestChildResult(
            BaseAssemblyNode nodeUnderTest,
            SupportJoinProcNode mockParentNode)
        {
            TestChildResult(nodeUnderTest, mockParentNode, supportJoinResultNodeFactory);
        }

        public static void TestChildResult(
            BaseAssemblyNode nodeUnderTest,
            SupportJoinProcNode mockParentNode,
            SupportJoinResultNodeFactory supportJoinResultNodeFactory)
        {
            EventBean[] childRow = new EventBean[4];
            childRow[3] = supportJoinResultNodeFactory.MakeEvent();

            EventBean myEvent = supportJoinResultNodeFactory.MakeEvent();
            Node myNode = supportJoinResultNodeFactory.MakeNode(3, 1);

            // indicate child result
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            nodeUnderTest.Result(childRow, 3, myEvent, myNode, resultFinalRows, null);

            // assert parent node got the row
            Assert.AreEqual(1, mockParentNode.RowsList.Count);
            EventBean[] resultRow = mockParentNode.RowsList[0];

            // assert the node has added his event to the row
            Assert.AreEqual(myEvent, resultRow[1]);
        }
    }
} // end of namespace
