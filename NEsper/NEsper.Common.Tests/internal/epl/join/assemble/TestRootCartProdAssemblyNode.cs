///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestRootCartProdAssemblyNode : AbstractTestBase
    {
        [SetUp]
        public void SetUp()
        {
            rootCartNodeOneReq = new RootCartProdAssemblyNode(1, 5, false, new[] { 0, 0, 0, 1, 2 });

            parentNode = new SupportJoinProcNode(-1, 5);
            parentNode.AddChild(rootCartNodeOneReq);

            // add child nodes to indicate what sub-streams to build the cartesian product from
            rootCartNodeOneReq.AddChild(new SupportJoinProcNode(2, 5));
            rootCartNodeOneReq.AddChild(new SupportJoinProcNode(3, 5));
            rootCartNodeOneReq.AddChild(new SupportJoinProcNode(4, 5));
        }

        private SupportJoinProcNode parentNode;
        private RootCartProdAssemblyNode rootCartNodeOneReq;

        [Test]
        public void TestComputeCombined()
        {
            Assert.IsNull(RootCartProdAssemblyNode.ComputeCombined(new[] { new[] { 2 } }));
            Assert.IsNull(RootCartProdAssemblyNode.ComputeCombined(new[] { new[] { 1 }, new[] { 2 } }));

            var result = RootCartProdAssemblyNode.ComputeCombined(
                new[] { new[] { 3, 4 }, new[] { 2, 5 }, new[] { 6 } });
            Assert.AreEqual(1, result.Length);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5 }, result[0]);

            result = RootCartProdAssemblyNode.ComputeCombined(
                new[] { new[] { 3, 4 }, new[] { 2, 5 }, new[] { 6 }, new[] { 0, 8, 9 } });
            Assert.AreEqual(2, result.Length);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5 }, result[0]);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5, 6 }, result[1]);

            result = RootCartProdAssemblyNode.ComputeCombined(
                new[] { new[] { 3, 4 }, new[] { 2, 5 }, new[] { 6 }, new[] { 0, 8, 9 }, new[] { 1 } });
            Assert.AreEqual(3, result.Length);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5 }, result[0]);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5, 6 }, result[1]);
            EPAssertionUtil.AssertEqualsAnyOrder(new[] { 3, 4, 2, 5, 6, 0, 8, 9 }, result[2]);
        }

        [Test]
        public void TestFlowOptional()
        {
            var rootCartNodeAllOpt = (RootCartProdAssemblyNode) new RootCartProdAssemblyNodeFactory(1, 5, true).MakeAssemblerUnassociated();
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(2, 5));
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(3, 5));
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(4, 5));

            parentNode.AddChild(rootCartNodeAllOpt);

            rootCartNodeAllOpt.Init(null);
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            rootCartNodeAllOpt.Process(null, resultFinalRows, null);

            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(1, parentNode.RowsList.Count);

            var rowArr = supportJoinResultNodeFactory.ConvertTo2DimArr(parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new EventBean[] {null, null, null, null, null}
                },
                rowArr);
        }

        [Test]
        public void TestFlowRequired()
        {
            rootCartNodeOneReq.Init(null);

            var stream2Events = supportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            var stream3Events = supportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            var stream4Events = supportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product

            // Post result from 3, send 2 rows
            IList<EventBean[]> resultFinalRows = new List<EventBean[]>();
            var childRow = new EventBean[5];
            childRow[3] = stream3Events[0];
            rootCartNodeOneReq.Result(childRow, 3, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[3] = stream3Events[1];
            rootCartNodeOneReq.Result(childRow, 3, null, null, resultFinalRows, null);

            // Post result from 2, send 2 rows
            childRow = new EventBean[5];
            childRow[2] = stream2Events[0];
            rootCartNodeOneReq.Result(childRow, 2, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[2] = stream2Events[1];
            rootCartNodeOneReq.Result(childRow, 2, null, null, resultFinalRows, null);

            // Post result from 4
            childRow = new EventBean[5];
            childRow[4] = stream4Events[0];
            rootCartNodeOneReq.Result(childRow, 4, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[4] = stream4Events[1];
            rootCartNodeOneReq.Result(childRow, 4, null, null, resultFinalRows, null);

            // process posted rows (child rows were stored and are compared to find other rows to generate)
            rootCartNodeOneReq.Process(null, resultFinalRows, null);

            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(8, parentNode.RowsList.Count);

            var rowArr = supportJoinResultNodeFactory.ConvertTo2DimArr(parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {null, null, stream2Events[0], stream3Events[0], stream4Events[0]},
                    new[] {null, null, stream2Events[0], stream3Events[1], stream4Events[0]},
                    new[] {null, null, stream2Events[1], stream3Events[0], stream4Events[0]},
                    new[] {null, null, stream2Events[1], stream3Events[1], stream4Events[0]},
                    new[] {null, null, stream2Events[0], stream3Events[0], stream4Events[1]},
                    new[] {null, null, stream2Events[0], stream3Events[1], stream4Events[1]},
                    new[] {null, null, stream2Events[1], stream3Events[0], stream4Events[1]},
                    new[] {null, null, stream2Events[1], stream3Events[1], stream4Events[1]}
                },
                rowArr);
        }
    }
} // end of namespace