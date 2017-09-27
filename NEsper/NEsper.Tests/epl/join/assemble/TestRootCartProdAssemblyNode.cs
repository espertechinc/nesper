///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.epl.join;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.assemble
{
    [TestFixture]
    public class TestRootCartProdAssemblyNode 
    {
        private SupportJoinProcNode _parentNode;
        private RootCartProdAssemblyNode _rootCartNodeOneReq;
    
        [SetUp]
        public void SetUp()
        {
            _rootCartNodeOneReq = new RootCartProdAssemblyNode(1, 5, false, new int[] {0, 0, 0, 1, 2});
    
            _parentNode = new SupportJoinProcNode(-1, 5);
            _parentNode.AddChild(_rootCartNodeOneReq);
    
            // add child nodes to indicate what sub-streams to build the cartesian product from
            _rootCartNodeOneReq.AddChild(new SupportJoinProcNode(2, 5));
            _rootCartNodeOneReq.AddChild(new SupportJoinProcNode(3, 5));
            _rootCartNodeOneReq.AddChild(new SupportJoinProcNode(4, 5));
        }
    
        [Test]
        public void TestFlowOptional()
        {
            RootCartProdAssemblyNode rootCartNodeAllOpt = (RootCartProdAssemblyNode)new RootCartProdAssemblyNodeFactory(1, 5, true).MakeAssemblerUnassociated();
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(2, 5));
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(3, 5));
            rootCartNodeAllOpt.AddChild(new SupportJoinProcNode(4, 5));
    
            _parentNode.AddChild(rootCartNodeAllOpt);
    
            rootCartNodeAllOpt.Init(null);
            List<EventBean[]> resultFinalRows = new List<EventBean[]>();
            rootCartNodeAllOpt.Process(null, resultFinalRows, null);
    
            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(1, _parentNode.RowsList.Count);
    
            EventBean[][] rowArr = SupportJoinResultNodeFactory.ConvertTo2DimArr(_parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] { new EventBean[] {null, null, null, null, null} }, rowArr);
        }
    
        [Test]
        public void TestFlowRequired()
        {
            _rootCartNodeOneReq.Init(null);
    
            EventBean[] stream2Events = SupportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            EventBean[] stream3Events = SupportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
            EventBean[] stream4Events = SupportJoinResultNodeFactory.MakeEvents(2); // for identifying rows in cartesian product
    
            // Post result from 3, send 2 rows
            List<EventBean[]> resultFinalRows = new List<EventBean[]>();
            EventBean[] childRow = new EventBean[5];
            childRow[3] = stream3Events[0];
            _rootCartNodeOneReq.Result(childRow, 3, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[3] = stream3Events[1];
            _rootCartNodeOneReq.Result(childRow, 3, null, null, resultFinalRows, null);

            // Post result from 2, send 2 rows
            childRow = new EventBean[5];
            childRow[2] = stream2Events[0];
            _rootCartNodeOneReq.Result(childRow, 2, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[2] = stream2Events[1];
            _rootCartNodeOneReq.Result(childRow, 2, null, null, resultFinalRows, null);

            // Post result from 4
            childRow = new EventBean[5];
            childRow[4] = stream4Events[0];
            _rootCartNodeOneReq.Result(childRow, 4, null, null, resultFinalRows, null);
            childRow = new EventBean[5];
            childRow[4] = stream4Events[1];
            _rootCartNodeOneReq.Result(childRow, 4, null, null, resultFinalRows, null);

            // process posted rows (child rows were stored and are compared to find other rows to generate)
            _rootCartNodeOneReq.Process(null, resultFinalRows, null);
    
            // 5 generated rows: 2 (stream 2) + 2 (stream 3) + 1 (self, Node 2)
            Assert.AreEqual(8, _parentNode.RowsList.Count);
    
            EventBean[][] rowArr = SupportJoinResultNodeFactory.ConvertTo2DimArr(_parentNode.RowsList);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[]
                {
                    new[] {null, null, stream2Events[0], stream3Events[0], stream4Events[0]},
                    new[] {null, null, stream2Events[0], stream3Events[1], stream4Events[0]},
                    new[] {null, null, stream2Events[1], stream3Events[0], stream4Events[0]},
                    new[] {null, null, stream2Events[1], stream3Events[1], stream4Events[0]},
                    new[] {null, null, stream2Events[0], stream3Events[0], stream4Events[1]},
                    new[] {null, null, stream2Events[0], stream3Events[1], stream4Events[1]},
                    new[] {null, null, stream2Events[1], stream3Events[0], stream4Events[1]},
                    new[] {null, null, stream2Events[1], stream3Events[1], stream4Events[1]},
                }, rowArr);
        }
    
        [Test]
        public void TestComputeCombined()
        {
            Assert.IsNull(RootCartProdAssemblyNode.ComputeCombined(new[] {new [] {2}} ));
            Assert.IsNull(RootCartProdAssemblyNode.ComputeCombined(new[] {new [] {1}, new [] {2}} ));
    
            int[][] result = RootCartProdAssemblyNode.ComputeCombined(
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
    }
}
